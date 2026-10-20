using Microsoft.EntityFrameworkCore;

namespace PotentialWind.Api.Data;

public static class DatabaseProviderExtensions
{
    public const string ProviderSqlite = "Sqlite";
    public const string ProviderMySql = "MySql";

    public static IServiceCollection AddConfiguredDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? ProviderSqlite;

        services.AddDbContext<AppDbContext>(options =>
        {
            if (provider.Equals(ProviderMySql, StringComparison.OrdinalIgnoreCase))
            {
                var mySqlConn = configuration.GetConnectionString("MySqlConnection");
                if (string.IsNullOrWhiteSpace(mySqlConn))
                {
                    throw new InvalidOperationException("A ConnectionString 'MySqlConnection' não foi configurada no appsettings.json.");
                }

                try
                {
                    // Tenta auto-detectar a versão do MySQL conectado
                    options.UseMySql(mySqlConn, ServerVersion.AutoDetect(mySqlConn), mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
                }
                catch
                {
                    // Fallback para MySQL 8.0 se o servidor não estiver respondendo na inicialização do contexto
                    options.UseMySql(mySqlConn, new MySqlServerVersion(new Version(8, 0, 36)), mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null);
                    });
                }
            }
            else
            {
                // Padrão: SQLite
                var sqliteConn = configuration.GetConnectionString("SqliteConnection") ?? "Data Source=potentialwind.db";
                options.UseSqlite(sqliteConn);
            }
        });

        return services;
    }
}
