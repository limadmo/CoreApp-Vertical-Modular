// GlobalSuppressions.cs para CoreApp Backend
// Configurações de supressão específicas para arquitetura de verticais

using System.Diagnostics.CodeAnalysis;

// Supressões específicas para arquitetura de verticais por composição
[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", 
    Justification = "IVerticalEntity permite propriedades públicas para composição")]

[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", 
    Scope = "type", Target = "~T:CoreApp.Domain.Entities.Base.BaseEntity",
    Justification = "BaseEntity é padrão estabelecido na arquitetura")]

[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays", 
    Scope = "member", Target = "~P:CoreApp.Domain.Interfaces.IVerticalEntity.VerticalProperties",
    Justification = "IVerticalEntity requer flexibilidade para diferentes verticais")]

// Supressões para Unit of Work pattern
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", 
    Scope = "type", Target = "~T:CoreApp.Infrastructure.UnitOfWork.UnitOfWork",
    Justification = "UnitOfWork implementa IDisposable conforme padrão estabelecido")]

[assembly: SuppressMessage("Reliability", "CA2007:Consider calling ConfigureAwait on the awaited task", 
    Justification = "ASP.NET Core apps não requerem ConfigureAwait(false)")]

// Supressões para Entity Framework e DTOs
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible", 
    Scope = "type", Target = "~T:CoreApp.Domain.Entities.Configuration",
    Justification = "Configurações aninhadas são apropriadas para organização")]

[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", 
    Scope = "type", Target = "~T:CoreApp.Infrastructure.Data.Configurations",
    Justification = "Configurações EF Core são instanciadas via reflection")]

// Supressões para Controllers e API
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", 
    Scope = "member", Target = "~M:CoreApp.Api.Controllers",
    Justification = "Validação de argumentos é feita via model binding do ASP.NET Core")]

[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", 
    Justification = "Sistema é específico para Brasil (PT-BR)")]

// Supressões para SOLID Principles específicos
[assembly: SuppressMessage("Design", "CA1040:Avoid empty interfaces", 
    Scope = "type", Target = "~T:CoreApp.Domain.Interfaces.IVerticalRepository",
    Justification = "Interface marcadora para repositórios de verticais (ISP)")]

[assembly: SuppressMessage("Design", "CA1005:Avoid excessive parameters on generic types", 
    Scope = "type", Target = "~T:CoreApp.Application.Services.Base.BaseService",
    Justification = "BaseService requer múltiplos parâmetros para SOLID compliance")]

// Supressões para Soft Delete e Interceptors
[assembly: SuppressMessage("Usage", "CA2254:Template should be a static expression", 
    Scope = "member", Target = "~M:CoreApp.Infrastructure.Interceptors.SoftDeleteInterceptor",
    Justification = "Interceptors requerem logging dinâmico")]

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", 
    Scope = "member", Target = "~M:CoreApp.Infrastructure.UnitOfWork.UnitOfWork.SaveChangesAsync",
    Justification = "UnitOfWork deve capturar todas as exceções para rollback")]

// Supressões para Multi-tenant
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness", 
    Scope = "member", Target = "~M:CoreApp.Infrastructure.MultiTenant.TenantService",
    Justification = "Randomness para tenant ID não é security-critical")]

[assembly: SuppressMessage("Design", "CA1056:URI-like properties should not be strings", 
    Scope = "member", Target = "~P:CoreApp.Domain.Configuration.TenantConfiguration.BaseUrl",
    Justification = "URLs de tenant podem ser strings simples")]

// Supressões para Testing com TestContainers
[assembly: SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", 
    Scope = "member", Target = "~M:CoreApp.Tests.Integration.BaseIntegrationTest",
    Justification = "TestContainers gerencia dispose automaticamente")]

[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", 
    Scope = "member", Target = "~M:CoreApp.Tests.Integration.SeedTenantsBrasileiros",
    Justification = "Métodos de seed precisam acessar contexto de instância")]

// Supressões para Migrations do EF Core
[assembly: SuppressMessage("Style", "IDE0058:Expression value is never used", 
    Scope = "type", Target = "~T:CoreApp.Infrastructure.Data.Migrations",
    Justification = "Migrations são geradas automaticamente pelo EF Core")]

[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores", 
    Scope = "type", Target = "~T:CoreApp.Infrastructure.Data.Migrations",
    Justification = "Migrations do EF Core seguem convenção com underscores")]

// Supressões para Domain Events (se implementado)
[assembly: SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", 
    Scope = "type", Target = "~T:CoreApp.Domain.Events",
    Justification = "Domain Events são gerenciados pelo MediatR")]