// Aluno: Vinicius de Liz da Conceição

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class AlunoRepository : BaseRepository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        public override async Task<IEnumerable<Aluno>> ObterTodos()
        {
            await using var connection = await GetOpenConnectionAsync();
            string query = $@"SELECT id_aluno, nome, cpf, nascimento, email, telefone, 
                             logradouro_id, numero, complemento, senha, foto
                             FROM {TableName}";

            await using var command = DbProvider.CreateCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var alunos = new List<Aluno>();
            while (await reader.ReadAsync())
            {
                try
                {
                    var aluno = await MapAsync(reader);
                    alunos.Add(aluno);
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("idade mínima"))
                {
                    // Log o ID do aluno que foi ignorado
                    var alunoId = Convert.ToInt32(reader["id_aluno"]);
                    System.Diagnostics.Debug.WriteLine($"[WARN] Aluno ID {alunoId} ignorado: idade inferior a 12 anos");
                    continue; // Pula para o próximo registro
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Erro ao mapear aluno: {ex.Message}");
                    continue; // Pula para o próximo registro
                }
            }
            return alunos;
        }

        protected override async Task<Aluno> MapAsync(DbDataReader reader)
        {
            // Busca o logradouro pelo ID (foreign key)
            var logradouroId = Convert.ToInt32(reader["logradouro_id"]);
            var logradouroRepo = new LogradouroRepository(_connectionString, _databaseType);
            var logradouro = await logradouroRepo.ObterPorId(logradouroId)
                              ?? throw new InvalidOperationException($"Logradouro {logradouroId} não encontrado.");

            // Foto segura
            Arquivo? foto = null;
            try
            {
                if (reader["foto"] is DBNull == false)
                {
                    foto = Arquivo.Criar((byte[])reader["foto"], ".jpg");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Erro ao mapear foto: {ex.Message}");
                foto = null;
            }

            // Trata a data de nascimento
            var dataNascimento = DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"]));
            
            // Verifica a idade
            var hoje = DateOnly.FromDateTime(DateTime.Today);
            var idade = hoje.Year - dataNascimento.Year;
            if (dataNascimento > hoje.AddYears(-idade)) idade--;

            if (idade < 12)
            {
                throw new InvalidOperationException("A idade mínima para cadastro é 12 anos.");
            }

            var aluno = Aluno.Criar(
                nome: reader["nome"].ToString()!,
                cpf: reader["cpf"].ToString()!,
                dataNascimento: dataNascimento,
                telefone: reader["telefone"].ToString()!,
                email: reader["email"] is DBNull ? string.Empty : reader["email"].ToString()!,
                endereco: logradouro,
                numero: reader["numero"].ToString()!,
                complemento: reader["complemento"] is DBNull ? string.Empty : reader["complemento"].ToString()!,
                senha: reader["senha"].ToString()!,
                foto: foto
            );

            typeof(Entity).GetProperty("Id")?.SetValue(aluno, Convert.ToInt32(reader["id_aluno"]));
            return aluno;
        }

        public override async Task<Aluno> Adicionar(Aluno entity)
        {
            await using var connection = await GetOpenConnectionAsync();

            string query = _databaseType == DatabaseType.SqlServer
                ? $@"INSERT INTO {TableName}
                        (cpf, telefone, nome, nascimento, email, logradouro_id, numero, complemento, senha, foto)
                     OUTPUT INSERTED.id_aluno
                     VALUES
                        (@Cpf, @Telefone, @Nome, @Nascimento, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto);"
                : $@"INSERT INTO {TableName}
                        (cpf, telefone, nome, nascimento, email, logradouro_id, numero, complemento, senha, foto)
                     VALUES
                        (@Cpf, @Telefone, @Nome, @Nascimento, @Email, @LogradouroId, @Numero, @Complemento, @Senha, @Foto);
                     SELECT LAST_INSERT_ID();";

            await using var command = DbProvider.CreateCommand(query, connection);
            AddAlunoParameters(command, entity);

            var id = await command.ExecuteScalarAsync();
            typeof(Entity).GetProperty("Id")?.SetValue(entity, Convert.ToInt32(id));
            return entity;
        }

        public override async Task<Aluno> Atualizar(Aluno entity)
        {
            await using var connection = await GetOpenConnectionAsync();

            string query = $@"UPDATE {TableName}
                              SET cpf=@Cpf,
                                  telefone=@Telefone,
                                  nome=@Nome,
                                  nascimento=@Nascimento,
                                  email=@Email,
                                  logradouro_id=@LogradouroId,
                                  numero=@Numero,
                                  complemento=@Complemento,
                                  senha=@Senha,
                                  foto=@Foto
                              WHERE id_aluno=@Id;";

            await using var command = DbProvider.CreateCommand(query, connection);
            AddAlunoParameters(command, entity);
            command.Parameters.Add(DbProvider.CreateParameter("@Id", entity.Id, DbType.Int32, _databaseType));

            int rows = await command.ExecuteNonQueryAsync();
            if (rows == 0)
                throw new InvalidOperationException($"ALUNO_NAO_LOCALIZADO_ID_{entity.Id}");

            return entity;
        }

        public async Task<Aluno?> ObterPorCpf(string cpf)
        {
            await using var connection = await GetOpenConnectionAsync();
            string query = $"SELECT * FROM {TableName} WHERE cpf=@Cpf";

            await using var command = DbProvider.CreateCommand(query, connection);
            command.Parameters.Add(DbProvider.CreateParameter("@Cpf", cpf, DbType.String, _databaseType));

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? await MapAsync(reader) : null;
        }

        public async Task<bool> CpfJaExiste(string cpf, int? id = null)
        {
            await using var connection = await GetOpenConnectionAsync();
            string query = $"SELECT COUNT(1) FROM {TableName} WHERE cpf=@Cpf";
            if (id.HasValue) query += " AND id_aluno<>@Id";

            await using var command = DbProvider.CreateCommand(query, connection);
            command.Parameters.Add(DbProvider.CreateParameter("@Cpf", cpf, DbType.String, _databaseType));
            if (id.HasValue)
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id.Value, DbType.Int32, _databaseType));

            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task<bool> TrocarSenha(int id, string novaSenha)
        {
            await using var connection = await GetOpenConnectionAsync();
            string query = $"UPDATE {TableName} SET senha=@Senha WHERE id_aluno=@Id";

            await using var command = DbProvider.CreateCommand(query, connection);
            command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Senha", novaSenha, DbType.String, _databaseType));

            return await command.ExecuteNonQueryAsync() > 0;
        }

        private void AddAlunoParameters(DbCommand command, Aluno entity)
        {
            command.Parameters.Add(DbProvider.CreateParameter("@Cpf", entity.Cpf, DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Telefone", entity.Telefone, DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Nome", entity.Nome, DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Nascimento", entity.DataNascimento, DbType.Date, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Email",
                string.IsNullOrWhiteSpace(entity.Email) ? (object)DBNull.Value : entity.Email,
                DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@LogradouroId", entity.Endereco.Id, DbType.Int32, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Numero", entity.Numero, DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Complemento",
                entity.Complemento is null ? (object)DBNull.Value : entity.Complemento,
                DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Senha", entity.Senha, DbType.String, _databaseType));
            command.Parameters.Add(DbProvider.CreateParameter("@Foto",
                entity.Foto?.Conteudo != null ? (object)entity.Foto.Conteudo : DBNull.Value,
                DbType.Binary, _databaseType));

            System.Diagnostics.Debug.WriteLine($"[DEBUG] AddAlunoParameters - Foto presente: {entity.Foto?.Conteudo != null}");
        }
    }
}
