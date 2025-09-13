/**
 * Interceptadores Globais para Debug Proativo
 * Captura API calls, console logs e performance automaticamente
 */

import { isDevelopment } from '@/utils/environment';

interface InterceptorConfig {
  api: boolean;
  console: boolean;
  performance: boolean;
  interactions: boolean;
}

// Interface para logging (será conectada ao useDebugLogger)
interface Logger {
  logError: (category: any, event: string, message: string, context?: any, suggestedFix?: string) => void;
  logWarning: (category: any, event: string, message: string, context?: any) => void;
  logInfo: (category: any, event: string, message: string, context?: any) => void;
}

let debugLogger: Logger | null = null;

export const setDebugLogger = (logger: Logger) => {
  debugLogger = logger;
};

export const initializeInterceptors = (config: InterceptorConfig = {
  api: true,
  console: true,
  performance: true,
  interactions: true
}) => {
  if (!isDevelopment()) return;

  console.log('🔧 Initializing Debug Interceptors...');

  // 1. API INTERCEPTOR (Fetch)
  if (config.api) {
    const originalFetch = window.fetch;

    window.fetch = async function(url: RequestInfo | URL, options?: RequestInit) {
      const startTime = Date.now();
      const method = options?.method || 'GET';
      const urlString = typeof url === 'string' ? url : url.toString();

      try {
        const response = await originalFetch(url, options);
        const endTime = Date.now();
        const duration = endTime - startTime;

        // Log performance issues
        if (duration > 1000) {
          debugLogger?.logWarning(
            'API',
            'slow_api_call',
            `API call took ${duration}ms: ${method} ${urlString}`,
            {
              url: urlString,
              method,
              time_taken: duration,
              status: response.status
            }
          );
        }

        // Log API errors
        if (!response.ok) {
          const errorText = await response.clone().text().catch(() => 'Unknown error');
          debugLogger?.logError(
            'API',
            'api_error',
            `API Error: ${response.status} ${response.statusText}`,
            {
              url: urlString,
              method,
              status: response.status,
              statusText: response.statusText,
              response_text: errorText,
              time_taken: duration
            },
            'Check API endpoint and network connection'
          );
        } else if (duration < 500) {
          // Log successful fast API calls (for positive feedback)
          debugLogger?.logInfo(
            'API',
            'api_success',
            `✅ ${method} ${urlString} - ${duration}ms`,
            {
              url: urlString,
              method,
              status: response.status,
              time_taken: duration
            }
          );
        }

        return response;
      } catch (error) {
        const endTime = Date.now();
        const duration = endTime - startTime;

        debugLogger?.logError(
          'API',
          'network_error',
          `Network Error: ${error}`,
          {
            url: urlString,
            method,
            error: error instanceof Error ? error.message : String(error),
            stack_trace: error instanceof Error ? error.stack : undefined,
            time_taken: duration
          },
          'Check internet connection and server availability'
        );

        throw error;
      }
    };
  }

  // 2. CONSOLE INTERCEPTOR
  if (config.console) {
    const originalConsoleError = console.error;
    const originalConsoleWarn = console.warn;

    console.error = function(...args) {
      // Filtrar ruído (erros do próprio debug system)
      const message = args.join(' ');
      if (!message.includes('CLAUDE MONITORING') && !message.includes('DebugIndicator')) {
        debugLogger?.logError(
          'UI',
          'console_error',
          `Console Error: ${message}`,
          {
            console_args: args,
            stack_trace: new Error().stack
          },
          'Check browser console for detailed error information'
        );
      }

      originalConsoleError.apply(console, args);
    };

    console.warn = function(...args) {
      const message = args.join(' ');
      if (!message.includes('CLAUDE MONITORING') && !message.includes('DebugIndicator')) {
        debugLogger?.logWarning(
          'UI',
          'console_warning',
          `Console Warning: ${message}`,
          {
            console_args: args
          }
        );
      }

      originalConsoleWarn.apply(console, args);
    };
  }

  // 3. PERFORMANCE INTERCEPTOR
  if (config.performance) {
    // Monitor long tasks
    if ('PerformanceObserver' in window) {
      const perfObserver = new PerformanceObserver((list) => {
        const entries = list.getEntries();
        entries.forEach((entry) => {
          if (entry.entryType === 'measure' && entry.duration > 100) {
            debugLogger?.logWarning(
              'PERFORMANCE',
              'slow_operation',
              `Slow operation detected: ${entry.name} took ${entry.duration.toFixed(2)}ms`,
              {
                operation: entry.name,
                duration: entry.duration,
                start_time: entry.startTime
              }
            );
          }
        });
      });

      try {
        perfObserver.observe({ entryTypes: ['measure', 'navigation'] });
      } catch (e) {
        console.warn('Performance Observer not fully supported');
      }
    }

    // Monitor memory usage (if available)
    if ('memory' in performance) {
      setInterval(() => {
        const memInfo = (performance as any).memory;
        const usedMB = Math.round(memInfo.usedJSHeapSize / 1048576);
        const totalMB = Math.round(memInfo.totalJSHeapSize / 1048576);

        // Alert if memory usage is high
        if (usedMB > 100) {
          debugLogger?.logWarning(
            'PERFORMANCE',
            'high_memory_usage',
            `High memory usage: ${usedMB}MB / ${totalMB}MB`,
            {
              used_memory_mb: usedMB,
              total_memory_mb: totalMB,
              limit_mb: memInfo.jsHeapSizeLimit / 1048576
            }
          );
        }
      }, 30000); // Check every 30 seconds
    }
  }

  // 4. INTERACTION INTERCEPTOR
  if (config.interactions) {
    // Monitor important click events
    document.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;

      // Log clicks on important elements
      if (
        target.tagName === 'BUTTON' ||
        target.type === 'submit' ||
        target.getAttribute('role') === 'button' ||
        target.closest('button') ||
        target.closest('[role="button"]')
      ) {
        const elementInfo = {
          tag: target.tagName,
          className: target.className,
          id: target.id,
          text: target.textContent?.trim().substring(0, 50),
          type: target.getAttribute('type')
        };

        debugLogger?.logInfo(
          'INTERACTION',
          'button_click',
          `Button clicked: ${elementInfo.text || elementInfo.className || elementInfo.tag}`,
          {
            element: elementInfo,
            coordinates: [event.clientX, event.clientY]
          }
        );
      }
    });

    // Monitor form submissions
    document.addEventListener('submit', (event) => {
      const form = event.target as HTMLFormElement;
      const formData = new FormData(form);
      const fields = Array.from(formData.keys());

      debugLogger?.logInfo(
        'INTERACTION',
        'form_submit',
        `Form submitted with ${fields.length} fields`,
        {
          form_id: form.id,
          form_className: form.className,
          fields: fields,
          action: form.action,
          method: form.method
        }
      );
    });
  }

  console.log('✅ Debug Interceptors initialized successfully');
};

// Cleanup function
export const cleanupInterceptors = () => {
  // Em uma implementação mais robusta, aqui restauraríamos os métodos originais
  console.log('🧹 Debug Interceptors cleaned up');
};