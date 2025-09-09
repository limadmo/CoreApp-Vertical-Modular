using Microsoft.EntityFrameworkCore;
using Farmacia.Domain.Entities;
using Farmacia.Domain.Interfaces;
using Farmacia.Infrastructure.Data;

namespace Farmacia.Infrastructure.Repositories;

/// <summary>
/// Repositório para gerenciar módulos do sistema farmacêutico brasileiro
/// Implementa operações específicas para módulos funcionais do SAAS
/// </summary>
/// <remarks>
/// Este repositório gerencia os módulos que podem ser ativados/desativados
/// conforme os planos comerciais contratados pelas farmácias
/// </remarks>
public class ModuloRepository : BaseRepository<ModuloEntity>, IModuloRepository
{
    public ModuloRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém módulo por código único
    /// </summary>
    public async Task<ModuloEntity?> GetByCodeAsync(string codigo)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.Codigo == codigo.ToUpper());
    }

    /// <summary>
    /// Obtém múltiplos módulos por códigos
    /// </summary>
    public async Task<List<ModuloEntity>> GetByCodesAsync(IEnumerable<string> codigos)
    {
        var codigosUpper = codigos.Select(c => c.ToUpper()).ToList();
        
        return await _dbSet
            .Where(m => codigosUpper.Contains(m.Codigo))
            .OrderBy(m => m.OrdemExibicao)
            .ThenBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém módulos por categoria
    /// </summary>
    public async Task<List<ModuloEntity>> GetByCategoryAsync(string categoria)
    {
        return await _dbSet
            .Where(m => m.Categoria == categoria.ToUpper() && m.Ativo)
            .OrderBy(m => m.OrdemExibicao)
            .ThenBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os módulos ativos
    /// </summary>
    public async Task<List<ModuloEntity>> GetActiveAsync()
    {
        return await _dbSet
            .Where(m => m.Ativo)
            .OrderBy(m => m.Categoria)
            .ThenBy(m => m.OrdemExibicao)
            .ThenBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém módulos essenciais (não podem ser desabilitados)
    /// </summary>
    public async Task<List<ModuloEntity>> GetEssentialAsync()
    {
        return await _dbSet
            .Where(m => m.ModuloEssencial && m.Ativo)
            .OrderBy(m => m.OrdemExibicao)
            .ThenBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Conta total de módulos ativos
    /// </summary>
    public async Task<int> CountActiveAsync()
    {
        return await _dbSet.CountAsync(m => m.Ativo);
    }

    /// <summary>
    /// Verifica se código de módulo já existe
    /// </summary>
    public async Task<bool> ExistsCodeAsync(string codigo, Guid? excludeId = null)
    {
        var query = _dbSet.Where(m => m.Codigo == codigo.ToUpper());
        
        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    /// <summary>
    /// Obtém módulos ordenados por categoria e ordem de exibição
    /// </summary>
    public async Task<List<ModuloEntity>> GetOrderedByCategoryAsync()
    {
        return await _dbSet
            .Where(m => m.Ativo)
            .OrderBy(m => m.Categoria)
            .ThenBy(m => m.OrdemExibicao)
            .ThenBy(m => m.Nome)
            .ToListAsync();
    }

    /// <summary>
    /// Busca módulos por texto (nome ou descrição)
    /// </summary>
    public async Task<List<ModuloEntity>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetActiveAsync();

        var term = searchTerm.ToLower();

        return await _dbSet
            .Where(m => m.Ativo && 
                   (m.Nome.ToLower().Contains(term) ||
                    (m.Descricao != null && m.Descricao.ToLower().Contains(term)) ||
                    m.Codigo.ToLower().Contains(term)))
            .OrderBy(m => m.Nome.ToLower().IndexOf(term)) // Prioriza matches no nome
            .ThenBy(m => m.Categoria)
            .ThenBy(m => m.OrdemExibicao)
            .ToListAsync();
    }
}

/// <summary>
/// Repositório para gerenciar planos comerciais brasileiros
/// </summary>
public class PlanoComercialRepository : BaseRepository<PlanoComercialEntity>, IPlanoComercialRepository
{
    public PlanoComercialRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém plano por código único
    /// </summary>
    public async Task<PlanoComercialEntity?> GetByCodeAsync(string codigo)
    {
        return await _dbSet
            .Include(p => p.Modulos)
                .ThenInclude(pm => pm.Modulo)
            .FirstOrDefaultAsync(p => p.Codigo == codigo.ToUpper());
    }

    /// <summary>
    /// Obtém planos ativos para contratação
    /// </summary>
    public async Task<List<PlanoComercialEntity>> GetActiveForContractAsync()
    {
        return await _dbSet
            .Include(p => p.Modulos)
                .ThenInclude(pm => pm.Modulo)
            .Where(p => p.Ativo && 
                   (!p.PlanoPromocional || 
                    (p.ValidadePromocional == null || p.ValidadePromocional > DateTime.UtcNow)))
            .OrderBy(p => p.OrdemExibicao)
            .ThenBy(p => p.PrecoMensalBRL)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém planos ordenados por preço
    /// </summary>
    public async Task<List<PlanoComercialEntity>> GetOrderedByPriceAsync(bool ascending = true)
    {
        var query = _dbSet
            .Include(p => p.Modulos)
                .ThenInclude(pm => pm.Modulo)
            .Where(p => p.Ativo);

        if (ascending)
            query = query.OrderBy(p => p.PrecoMensalBRL);
        else
            query = query.OrderByDescending(p => p.PrecoMensalBRL);

        return await query.ToListAsync();
    }

    /// <summary>
    /// Obtém plano com seus módulos incluídos
    /// </summary>
    public async Task<PlanoComercialEntity?> GetWithModulesAsync(Guid planoId)
    {
        return await _dbSet
            .Include(p => p.Modulos.Where(pm => pm.Ativo))
                .ThenInclude(pm => pm.Modulo)
            .FirstOrDefaultAsync(p => p.Id == planoId);
    }

    /// <summary>
    /// Obtém planos promocionais válidos
    /// </summary>
    public async Task<List<PlanoComercialEntity>> GetActivePromotionalAsync()
    {
        return await _dbSet
            .Include(p => p.Modulos)
                .ThenInclude(pm => pm.Modulo)
            .Where(p => p.Ativo && 
                   p.PlanoPromocional && 
                   (p.ValidadePromocional == null || p.ValidadePromocional > DateTime.UtcNow))
            .OrderBy(p => p.PrecoMensalBRL)
            .ToListAsync();
    }

    /// <summary>
    /// Verifica se código de plano já existe
    /// </summary>
    public async Task<bool> ExistsCodeAsync(string codigo, Guid? excludeId = null)
    {
        var query = _dbSet.Where(p => p.Codigo == codigo.ToUpper());
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }
}

/// <summary>
/// Repositório para associações entre planos e módulos
/// </summary>
public class PlanoModuloRepository : BaseRepository<PlanoModuloEntity>, IPlanoModuloRepository
{
    public PlanoModuloRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém associações por plano
    /// </summary>
    public async Task<List<PlanoModuloEntity>> GetByPlanoAsync(Guid planoId)
    {
        return await _dbSet
            .Include(pm => pm.Modulo)
            .Where(pm => pm.PlanoId == planoId && pm.Ativo)
            .OrderBy(pm => pm.Modulo!.Categoria)
            .ThenBy(pm => pm.Modulo!.OrdemExibicao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém planos que incluem um módulo específico
    /// </summary>
    public async Task<List<PlanoModuloEntity>> GetByModuloAsync(Guid moduloId)
    {
        return await _dbSet
            .Include(pm => pm.Plano)
            .Where(pm => pm.ModuloId == moduloId && pm.Ativo)
            .OrderBy(pm => pm.Plano!.PrecoMensalBRL)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém associação específica entre plano e módulo
    /// </summary>
    public async Task<PlanoModuloEntity?> GetByPlanoAndModuloAsync(Guid planoId, Guid moduloId)
    {
        return await _dbSet
            .Include(pm => pm.Plano)
            .Include(pm => pm.Modulo)
            .FirstOrDefaultAsync(pm => pm.PlanoId == planoId && pm.ModuloId == moduloId);
    }

    /// <summary>
    /// Remove todos os módulos de um plano
    /// </summary>
    public async Task RemoveAllByPlanoAsync(Guid planoId)
    {
        var associacoes = await _dbSet
            .Where(pm => pm.PlanoId == planoId)
            .ToListAsync();

        if (associacoes.Any())
        {
            _dbSet.RemoveRange(associacoes);
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Remove associação específica
    /// </summary>
    public async Task RemoveByPlanoAndModuloAsync(Guid planoId, Guid moduloId)
    {
        var associacao = await GetByPlanoAndModuloAsync(planoId, moduloId);
        if (associacao != null)
        {
            _dbSet.Remove(associacao);
            await SaveChangesAsync();
        }
    }
}

/// <summary>
/// Repositório para contratações de planos pelos tenants
/// </summary>
public class TenantPlanoRepository : BaseRepository<TenantPlanoEntity>, ITenantPlanoRepository
{
    public TenantPlanoRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém contratação ativa de um tenant
    /// </summary>
    public async Task<TenantPlanoEntity?> GetActiveByTenantAsync(string tenantId)
    {
        return await _dbSet
            .Include(tp => tp.Plano)
                .ThenInclude(p => p!.Modulos)
                    .ThenInclude(pm => pm.Modulo)
            .FirstOrDefaultAsync(tp => tp.TenantId == tenantId && tp.EstaAtiva());
    }

    /// <summary>
    /// Obtém histórico de contratações de um tenant
    /// </summary>
    public async Task<List<TenantPlanoEntity>> GetHistoryByTenantAsync(string tenantId)
    {
        return await _dbSet
            .Include(tp => tp.Plano)
            .Where(tp => tp.TenantId == tenantId)
            .OrderByDescending(tp => tp.DataCriacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contratações que vencem em X dias
    /// </summary>
    public async Task<List<TenantPlanoEntity>> GetExpiringInDaysAsync(int days)
    {
        var limitDate = DateTime.UtcNow.AddDays(days);

        return await _dbSet
            .Include(tp => tp.Plano)
            .Where(tp => tp.Status == "ATIVA" && 
                   tp.DataFim != null && 
                   tp.DataFim <= limitDate && 
                   tp.DataFim > DateTime.UtcNow)
            .OrderBy(tp => tp.DataFim)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contratações vencidas
    /// </summary>
    public async Task<List<TenantPlanoEntity>> GetExpiredAsync()
    {
        return await _dbSet
            .Include(tp => tp.Plano)
            .Where(tp => tp.Status == "ATIVA" && 
                   tp.DataFim != null && 
                   tp.DataFim <= DateTime.UtcNow)
            .OrderBy(tp => tp.DataFim)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contratações em período de teste
    /// </summary>
    public async Task<List<TenantPlanoEntity>> GetTrialPeriodsAsync()
    {
        return await _dbSet
            .Include(tp => tp.Plano)
            .Where(tp => tp.PeriodoTeste && tp.DiasTesteRestantes > 0)
            .OrderBy(tp => tp.DiasTesteRestantes)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém contratações por plano
    /// </summary>
    public async Task<List<TenantPlanoEntity>> GetByPlanoAsync(Guid planoId)
    {
        return await _dbSet
            .Where(tp => tp.PlanoId == planoId)
            .OrderByDescending(tp => tp.DataCriacao)
            .ToListAsync();
    }

    /// <summary>
    /// Conta contratações ativas por plano
    /// </summary>
    public async Task<int> CountActiveByPlanoAsync(Guid planoId)
    {
        return await _dbSet
            .CountAsync(tp => tp.PlanoId == planoId && tp.EstaAtiva());
    }

    /// <summary>
    /// Obtém contratação com informações do plano carregadas
    /// </summary>
    public async Task<TenantPlanoEntity?> GetWithPlanByTenantAsync(string tenantId)
    {
        return await _dbSet
            .Include(tp => tp.Plano)
                .ThenInclude(p => p!.Modulos.Where(pm => pm.Ativo))
                    .ThenInclude(pm => pm.Modulo)
            .Include(tp => tp.ModulosAtivos.Where(tm => tm.Ativo))
                .ThenInclude(tm => tm.Modulo)
            .FirstOrDefaultAsync(tp => tp.TenantId == tenantId && tp.EstaAtiva());
    }

    /// <summary>
    /// Cancela todas as contratações ativas de um tenant
    /// </summary>
    public async Task CancelAllActiveByTenantAsync(string tenantId, string motivo, string usuario)
    {
        var contratacoesAtivas = await _dbSet
            .Where(tp => tp.TenantId == tenantId && tp.Status == "ATIVA")
            .ToListAsync();

        foreach (var contratacao in contratacoesAtivas)
        {
            contratacao.Cancelar(motivo, usuario);
        }

        if (contratacoesAtivas.Any())
        {
            await SaveChangesAsync();
        }
    }
}

/// <summary>
/// Repositório para módulos específicos dos tenants
/// </summary>
public class TenantModuloRepository : BaseRepository<TenantModuloEntity>, ITenantModuloRepository
{
    public TenantModuloRepository(ApplicationDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Obtém módulos ativos de um tenant
    /// </summary>
    public async Task<List<TenantModuloEntity>> GetActiveByTenantAsync(string tenantId)
    {
        return await _dbSet
            .Include(tm => tm.Modulo)
            .Where(tm => tm.TenantId == tenantId && tm.Ativo)
            .OrderBy(tm => tm.Modulo!.Categoria)
            .ThenBy(tm => tm.Modulo!.OrdemExibicao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém todos os módulos de um tenant (ativos e inativos)
    /// </summary>
    public async Task<List<TenantModuloEntity>> GetAllByTenantAsync(string tenantId)
    {
        return await _dbSet
            .Include(tm => tm.Modulo)
            .Where(tm => tm.TenantId == tenantId)
            .OrderBy(tm => tm.Modulo!.Categoria)
            .ThenBy(tm => tm.Modulo!.OrdemExibicao)
            .ThenByDescending(tm => tm.DataAtivacao)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém associação específica entre tenant e módulo
    /// </summary>
    public async Task<TenantModuloEntity?> GetByTenantAndModuleAsync(string tenantId, Guid moduloId)
    {
        return await _dbSet
            .Include(tm => tm.Modulo)
            .FirstOrDefaultAsync(tm => tm.TenantId == tenantId && tm.ModuloId == moduloId);
    }

    /// <summary>
    /// Obtém tenants que têm um módulo específico ativo
    /// </summary>
    public async Task<List<TenantModuloEntity>> GetActiveByModuleAsync(Guid moduloId)
    {
        return await _dbSet
            .Include(tm => tm.Modulo)
            .Where(tm => tm.ModuloId == moduloId && tm.Ativo)
            .OrderBy(tm => tm.TenantId)
            .ToListAsync();
    }

    /// <summary>
    /// Conta quantos tenants têm um módulo ativo
    /// </summary>
    public async Task<int> CountActiveByModuleAsync(Guid moduloId)
    {
        return await _dbSet
            .CountAsync(tm => tm.ModuloId == moduloId && tm.Ativo);
    }

    /// <summary>
    /// Remove todos os módulos de um tenant
    /// </summary>
    public async Task RemoveAllByTenantAsync(string tenantId)
    {
        var modulos = await _dbSet
            .Where(tm => tm.TenantId == tenantId)
            .ToListAsync();

        if (modulos.Any())
        {
            _dbSet.RemoveRange(modulos);
            await SaveChangesAsync();
        }
    }

    /// <summary>
    /// Ativa/desativa módulo específico para tenant
    /// </summary>
    public async Task SetModuleStatusAsync(string tenantId, Guid moduloId, bool ativo, string usuario, string? motivo = null)
    {
        var tenantModulo = await GetByTenantAndModuleAsync(tenantId, moduloId);
        
        if (tenantModulo == null && ativo)
        {
            // Criar novo se não existe e está ativando
            tenantModulo = TenantModuloEntity.CriarNova(tenantId, moduloId, usuario);
            tenantModulo.Motivo = motivo;
            await AddAsync(tenantModulo);
        }
        else if (tenantModulo != null)
        {
            // Atualizar existente
            if (ativo)
                tenantModulo.Ativar(usuario, motivo);
            else
                tenantModulo.Desativar(usuario, motivo ?? "Desativação manual");
            
            await UpdateAsync(tenantModulo);
        }
    }

    /// <summary>
    /// Sincroniza módulos do tenant com módulos do plano contratado
    /// </summary>
    public async Task SyncWithPlanModulesAsync(string tenantId, List<string> modulosPlano, string usuario)
    {
        using var transaction = await BeginTransactionAsync();
        
        try
        {
            // Obtém módulos atuais do tenant
            var modulosAtuais = await GetAllByTenantAsync(tenantId);
            var modulosAtuaisDict = modulosAtuais.ToDictionary(tm => tm.Modulo?.Codigo ?? string.Empty);

            // Obtém entidades dos módulos do plano
            var modulosPlanoEntities = await _context.Set<ModuloEntity>()
                .Where(m => modulosPlano.Contains(m.Codigo))
                .ToListAsync();

            // Ativa módulos que devem estar no plano
            foreach (var modulo in modulosPlanoEntities)
            {
                if (modulosAtuaisDict.ContainsKey(modulo.Codigo))
                {
                    // Já existe - ativa se não estiver ativo
                    var tenantModulo = modulosAtuaisDict[modulo.Codigo];
                    if (!tenantModulo.Ativo)
                    {
                        tenantModulo.Ativar(usuario, "Sincronização com plano contratado");
                        await UpdateAsync(tenantModulo);
                    }
                }
                else
                {
                    // Não existe - cria e ativa
                    var novoTenantModulo = TenantModuloEntity.CriarNova(tenantId, modulo.Id, usuario);
                    novoTenantModulo.Motivo = "Sincronização com plano contratado";
                    await AddAsync(novoTenantModulo);
                }
            }

            // Desativa módulos que não estão no plano
            var codigosModulosPlano = modulosPlano.ToHashSet();
            foreach (var tenantModulo in modulosAtuais)
            {
                if (tenantModulo.Modulo != null && 
                    !codigosModulosPlano.Contains(tenantModulo.Modulo.Codigo) && 
                    tenantModulo.Ativo)
                {
                    tenantModulo.Desativar(usuario, "Módulo não incluído no plano contratado");
                    await UpdateAsync(tenantModulo);
                }
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}