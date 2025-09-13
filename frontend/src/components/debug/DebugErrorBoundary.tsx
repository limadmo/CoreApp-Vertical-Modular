/**
 * Error Boundary Inteligente para Debug
 * Captura crashes React e permite recovery automático
 */

'use client';

import React, { Component, ReactNode } from 'react';
import { isDevelopment } from '@/utils/environment';

interface Props {
  children: ReactNode;
  fallback?: ReactNode;
  onError?: (error: Error, errorInfo: React.ErrorInfo) => void;
}

interface State {
  hasError: boolean;
  error: Error | null;
  errorInfo: React.ErrorInfo | null;
  retryCount: number;
}

export class DebugErrorBoundary extends Component<Props, State> {
  private retryTimer: NodeJS.Timeout | null = null;
  private maxRetries = 3;

  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false,
      error: null,
      errorInfo: null,
      retryCount: 0
    };
  }

  static getDerivedStateFromError(error: Error): Partial<State> {
    return {
      hasError: true,
      error
    };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    this.setState({
      error,
      errorInfo
    });

    // Log estruturado para meu monitoramento proativo
    const structuredError = {
      timestamp: new Date().toISOString(),
      level: 'ERROR',
      category: 'REACT',
      event: 'component_crash',
      message: `React component crashed: ${error.message}`,
      context: {
        error_message: error.message,
        error_name: error.name,
        stack_trace: error.stack,
        component_stack: errorInfo.componentStack,
        retry_count: this.state.retryCount,
        url: typeof window !== 'undefined' ? window.location.href : 'unknown'
      },
      action_needed: true,
      suggested_fix: 'Check component props and state management'
    };

    // Console log estruturado para eu monitorar
    console.group('🤖 CLAUDE MONITORING - ERROR - REACT');
    console.error('💥 React Component Crash Detected!');
    console.log('📊 Structured Error Log:', structuredError);
    console.log('🔍 Error Details:', {
      message: error.message,
      stack: error.stack,
      componentStack: errorInfo.componentStack
    });
    console.log('🚨 ACTION NEEDED: Component recovery required');
    console.groupEnd();

    // Callback personalizado
    this.props.onError?.(error, errorInfo);

    // Auto-retry em development (com limite)
    if (isDevelopment() && this.state.retryCount < this.maxRetries) {
      console.log(`🔄 Auto-retry ${this.state.retryCount + 1}/${this.maxRetries} in 3 seconds...`);

      this.retryTimer = setTimeout(() => {
        this.setState(prevState => ({
          hasError: false,
          error: null,
          errorInfo: null,
          retryCount: prevState.retryCount + 1
        }));
      }, 3000);
    }
  }

  componentWillUnmount() {
    if (this.retryTimer) {
      clearTimeout(this.retryTimer);
    }
  }

  handleManualRetry = () => {
    this.setState({
      hasError: false,
      error: null,
      errorInfo: null,
      retryCount: this.state.retryCount + 1
    });
  };

  handleReload = () => {
    window.location.reload();
  };

  render() {
    if (this.state.hasError) {
      // Fallback customizado
      if (this.props.fallback) {
        return this.props.fallback;
      }

      // Fallback padrão em development
      if (isDevelopment()) {
        return (
          <div
            style={{
              padding: '20px',
              margin: '20px',
              border: '2px solid #f44336',
              borderRadius: '8px',
              backgroundColor: '#ffebee',
              fontFamily: 'monospace'
            }}
          >
            <h2 style={{ color: '#d32f2f', margin: '0 0 16px 0' }}>
              🔴 React Component Crash
            </h2>

            <div style={{ marginBottom: '16px' }}>
              <strong>Error:</strong> {this.state.error?.message}
            </div>

            {this.state.error?.stack && (
              <details style={{ marginBottom: '16px' }}>
                <summary style={{ cursor: 'pointer', fontWeight: 'bold' }}>
                  📋 Stack Trace
                </summary>
                <pre
                  style={{
                    backgroundColor: '#f5f5f5',
                    padding: '8px',
                    borderRadius: '4px',
                    overflow: 'auto',
                    fontSize: '12px',
                    marginTop: '8px'
                  }}
                >
                  {this.state.error.stack}
                </pre>
              </details>
            )}

            {this.state.errorInfo?.componentStack && (
              <details style={{ marginBottom: '16px' }}>
                <summary style={{ cursor: 'pointer', fontWeight: 'bold' }}>
                  🧩 Component Stack
                </summary>
                <pre
                  style={{
                    backgroundColor: '#f5f5f5',
                    padding: '8px',
                    borderRadius: '4px',
                    overflow: 'auto',
                    fontSize: '12px',
                    marginTop: '8px'
                  }}
                >
                  {this.state.errorInfo.componentStack}
                </pre>
              </details>
            )}

            <div style={{ display: 'flex', gap: '8px', marginTop: '16px' }}>
              <button
                onClick={this.handleManualRetry}
                disabled={this.state.retryCount >= this.maxRetries}
                style={{
                  padding: '8px 16px',
                  backgroundColor: '#4CAF50',
                  color: 'white',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                🔄 Retry ({this.state.retryCount}/{this.maxRetries})
              </button>

              <button
                onClick={this.handleReload}
                style={{
                  padding: '8px 16px',
                  backgroundColor: '#ff9800',
                  color: 'white',
                  border: 'none',
                  borderRadius: '4px',
                  cursor: 'pointer'
                }}
              >
                🔃 Reload Page
              </button>
            </div>

            {this.state.retryCount >= this.maxRetries && (
              <div
                style={{
                  marginTop: '16px',
                  padding: '8px',
                  backgroundColor: '#fff3e0',
                  border: '1px solid #ff9800',
                  borderRadius: '4px'
                }}
              >
                ⚠️ Max retries reached. Please reload the page or check the console for more details.
              </div>
            )}

            {this.retryTimer && (
              <div style={{ marginTop: '8px', color: '#666' }}>
                🔄 Auto-retry in progress...
              </div>
            )}
          </div>
        );
      }

      // Fallback simples em production
      return (
        <div
          style={{
            padding: '20px',
            textAlign: 'center',
            backgroundColor: '#f5f5f5',
            border: '1px solid #ddd',
            borderRadius: '8px',
            margin: '20px'
          }}
        >
          <h3>Oops! Something went wrong</h3>
          <p>Please refresh the page to continue.</p>
          <button
            onClick={this.handleReload}
            style={{
              padding: '8px 16px',
              backgroundColor: '#4CAF50',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: 'pointer'
            }}
          >
            Refresh Page
          </button>
        </div>
      );
    }

    return this.props.children;
  }
}