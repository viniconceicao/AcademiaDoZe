﻿// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Repositories;
using AcademiaDoZe.Infrastructure.Exceptions;
using System.Data;
using System.Data.Common;

namespace AcademiaDoZe.Infrastructure.Data
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity>, IAsyncDisposable where TEntity : Entity
    {
        protected readonly string _connectionString;
        protected readonly DatabaseType _databaseType;
        private DbConnection? _connection;
        private bool _disposed = false;
        protected BaseRepository(string connectionString, DatabaseType databaseType)
        {
            // Corrigido para usar o tipo InfrastructureException da infraestrutura explicitamente
            _connectionString = connectionString ?? throw new AcademiaDoZe.Infrastructure.Exceptions.InfrastructureException("ERRO_STRING_CONEXAO" + nameof(connectionString));
            _databaseType = databaseType;
        }
        protected virtual string TableName => $"tb_{typeof(TEntity).Name.ToLower()}";
        protected virtual string IdTableName => $"id_{typeof(TEntity).Name.ToLower()}";
        protected virtual async Task<DbConnection> GetOpenConnectionAsync()
        {
            try
            {
                if (_connection == null)
                {
                    _connection = DbProvider.CreateConnection(_connectionString, _databaseType);
                    await _connection.OpenAsync();
                }
                else if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }
                return _connection;
            }
            catch (DbException ex) { throw new InvalidOperationException($"FALHA_ABRIR_CONEXAO", ex); }
        }
        public virtual async ValueTask DisposeAsync()
        {
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }
        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _connection != null)
                {
                    await _connection.DisposeAsync();
                    _connection = null;
                }
                _disposed = true;
            }
        }
        ~BaseRepository()
        {
            DisposeAsync(false).AsTask().GetAwaiter().GetResult();
        }
        #region métodos de uso geral, não dependem de dados específicos de cada entidade
        #endregion
        #region métodos de uso específico, que devem ser implementados nas classes derivadas
        public abstract Task<TEntity> Adicionar(TEntity entity);
        public abstract Task<TEntity> Atualizar(TEntity entity);
        protected abstract Task<TEntity> MapAsync(DbDataReader reader);
        #endregion

        #region métodos de uso geral, não dependem de dados específicos de cada entidade
        public virtual async Task<TEntity?> ObterPorId(int id)
        {
            if (id <= 0) { throw new ArgumentException("ID_NAO_INFORMADO_MENOR_UM", nameof(id)); }
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"SELECT * FROM {TableName} WHERE {IdTableName} = @Id";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));
                await using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return await MapAsync(reader);
                }
                return null;
            }
            catch (DbException ex) { throw new InvalidOperationException($"ERRO_OBTER_DADOS_ID_{id}", ex); }
        }
        public virtual async Task<IEnumerable<TEntity>> ObterTodos()
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
                var entities = new List<TEntity>();
                while (await reader.ReadAsync())
                {
                    entities.Add(await MapAsync(reader));
                }
                return entities;
            }
            catch (DbException ex) { throw new InvalidOperationException($"ERRO_OBTER_DADOS_TODOS", ex); }
        }
        public virtual async Task<bool> Remover(int id)
        {
            if (id <= 0) { throw new ArgumentException("ID_NAO_INFORMADO_MENOR_UM", nameof(id)); }
            try
            {
                await using var connection = await GetOpenConnectionAsync();
                string query = $"DELETE FROM {TableName} WHERE {IdTableName} = @Id";
                await using var command = DbProvider.CreateCommand(query, connection);
                command.Parameters.Add(DbProvider.CreateParameter("@Id", id, DbType.Int32, _databaseType));
                var result = await command.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (DbException ex)
            {
                throw new InvalidOperationException($"ERRO_REMOVER_ID_{id}", ex);
            }
        }

        public virtual Task<IEnumerable<Logradouro>> ObterPorCidade(string cidade)
        {
            throw new NotImplementedException("ObterPorCidade só deve ser implementado em repositórios de Logradouro.");
        }
        #endregion
    }
}