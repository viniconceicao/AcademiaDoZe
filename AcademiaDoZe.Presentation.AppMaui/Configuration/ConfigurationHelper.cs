using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
namespace AcademiaDoZe.Presentation.AppMaui.Configuration
{
    public static class ConfigurationHelper
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            // dados conexão

            const string dbServer = "127.0.0.1";
            const string dbDatabase = "db_academia_do_ze";
            const string dbUser = "root";
            const string dbPassword = "admin";
            const string dbComplemento = "Port=3306;";
            // se for necessário indicar a porta, incluir junto em dbComplemento

            // Configurações de conexão
            const string connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";

            const EAppDatabaseType databaseType = EAppDatabaseType.MySql;
            // Configura a fábrica de repositórios com a string de conexão e tipo de banco
            services.AddSingleton(new RepositoryConfig
            {
                ConnectionString = connectionString,
                DatabaseType = databaseType
            });
            // configura os serviços da camada de aplicação
            services.AddApplicationServices();
        }
    }
}