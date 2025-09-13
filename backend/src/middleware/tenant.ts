import { Request, Response, NextFunction } from 'express';

export interface TenantRequest extends Request {
  tenantId: string;
}

export const tenantMiddleware = (req: Request, res: Response, next: NextFunction) => {
  // Extrair tenant ID do header x-tenant-id ou usar padrão
  const tenantId = req.headers['x-tenant-id'] as string || 
                   process.env.DEFAULT_TENANT_ID || 
                   'padaria-demo';
  
  // Adicionar tenant ao request
  (req as TenantRequest).tenantId = tenantId;
  
  // Log para debug (apenas em desenvolvimento)
  if (process.env.NODE_ENV === 'development') {
    console.log(`🏢 Tenant: ${tenantId} | ${req.method} ${req.path}`);
  }
  
  next();
};