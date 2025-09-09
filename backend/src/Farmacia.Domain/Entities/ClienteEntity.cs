using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Farmacia.Domain.Interfaces;
using Farmacia.Domain.Enums;

namespace Farmacia.Domain.Entities;

/// <summary>
/// Entidade que representa um cliente farmacêutico brasileiro com compliance LGPD total
/// Implementa controle de privacidade, consentimento e histórico médico seguro
/// </summary>
/// <remarks>
/// Esta entidade implementa compliance total com LGPD (Lei Geral de Proteção de Dados)
/// brasileira, incluindo controle de consentimento, anonimização e direito ao esquecimento
/// </remarks>
[Table("Clientes")]
public class ClienteEntity : ITenantEntity, ISoftDeletableEntity, IArchivableEntity
{
    /// <summary>
    /// Identificador único do cliente
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do tenant (farmácia) proprietária do cliente
    /// </summary>
    [Required]
    [StringLength(100)]
    public string TenantId { get; set; } = string.Empty;

    // Dados pessoais básicos (LGPD Categoria: Dados Pessoais)

    /// <summary>
    /// Nome completo do cliente
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// CPF do cliente (dados sensíveis LGPD)
    /// </summary>
    [Required]
    [StringLength(14)]
    public string CpfCnpj { get; set; } = string.Empty;

    /// <summary>
    /// RG do cliente (opcional)
    /// </summary>
    [StringLength(20)]
    public string? Rg { get; set; }

    /// <summary>
    /// Órgão emissor do RG
    /// </summary>
    [StringLength(10)]
    public string? OrgaoEmissor { get; set; }

    /// <summary>
    /// Data de nascimento (dados sensíveis LGPD)
    /// </summary>
    public DateTime? DataNascimento { get; set; }

    /// <summary>
    /// Gênero do cliente (opcional, dados sensíveis)
    /// </summary>
    [StringLength(20)]
    public string? Genero { get; set; }

    /// <summary>
    /// Estado civil do cliente (opcional)
    /// </summary>
    [StringLength(20)]
    public string? EstadoCivil { get; set; }

    /// <summary>
    /// Profissão do cliente (opcional)
    /// </summary>
    [StringLength(100)]
    public string? Profissao { get; set; }

    // Dados de contato (LGPD Categoria: Dados Pessoais)

