// Aluno: Vinicius de Liz da Conceição

using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Domain.ValueObjects;
using AcademiaDoZe.Infrastructure.Data;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Repositories
{
    public class MatriculaRepository : BaseRepository<Matricula>, IMatriculaRepository
    {
        public MatriculaRepository(string connectionString, DatabaseType databaseType)
            : base(connectionString, databaseType) { }

        protected override async Task<Matricula> MapAsync(DbDataReader reader)
        {
            await Task.Yield();

            var endereco = Logradouro.Criar(
                id: Convert.ToInt32(reader["id_logradouro"]),
                cep: reader["cep"].ToString()!,
                nome: reader["logradouro"].ToString()!,
                bairro: reader["bairro"].ToString()!,
                cidade: reader["cidade"].ToString()!,
                estado: reader["estado"].ToString()!,
                pais: reader["pais"].ToString()!
            );

            Arquivo? foto = null;
            if (reader["foto"] != DBNull.Value)
            {
                var fotoBytes = (byte[])reader["foto"];
                if (fotoBytes.Length > 0)
                {
                    foto = Arquivo.Criar(fotoBytes, ".jpg");
                }
            }

            var aluno = Aluno.Criar(
                nome: reader["nome"].ToString()!,
                cpf: reader["cpf"].ToString()!,
                dataNascimento: DateOnly.FromDateTime(Convert.ToDateTime(reader["nascimento"])),
                telefone: reader["telefone"].ToString()!,
                email: reader["email"].ToString()!,
                endereco: endereco,
                numero: reader["numero"].ToString()!,
                complemento: reader["complemento"].ToString()!,
                senha: reader["senha"].ToString()!,
                foto: foto!
            );

            typeof(Entity).GetProperty("Id")?.SetValue(aluno, Convert.ToInt32(reader["id_aluno"]));

            Arquivo? laudoMedico = null; 
            if (reader["laudo_medico"] != DBNull.Value)
            {
                var bytes = (byte[])reader["laudo_medico"];
                if (bytes.Length > 0)
                {
                    laudoMedico = Arquivo.Criar(bytes, ".pdf");
                }
            }

            var matricula = Matricula.Criar(
                alunoMatricula: aluno,
                plano: (EMatriculaPlano)Convert.ToInt32(reader["plano"]),
                dataInicio: DateOnly.FromDateTime(Convert.ToDateTime(reader["data_inicio"])),
                dataFim: DateOnly.FromDateTime(Convert.ToDateTime(reader["data_fim"])),
                objetivo: reader["objetivo"].ToString()!,
                restricoesMedicas: (EMatriculaRestricoes)Convert.ToInt32(reader["restricao_medica"]),
                laudoMedico: laudoMedico,
                observacoesRestricoes: reader["obs_restricao"]?.ToString() ?? string.Empty
            );

            typeof(Entity).GetProperty("Id")?.SetValue(matricula, Convert.ToInt32(reader["id_matricula"]));

            return matricula;
        }

        // ✅ SOBRESCREVER ObterPorId para incluir JOINs necessários
        public override async Task<Matricula?> ObterPorId(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("ID_NAO_INFORMADO_MENOR_UM", nameof(id));
            }

            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $@"
                    SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo,
                           m.restricao_medica, m.laudo_medico, m.obs_restricao,
                           a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                           a.numero, a.complemento, a.senha, a.foto,
                           l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
                    FROM tb_matricula m
                    INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
                    INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                    WHERE m.id_matricula = @Id";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));
                
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return await MapAsync(reader);
                }
                return null;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"ERRO_OBTER_MATRICULA_ID_{id}", ex);
            }
        }

        public override async Task<Matricula> Adicionar(Matricula entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaRepository.Adicionar - Aluno.Id: {entity.AlunoMatricula.Id}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] MatriculaRepository.Adicionar - Aluno.Nome: {entity.AlunoMatricula.Nome}");
                
                string query = _databaseType == DatabaseType.SqlServer
                    ? $"INSERT INTO {TableName} (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, laudo_medico, obs_restricao) " +
                      "OUTPUT INSERTED.id_matricula " +
                      "VALUES (@IdAluno, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @LaudoMedico, @ObsRestricao);"
                    : $"INSERT INTO {TableName} (aluno_id, plano, data_inicio, data_fim, objetivo, restricao_medica, laudo_medico, obs_restricao) " +
                      "VALUES (@IdAluno, @Plano, @DataInicio, @DataFim, @Objetivo, @RestricaoMedica, @LaudoMedico, @ObsRestricao); " +
                      "SELECT LAST_INSERT_ID();";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@IdAluno", entity.AlunoMatricula.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Plano", (int)entity.Plano, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataInicio", entity.DataInicio.ToDateTime(TimeOnly.MinValue), DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataFim", entity.DataFim.ToDateTime(TimeOnly.MinValue), DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Objetivo", entity.Objetivo, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@LaudoMedico", entity.LaudoMedico?.Conteudo ?? Array.Empty<byte>(), DbType.Binary, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@ObsRestricao", entity.ObservacoesRestricoes ?? string.Empty, DbType.String, _databaseType));

                var id = await command.ExecuteScalarAsync();
                if (id != null && id != DBNull.Value)
                {
                    typeof(Entity).GetProperty("Id")?.SetValue(entity, Convert.ToInt32(id));
                }
                return entity;
            }
            catch (DbException ex) 
            { 
                System.Diagnostics.Debug.WriteLine($"[ERROR] MatriculaRepository.Adicionar - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
                throw new InvalidOperationException($"ERRO_ADD_MATRICULA: {ex.Message}", ex); 
            }
        }

        public override async Task<Matricula> Atualizar(Matricula entity)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"UPDATE {TableName} " +
                    "SET aluno_id = @IdAluno, " +
                    "plano = @Plano, " +
                    "data_inicio = @DataInicio, " +
                    "data_fim = @DataFim, " +
                    "objetivo = @Objetivo, " +
                    "restricao_medica = @RestricaoMedica, " +
                    "laudo_medico = @LaudoMedico, " +
                    "obs_restricao = @ObsRestricao " +
                    "WHERE id_matricula = @Id";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", entity.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@IdAluno", entity.AlunoMatricula.Id, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Plano", (int)entity.Plano, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataInicio", entity.DataInicio.ToDateTime(TimeOnly.MinValue), DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@DataFim", entity.DataFim.ToDateTime(TimeOnly.MinValue), DbType.Date, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@Objetivo", entity.Objetivo, DbType.String, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@RestricaoMedica", (int)entity.RestricoesMedicas, DbType.Int32, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@LaudoMedico", entity.LaudoMedico?.Conteudo ?? Array.Empty<byte>(), DbType.Binary, _databaseType));
                command.Parameters.Add(DbProvider.CreateParameter("@ObsRestricao", entity.ObservacoesRestricoes ?? string.Empty, DbType.String, _databaseType));

                int rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new InvalidOperationException($"MATRICULA_NAO_LOCALIZADA_ID_{entity.Id}");
                }
                return entity;
            }
            catch (DbException ex) { throw new InvalidOperationException($"ERRO_UPDATE_MATRICULA: {ex.Message}", ex); }
        }

        public async Task<IEnumerable<Matricula>> ObterPorAluno(int alunoId)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $@"
                    SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo, 
                           m.restricao_medica, m.laudo_medico, m.obs_restricao,
                           a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                           a.numero, a.complemento, a.senha, a.foto,
                           l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
                    FROM tb_matricula m
                    INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
                    INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                    WHERE m.aluno_id = @IdAluno";

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@IdAluno", alunoId, DbType.Int32, _databaseType));

                using var reader = await command.ExecuteReaderAsync();
                var matriculas = new List<Matricula>();
                while (await reader.ReadAsync())
                {
                    matriculas.Add(await MapAsync(reader));
                }
                return matriculas;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"ERRO_OBTER_MATRICULA_POR_ALUNO_{alunoId}", ex);
            }
        }

        public async Task<IEnumerable<Matricula>> ObterAtivas(int idAluno = 0)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $@"
            SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo,
                   m.restricao_medica, m.laudo_medico, m.obs_restricao,
                   a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                   a.numero, a.complemento, a.senha, a.foto,
                   l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
            FROM tb_matricula m
            INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
            INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
            WHERE m.data_fim >= {(_databaseType == DatabaseType.SqlServer ? "GETDATE()" : "CURRENT_DATE()")}
            {(idAluno > 0 ? "AND m.aluno_id = @id" : "")}";
                await using var command = DbProvider.CreateCommand(query, connection);
                if (idAluno > 0)
                {
                    command.Parameters.Add(DbProvider.CreateParameter("@id", idAluno, DbType.Int32, _databaseType));
                }
                using var reader = await command.ExecuteReaderAsync();
                var matriculas = new List<Matricula>();
                while (await reader.ReadAsync())
                {
                    matriculas.Add(await MapAsync(reader));
                }
                return matriculas;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"Erro ao obter matrículas ativas: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<Matricula>> ObterVencendoEmDias(int dias)
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query;
                if (_databaseType == DatabaseType.SqlServer)
                {
                    query = @"
                            SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo,
                                   m.restricao_medica, m.laudo_medico, m.obs_restricao,
                                   a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                                   a.numero, a.complemento, a.senha, a.foto,
                                   l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
                            FROM tb_matricula m
                            INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
                            INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                            WHERE m.data_fim BETWEEN GETDATE() AND DATEADD(DAY, @dias, GETDATE())
                        ";
                }
                else // MySql
                {
                    query = @"
                            SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo,
                                   m.restricao_medica, m.laudo_medico, m.obs_restricao,
                                   a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                                   a.numero, a.complemento, a.senha, a.foto,
                                   l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
                            FROM tb_matricula m
                            INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
                            INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id
                            WHERE m.data_fim BETWEEN CURRENT_DATE() AND DATE_ADD(CURRENT_DATE(), INTERVAL @dias DAY)
                        ";
                }

                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@dias", dias, DbType.Int32, _databaseType));
                using var reader = await command.ExecuteReaderAsync();
                var matriculas = new List<Matricula>();
                while (await reader.ReadAsync())
                {
                    matriculas.Add(await MapAsync(reader));
                }
                return matriculas;
            }
            catch (DbException ex) { throw new InvalidOperationException($"ERRO_OBTER_MATRICULAS_VENCENDO_EM_{dias}_DIAS", ex); }
        }

        public override async Task<IEnumerable<Matricula>> ObterTodos()
        {
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $@"
                    SELECT m.id_matricula, m.plano, m.data_inicio, m.data_fim, m.objetivo,
                           m.restricao_medica, m.laudo_medico, m.obs_restricao,
                           a.id_aluno, a.nome, a.cpf, a.nascimento, a.telefone, a.email,
                           a.numero, a.complemento, a.senha, a.foto,
                           l.id_logradouro, l.cep, l.nome AS logradouro, l.bairro, l.cidade, l.estado, l.pais
                    FROM tb_matricula m
                    INNER JOIN tb_aluno a ON a.id_aluno = m.aluno_id
                    INNER JOIN tb_logradouro l ON l.id_logradouro = a.logradouro_id";

                await using var command = DbProvider.CreateCommand(query, connection);
                await using var reader = await command.ExecuteReaderAsync();
                var entities = new List<Matricula>();

                while (await reader.ReadAsync())
                {
                    entities.Add(await MapAsync(reader));
                }
                return entities;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"ERRO_OBTER_MATRICULAS", ex);
            }
        }
    }
}
