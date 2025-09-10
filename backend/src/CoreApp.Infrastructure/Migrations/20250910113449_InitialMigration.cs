using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassificacoesAnvisa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoReceita = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CorReceita = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    RequerRetencaoReceita = table.Column<bool>(type: "boolean", nullable: false),
                    DiasValidadeReceita = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMaximaPorReceita = table.Column<int>(type: "integer", nullable: true),
                    RequerAutorizacaoEspecial = table.Column<bool>(type: "boolean", nullable: false),
                    ReportarSNGPC = table.Column<bool>(type: "boolean", nullable: false),
                    Categoria = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CorDestaque = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    NivelPermissaoMinimo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RegrasValidacao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    IsOficialAnvisa = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguracaoAnvisaOficialId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassificacoesAnvisa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassificacoesAnvisa_ClassificacoesAnvisa_ConfiguracaoAnvis~",
                        column: x => x.ConfiguracaoAnvisaOficialId,
                        principalTable: "ClassificacoesAnvisa",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CPF = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FormasPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    TipoPagamento = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    PermiteParcelamento = table.Column<bool>(type: "boolean", nullable: false),
                    MaximoParcelas = table.Column<int>(type: "integer", nullable: true),
                    ValorMinimoParcelamento = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxaJurosMensal = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxaFixa = table.Column<decimal>(type: "numeric", nullable: true),
                    TaxaPercentual = table.Column<decimal>(type: "numeric", nullable: true),
                    PermiteDesconto = table.Column<bool>(type: "boolean", nullable: false),
                    PercentualDesconto = table.Column<decimal>(type: "numeric", nullable: true),
                    PermiteTroco = table.Column<bool>(type: "boolean", nullable: false),
                    RequerComprovante = table.Column<bool>(type: "boolean", nullable: false),
                    RequerAutenticacao = table.Column<bool>(type: "boolean", nullable: false),
                    GatewayPagamento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ConfiguracaoGateway = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrazoCompensacao = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DisponivelPDV = table.Column<bool>(type: "boolean", nullable: false),
                    DisponivelOnline = table.Column<bool>(type: "boolean", nullable: false),
                    HorarioInicio = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    HorarioFim = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    ValorMinimo = table.Column<decimal>(type: "numeric", nullable: true),
                    ValorMaximo = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSistema = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguracaoGlobalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormasPagamento_FormasPagamento_ConfiguracaoGlobalId",
                        column: x => x.ConfiguracaoGlobalId,
                        principalTable: "FormasPagamento",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Fornecedores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fornecedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MovimentacoesEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimentacoesEstoque", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanosComerciais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosComerciais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanosModulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanosModulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Promocoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Promocoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromocoesCategorias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocoesCategorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromocoesProdutos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromocoesProdutos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RelatoriosArquivamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelatoriosArquivamento", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    PercentualMinimo = table.Column<decimal>(type: "numeric", nullable: true),
                    PercentualMaximo = table.Column<decimal>(type: "numeric", nullable: true),
                    GerarAlerta = table.Column<bool>(type: "boolean", nullable: false),
                    PrioridadeAlerta = table.Column<int>(type: "integer", nullable: true),
                    NotificarEmail = table.Column<bool>(type: "boolean", nullable: false),
                    NotificarWhatsApp = table.Column<bool>(type: "boolean", nullable: false),
                    BloquearVendas = table.Column<bool>(type: "boolean", nullable: false),
                    MensagemAlerta = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AcaoRecomendada = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    IsSistema = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguracaoGlobalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusEstoque", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusEstoque_StatusEstoque_ConfiguracaoGlobalId",
                        column: x => x.ConfiguracaoGlobalId,
                        principalTable: "StatusEstoque",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusPagamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    IsStatusFinal = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteCancelamento = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteEstorno = table.Column<bool>(type: "boolean", nullable: false),
                    GerarComprovante = table.Column<bool>(type: "boolean", nullable: false),
                    EnviarNotificacao = table.Column<bool>(type: "boolean", nullable: false),
                    LiberarProdutos = table.Column<bool>(type: "boolean", nullable: false),
                    AtualizarEstoque = table.Column<bool>(type: "boolean", nullable: false),
                    TempoLimiteMinutos = table.Column<int>(type: "integer", nullable: true),
                    StatusAposTimeout = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MensagemPadrao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProximosStatusValidos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AparecerRelatorios = table.Column<bool>(type: "boolean", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    IsSistema = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguracaoGlobalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusPagamento_StatusPagamento_ConfiguracaoGlobalId",
                        column: x => x.ConfiguracaoGlobalId,
                        principalTable: "StatusPagamento",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StatusSincronizacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Categoria = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Cor = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: true),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    IsStatusFinal = table.Column<bool>(type: "boolean", nullable: false),
                    PermiteRetry = table.Column<bool>(type: "boolean", nullable: false),
                    MaximoTentativas = table.Column<int>(type: "integer", nullable: true),
                    IntervaloTentativasMinutos = table.Column<int>(type: "integer", nullable: true),
                    GerarAlerta = table.Column<bool>(type: "boolean", nullable: false),
                    PrioridadeAlerta = table.Column<int>(type: "integer", nullable: true),
                    NotificarAdministradores = table.Column<bool>(type: "boolean", nullable: false),
                    BloquearOperacoesOffline = table.Column<bool>(type: "boolean", nullable: false),
                    AparecerDashboard = table.Column<bool>(type: "boolean", nullable: false),
                    TempoLimiteMinutos = table.Column<int>(type: "integer", nullable: true),
                    StatusAposTimeout = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MensagemPadrao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProximosStatusValidos = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SincronizacaoImediata = table.Column<bool>(type: "boolean", nullable: false),
                    TiposDadosAplicaveis = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    IsSistema = table.Column<bool>(type: "boolean", nullable: false),
                    ConfiguracaoGlobalId = table.Column<Guid>(type: "uuid", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CriadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusSincronizacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatusSincronizacao_StatusSincronizacao_ConfiguracaoGlobalId",
                        column: x => x.ConfiguracaoGlobalId,
                        principalTable: "StatusSincronizacao",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantsModulos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantsModulos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TenantsPlanos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantsPlanos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposMovimentacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposMovimentacao", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Produtos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CodigoBarras = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CodigoInterno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PrecoVenda = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    PrecoCusto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    MargemLucro = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    UnidadeMedida = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "UN"),
                    EstoqueAtual = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    EstoqueMinimo = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    CategoriaId = table.Column<Guid>(type: "uuid", nullable: true),
                    NCM = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CEST = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Origem = table.Column<int>(type: "integer", nullable: true),
                    CST = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    AliquotaICMS = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    VerticalType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "GENERICO"),
                    VerticalProperties = table.Column<string>(type: "jsonb", nullable: true),
                    VerticalConfiguration = table.Column<string>(type: "jsonb", nullable: true),
                    VerticalSchemaVersion = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "1.0"),
                    VerticalActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produtos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Produtos_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Vendas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumeroVenda = table.Column<long>(type: "bigint", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: true),
                    UsuarioVendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataVenda = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ValorProdutos = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    ValorImpostos = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorPago = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    ValorTroco = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "PENDENTE"),
                    TipoVenda = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BALCAO"),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    NumeroNFCe = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ChaveNFCe = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataCancelamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoCancelamento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Excluido = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DataExclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioExclusao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MotivoExclusao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Arquivado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DataArquivamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UltimaMovimentacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Vendas_Usuarios_UsuarioVendaId",
                        column: x => x.UsuarioVendaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FormasPagamentoVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendaId1 = table.Column<Guid>(type: "uuid", nullable: true),
                    FormaPagamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NumeroParcelas = table.Column<int>(type: "integer", nullable: true),
                    DadosAdicionais = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TransacaoExternaId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DataProcessamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormasPagamentoVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormasPagamentoVenda_FormasPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormasPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormasPagamentoVenda_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormasPagamentoVenda_Vendas_VendaId1",
                        column: x => x.VendaId1,
                        principalTable: "Vendas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ItensVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenantId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    VendaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProdutoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<decimal>(type: "numeric(18,3)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ValorDesconto = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    PercentualDesconto = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m),
                    NomeProduto = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CodigoProduto = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnidadeMedida = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "UN"),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioCriacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UsuarioAtualizacao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItensVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Produtos_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produtos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItensVenda_Vendas_VendaId",
                        column: x => x.VendaId,
                        principalTable: "Vendas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassificacoesAnvisa_ConfiguracaoAnvisaOficialId",
                table: "ClassificacoesAnvisa",
                column: "ConfiguracaoAnvisaOficialId");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagamento_ConfiguracaoGlobalId",
                table: "FormasPagamento",
                column: "ConfiguracaoGlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagamentoVenda_FormaPagamentoId",
                table: "FormasPagamentoVenda",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagamentoVenda_VendaId",
                table: "FormasPagamentoVenda",
                column: "VendaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormasPagamentoVenda_VendaId1",
                table: "FormasPagamentoVenda",
                column: "VendaId1");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_ProdutoId",
                table: "ItensVenda",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_TenantId_ProdutoId",
                table: "ItensVenda",
                columns: new[] { "TenantId", "ProdutoId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_TenantId_VendaId",
                table: "ItensVenda",
                columns: new[] { "TenantId", "VendaId" });

            migrationBuilder.CreateIndex(
                name: "IX_ItensVenda_VendaId_ProdutoId",
                table: "ItensVenda",
                columns: new[] { "VendaId", "ProdutoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_CategoriaId",
                table: "Produtos",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_Ativo",
                table: "Produtos",
                columns: new[] { "TenantId", "Ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_CodigoBarras",
                table: "Produtos",
                columns: new[] { "TenantId", "CodigoBarras" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_CodigoInterno",
                table: "Produtos",
                columns: new[] { "TenantId", "CodigoInterno" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_Excluido",
                table: "Produtos",
                columns: new[] { "TenantId", "Excluido" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_Nome",
                table: "Produtos",
                columns: new[] { "TenantId", "Nome" });

            migrationBuilder.CreateIndex(
                name: "IX_Produtos_TenantId_VerticalType",
                table: "Produtos",
                columns: new[] { "TenantId", "VerticalType" });

            migrationBuilder.CreateIndex(
                name: "IX_StatusEstoque_ConfiguracaoGlobalId",
                table: "StatusEstoque",
                column: "ConfiguracaoGlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusPagamento_ConfiguracaoGlobalId",
                table: "StatusPagamento",
                column: "ConfiguracaoGlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusSincronizacao_ConfiguracaoGlobalId",
                table: "StatusSincronizacao",
                column: "ConfiguracaoGlobalId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_ClienteId",
                table: "Vendas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_ClienteId",
                table: "Vendas",
                columns: new[] { "TenantId", "ClienteId" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_DataVenda",
                table: "Vendas",
                columns: new[] { "TenantId", "DataVenda" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_Excluido",
                table: "Vendas",
                columns: new[] { "TenantId", "Excluido" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_NumeroVenda",
                table: "Vendas",
                columns: new[] { "TenantId", "NumeroVenda" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_Status",
                table: "Vendas",
                columns: new[] { "TenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_TenantId_UsuarioVendaId",
                table: "Vendas",
                columns: new[] { "TenantId", "UsuarioVendaId" });

            migrationBuilder.CreateIndex(
                name: "IX_Vendas_UsuarioVendaId",
                table: "Vendas",
                column: "UsuarioVendaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificacoesAnvisa");

            migrationBuilder.DropTable(
                name: "FormasPagamentoVenda");

            migrationBuilder.DropTable(
                name: "Fornecedores");

            migrationBuilder.DropTable(
                name: "ItensVenda");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.DropTable(
                name: "MovimentacoesEstoque");

            migrationBuilder.DropTable(
                name: "PlanosComerciais");

            migrationBuilder.DropTable(
                name: "PlanosModulos");

            migrationBuilder.DropTable(
                name: "Promocoes");

            migrationBuilder.DropTable(
                name: "PromocoesCategorias");

            migrationBuilder.DropTable(
                name: "PromocoesProdutos");

            migrationBuilder.DropTable(
                name: "RelatoriosArquivamento");

            migrationBuilder.DropTable(
                name: "StatusEstoque");

            migrationBuilder.DropTable(
                name: "StatusPagamento");

            migrationBuilder.DropTable(
                name: "StatusSincronizacao");

            migrationBuilder.DropTable(
                name: "TenantsModulos");

            migrationBuilder.DropTable(
                name: "TenantsPlanos");

            migrationBuilder.DropTable(
                name: "TiposMovimentacao");

            migrationBuilder.DropTable(
                name: "FormasPagamento");

            migrationBuilder.DropTable(
                name: "Produtos");

            migrationBuilder.DropTable(
                name: "Vendas");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
