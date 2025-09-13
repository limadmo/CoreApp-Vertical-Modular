/**
 * Serviço de Senhas - Criptografia e Validação
 * Sistema seguro com bcryptjs sem regex
 */

import bcrypt from 'bcryptjs';
import { 
  validatePassword, 
  validateLogin, 
  isPasswordExpired, 
  shouldLockAccount,
  DEFAULT_CREDENTIAL_RULES,
  PasswordRule,
  LoginRule 
} from '../config/credentialsRules';

export interface PasswordValidationResult {
  valid: boolean;
  message: string;
  shouldForceChange?: boolean;
  accountLocked?: boolean;
  lockoutMinutes?: number;
}

/**
 * Configurações do bcrypt
 */
const BCRYPT_SALT_ROUNDS = 12; // Rounds altos para maior segurança

/**
 * Gera hash da senha usando bcryptjs
 * @param password - Senha em texto plano
 * @returns Promise com hash da senha
 */
export async function hashPassword(password: string): Promise<string> {
  return bcrypt.hash(password, BCRYPT_SALT_ROUNDS);
}

/**
 * Compara senha em texto plano com hash
 * @param password - Senha em texto plano
 * @param hashedPassword - Hash armazenado no banco
 * @returns Promise com resultado da comparação
 */
export async function comparePassword(
  password: string, 
  hashedPassword: string
): Promise<boolean> {
  return bcrypt.compare(password, hashedPassword);
}

/**
 * Valida formato da senha e critérios de segurança
 * @param password - Senha a ser validada
 * @param rules - Regras de validação (opcional)
 * @returns Resultado da validação
 */
export function validatePasswordFormat(
  password: string, 
  rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
): PasswordValidationResult {
  const formatValidation = validatePassword(password, rules);
  
  return {
    valid: formatValidation.valid,
    message: formatValidation.message
  };
}

/**
 * Valida formato do login
 * @param login - Login a ser validado
 * @param rules - Regras de validação (opcional)
 * @returns Resultado da validação
 */
export function validateLoginFormat(
  login: string,
  rules: LoginRule = DEFAULT_CREDENTIAL_RULES.login
): PasswordValidationResult {
  const formatValidation = validateLogin(login, rules);
  
  return {
    valid: formatValidation.valid,
    message: formatValidation.message
  };
}

/**
 * Valida senha considerando expiração e bloqueios
 * @param password - Senha em texto plano
 * @param hashedPassword - Hash armazenado
 * @param lastPasswordChange - Data da última mudança
 * @param failedAttempts - Número de tentativas falhadas
 * @param lastFailedAttempt - Data da última tentativa falhada
 * @param forceChangeOnFirstLogin - Se deve forçar mudança no primeiro login
 * @param rules - Regras de validação
 * @returns Resultado completo da validação
 */
export async function validatePasswordComplete(
  password: string,
  hashedPassword: string,
  lastPasswordChange: Date | null,
  failedAttempts: number = 0,
  lastFailedAttempt: Date | null = null,
  forceChangeOnFirstLogin: boolean = false,
  rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
): Promise<PasswordValidationResult> {
  
  // Verificar se conta está bloqueada
  const lockStatus = shouldLockAccount(failedAttempts, lastFailedAttempt, rules);
  if (lockStatus.locked) {
    return {
      valid: false,
      message: lockStatus.message,
      accountLocked: true,
      lockoutMinutes: rules.lockoutMinutes
    };
  }

  // Verificar formato da senha
  const formatValidation = validatePasswordFormat(password, rules);
  if (!formatValidation.valid) {
    return formatValidation;
  }

  // Comparar senha com hash
  const passwordMatch = await comparePassword(password, hashedPassword);
  if (!passwordMatch) {
    return {
      valid: false,
      message: "Senha incorreta"
    };
  }

  // Verificar se deve forçar mudança no primeiro login
  if (forceChangeOnFirstLogin) {
    return {
      valid: true,
      message: "Senha válida, mas deve ser alterada",
      shouldForceChange: true
    };
  }

  // Verificar expiração da senha
  if (lastPasswordChange && isPasswordExpired(lastPasswordChange, rules)) {
    return {
      valid: true,
      message: "Senha expirada, deve ser alterada",
      shouldForceChange: true
    };
  }

  return {
    valid: true,
    message: "Senha válida"
  };
}

/**
 * Gera senha temporária seguindo as regras
 * @param rules - Regras de geração (opcional)
 * @returns Senha temporária válida
 */
export function generateTemporaryPassword(
  rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
): string {
  const letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const digits = '0123456789';

  // Gerar padrão X0000X
  const firstLetter = letters[Math.floor(Math.random() * letters.length)];
  
  let numbers = '';
  for (let i = 0; i < 4; i++) {
    numbers += digits[Math.floor(Math.random() * digits.length)];
  }
  
  const lastLetter = letters[Math.floor(Math.random() * letters.length)];
  
  return firstLetter + numbers + lastLetter;
}

/**
 * Gera login seguindo as regras
 * @param nomeCompleto - Nome completo do usuário para gerar iniciais
 * @param rules - Regras de geração (opcional)
 * @returns Login válido gerado
 */
export function generateLogin(
  nomeCompleto: string,
  rules: LoginRule = DEFAULT_CREDENTIAL_RULES.login
): string {
  const names = nomeCompleto.trim().split(' ');
  const firstName = names[0] || 'US';
  const lastName = names[names.length - 1] || names[0] || 'ER';
  
  // Pegar primeiras letras dos nomes
  let initials = (firstName[0] + lastName[0]).toUpperCase();
  
  // Se não tiver 2 letras, completar com letras aleatórias
  if (initials.length < 2) {
    const letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    initials = initials.padEnd(2, letters[Math.floor(Math.random() * letters.length)]);
  }
  
  // Gerar 5 números aleatórios
  let numbers = '';
  for (let i = 0; i < 5; i++) {
    numbers += Math.floor(Math.random() * 10);
  }
  
  // Gerar letra final aleatória
  const letters = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
  const finalLetter = letters[Math.floor(Math.random() * letters.length)];
  
  return initials + numbers + finalLetter;
}

/**
 * Valida credenciais completas (login + senha)
 * @param login - Login a ser validado
 * @param password - Senha a ser validada
 * @param loginRules - Regras de login
 * @param passwordRules - Regras de senha
 * @returns Resultado da validação completa
 */
export function validateCredentials(
  login: string,
  password: string,
  loginRules: LoginRule = DEFAULT_CREDENTIAL_RULES.login,
  passwordRules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
): { loginValid: PasswordValidationResult; passwordValid: PasswordValidationResult } {
  
  return {
    loginValid: validateLoginFormat(login, loginRules),
    passwordValid: validatePasswordFormat(password, passwordRules)
  };
}

/**
 * Cria hash inicial para novo usuário
 * @param password - Senha inicial
 * @param shouldForceChange - Se deve forçar mudança no primeiro login
 * @returns Promise com dados para criação do usuário
 */
export async function createInitialPassword(
  password: string,
  shouldForceChange: boolean = true
): Promise<{
  hashedPassword: string;
  forceChangeOnFirstLogin: boolean;
  passwordCreatedAt: Date;
}> {
  const hashedPassword = await hashPassword(password);
  
  return {
    hashedPassword,
    forceChangeOnFirstLogin: shouldForceChange,
    passwordCreatedAt: new Date()
  };
}