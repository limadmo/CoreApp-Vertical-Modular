using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using CoreApp.Infrastructure.Services;
using CoreApp.Infrastructure.Middleware;
using CoreApp.Infrastructure.Data.Context;
using CoreApp.Infrastructure.Data.Seeds;
using CoreApp.Domain.Interfaces.Services;
using CoreApp.Domain.Interfaces.Common;
using CoreApp.Api.Services;
using CoreApp.Verticals.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CoreApp API",
        Version = "v1",
        Description = "Sistema SAAS Multi-tenant CoreApp"
    });
    
    // Incluir comentários XML
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Core services
// *** CACHE DESABILITADO EM DESENVOLVIMENTO ***
if (builder.Environment.IsDevelopment())
{
    // Memory cache desabilitado em desenvolvimento para facilitar debugging
    builder.Services.AddSingleton<Microsoft.Extensions.Caching.Memory.IMemoryCache, DisabledMemoryCache>();
}
else
{
    // Cache habilitado apenas em produção
    builder.Services.AddMemoryCache();
}

builder.Services.AddLogging();
builder.Services.AddHttpContextAccessor();

// Add Entity Framework database context
builder.Services.AddDbContext<CoreAppDbContext>(options =>
{
    // Connection string para desenvolvimento local
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? 
                          "Host=localhost;Database=coreapp_dev;Username=postgres;Password=postgres;Port=5432";
    
    options.UseNpgsql(connectionString);
    
    // *** CACHE EF CORE DESABILITADO EM DESENVOLVIMENTO ***
    if (builder.Environment.IsDevelopment())
    {
        // Desabilita cache de service provider do EF Core
        options.EnableServiceProviderCaching(false);
        
        // Habilita logs detalhados e sensitive data para debugging
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging()
               .EnableDetailedErrors();
    }
});

// Add custom services (usando mocks para desenvolvimento)
builder.Services.AddScoped<IModuleValidationService, MockModuleValidationService>();

// Configuração básica de tenant context
builder.Services.AddScoped<ITenantContext, CoreApp.Infrastructure.Services.TenantContext>();

// Database seeder para dados com Bogus
builder.Services.AddScoped<DatabaseSeeder>();

// *** SISTEMA DE VERTICAIS DINÂMICAS ***
// Adiciona o sistema completo de verticais com DI avançado
builder.Services.AddVerticalSystem();

var app = builder.Build();

// *** CONFIGURAÇÃO AUTOMÁTICA DE VERTICAIS ***
// Registra todas as verticais no sistema de forma automática
app.ConfigureVerticals();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CoreApp API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

// *** MIDDLEWARE DE RESOLUÇÃO DE TENANT ***
// Resolve tenant automaticamente via header ou subdomínio
app.UseTenantResolution();

// *** MIDDLEWARE DE INTERCEPTAÇÃO VERTICAL ***
// Aplica automaticamente validações de verticais nos requests
app.UseVerticalInterception();

app.UseAuthorization();
app.MapControllers();

app.Run();
