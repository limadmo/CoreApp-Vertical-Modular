/**
 * Regras de Credenciais - Sistema de Validação Sem Regex
 * Configurável e auto-explicativo para fácil manutenção
 */

export interface LoginRule {
  minLength: number;
  maxLength: number;
  requiredPattern: string;
  description: string;
  active: boolean;
}

export interface PasswordRule {
  minLength: number;
  maxLength: number;
  requiredPattern: string;
  expirationDays: number;
  forceChangeOnFirstLogin: boolean;
  maxFailedAttempts: number;
  lockoutMinutes: number;
  description: string;
  active: boolean;
}

export interface CredentialRules {
  login: LoginRule;
  password: PasswordRule;
  lastUpdated: Date;
  updatedBy: string;
}

/**
 * Configuração Padrão das Regras de Credenciais
 * Formato: XX00000X (2 letras + 5 números + 1 letra) - Login
 * Formato: X0000X (1 letra + 4 números + 1 letra) - Senha
 */
export const DEFAULT_CREDENTIAL_RULES: CredentialRules = {
  login: {
    minLength: 8,
    maxLength: 8,
    requiredPattern: "XX00000X", // 2 letras + 5 números + 1 letra
    description: "Login deve ter 8 caracteres: 2 letras maiúsculas, 5 números, 1 letra maiúscula",
    active: false // TEMPORARIAMENTE DESABILITADO - permite credenciais simples
  },
  password: {
    minLength: 6,
    maxLength: 6,
    requiredPattern: "X0000X", // 1 letra + 4 números + 1 letra
    expirationDays: 90,
    forceChangeOnFirstLogin: true,
    maxFailedAttempts: 5,
    lockoutMinutes: 30,
    description: "Senha deve ter 6 caracteres: 1 letra maiúscula, 4 números, 1 letra maiúscula",
    active: false // TEMPORARIAMENTE DESABILITADO - permite credenciais simples
  },
  lastUpdated: new Date(),
  updatedBy: "system"
};

/**
 * Valida se um caractere é uma letra maiúscula
 */
function isUppercaseLetter(char: string): boolean {
  return char.length === 1 && char >= 'A' && char <= 'Z';
}

/**
 * Valida se um caractere é um número
 */
function isDigit(char: string): boolean {
  return char.length === 1 && char >= '0' && char <= '9';
}

/**
 * Valida login seguindo o padrão XX00000X
 * @param login - String do login a ser validada
 * @param rules - Regras de validação (opcional, usa padrão se não informado)
 * @returns Objeto com resultado da validação
 */
export function validateLogin(login: string, rules: LoginRule = DEFAULT_CREDENTIAL_RULES.login) {
  if (!rules.active) {
    return { valid: true, message: "Validação desabilitada" };
  }

  // Verificar comprimento
  if (login.length !== rules.minLength) {
    return {
      valid: false,
      message: `Login deve ter exatamente ${rules.minLength} caracteres`
    };
  }

  // Verificar padrão XX00000X (2 letras + 5 números + 1 letra)
  const chars = login.split('');
  
  // Posições 0 e 1: letras maiúsculas
  if (!isUppercaseLetter(chars[0]) || !isUppercaseLetter(chars[1])) {
    return {
      valid: false,
      message: "Os dois primeiros caracteres devem ser letras maiúsculas"
    };
  }

  // Posições 2-6: números
  for (let i = 2; i <= 6; i++) {
    if (!isDigit(chars[i])) {
      return {
        valid: false,
        message: `O caractere na posição ${i + 1} deve ser um número`
      };
    }
  }

  // Posição 7: letra maiúscula
  if (!isUppercaseLetter(chars[7])) {
    return {
      valid: false,
      message: "O último caractere deve ser uma letra maiúscula"
    };
  }

  return { valid: true, message: "Login válido" };
}

/**
 * Valida senha seguindo o padrão X0000X
 * @param password - String da senha a ser validada
 * @param rules - Regras de validação (opcional, usa padrão se não informado)
 * @returns Objeto com resultado da validação
 */
