import { Request, Response, NextFunction } from 'express';

export interface ApiError extends Error {
  statusCode?: number;
  isOperational?: boolean;
}

export const errorHandler = (
  error: ApiError,
  req: Request,
  res: Response,
  next: NextFunction
) => {
  // Log do erro
  console.error(`❌ Erro na API:`, {
    message: error.message,
    stack: process.env.NODE_ENV === 'development' ? error.stack : undefined,
    url: req.url,
    method: req.method,
    timestamp: new Date().toISOString()
  });

  // Status code padrão
  const statusCode = error.statusCode || 500;
  
  // Resposta de erro padronizada
  const errorResponse = {
    success: false,
    error: {
      message: error.message || 'Internal Server Error',
      code: statusCode,
      timestamp: new Date().toISOString()
    },
    // Stack trace apenas em desenvolvimento
    ...(process.env.NODE_ENV === 'development' && { stack: error.stack })
  };

  res.status(statusCode).json(errorResponse);
};

// Helper para criar erros customizados
export const createError = (message: string, statusCode: number = 500): ApiError => {
  const error = new Error(message) as ApiError;
  error.statusCode = statusCode;
  error.isOperational = true;
  return error;
};