using Farmacia.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog para logging estruturado
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.WithProperty("Application", "Farmacia.Api")
    .Enrich.WithProperty("Version", "3.0.0-SAAS-BR")
    .CreateLogger();

builder.Host.UseSerilog();

// Configurar versionamento de API no padrão Rails
builder.Services.AddApiVersioning(opt =>
{
    opt.DefaultApiVersion = new ApiVersion(1, 0);
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
});

builder.Services.AddVersionedApiExplorer(setup =>
{
    setup.GroupNameFormat = "'v'VVV";
    setup.SubstituteApiVersionInUrl = true;
});

// Registrar serviços de infraestrutura (EF Core + Redis + Multi-tenant)
builder.Services.AddInfrastructure(builder.Configuration);

// Configurar controllers
builder.Services.AddControllers();

// Configurar CORS para multi-tenant brasileiro
var allowedOrigins = builder.Configuration.GetValue<string>("CORS:AllowedOrigins")?.Split(',') 
    ?? new[] { "http://localhost", "http://*.localhost", "https://*.diegolima.dev" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("FarmaciaMultiTenantPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .SetIsOriginAllowedToAllowWildcardSubdomains()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Configurar Swagger para cada versão
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Farmácia SAAS Multi-tenant API",
        Version = "v1",
        Description = "Sistema brasileiro para gestão de farmácias com compliance ANVISA",
        Contact = new OpenApiContact
        {
            Name = "Suporte Técnico",
            Email = "suporte@farmacia.com.br"
        },
        License = new OpenApiLicense
        {
            Name = "Proprietário - Uso Comercial",
            Url = new Uri("https://farmacia.com.br/licenca")
        }
    });

    // Incluir comentários XML na documentação
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configurar autenticação JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configurar pipeline de middleware

// Swagger apenas em desenvolvimento e staging
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Gerar UI para cada versão da API
        foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                             $"Farmácia API {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
    });
}

// CORS deve vir antes da autenticação
app.UseCors("FarmaciaMultiTenantPolicy");

// HTTPS redirect apenas em produção
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Configurar roteamento com versionamento
app.MapControllers();

// Health check para monitoramento
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "v1",
    Environment = app.Environment.EnvironmentName,
    Database = "Connected", // TODO: Implementar check real do banco
    Cache = "Connected",    // TODO: Implementar check real do Redis
    Uptime = $"{DateTime.UtcNow - Process.GetCurrentProcess().StartTime:dd\\:hh\\:mm\\:ss}"
})).WithTags("Health");

// Endpoint raiz com informações da API
app.MapGet("/", () => Results.Ok(new
{
    Application = "Farmácia SAAS Multi-tenant API",
    Version = "v1",
    Description = "Sistema brasileiro para gestão de farmácias com compliance ANVISA",
    Documentation = "/swagger",
    Health = "/health",
    Endpoints = new
    {
        V1 = new
        {
            Produtos = "/v1/produtos",
            Vendas = "/v1/vendas",
            Estoque = "/v1/estoque",
            Clientes = "/v1/clientes",
            Relatorios = "/v1/relatorios"
        }
    },
    MultiTenant = new
    {
        DefaultTenant = "demo",
        TenantHeader = "X-Tenant-ID",
        SubdomainPattern = "{tenant}.farmacia.com.br"
    }
})).WithTags("Info");

try
{
    Log.Information("🚀 Iniciando Farmácia SAAS Multi-tenant API v1");
    Log.Information("🇧🇷 Sistema 100% brasileiro com compliance ANVISA");
    Log.Information("🏢 Multi-tenant: {DefaultTenant}", builder.Configuration["MultiTenant:DefaultTenant"]);
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Falha crítica ao iniciar aplicação");
}
finally
{
    Log.CloseAndFlush();
}