/**
 * Utilitários para detecção de ambiente
 * Development, Staging, Production
 */

export type Environment = 'development' | 'staging' | 'production';

export const getEnvironment = (): Environment => {
  // Verifica NODE_ENV primeiro
  if (process.env.NODE_ENV === 'development') return 'development';
  if (process.env.NODE_ENV === 'production') {
    // Em produção, verifica se é staging
    if (typeof window !== 'undefined') {
      const hostname = window.location.hostname;
      const href = window.location.href;

      // Staging URLs
      if (hostname.includes('staging') ||
          hostname.includes('stg') ||
          hostname.includes('test') ||
          href.includes('staging-coreapp') ||
          href.includes('vercel.app') ||
          hostname.includes('preview')) {
        return 'staging';
      }
    }
    return 'production';
  }

  // Fallback: detecta por hostname/URL (client-side)
  if (typeof window !== 'undefined') {
    const hostname = window.location.hostname;
    const href = window.location.href;

    // Development
    if (hostname === 'localhost' ||
        hostname.includes('127.0.0.1') ||
        hostname.includes('.local') ||
        hostname.includes('dev.')) {
      return 'development';
    }

    // Staging
    if (hostname.includes('staging') ||
        hostname.includes('stg') ||
        hostname.includes('test') ||
        href.includes('staging-coreapp') ||
        href.includes('vercel.app')) {
      return 'staging';
    }
  }

  // Production (padrão)
  return 'production';
};

export const isDevelopment = (): boolean => getEnvironment() === 'development';
export const isStaging = (): boolean => getEnvironment() === 'staging';
export const isProduction = (): boolean => getEnvironment() === 'production';
export const allowDevTools = (): boolean => isDevelopment() || isStaging();

export const getEnvironmentConfig = () => {
  const env = getEnvironment();

  return {
    environment: env,
    allowDevTools: allowDevTools(),
    showBanner: env !== 'production',
    enableLogging: env !== 'production',
    apiUrl: process.env.NEXT_PUBLIC_API_URL ||
      (env === 'development' ? 'http://localhost:3001' : 'https://api-coreapp.vercel.app')
  };
};