﻿// Aluno: Vinicius de Liz da Conceição
using AcademiaDoZe.Infrastructure.Data;
namespace AcademiaDoZe.Infrastructure.Tests
{
    public abstract class TestBase
    {
        protected string ConnectionString { get; private set; }
        protected DatabaseType DatabaseType { get; private set; }
        protected TestBase()
        {
            // var config = CreateSqlServerConfig();
            var config = CreateMySqlConfig();
            ConnectionString = config.ConnectionString;
            DatabaseType = config.DatabaseType;
        }
        private (string ConnectionString, DatabaseType DatabaseType) CreateSqlServerConfig()
        {
            var connectionString = "Server=localhost;Database=db_academia_do_ze;User Id=sa;Password=abcBolinhas12345;TrustServerCertificate=True;Encrypt=True;";

            return (connectionString, DatabaseType.SqlServer);

        }
        private (string ConnectionString, DatabaseType DatabaseType) CreateMySqlConfig()
        {
            var connectionString = "Server=localhost;Database=db_academia_do_ze;User Id=root;Password=admin;";

            return (connectionString, DatabaseType.MySql);

        }
    }
}