/**
 * Página de Vendas - Módulo Principal PADARIA
 * Implementa PDV especializado para produtos de padaria
 */
import React, { useState } from 'react';
import '../styles/verticals/padaria.css';
import {
  Stack,
  Grid,
  Card,
  Text,
  Title,
  Button,
  Badge,
  Group,
  NumberInput,
  TextInput,
  Select,
  Table,
  ActionIcon,
  Paper,
  Divider,
  Alert,
} from '@mantine/core';
import {
  IconShoppingCart,
  IconPlus,
  IconMinus,
  IconTrash,
  IconReceipt,
  IconAlertCircle,
  IconClock,
  IconPackage,
} from '@tabler/icons-react';
import { ProductSearchInput } from '@components/search';

interface ProdutoPadaria {
  id: string;
  nome: string;
  preco: number;
  categoria: 'PAES' | 'DOCES' | 'SALGADOS' | 'BOLOS' | 'BEBIDAS';
  validadeHoras: number;
  estoque: number;
  alergenos: string[];
  pesoMedio: number;
  temperaturaArmazenamento: 'AMBIENTE' | 'REFRIGERADO' | 'CONGELADO';
  codigoBarras: string;
}

interface ItemVenda {
  produto: ProdutoPadaria;
  quantidade: number;
  subtotal: number;
}

/**
 * Produtos demo especializados para PADARIA com códigos de barras
 */
const PRODUTOS_DEMO: ProdutoPadaria[] = [
  {
    id: '1',
    nome: 'PÃO FRANCÊS',
    preco: 0.85,
    categoria: 'PAES',
    validadeHoras: 24,
    estoque: 150,
    alergenos: ['GLÚTEN'],
    pesoMedio: 50,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000001011',
  },
  {
    id: '2',
    nome: 'CROISSANT DE CHOCOLATE',
    preco: 4.50,
    categoria: 'DOCES',
    validadeHoras: 48,
    estoque: 25,
    alergenos: ['GLÚTEN', 'LEITE', 'OVOS'],
    pesoMedio: 80,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000002011',
  },
  {
    id: '3',
    nome: 'COXINHA DE FRANGO',
    preco: 3.20,
    categoria: 'SALGADOS',
    validadeHoras: 12,
    estoque: 40,
    alergenos: ['GLÚTEN', 'OVOS'],
    pesoMedio: 120,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000003011',
  },
  {
    id: '4',
    nome: 'BOLO DE CHOCOLATE 1KG',
    preco: 35.00,
    categoria: 'BOLOS',
    validadeHoras: 72,
    estoque: 8,
    alergenos: ['GLÚTEN', 'LEITE', 'OVOS', 'CHOCOLATE'],
    pesoMedio: 1000,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000004011',
  },
  {
    id: '5',
    nome: 'PÃO DE AÇÚCAR',
    preco: 1.20,
    categoria: 'PAES',
    validadeHoras: 24,
    estoque: 80,
    alergenos: ['GLÚTEN'],
    pesoMedio: 60,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000005011',
  },
  {
    id: '6',
    nome: 'BRIGADEIRO',
    preco: 2.50,
    categoria: 'DOCES',
    validadeHoras: 48,
    estoque: 30,
    alergenos: ['LEITE', 'CHOCOLATE'],
    pesoMedio: 25,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000006011',
  },
  {
    id: '7',
    nome: 'PASTEL DE QUEIJO',
    preco: 4.00,
    categoria: 'SALGADOS',
    validadeHoras: 8,
    estoque: 20,
    alergenos: ['GLÚTEN', 'LEITE'],
    pesoMedio: 100,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000007011',
  },
  {
    id: '8',
    nome: 'TORTA DE MORANGO',
    preco: 28.00,
    categoria: 'BOLOS',
    validadeHoras: 48,
    estoque: 5,
    alergenos: ['GLÚTEN', 'LEITE', 'OVOS'],
    pesoMedio: 800,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000008011',
  },
  {
    id: '9',
    nome: 'SUCO DE LARANJA NATURAL',
    preco: 6.50,
    categoria: 'BEBIDAS',
    validadeHoras: 24,
    estoque: 15,
    alergenos: [],
    pesoMedio: 300,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000009011',
  },
  {
    id: '10',
    nome: 'CAFÉ EXPRESSO',
    preco: 3.50,
    categoria: 'BEBIDAS',
    validadeHoras: 2,
    estoque: 0,
    alergenos: [],
    pesoMedio: 50,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000010011',
  },
  {
    id: '11',
    nome: 'PÃO INTEGRAL',
    preco: 1.50,
    categoria: 'PAES',
    validadeHoras: 48,
    estoque: 45,
    alergenos: ['GLÚTEN'],
    pesoMedio: 70,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000011011',
  },
  {
    id: '12',
    nome: 'QUINDIM',
    preco: 3.80,
    categoria: 'DOCES',
    validadeHoras: 72,
    estoque: 12,
    alergenos: ['OVOS'],
    pesoMedio: 60,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000012011',
  },
  {
    id: '13',
    nome: 'EMPADA DE PALMITO',
    preco: 5.20,
    categoria: 'SALGADOS',
    validadeHoras: 24,
    estoque: 18,
    alergenos: ['GLÚTEN', 'LEITE'],
    pesoMedio: 90,
    temperaturaArmazenamento: 'REFRIGERADO',
    codigoBarras: '7891000013011',
  },
  {
    id: '14',
    nome: 'BOLO DE CENOURA',
    preco: 22.00,
    categoria: 'BOLOS',
    validadeHoras: 72,
    estoque: 6,
    alergenos: ['GLÚTEN', 'OVOS', 'CHOCOLATE'],
    pesoMedio: 700,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000014011',
  },
  {
    id: '15',
    nome: 'ÁGUA MINERAL 500ML',
    preco: 2.00,
    categoria: 'BEBIDAS',
    validadeHoras: 8760, // 1 ano
    estoque: 50,
    alergenos: [],
    pesoMedio: 500,
    temperaturaArmazenamento: 'AMBIENTE',
    codigoBarras: '7891000015011',
  }
];