export function validatePassword(password: string, rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password) {
  if (!rules.active) {
    return { valid: true, message: "Validação desabilitada" };
  }

  // Verificar comprimento
  if (password.length !== rules.minLength) {
    return {
      valid: false,
      message: `Senha deve ter exatamente ${rules.minLength} caracteres`
    };
  }

  // Verificar padrão X0000X (1 letra + 4 números + 1 letra)
  const chars = password.split('');
  
  // Posição 0: letra maiúscula
  if (!isUppercaseLetter(chars[0])) {
    return {
      valid: false,
      message: "O primeiro caractere deve ser uma letra maiúscula"
    };
  }

  // Posições 1-4: números
  for (let i = 1; i <= 4; i++) {
    if (!isDigit(chars[i])) {
      return {
        valid: false,
        message: `O caractere na posição ${i + 1} deve ser um número`
      };
    }
  }

  // Posição 5: letra maiúscula
  if (!isUppercaseLetter(chars[5])) {
    return {
      valid: false,
      message: "O último caractere deve ser uma letra maiúscula"
    };
  }

  return { valid: true, message: "Senha válida" };
}

/**
 * Verifica se a senha está expirada
 * @param lastPasswordChange - Data da última mudança de senha
 * @param rules - Regras de expiração
 * @returns Boolean indicando se a senha expirou
 */
export function isPasswordExpired(
  lastPasswordChange: Date, 
  rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
): boolean {
  if (!rules.active) {
    return false;
  }

  const daysSinceChange = Math.floor(
    (Date.now() - lastPasswordChange.getTime()) / (1000 * 60 * 60 * 24)
  );
  
  return daysSinceChange >= rules.expirationDays;
}

/**
 * Verifica se o usuário deve ser bloqueado por tentativas falhadas
 * @param failedAttempts - Número de tentativas falhadas
 * @param lastFailedAttempt - Data da última tentativa falhada
 * @param rules - Regras de bloqueio
 * @returns Objeto com status de bloqueio
 */
export function shouldLockAccount(
  failedAttempts: number,
  lastFailedAttempt: Date | null,
  rules: PasswordRule = DEFAULT_CREDENTIAL_RULES.password
) {
  if (!rules.active) {
    return { locked: false, message: "Validação desabilitada" };
  }

  if (failedAttempts < rules.maxFailedAttempts) {
    return { locked: false, message: "Conta não bloqueada" };
  }

  // Se não há registro de tentativa falhada, bloquear
  if (!lastFailedAttempt) {
    return { locked: true, message: "Conta bloqueada por muitas tentativas falhadas" };
  }

  // Verificar se o tempo de bloqueio já passou
  const minutesSinceLastAttempt = Math.floor(
    (Date.now() - lastFailedAttempt.getTime()) / (1000 * 60)
  );

  if (minutesSinceLastAttempt >= rules.lockoutMinutes) {
    return { locked: false, message: "Período de bloqueio expirou" };
  }

  return { 
    locked: true, 
    message: `Conta bloqueada. Tente novamente em ${rules.lockoutMinutes - minutesSinceLastAttempt} minutos` 
  };
}

/**
 * Atualiza as regras de credenciais
 * Esta função permite alterar as regras em tempo de execução
 * @param newRules - Novas regras a serem aplicadas
 * @param updatedBy - Usuário que está fazendo a atualização
 * @returns Novas regras atualizadas
 */
export function updateCredentialRules(
  newRules: Partial<CredentialRules>, 
  updatedBy: string
): CredentialRules {
  return {
    ...DEFAULT_CREDENTIAL_RULES,
    ...newRules,
    lastUpdated: new Date(),
    updatedBy
  };
}

/**
 * Gera exemplos válidos de credenciais para testes/demonstração
 * @param rules - Regras atuais (opcional)
 * @returns Objeto com exemplos de login e senha válidos
 */
export function generateValidExamples(rules: CredentialRules = DEFAULT_CREDENTIAL_RULES) {
  return {
    loginExamples: [
      "AB12345C",
      "XY98765Z", 
      "QW11111A"
    ],
    passwordExamples: [
      "A1234B",
      "Z9876X",
      "M0000N"
    ],
    rules: {
      loginPattern: rules.login.requiredPattern,
      passwordPattern: rules.password.requiredPattern,
      loginDescription: rules.login.description,
      passwordDescription: rules.password.description
    }
  };
}