using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PotentialWind.Api.Data;
using PotentialWind.Api.Models.DTOs;

namespace PotentialWind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public DatabaseController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("status")]
    public async Task<ActionResult<DatabaseStatusDto>> GetDatabaseStatus()
    {
        var provider = _configuration.GetValue<string>("DatabaseProvider") ?? DatabaseProviderExtensions.ProviderSqlite;
        bool canConnect = false;
        int farmsCount = 0;
        int turbinesCount = 0;
        int measurementsCount = 0;

        try
        {
            canConnect = await _context.Database.CanConnectAsync();
            if (canConnect)
            {
                farmsCount = await _context.WindFarms.CountAsync();
                turbinesCount = await _context.WindTurbines.CountAsync();
                measurementsCount = await _context.WindMeasurements.CountAsync();
            }
        }
        catch
        {
            canConnect = false;
        }

        string connString = provider.Equals(DatabaseProviderExtensions.ProviderMySql, StringComparison.OrdinalIgnoreCase)
            ? _configuration.GetConnectionString("MySqlConnection") ?? "Não configurada"
            : _configuration.GetConnectionString("SqliteConnection") ?? "Data Source=potentialwind.db";

        // Mascara senha se houver no MySQL
        string maskedConn = connString;
        if (maskedConn.Contains("Password=", StringComparison.OrdinalIgnoreCase))
        {
            maskedConn = System.Text.RegularExpressions.Regex.Replace(maskedConn, "(Password=)([^;]+)", "$1********", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        string migrationNotes = provider.Equals(DatabaseProviderExtensions.ProviderMySql, StringComparison.OrdinalIgnoreCase)
            ? "O backend está operando com MySQL via Pomelo.EntityFrameworkCore.MySql."
            : "O backend está utilizando SQLite (potentialwind.db). Para migrar para MySQL, altere 'DatabaseProvider' para 'MySql' em appsettings.json e configure 'MySqlConnection'. Os esquemas e tipos foram modelados de forma compatível (ANSI SQL standard) para migração instantânea via EnsureCreated() ou EF Core Migrations.";

        return Ok(new DatabaseStatusDto
        {
            CurrentProvider = provider,
            ConnectionStringSample = maskedConn,
            CanConnect = canConnect,
            WindFarmsCount = farmsCount,
            WindTurbinesCount = turbinesCount,
            MeasurementsCount = measurementsCount,
            MigrationNotes = migrationNotes
        });
    }

    /// <summary>
    /// Retorna instruções detalhadas em markdown para a equipe migrar para MySQL
    /// </summary>
    [HttpGet("migration-guide")]
    public IActionResult GetMigrationGuide()
    {
        var guide = new
        {
            title = "Guia de Migração SQLite -> MySQL para PotentialWind",
            steps = new[]
            {
                "1. Ter um servidor MySQL / MariaDB em execução (ex: porta 3306).",
                "2. Criar uma base de dados vazia: `CREATE DATABASE potentialwind CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;`",
                "3. No arquivo `appsettings.json` (ou variáveis de ambiente), alterar:",
                "   \"DatabaseProvider\": \"MySql\"",
                "   \"ConnectionStrings\": { \"MySqlConnection\": \"Server=localhost;Port=3306;Database=potentialwind;User=root;Password=sua_senha;\" }",
                "4. Reiniciar a aplicação backend. O método SeedData.InitializeAsync() executará context.Database.EnsureCreatedAsync() automaticamente, gerando todas as tabelas e dados iniciais no MySQL.",
                "5. (Opcional para ambientes CI/CD) Para gerar migrações tipadas com versionamento estrito, execute: `dotnet ef migrations add InitialMySql --context AppDbContext` e `dotnet ef database update`."
            }
        };

        return Ok(guide);
    }
}
