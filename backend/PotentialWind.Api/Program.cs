using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using PotentialWind.Api.Data;
using PotentialWind.Api.Services;
using PotentialWind.Api.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Controllers e Serialização JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Configuração de CORS para permitir requisições do Angular (porta 4200 e localhost)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuração do Banco de Dados (SQLite com suporte transparente a MySQL)
builder.Services.AddConfiguredDatabase(builder.Configuration);

// Injeção de Dependências dos Serviços de Engenharia Eólica
builder.Services.AddScoped<IWindCalculationService, WindCalculationService>();

// Configuração do Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PotentialWind API",
        Version = "v1",
        Description = "API de Cálculo e Análise de Potencial Energético para Parques Eólicos. Modelagem de Weibull, perfis verticais de vento, efeito esteira e viabilidade econômica.",
        Contact = new OpenApiContact
        {
            Name = "PotentialWind Engineering",
            Email = "contato@potentialwind.local"
        }
    });
});

var app = builder.Build();

// Inicialização e Seed do Banco de Dados
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        await SeedData.InitializeAsync(context);
        app.Logger.LogInformation("Banco de dados inicializado e sincronizado com sucesso.");
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Ocorreu um erro ao inicializar o banco de dados.");
    }
}

// Configuração do pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PotentialWind API v1");
    c.RoutePrefix = string.Empty; // Swagger na raiz http://localhost:5000/
});

app.UseCors("AllowAngularDev");

app.UseAuthorization();

app.MapControllers();

app.Run();
