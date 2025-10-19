﻿// Aluno: Vinicius de Liz da Conceição

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;
namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class ColaboradorRepository : BaseRepository<Colaborador>, IColaboradorRepository
    {
        public ColaboradorRepository(string connectionString, DatabaseType databaseType) : base(connectionString, databaseType) { }
        protected override async Task<Colaborador> MapAsync(DbDataReader reader)
        {
            try
            {
                // Obtém o logradouro de forma assíncrona
                var logradouroId = Convert.ToInt32(reader["logradouro_id"]);
                var logradouroRepository = new LogradouroRepository(_connectionString, _databaseType);
                var logradouro = await logradouroRepository.ObterPorId(logradouroId) ?? throw new InvalidOperationException($"Logradouro com ID {logradouroId} não encontrado.");
                // Cria o objeto Colaborador usando o método de fábrica
                var colaborador = Colaborador.Criar(
                cpf: reader["cpf"].ToString()!,
                telefone: reader["telefone"].ToString()!,
                nome: reader["nome"].ToString()!,
                dataNascimento: DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"])),
                email: reader["email"].ToString()!,
                endereco: logradouro,
                numero: reader["numero"].ToString()!,
                complemento: reader["complemento"]?.ToString(),
                senha: reader["senha"].ToString()!,
                foto: reader["foto"] is DBNull ? null : Arquivo.Criar((byte[])reader["foto"], ".jpg"),
                dataAdmissao: DateOnly.FromDateTime(Convert.ToDateTime(reader["admissao"])),
                tipo: (EColaboradorTipo)Convert.ToInt32(reader["tipo"]),
                vinculo: (EColaboradorVinculo)Convert.ToInt32(reader["vinculo"])
                );
                // Define o ID usando reflection
                var idProperty = typeof(Entity).GetProperty("Id");
                idProperty?.SetValue(colaborador, Convert.ToInt32(reader["id_colaborador"]));
                return colaborador;
            }
            catch (DbException ex) { throw new InvalidOperationException($"Erro ao mapear dados do colaborador: {ex.Message}", ex); }
        }

        public override async Task<Colaborador> Adicionar(Colaborador entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = _databaseType == DatabaseType.SqlServer
                ? $"INSERT INTO {TableName} (cpf, telefone, nome, nascimento, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) "
                + "OUTPUT INSERTED.id_colaborador "
                + "VALUES (@Cpf, @Telefone, @Nome, @Nascimento, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo);"
                : $"INSERT INTO {TableName} (cpf, telefone, nome, nascimento, email, logradouro_id, numero, complemento, senha, foto, admissao, tipo, vinculo) "
                + "VALUES (@Cpf, @Telefone, @Nome, @Nascimento, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto, @Admissao, @Tipo, @Vinculo); "
                + "SELECT LAST_INSERT_ID();";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Cpf", entity.Cpf, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Telefone", entity.Telefone, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Nome", entity.Nome, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Nascimento", entity.DataNascimento, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Email", entity.Email, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@LogradouroId", entity.Endereco.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Numero", entity.Numero, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Foto", entity.Foto != null ? (object)entity.Foto.Conteudo : DBNull.Value, DbType.Binary, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Complemento", (object)entity.Complemento ?? DBNull.Value, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Senha", entity.Senha, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Admissao", entity.DataAdmissao, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Tipo", (int)entity.Tipo, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32, _databaseType));
                var id = await command.ExecuteScalarAsync();
                if (id != null && id != DBNull.Value)
                {
                    // Define o ID usando reflection
                    var idProperty = typeof(Entity).GetProperty("Id");
                    idProperty?.SetValue(entity, Convert.ToInt32(id));
                }
                return entity;
            }
            catch (DbException ex) { throw new InvalidOperationException($"Erro ao adicionar colaborador: {ex.Message}", ex); }
        }
        public async Task<Colaborador?> ObterPorCpf(string cpf)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT * FROM {TableName} WHERE cpf = @Cpf";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Cpf", cpf, DbType.String, _databaseType));
                using var reader = await command.ExecuteReaderAsync();
                return await reader.ReadAsync() ? await MapAsync(reader) : null;
            }
            catch (DbException ex) { throw new InvalidOperationException($"Erro ao obter colaborador pelo CPF {cpf}: {ex.Message}", ex); }
        }
        public override async Task<Colaborador> Atualizar(Colaborador entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Repository.Atualizar - Iniciando atualização do ID: {entity.Id}");

                string query = $@"UPDATE {TableName} 
                    SET cpf = @Cpf,
                        telefone = @Telefone,
                        nome = @Nome,
                        nascimento = @Nascimento,
                        email = @Email,
                        logradouro_id = @LogradouroId,
                        numero = @Numero,
                        complemento = @Complemento,
                        senha = @Senha,
                        foto = @Foto,
                        admissao = @Admissao,
                        tipo = @Tipo,
                        vinculo = @Vinculo
                    WHERE id_colaborador = @Id";

                await using var command = DbProvider.CreateCommand(query, connection);

                command.Parameters.Add(DbProvider.CreateParameter("@Id", entity.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Cpf", entity.Cpf, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Telefone", entity.Telefone, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Nome", entity.Nome, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Nascimento", entity.DataNascimento, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Email", entity.Email, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@LogradouroId", entity.Endereco.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Numero", entity.Numero, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Complemento", 
                    entity.Complemento != null ? (object)entity.Complemento : DBNull.Value, 
                    DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Senha", entity.Senha, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Foto", 
                    entity.Foto?.Conteudo != null ? (object)entity.Foto.Conteudo : DBNull.Value, 
                    DbType.Binary, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Admissao", entity.DataAdmissao, DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Tipo", (int)entity.Tipo, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Vinculo", (int)entity.Vinculo, DbType.Int32, _databaseType));

                int rowsAffected = await command.ExecuteNonQueryAsync();
                
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"Colaborador ID {entity.Id} não encontrado.");
                }

                // Retorna o colaborador atualizado
                return await ObterPorId(entity.Id) ?? throw new InvalidOperationException("Erro ao recuperar colaborador após atualização");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Repository.Atualizar - {ex.Message}");
                throw;
            }
        }
        public async Task<bool> CpfJaExiste(string cpf, int? id = null)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT COUNT(1) FROM {TableName} WHERE cpf = @Cpf";
                if (id.HasValue)
                {
                    query += " AND id_colaborador != @Id";
                }
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Cpf", cpf, DbType.String, _databaseType));
                if (id.HasValue)
                {
                    command.Parameters.Add(DbProvider.CreateParameter("@Id", id.Value, DbType.Int32, _databaseType));
                }
                var count = await command.ExecuteScalarAsync();
                return Convert.ToInt32(count) > 0;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao verificar se o CPF {cpf} já existe: {ex.Message}", ex);
            }
        }
        public async Task<bool> TrocarSenha(int id, string novaSenha)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"UPDATE {TableName} SET senha = @NovaSenha WHERE id_colaborador = @Id";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@NovaSenha", novaSenha, DbType.String, _databaseType));
                var linhasAfetadas = await command.ExecuteNonQueryAsync();
                return linhasAfetadas > 0;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao trocar senha do colaborador ID {id}: {ex.Message}", ex);
            }
        }

        public override async Task<IEnumerable<Colaborador>> ObterTodos()
        {
            await using var connection = await GetOpenConnectionAsync();
            string query = $@"
        SELECT id_colaborador, cpf, telefone, nome, nascimento, email,
               logradouro_id, numero, complemento, senha, foto,
               admissao, tipo, vinculo
        FROM {TableName};";

            await using var command = DbProvider.CreateCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var lista = new List<Colaborador>();
            while (await reader.ReadAsync())
                lista.Add(await MapAsync(reader));

            return lista;
        }

    }
}