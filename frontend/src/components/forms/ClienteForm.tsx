/**
 * Formulário completo para Cliente Brasileiro
 * Inclui todos os campos, validações, máscaras e integração ViaCEP
 */
import React, { useState, useEffect, useCallback } from 'react';
import {
  TextInput,
  Select,
  Textarea,
  Button,
  Group,
  Stack,
  Grid,
  Card,
  Title,
  Text,
  Switch,
  NumberInput,
  Alert,
  Loader,
  ActionIcon,
  Tooltip,
  Badge,
  Divider
} from '@mantine/core';
import { useForm } from '@mantine/form';
import { DateInput } from '@mantine/dates';
import { notifications } from '@mantine/notifications';
import { IconCheck, IconX, IconRefresh, IconInfoCircle } from '@tabler/icons-react';
import { useCepAutoComplete } from '../../hooks/useCepLookup';
import { clienteService, formatarCpf, formatarTelefone, limparCpf, obterIpCliente } from '../../services/clienteService';
import {
  Cliente,
  CriarClienteRequest,
  AtualizarClienteRequest,
  Endereco,
  CpfValidationResult,
  TelefoneValidationResult
} from '../../types/cliente';

interface ClienteFormProps {
  cliente?: Cliente;
  onSave: (data: CriarClienteRequest | AtualizarClienteRequest) => Promise<void>;
  onCancel: () => void;
  loading?: boolean;
  mode?: 'create' | 'edit' | 'view';
}

/**
 * Componente para formulário de endereço com ViaCEP
 */