/**
 * Página de Vendas PDV especializada para PADARIA
 */
export const VendasPage: React.FC = () => {
  const [itensVenda, setItensVenda] = useState<ItemVenda[]>([]);
  const [clienteNome, setClienteNome] = useState('');
  const [clienteTelefone, setClienteTelefone] = useState('');
  const [formaPagamento, setFormaPagamento] = useState<string>('DINHEIRO');
  const [valorRecebido, setValorRecebido] = useState<number | ''>('');
  
  /**
   * Suporte global para Ctrl+F focar na busca
   */
  React.useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if ((event.ctrlKey || event.metaKey) && event.key === 'f') {
        event.preventDefault();
        const searchInput = document.querySelector('[data-search-input] input') as HTMLInputElement;
        if (searchInput) {
          searchInput.focus();
          searchInput.select();
        }
      }
    };

    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, []);

  /**
   * Calcular total da venda
   */
  const totalVenda = itensVenda.reduce((acc, item) => acc + item.subtotal, 0);

  /**
   * Calcular troco
   */
  const troco = typeof valorRecebido === 'number' && valorRecebido > totalVenda 
    ? valorRecebido - totalVenda 
    : 0;

  /**
   * Adicionar produto à venda
   */
  const adicionarProduto = (produto: ProdutoPadaria, quantidade: number = 1) => {
    const itemExistente = itensVenda.find(item => item.produto.id === produto.id);
    
    if (itemExistente) {
      setItensVenda(itens =>
        itens.map(item =>
          item.produto.id === produto.id
            ? { ...item, quantidade: item.quantidade + quantidade, subtotal: (item.quantidade + quantidade) * produto.preco }
            : item
        )
      );
    } else {
      const novoItem: ItemVenda = {
        produto,
        quantidade,
        subtotal: produto.preco * quantidade,
      };
      setItensVenda(itens => [...itens, novoItem]);
    }
  };

  /**
   * Remover produto da venda
   */
  const removerProduto = (produtoId: string) => {
    setItensVenda(itens => itens.filter(item => item.produto.id !== produtoId));
  };

  /**
   * Atualizar quantidade do produto
   */
  const atualizarQuantidade = (produtoId: string, novaQuantidade: number) => {
    if (novaQuantidade <= 0) {
      removerProduto(produtoId);
      return;
    }

    setItensVenda(itens =>
      itens.map(item =>
        item.produto.id === produtoId
          ? { ...item, quantidade: novaQuantidade, subtotal: novaQuantidade * item.produto.preco }
          : item
      )
    );
  };

  /**
   * Finalizar venda
   */
  const finalizarVenda = () => {
    if (itensVenda.length === 0) {
      alert('Adicione produtos à venda!');
      return;
    }

    if (formaPagamento === 'DINHEIRO' && (typeof valorRecebido !== 'number' || valorRecebido < totalVenda)) {
      alert('Valor recebido insuficiente!');
      return;
    }

    // Limpar PDV
    setItensVenda([]);
    setClienteNome('');
    setClienteTelefone('');
    setValorRecebido('');
    
    alert(`Venda finalizada!\nTotal: R$ ${totalVenda.toFixed(2)}\nTroco: R$ ${troco.toFixed(2)}`);
  };

  /**
   * Obter classe CSS da categoria para PADARIA
   */
  const getClasseCategoria = (categoria: ProdutoPadaria['categoria']) => {
    switch (categoria) {
      case 'PAES': return 'badge-categoria-paes';
      case 'DOCES': return 'badge-categoria-doces';
      case 'SALGADOS': return 'badge-categoria-salgados';
      case 'BOLOS': return 'badge-categoria-bolos';
      case 'BEBIDAS': return 'badge-categoria-bebidas';
      default: return 'badge-categoria-paes';
    }
  };

  return (
    <div className="padaria-page-container padaria-theme">
      <Stack gap="lg">
      {/* Header */}
      <div>
        <Title order={1} size="h2" className="padaria-brand">
          <IconShoppingCart size={32} style={{ marginRight: 8, verticalAlign: 'middle' }} />
          PDV Padaria - Vendas (F1)
        </Title>
        <Text size="md" className="padaria-text-secondary" mt="xs" fw={500}>
          Ponto de venda especializado - Use código de barras ou nome do produto
        </Text>
      </div>

      {/* Campo de Busca Principal */}
      <Paper p="lg" withBorder style={{ backgroundColor: '#fafafa' }}>
        <Stack gap="md">
          <Group justify="space-between" align="center">
            <Text fw={700} size="xl" className="padaria-text-primary">
              🔍 Busca Rápida de Produtos
            </Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              Ctrl+F para focar | Scanner compatível
            </Text>
          </Group>
          
          <ProductSearchInput
            products={PRODUTOS_DEMO}
            onProductSelect={(produto) => adicionarProduto(produto, 1)}
            placeholder="Digite código de barras ou nome do produto..."
            autoFocus={true}
          />
          
          <Text size="sm" className="padaria-text-secondary" ta="center" fw={500}>
            💡 Use ↑/↓ para navegar pelos resultados e Enter para adicionar
          </Text>
        </Stack>
      </Paper>

      <Grid>
        {/* Coluna Esquerda - Produtos Recentes e Categorias */}
        <Grid.Col span={{ base: 12, lg: 7 }}>
          <Stack gap="md">
            {/* Produtos Recentes */}
            <Paper p="md" withBorder>
              <Group justify="space-between" mb="md">
                <Title order={3} size="h4" className="padaria-text-primary" fw={700}>
                  <IconClock size={20} style={{ marginRight: 8, verticalAlign: 'middle' }} />
                  Produtos Mais Vendidos
                </Title>
                <Badge variant="light" color="blue">
                  {PRODUTOS_DEMO.filter(p => p.estoque > 0).length} disponíveis
                </Badge>
              </Group>
              
              <Grid>
                {PRODUTOS_DEMO
                  .filter(produto => produto.estoque > 0)
                  .slice(0, 8) // Mostrar apenas os 8 primeiros
                  .map((produto) => (
                  <Grid.Col span={{ base: 12, sm: 6 }} key={produto.id}>
                    <Card 
                      padding="sm" 
                      withBorder 
                      className="hover:shadow-md transition-shadow cursor-pointer"
                      onClick={() => adicionarProduto(produto)}
                      style={{ 
                        borderLeft: produto.estoque <= 10 ? '3px solid orange' : '3px solid transparent' 
                      }}
                    >
                      <Group justify="space-between" mb="xs">
                        <Text fw={600} size="md" lineClamp={1} className="padaria-text-primary">
                          {produto.nome}
                        </Text>
                        <Badge 
                          className={getClasseCategoria(produto.categoria)}
                          variant="filled" 
                          size="sm"
                        >
                          {produto.categoria}
                        </Badge>
                      </Group>

                      <Group justify="space-between" align="center">
                        <Text size="lg" className="padaria-price-primary">
                          R$ {produto.preco.toFixed(2)}
                        </Text>
                        <Group gap="xs">
                          {produto.estoque <= 10 && (
                            <IconAlertCircle size={12} color="orange" />
                          )}
                          <Text size="sm" c={produto.estoque <= 10 ? "#fd7e14" : "#495057"} fw={500}>
                            Est: {produto.estoque}
                          </Text>
                        </Group>
                      </Group>

                      <Text size="sm" className="padaria-text-muted" mt="xs" fw={500}>
                        {produto.codigoBarras} • {produto.validadeHoras}h
                      </Text>
                    </Card>
                  </Grid.Col>
                ))}
              </Grid>
            </Paper>
          </Stack>
        </Grid.Col>

        {/* Coluna Direita - Carrinho e Pagamento */}
        <Grid.Col span={{ base: 12, lg: 5 }}>
          <Stack gap="md">
            {/* Carrinho */}
            <Paper p="md" withBorder>
              <Title order={3} size="h4" mb="md" className="padaria-text-primary" fw={700}>Itens da Venda</Title>
              
              {itensVenda.length === 0 ? (
                <Alert icon={<IconShoppingCart size={16} />} title="Carrinho vazio">
                  Adicione produtos para iniciar a venda
                </Alert>
              ) : (
                <Table>
                  <Table.Thead>
                    <Table.Tr>
                      <Table.Th>Produto</Table.Th>
                      <Table.Th>Qtd</Table.Th>
                      <Table.Th>Subtotal</Table.Th>
                      <Table.Th>Ações</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {itensVenda.map((item) => (
                      <Table.Tr key={item.produto.id}>
                        <Table.Td>
                          <div>
                            <Text size="md" fw={600} className="padaria-text-primary">{item.produto.nome}</Text>
                            <Text size="sm" className="padaria-price-unit">R$ {item.produto.preco.toFixed(2)}</Text>
                          </div>
                        </Table.Td>
                        <Table.Td>
                          <Group gap="xs">
                            <ActionIcon
                              size="sm"
                              variant="outline"
                              onClick={() => atualizarQuantidade(item.produto.id, item.quantidade - 1)}
                            >
                              <IconMinus size={12} />
                            </ActionIcon>
                            <Text size="sm" ta="center" style={{ minWidth: 30 }}>
                              {item.quantidade}
                            </Text>
                            <ActionIcon
                              size="sm"
                              variant="outline"
                              onClick={() => atualizarQuantidade(item.produto.id, item.quantidade + 1)}
                            >
                              <IconPlus size={12} />
                            </ActionIcon>
                          </Group>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm" className="padaria-subtotal">
                            R$ {item.subtotal.toFixed(2)}
                          </Text>
                        </Table.Td>
                        <Table.Td>
                          <ActionIcon
                            color="red"
                            variant="outline"
                            onClick={() => removerProduto(item.produto.id)}
                          >
                            <IconTrash size={16} />
                          </ActionIcon>
                        </Table.Td>
                      </Table.Tr>
                    ))}
                  </Table.Tbody>
                </Table>
              )}

              {itensVenda.length > 0 && (
                <>
                  <Divider my="md" />
                  <Group justify="space-between">
                    <Text size="lg" className="padaria-text-primary">Total:</Text>
                    <Text size="xl" className="padaria-price-primary">
                      R$ {totalVenda.toFixed(2)}
                    </Text>
                  </Group>
                </>
              )}
            </Paper>

            {/* Dados do Cliente */}
            <Paper p="md" withBorder>
              <Title order={4} size="h5" mb="md" className="padaria-text-primary" fw={700}>Cliente (Opcional)</Title>
              <Stack gap="sm">
                <TextInput
                  label="Nome"
                  placeholder="Nome do cliente"
                  value={clienteNome}
                  onChange={(e) => setClienteNome(e.target.value)}
                />
                <TextInput
                  label="Telefone"
                  placeholder="(11) 99999-9999"
                  value={clienteTelefone}
                  onChange={(e) => setClienteTelefone(e.target.value)}
                />
              </Stack>
            </Paper>

            {/* Pagamento */}
            <Paper p="md" withBorder>
              <Title order={4} size="h5" mb="md" className="padaria-text-primary" fw={700}>Pagamento</Title>
              
              <Stack gap="sm">
                <Select
                  label="Forma de Pagamento"
                  value={formaPagamento}
                  onChange={(value) => setFormaPagamento(value || 'DINHEIRO')}
                  data={[
                    { value: 'DINHEIRO', label: '💰 Dinheiro' },
                    { value: 'CARTAO_DEBITO', label: '💳 Cartão Débito' },
                    { value: 'CARTAO_CREDITO', label: '💳 Cartão Crédito' },
                    { value: 'PIX', label: '📱 PIX' },
                  ]}
                  classNames={{
                    dropdown: 'padaria-select-dropdown',
                    option: 'padaria-select-option',
                    input: 'padaria-select-input',
                    label: 'padaria-select-label',
                  }}
                  styles={{
                    dropdown: {
                      backgroundColor: '#ffffff !important',
                      border: '2px solid #374151 !important',
                      boxShadow: '0 4px 12px rgba(0, 0, 0, 0.15) !important',
                      zIndex: 9999,
                    },
                    option: {
                      color: '#1a1b1e !important',
                      fontSize: '16px !important',
                      fontWeight: '600 !important',
                      padding: '12px 16px !important',
                      backgroundColor: '#ffffff !important',
                      '&[data-selected="true"]': {
                        backgroundColor: '#e7f3ff !important',
                        color: '#1a1b1e !important',
                        fontWeight: '700 !important',
                      },
                      '&:hover': {
                        backgroundColor: '#f8f9fa !important',
                        color: '#1a1b1e !important',
                      },
                    },
                    input: {
                      fontSize: '16px !important',
                      color: '#1a1b1e !important',
                      backgroundColor: '#ffffff !important',
                    },
                    label: {
                      fontSize: '16px !important',
                      fontWeight: '600 !important',
                      color: '#1a1b1e !important',
                    },
                  }}
                />

                {formaPagamento === 'DINHEIRO' && (
                  <>
                    <NumberInput
                      label="Valor Recebido"
                      placeholder="0,00"
                      min={0}
                      decimalScale={2}
                      fixedDecimalScale
                      thousandSeparator="."
                      decimalSeparator=","
                      prefix="R$ "
                      value={valorRecebido}
                      onChange={(value) => setValorRecebido(value as number | '')}
                    />
                    
                    {typeof valorRecebido === 'number' && valorRecebido > 0 && (
                      <Group justify="space-between">
                        <Text size="md" className="padaria-text-primary">Troco:</Text>
                        <Text size="md" className={troco > 0 ? 'padaria-money-success' : 'padaria-money-error'}>
                          R$ {troco.toFixed(2)}
                        </Text>
                      </Group>
                    )}
                  </>
                )}
              </Stack>
            </Paper>

            {/* Finalizar Venda */}
            <Button
              size="lg"
              fullWidth
              onClick={finalizarVenda}
              disabled={itensVenda.length === 0}
              leftSection={<IconReceipt size={20} />}
            >
              Finalizar Venda (F9)
            </Button>
          </Stack>
        </Grid.Col>
      </Grid>

      {/* Atalhos de Teclado */}
      <Paper p="md" withBorder style={{ backgroundColor: '#f8f9fa' }}>
        <Group justify="space-between" align="center">
          <Group gap="xl">
            <Text size="md" className="padaria-text-primary" fw={600}>
              <strong>⌨️ Atalhos:</strong>
            </Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              <kbd className="padaria-kbd">Ctrl+F</kbd> Buscar
            </Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              <kbd className="padaria-kbd">↑/↓</kbd> Navegar
            </Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              <kbd className="padaria-kbd">Enter</kbd> Adicionar
            </Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              <kbd className="padaria-kbd">Esc</kbd> Cancelar
            </Text>
          </Group>
          <Group gap="sm">
            <Text size="md" className="padaria-text-secondary" fw={500}>
              Total de itens: <strong className="padaria-text-primary">{itensVenda.length}</strong>
            </Text>
            <Text size="md" className="padaria-text-muted">|</Text>
            <Text size="md" className="padaria-text-secondary" fw={500}>
              Produtos disponíveis: <strong className="padaria-text-primary">{PRODUTOS_DEMO.filter(p => p.estoque > 0).length}</strong>
            </Text>
          </Group>
        </Group>
      </Paper>
      </Stack>
    </div>
  );
};