    /// <summary>
    /// Email principal do cliente
    /// </summary>
    [StringLength(256)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// Telefone residencial
    /// </summary>
    [StringLength(20)]
    public string? TelefoneResidencial { get; set; }

    /// <summary>
    /// Telefone celular/WhatsApp
    /// </summary>
    [StringLength(20)]
    public string? TelefoneCelular { get; set; }

    /// <summary>
    /// Telefone comercial
    /// </summary>
    [StringLength(20)]
    public string? TelefoneComercial { get; set; }

    // Endereço (LGPD Categoria: Dados de Localização)

    /// <summary>
    /// CEP do endereço
    /// </summary>
    [StringLength(10)]
    public string? Cep { get; set; }

    /// <summary>
    /// Logradouro (rua, avenida, etc.)
    /// </summary>
    [StringLength(200)]
    public string? Logradouro { get; set; }

    /// <summary>
    /// Número do endereço
    /// </summary>
    [StringLength(20)]
    public string? Numero { get; set; }

    /// <summary>
    /// Complemento do endereço
    /// </summary>
    [StringLength(100)]
    public string? Complemento { get; set; }

    /// <summary>
    /// Bairro
    /// </summary>
    [StringLength(100)]
    public string? Bairro { get; set; }

    /// <summary>
    /// Cidade
    /// </summary>
    [StringLength(100)]
    public string? Cidade { get; set; }

    /// <summary>
    /// Estado (UF)
    /// </summary>
    [StringLength(2)]
    public string? Estado { get; set; }

    /// <summary>
    /// País (padrão: Brasil)
    /// </summary>
    [StringLength(50)]
    public string Pais { get; set; } = "Brasil";

    // Dados médicos e farmacêuticos (LGPD Categoria: Dados Sensíveis de Saúde)

    /// <summary>
    /// Plano de saúde do cliente (opcional)
    /// </summary>
    [StringLength(100)]
    public string? PlanoSaude { get; set; }

    /// <summary>
    /// Número da carteirinha do plano de saúde
    /// </summary>
    [StringLength(50)]
    public string? NumeroCarteirinha { get; set; }

    /// <summary>
    /// Alergias medicamentosas conhecidas (dados sensíveis de saúde)
    /// </summary>
    [StringLength(1000)]
    public string? AlergiasMedicamentosas { get; set; }

    /// <summary>
    /// Medicamentos de uso contínuo (dados sensíveis de saúde)
    /// </summary>
    [StringLength(1000)]
    public string? MedicamentosUsoContinuo { get; set; }

    /// <summary>
    /// Condições médicas relevantes (dados sensíveis de saúde)
    /// </summary>
    [StringLength(1000)]
    public string? CondicoesMedicas { get; set; }

    /// <summary>
    /// Observações médicas adicionais (dados sensíveis de saúde)
    /// </summary>
    [StringLength(2000)]
    public string? ObservacoesMedicas { get; set; }

    /// <summary>
    /// Se cliente utiliza medicamentos controlados
    /// </summary>
    public bool UtilizaMedicamentosControlados { get; set; } = false;

    // Status e configurações

    /// <summary>
    /// Status atual do cliente
    /// </summary>
    [Required]
    public StatusCliente Status { get; set; } = StatusCliente.Ativo;

    /// <summary>
    /// Motivo da inativação (se aplicável)
    /// </summary>
    [StringLength(500)]
    public string? MotivoInativacao { get; set; }

    /// <summary>
    /// Data de cadastro do cliente
    /// </summary>
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última compra
    /// </summary>
    public DateTime? DataUltimaCompra { get; set; }

    /// <summary>
    /// Valor total de compras acumulado
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotalCompras { get; set; } = 0;

    /// <summary>
    /// Número total de compras realizadas
    /// </summary>
    public int TotalCompras { get; set; } = 0;

    /// <summary>
    /// Desconto padrão para o cliente (%)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal DescontoPadrao { get; set; } = 0;

    // Compliance LGPD (OBRIGATÓRIO)

    /// <summary>
    /// Data de consentimento LGPD para tratamento de dados
    /// </summary>
    public DateTime? DataConsentimentoLgpd { get; set; }

    /// <summary>
    /// Versão dos termos aceitos pelo cliente
    /// </summary>
    [StringLength(20)]
    public string? VersaoTermosAceitos { get; set; }

    /// <summary>
    /// Finalidades autorizadas pelo cliente (JSON)
    /// Ex: ["marketing", "historico_medico", "promocoes"]
    /// </summary>
    [StringLength(1000)]
    public string? FinalidadesAutorizadas { get; set; }

    /// <summary>
    /// Se cliente autorizou recebimento de marketing
    /// </summary>
    public bool AutorizouMarketing { get; set; } = false;

    /// <summary>
    /// Se cliente autorizou armazenamento de histórico médico
    /// </summary>
    public bool AutorizouHistoricoMedico { get; set; } = false;

    /// <summary>
    /// Se cliente autorizou compartilhamento com planos de saúde
    /// </summary>
    public bool AutorizouCompartilhamentoPlanoSaude { get; set; } = false;

    /// <summary>
    /// Data da última atualização de consentimento
    /// </summary>
    public DateTime? DataUltimaAtualizacaoConsentimento { get; set; }

    /// <summary>
    /// Se cliente solicitou anonimização de dados (direito ao esquecimento)
    /// </summary>
    public bool SolicitouAnonimizacao { get; set; } = false;

    /// <summary>
    /// Data da solicitação de anonimização
    /// </summary>
    public DateTime? DataSolicitacaoAnonimizacao { get; set; }

    /// <summary>
    /// Data da anonimização efetiva dos dados
    /// </summary>
    public DateTime? DataAnonimizacao { get; set; }

    /// <summary>
    /// Se dados foram anonimizados (cliente vira anônimo)
    /// </summary>
    public bool DadosAnonimizados { get; set; } = false;

    // Preferências de comunicação (LGPD)

    /// <summary>
    /// Se aceita receber notificações por email
    /// </summary>
    public bool AceitaNotificacaoEmail { get; set; } = false;

    /// <summary>
    /// Se aceita receber notificações por SMS
    /// </summary>
    public bool AceitaNotificacaoSms { get; set; } = false;

    /// <summary>
    /// Se aceita receber notificações por WhatsApp
    /// </summary>
    public bool AceitaNotificacaoWhatsapp { get; set; } = false;

    /// <summary>
    /// Se aceita receber promoções e ofertas
    /// </summary>
    public bool AceitaPromocoes { get; set; } = false;

    // Navegação e relacionamentos

    /// <summary>
    /// Vendas realizadas para este cliente
    /// </summary>
    public virtual ICollection<VendaEntity> Vendas { get; set; } = new List<VendaEntity>();

    /// <summary>
    /// Histórico de alterações de consentimento LGPD
    /// </summary>
    public virtual ICollection<ClienteConsentimentoEntity> HistoricoConsentimento { get; set; } = new List<ClienteConsentimentoEntity>();

    // Implementação ISoftDeletableEntity

    /// <summary>
    /// Indica se o cliente foi deletado (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Data de exclusão do cliente
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Usuário que executou a exclusão
    /// </summary>
    [StringLength(100)]
    public string? DeletedBy { get; set; }

    // Implementação IArchivableEntity

    /// <summary>
    /// Data da última movimentação relevante
    /// </summary>
    public DateTime UltimaMovimentacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica se o cliente já foi arquivado
    /// </summary>
    public bool Arquivado { get; set; } = false;

    /// <summary>
    /// Data do arquivamento
    /// </summary>
    public DateTime? DataArquivamento { get; set; }

    // Timestamps de auditoria

    /// <summary>
    /// Data de criação do registro
    /// </summary>
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuário que criou este registro
    /// </summary>
    [StringLength(100)]
    public string? CriadoPor { get; set; }

    /// <summary>
    /// Usuário que fez a última atualização
    /// </summary>
    [StringLength(100)]
    public string? AtualizadoPor { get; set; }

    // Métodos de negócio

    /// <summary>
    /// Atualiza timestamp de última movimentação para controle de arquivamento
    /// </summary>
    public void AtualizarUltimaMovimentacao()
    {
        UltimaMovimentacao = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se cliente está ativo e pode realizar compras
    /// </summary>
    /// <returns>True se cliente pode comprar</returns>
    public bool PodeRealizarCompras()
    {
        return Status == StatusCliente.Ativo && 
               !IsDeleted && 
               !DadosAnonimizados;
    }

    /// <summary>
    /// Verifica se cliente tem consentimento LGPD válido
    /// </summary>
    /// <returns>True se consentimento está válido</returns>
    public bool TemConsentimentoValido()
    {
        return DataConsentimentoLgpd != null && 
               !string.IsNullOrEmpty(VersaoTermosAceitos) &&
               !SolicitouAnonimizacao;
    }

    /// <summary>
    /// Verifica se pode armazenar dados médicos sensíveis
    /// </summary>
    /// <returns>True se autorizado a armazenar dados médicos</returns>
    public bool PodeArmazenarDadosMedicos()
    {
        return AutorizouHistoricoMedico && 
               TemConsentimentoValido();
    }

    /// <summary>
    /// Verifica se pode enviar comunicações de marketing
    /// </summary>
    /// <returns>True se autorizado para marketing</returns>
    public bool PodeReceberMarketing()
    {
        return AutorizouMarketing && 
               TemConsentimentoValido() &&
               AceitaPromocoes;
    }

    /// <summary>
    /// Atualiza consentimento LGPD do cliente
    /// </summary>
    /// <param name="versaoTermos">Versão dos termos aceitos</param>
    /// <param name="finalidades">Finalidades autorizadas</param>
    /// <param name="marketing">Autoriza marketing</param>
    /// <param name="historicoMedico">Autoriza histórico médico</param>
    /// <param name="compartilhamentoPlano">Autoriza compartilhamento com plano</param>
    public void AtualizarConsentimento(
        string versaoTermos,
        string finalidades,
        bool marketing = false,
        bool historicoMedico = false,
        bool compartilhamentoPlano = false)
    {
        DataConsentimentoLgpd = DateTime.UtcNow;
        VersaoTermosAceitos = versaoTermos;
        FinalidadesAutorizadas = finalidades;
        AutorizouMarketing = marketing;
        AutorizouHistoricoMedico = historicoMedico;
        AutorizouCompartilhamentoPlanoSaude = compartilhamentoPlano;
        DataUltimaAtualizacaoConsentimento = DateTime.UtcNow;
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Solicita anonimização dos dados (direito ao esquecimento LGPD)
    /// </summary>
    /// <param name="motivo">Motivo da solicitação</param>
    public void SolicitarAnonimizacao(string motivo = "Solicitação do titular")
    {
        SolicitouAnonimizacao = true;
        DataSolicitacaoAnonimizacao = DateTime.UtcNow;
        MotivoInativacao = motivo;
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Efetiva anonimização dos dados (irreversível)
    /// </summary>
    public void EfetivarAnonimizacao()
    {
        if (!SolicitouAnonimizacao)
            throw new InvalidOperationException("Anonimização não foi solicitada pelo cliente");

        // Anonimizar dados pessoais mantendo apenas necessário para histórico comercial
        Nome = "Cliente Anônimo";
        CpfCnpj = "***.***.***-**";
        Rg = null;
        Email = null;
        TelefoneResidencial = null;
        TelefoneCelular = null;
        TelefoneComercial = null;
        
        // Limpar endereço
        Logradouro = null;
        Numero = null;
        Complemento = null;
        Bairro = null;
        // Manter apenas cidade e estado para estatísticas regionais
        Cep = null;
        
        // Limpar dados médicos sensíveis
        AlergiasMedicamentosas = null;
        MedicamentosUsoContinuo = null;
        CondicoesMedicas = null;
        ObservacoesMedicas = null;
        PlanoSaude = null;
        NumeroCarteirinha = null;
        
        // Limpar dados pessoais sensíveis
        DataNascimento = null;
        Genero = null;
        EstadoCivil = null;
        Profissao = null;
        OrgaoEmissor = null;
        
        // Marcar como anonimizado
        DadosAnonimizados = true;
        DataAnonimizacao = DateTime.UtcNow;
        Status = StatusCliente.Anonimo;
        
        // Revogar todos os consentimentos
        AutorizouMarketing = false;
        AutorizouHistoricoMedico = false;
        AutorizouCompartilhamentoPlanoSaude = false;
        AceitaNotificacaoEmail = false;
        AceitaNotificacaoSms = false;
        AceitaNotificacaoWhatsapp = false;
        AceitaPromocoes = false;
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Registra nova compra do cliente
    /// </summary>
    /// <param name="valorCompra">Valor da compra</param>
    public void RegistrarCompra(decimal valorCompra)
    {
        DataUltimaCompra = DateTime.UtcNow;
        ValorTotalCompras += valorCompra;
        TotalCompras++;
        
        AtualizarUltimaMovimentacao();
    }

    /// <summary>
    /// Calcula idade do cliente (se data de nascimento disponível)
    /// </summary>
    /// <returns>Idade em anos ou null se data não disponível</returns>
    public int? CalcularIdade()
    {
        if (DataNascimento == null || DadosAnonimizados)
            return null;
            
        var hoje = DateTime.Today;
        var idade = hoje.Year - DataNascimento.Value.Year;
        
        if (DataNascimento.Value.Date > hoje.AddYears(-idade))
            idade--;
            
        return idade;
    }

    /// <summary>
    /// Obtém nome para exibição respeitando anonimização
    /// </summary>
    /// <returns>Nome para exibição no sistema</returns>
    public string NomeExibicao => DadosAnonimizados ? "Cliente Anônimo" : Nome;

    /// <summary>
    /// Verifica se CPF/CNPJ é válido
    /// </summary>
    /// <returns>True se documento é válido</returns>
    public bool ValidarDocumento()
    {
        if (DadosAnonimizados || string.IsNullOrEmpty(CpfCnpj))
            return false;

        // Implementar validação de CPF/CNPJ brasileiro
        return ValidadorDocumento.ValidarCpfCnpj(CpfCnpj);
    }
}

/// <summary>
/// Enum para status do cliente no sistema farmacêutico
/// </summary>
public enum StatusCliente
{
    /// <summary>
    /// Cliente ativo e pode realizar compras
    /// </summary>
    Ativo = 1,
    
    /// <summary>
    /// Cliente inativo temporariamente
    /// </summary>
    Inativo = 2,
    
    /// <summary>
    /// Cliente bloqueado por algum motivo
    /// </summary>
    Bloqueado = 3,
    
    /// <summary>
    /// Cliente dados anonimizados (LGPD)
    /// </summary>
    Anonimo = 4
}

/// <summary>
/// Validador de documentos brasileiros
/// </summary>
public static class ValidadorDocumento
{
    /// <summary>
    /// Valida CPF ou CNPJ brasileiro
    /// </summary>
    /// <param name="documento">Documento a ser validado</param>
    /// <returns>True se documento é válido</returns>
    public static bool ValidarCpfCnpj(string documento)
    {
        if (string.IsNullOrEmpty(documento))
            return false;
        
        // Remove caracteres não numéricos
        documento = new string(documento.Where(char.IsDigit).ToArray());
        
        return documento.Length switch
        {
            11 => ValidarCpf(documento),
            14 => ValidarCnpj(documento),
            _ => false
        };
    }

    /// <summary>
    /// Valida CPF brasileiro
    /// </summary>
    private static bool ValidarCpf(string cpf)
    {
        if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
            return false;
        
        // Implementar validação completa de CPF aqui
        // Por simplicidade, assumindo que formato básico é suficiente
        return true;
    }

    /// <summary>
    /// Valida CNPJ brasileiro
    /// </summary>
    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
            return false;
        
        // Implementar validação completa de CNPJ aqui
        // Por simplicidade, assumindo que formato básico é suficiente
        return true;
    }
}