const EnderecoForm: React.FC<{
  value?: Endereco;
  onChange: (endereco: Endereco | null) => void;
  readonly?: boolean;
}> = ({ value, onChange, readonly = false }) => {
  const { handleCepChange, endereco, loading, error, isCepValido } = useCepAutoComplete(onChange);

  useEffect(() => {
    if (value?.cep) {
      handleCepChange(value.cep);
    }
  }, [value?.cep, handleCepChange]);

  const enderecoAtual = endereco || value || {};

  return (
    <Card withBorder p="md">
      <Title order={4} mb="md">📍 Endereço</Title>
      
      <Grid>
        <Grid.Col span={6}>
          <TextInput
            label="CEP"
            placeholder="00000-000"
            value={enderecoAtual.cep || ''}
            onChange={(e) => !readonly && handleCepChange(e.target.value)}
            maxLength={9}
            error={error}
            rightSection={loading ? <Loader size="xs" /> : isCepValido ? <IconCheck color="green" /> : null}
            readOnly={readonly}
          />
        </Grid.Col>
        
        <Grid.Col span={3}>
          <TextInput
            label="UF"
            placeholder="SP"
            value={enderecoAtual.uf || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, uf: e.target.value })}
            maxLength={2}
            style={{ textTransform: 'uppercase' }}
            readOnly={readonly}
          />
        </Grid.Col>
        
        <Grid.Col span={3}>
          <TextInput
            label="Código IBGE"
            value={enderecoAtual.codigoIBGE || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, codigoIBGE: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>

        <Grid.Col span={8}>
          <TextInput
            label="Cidade"
            placeholder="São Paulo"
            value={enderecoAtual.cidade || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, cidade: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>
        
        <Grid.Col span={4}>
          <TextInput
            label="Bairro"
            placeholder="Centro"
            value={enderecoAtual.bairro || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, bairro: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>

        <Grid.Col span={8}>
          <TextInput
            label="Logradouro"
            placeholder="Rua das Flores"
            value={enderecoAtual.logradouro || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, logradouro: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>
        
        <Grid.Col span={4}>
          <TextInput
            label="Número"
            placeholder="123"
            value={enderecoAtual.numero || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, numero: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>

        <Grid.Col span={6}>
          <TextInput
            label="Complemento"
            placeholder="Apto 45, Bloco B"
            value={enderecoAtual.complemento || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, complemento: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>
        
        <Grid.Col span={6}>
          <TextInput
            label="Ponto de Referência"
            placeholder="Próximo ao shopping"
            value={enderecoAtual.pontoReferencia || ''}
            onChange={(e) => !readonly && onChange?.({ ...enderecoAtual, pontoReferencia: e.target.value })}
            readOnly={readonly}
          />
        </Grid.Col>
      </Grid>
    </Card>
  );
};

/**
 * Hook para validação de CPF em tempo real
 */
const useCpfValidation = () => {
  const [validation, setValidation] = useState<CpfValidationResult | null>(null);
  const [loading, setLoading] = useState(false);

  const validateCpf = useCallback(async (cpf: string) => {
    if (!cpf || limparCpf(cpf).length !== 11) {
      setValidation(null);
      return;
    }

    setLoading(true);
    try {
      const result = await clienteService.validarCpf(cpf);
      setValidation(result);
    } catch (error) {
      setValidation(null);
    } finally {
      setLoading(false);
    }
  }, []);

  return { validation, loading, validateCpf };
};

/**
 * Hook para validação de telefone em tempo real
 */
const useTelefoneValidation = () => {
  const [validation, setValidation] = useState<TelefoneValidationResult | null>(null);
  const [loading, setLoading] = useState(false);

  const validateTelefone = useCallback(async (telefone: string) => {
    if (!telefone || telefone.length < 10) {
      setValidation(null);
      return;
    }

    setLoading(true);
    try {
      const result = await clienteService.validarTelefone(telefone);
      setValidation(result);
    } catch (error) {
      setValidation(null);
    } finally {
      setLoading(false);
    }
  }, []);

  return { validation, loading, validateTelefone };
};

export const ClienteForm: React.FC<ClienteFormProps> = ({
  cliente,
  onSave,
  onCancel,
  loading = false,
  mode = 'create'
}) => {
  const isReadOnly = mode === 'view';
  const isEdit = mode === 'edit';
  
  // Validações em tempo real
  const { validation: cpfValidation, loading: validatingCpf, validateCpf } = useCpfValidation();
  const { validation: telefoneValidation, loading: validatingTelefone, validateTelefone } = useTelefoneValidation();

  // Form state
  const form = useForm<CriarClienteRequest>({
    initialValues: {
      // Dados pessoais
      nome: cliente?.nome || '',
      nomeCompleto: cliente?.nomeCompleto || '',
      nomeMae: cliente?.nomeMae || '',
      nomePai: cliente?.nomePai || '',
      dataNascimento: cliente?.dataNascimento || '',
      genero: cliente?.genero || undefined,
      estadoCivil: cliente?.estadoCivil || '',
      profissao: cliente?.profissao || '',
      nacionalidade: cliente?.nacionalidade || 'Brasileira',

      // Documentos
      cpf: cliente?.cpf || '',
      rg: cliente?.rg || '',
      rgOrgaoExpedidor: cliente?.rgOrgaoExpedidor || '',
      rgUFExpedicao: cliente?.rgUFExpedicao || '',
      rgDataExpedicao: cliente?.rgDataExpedicao || '',

      // Contato
      email: cliente?.email || '',
      telefoneCelular: cliente?.telefoneCelular || '',
      telefoneFixo: cliente?.telefoneFixo || '',
      whatsApp: cliente?.whatsApp || '',

      // Endereço
      endereco: cliente?.endereco || undefined,

      // Comercial
      categoriaCliente: cliente?.categoriaCliente || 'Regular',
      limiteCredito: cliente?.limiteCredito || 0,
      descontoPadrao: cliente?.descontoPadrao || 0,

      // Observações
      observacoes: cliente?.observacoes || '',
      preferenciaContato: cliente?.preferenciaContato || '',
      restricoesDietarias: cliente?.restricoesDietarias || '',
      alergias: cliente?.alergias || '',

      // LGPD
      consentimentoColeta: cliente?.consentimentoColeta || false,
      consentimentoMarketing: cliente?.consentimentoMarketing || false,
      consentimentoCompartilhamento: cliente?.consentimentoCompartilhamento || false,
      finalidadeColeta: cliente?.finalidadeColeta || 'Gestão de relacionamento com clientes',
    },

    validate: {
      nome: (value) => (!value ? 'Nome é obrigatório' : null),
      cpf: (value) => {
        if (!value) return null;
        if (cpfValidation && !cpfValidation.isValid) {
          return cpfValidation.message;
        }
        return null;
      },
      email: (value) => {
        if (!value) return null;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return emailRegex.test(value) ? null : 'Email inválido';
      },
      telefoneCelular: (value) => {
        if (!value) return null;
        if (telefoneValidation && !telefoneValidation.isValid) {
          return telefoneValidation.message;
        }
        return null;
      },
      consentimentoColeta: (value) => (!value ? 'Consentimento LGPD é obrigatório' : null),
    },
  });

  // Handlers
  const handleCpfChange = useCallback((value: string) => {
    const formatted = formatarCpf(value);
    form.setFieldValue('cpf', formatted);
    validateCpf(value);
  }, [form, validateCpf]);

  const handleTelefoneChange = useCallback((field: string, value: string) => {
    const formatted = formatarTelefone(value);
    form.setFieldValue(field as any, formatted);
    if (field === 'telefoneCelular') {
      validateTelefone(value);
    }
  }, [form, validateTelefone]);

  const handleSubmit = useCallback(async (values: CriarClienteRequest) => {
    try {
      // Obter IP do cliente para LGPD
      const ip = await obterIpCliente();
      
      const dataToSave = {
        ...values,
        ipConsentimento: ip,
      };

      await onSave(dataToSave);
      
      notifications.show({
        title: 'Sucesso!',
        message: `Cliente ${isEdit ? 'atualizado' : 'criado'} com sucesso`,
        color: 'green',
        icon: <IconCheck />,
      });
    } catch (error: any) {
      notifications.show({
        title: 'Erro!',
        message: error.message || `Erro ao ${isEdit ? 'atualizar' : 'criar'} cliente`,
        color: 'red',
        icon: <IconX />,
      });
    }
  }, [onSave, isEdit]);

  return (
    <div className="padaria-theme">
      <form onSubmit={form.onSubmit(handleSubmit)} className="padaria-form-compact">
        <Stack gap="sm">
        {/* LGPD Alert */}
        {!isReadOnly && (
          <Alert
            icon={<IconInfoCircle />}
            title="Consentimento LGPD"
            color="blue"
          >
            Este formulário coleta dados pessoais conforme a Lei Geral de Proteção de Dados (LGPD).
            O consentimento do cliente é obrigatório.
          </Alert>
        )}

        {/* Dados Pessoais Compactos */}
        <div className="padaria-card-compact">
          <h4 className="padaria-section-title">👤 Dados Pessoais</h4>
          
          <div className="padaria-grid-4">
            <TextInput
              label="Nome Completo"
              placeholder="João da Silva"
              required
              {...form.getInputProps('nome')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <Select
              label="Gênero"
              placeholder="Selecione"
              data={[
                { value: 'M', label: 'Masculino' },
                { value: 'F', label: 'Feminino' },
                { value: 'O', label: 'Outro' },
                { value: 'N', label: 'Não informar' },
              ]}
              {...form.getInputProps('genero')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />

            <TextInput
              label="Nome da Mãe"
              placeholder="Maria da Silva"
              {...form.getInputProps('nomeMae')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="Nome do Pai"
              placeholder="José da Silva"
              {...form.getInputProps('nomePai')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />

          </div>
          
          <div className="padaria-grid-4" style={{ marginTop: '8px' }}>
            <DateInput
              label="Data de Nascimento"
              placeholder="dd/mm/aaaa"
              valueFormat="DD/MM/YYYY"
              value={form.values.dataNascimento ? new Date(form.values.dataNascimento) : null}
              onChange={(date) => form.setFieldValue('dataNascimento', date?.toISOString() || '')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <Select
              label="Estado Civil"
              placeholder="Selecione"
              data={[
                { value: 'Solteiro', label: 'Solteiro(a)' },
                { value: 'Casado', label: 'Casado(a)' },
                { value: 'Divorciado', label: 'Divorciado(a)' },
                { value: 'Viúvo', label: 'Viúvo(a)' },
                { value: 'União Estável', label: 'União Estável' },
              ]}
              {...form.getInputProps('estadoCivil')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="Profissão"
              placeholder="Desenvolvedor"
              {...form.getInputProps('profissao')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="CPF"
              placeholder="000.000.000-00"
              value={form.values.cpf}
              onChange={(e) => handleCpfChange(e.target.value)}
              maxLength={14}
              error={form.errors.cpf}
              rightSection={
                validatingCpf ? (
                  <Loader size="xs" />
                ) : cpfValidation?.isValid ? (
                  <Tooltip label="CPF válido">
                    <IconCheck color="green" />
                  </Tooltip>
                ) : cpfValidation?.isValid === false ? (
                  <Tooltip label={cpfValidation.message}>
                    <IconX color="red" />
                  </Tooltip>
                ) : null
              }
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
          </div>
        </div>

        {/* Documentos + Contato Compactos */}
        <div className="padaria-card-compact">
          <h4 className="padaria-section-title">📄 Documentos & 📞 Contato</h4>
          
          <div className="padaria-grid-4">
            <TextInput
              label="RG"
              placeholder="12.345.678-9"
              {...form.getInputProps('rg')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="Órgão Expedidor"
              placeholder="SSP"
              {...form.getInputProps('rgOrgaoExpedidor')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="UF Expedição"
              placeholder="SP"
              maxLength={2}
              style={{ textTransform: 'uppercase' }}
              {...form.getInputProps('rgUFExpedicao')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <DateInput
              label="Data Expedição"
              placeholder="dd/mm/aaaa"
              valueFormat="DD/MM/YYYY"
              value={form.values.rgDataExpedicao ? new Date(form.values.rgDataExpedicao) : null}
              onChange={(date) => form.setFieldValue('rgDataExpedicao', date?.toISOString() || '')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
          </div>
          
          <div className="padaria-grid-4" style={{ marginTop: '8px' }}>
            <TextInput
              label="Email"
              placeholder="joao@email.com"
              type="email"
              {...form.getInputProps('email')}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="Telefone Celular"
              placeholder="(11) 99999-9999"
              value={form.values.telefoneCelular}
              onChange={(e) => handleTelefoneChange('telefoneCelular', e.target.value)}
              maxLength={15}
              error={form.errors.telefoneCelular}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="Telefone Fixo"
              placeholder="(11) 3333-3333"
              value={form.values.telefoneFixo}
              onChange={(e) => handleTelefoneChange('telefoneFixo', e.target.value)}
              maxLength={14}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
            
            <TextInput
              label="WhatsApp"
              placeholder="(11) 99999-9999"
              value={form.values.whatsApp}
              onChange={(e) => handleTelefoneChange('whatsApp', e.target.value)}
              maxLength={15}
              readOnly={isReadOnly}
              className="padaria-input-compact"
              styles={{ label: { fontSize: '13px', fontWeight: 700, color: 'var(--padaria-accent)' } }}
            />
          </div>
        </div>


        {/* Endereço */}
        <EnderecoForm
          value={form.values.endereco}
          onChange={(endereco) => form.setFieldValue('endereco', endereco || undefined)}
          readonly={isReadOnly}
        />

        {/* Dados Comerciais */}
        <Card withBorder p="md">
          <Title order={4} mb="md">💼 Dados Comerciais</Title>
          
          <Grid>
            <Grid.Col span={4}>
              <Select
                label="Categoria"
                placeholder="Selecione"
                data={[
                  { value: 'Bronze', label: 'Bronze' },
                  { value: 'Regular', label: 'Regular' },
                  { value: 'Premium', label: 'Premium' },
                  { value: 'VIP', label: 'VIP' },
                ]}
                {...form.getInputProps('categoriaCliente')}
                readOnly={isReadOnly}
              />
            </Grid.Col>
            
            <Grid.Col span={4}>
              <NumberInput
                label="Limite de Crédito"
                placeholder="0,00"
                min={0}
                decimalScale={2}
                prefix="R$ "
                thousandSeparator="."
                decimalSeparator=","
                {...form.getInputProps('limiteCredito')}
                readOnly={isReadOnly}
              />
            </Grid.Col>
            
            <Grid.Col span={4}>
              <NumberInput
                label="Desconto Padrão (%)"
                placeholder="0,00"
                min={0}
                max={100}
                decimalScale={2}
                suffix=" %"
                {...form.getInputProps('descontoPadrao')}
                readOnly={isReadOnly}
              />
            </Grid.Col>
          </Grid>
        </Card>

        {/* Observações */}
        <Card withBorder p="md">
          <Title order={4} mb="md">📝 Observações</Title>
          
          <Stack gap="md">
            <Textarea
              label="Observações Gerais"
              placeholder="Informações adicionais sobre o cliente..."
              minRows={3}
              maxRows={5}
              {...form.getInputProps('observacoes')}
              readOnly={isReadOnly}
            />
            
            <Grid>
              <Grid.Col span={4}>
                <Textarea
                  label="Preferências de Contato"
                  placeholder="Email, WhatsApp, etc."
                  minRows={2}
                  {...form.getInputProps('preferenciaContato')}
                  readOnly={isReadOnly}
                />
              </Grid.Col>
              
              <Grid.Col span={4}>
                <Textarea
                  label="Restrições Dietárias"
                  placeholder="Vegetariano, celíaco, etc."
                  minRows={2}
                  {...form.getInputProps('restricoesDietarias')}
                  readOnly={isReadOnly}
                />
              </Grid.Col>
              
              <Grid.Col span={4}>
                <Textarea
                  label="Alergias"
                  placeholder="Lactose, amendoim, etc."
                  minRows={2}
                  {...form.getInputProps('alergias')}
                  readOnly={isReadOnly}
                />
              </Grid.Col>
            </Grid>
          </Stack>
        </Card>

        {/* LGPD */}
        {!isReadOnly && (
          <Card withBorder p="md">
            <Title order={4} mb="md">🔒 Consentimento LGPD</Title>
            
            <Stack gap="md">
              <TextInput
                label="Finalidade da Coleta"
                placeholder="Gestão de relacionamento com clientes"
                {...form.getInputProps('finalidadeColeta')}
                required
              />
              
              <Stack gap="xs">
                <Switch
                  label="Consentimento para coleta e processamento de dados pessoais"
                  description="Obrigatório conforme LGPD Art. 7º, I"
                  {...form.getInputProps('consentimentoColeta', { type: 'checkbox' })}
                  color="green"
                  size="md"
                />
                
                <Switch
                  label="Consentimento para marketing e comunicações promocionais"
                  description="Opcional - pode ser alterado a qualquer momento"
                  {...form.getInputProps('consentimentoMarketing', { type: 'checkbox' })}
                  color="blue"
                  size="md"
                />
                
                <Switch
                  label="Consentimento para compartilhamento com parceiros"
                  description="Opcional - apenas com empresas do grupo"
                  {...form.getInputProps('consentimentoCompartilhamento', { type: 'checkbox' })}
                  color="orange"
                  size="md"
                />
              </Stack>
              
              <Text size="sm" color="dimmed">
                ⚖️ O cliente pode revogar estes consentimentos a qualquer momento através do menu "Meus Dados".
              </Text>
            </Stack>
          </Card>
        )}

        {/* Actions */}
        {!isReadOnly && (
          <>
            <Divider />
            <Group justify="flex-end" mt="md">
              <Button
                variant="subtle"
                onClick={onCancel}
                disabled={loading}
              >
                Cancelar
              </Button>
              
              <Button
                type="submit"
                loading={loading}
                color="green"
              >
                {isEdit ? 'Atualizar Cliente' : 'Criar Cliente'}
              </Button>
            </Group>
          </>
        )}

        {/* Info for view mode */}
        {isReadOnly && cliente && (
          <Card withBorder p="md" bg="gray.0">
            <Group justify="space-between">
              <div>
                <Text size="sm" color="dimmed">Cliente criado em</Text>
                <Text fw={500}>{new Date(cliente.dataCadastro).toLocaleDateString('pt-BR')}</Text>
              </div>
              <div>
                <Text size="sm" color="dimmed">Última atualização</Text>
                <Text fw={500}>{new Date(cliente.dataUltimaAtualizacao).toLocaleDateString('pt-BR')}</Text>
              </div>
              <div>
                <Text size="sm" color="dimmed">Status</Text>
                <Badge color={cliente.ativo ? 'green' : 'red'}>
                  {cliente.ativo ? 'Ativo' : 'Inativo'}
                </Badge>
              </div>
            </Group>
          </Card>
        )}
        </Stack>
      </form>
    </div>
  );
};