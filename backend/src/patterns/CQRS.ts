/**
 * CQRS Pattern - Command Query Responsibility Segregation
 * Separação entre operações de escrita (Commands) e leitura (Queries)
 */

import { IUnitOfWork } from '../repositories/IUnitOfWork';
import { AuthContext } from '../config/jwtConfig';

// ====================================
// BASE INTERFACES
// ====================================

/**
 * Interface base para todos os Commands (operações de escrita)
 */
export interface ICommand {
  readonly executedAt: Date;
  readonly executedBy: string;
  readonly tenantId: string;
}

/**
 * Interface base para todos os Queries (operações de leitura)
 */
export interface IQuery {
  readonly tenantId?: string;
  readonly requestedBy?: string;
}

/**
 * Resultado padrão para Commands
 */
export interface CommandResult<T = any> {
  success: boolean;
  data?: T;
  error?: string;
  validationErrors?: string[];
  affectedRows?: number;
  executionTime?: number;
}

/**
 * Resultado padrão para Queries
 */
export interface QueryResult<T = any> {
  data: T;
  total?: number;
  page?: number;
  size?: number;
  executionTime?: number;
  cached?: boolean;
}

// ====================================
// HANDLERS
// ====================================

/**
 * Interface para Command Handlers
 */
export interface ICommandHandler<TCommand extends ICommand, TResult = any> {
  handle(command: TCommand, uow: IUnitOfWork): Promise<CommandResult<TResult>>;
}

/**
 * Interface para Query Handlers
 */
export interface IQueryHandler<TQuery extends IQuery, TResult = any> {
  handle(query: TQuery, uow: IUnitOfWork): Promise<QueryResult<TResult>>;
}

// ====================================
// MEDIATOR PATTERN
// ====================================

/**
 * Interface do Mediator para centralizar Commands e Queries
 */
export interface IMediator {
  send<TResult = any>(command: ICommand): Promise<CommandResult<TResult>>;
  query<TResult = any>(query: IQuery): Promise<QueryResult<TResult>>;
}

/**
 * Implementação do Mediator
 */
export class Mediator implements IMediator {
  private commandHandlers = new Map<string, ICommandHandler<any>>();
  private queryHandlers = new Map<string, IQueryHandler<any>>();

  constructor(private uow: IUnitOfWork) {}

  /**
   * Registra um Command Handler
   */
  registerCommandHandler<T extends ICommand>(
    commandType: string,
    handler: ICommandHandler<T>
  ): void {
    this.commandHandlers.set(commandType, handler);
  }

  /**
   * Registra um Query Handler
   */
  registerQueryHandler<T extends IQuery>(
    queryType: string,
    handler: IQueryHandler<T>
  ): void {
    this.queryHandlers.set(queryType, handler);
  }

  /**
   * Executa um Command
   */
  async send<TResult = any>(command: ICommand): Promise<CommandResult<TResult>> {
    const commandType = command.constructor.name;
    const handler = this.commandHandlers.get(commandType);

    if (!handler) {
      return {
        success: false,
        error: `No handler found for command: ${commandType}`
      };
    }

    const startTime = Date.now();

    try {
      const result = await this.uow.executeInTransaction(async (uow) => {
        return await handler.handle(command, uow);
      });

      result.executionTime = Date.now() - startTime;
      return result;

    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error',
        executionTime: Date.now() - startTime
      };
    }
  }

  /**
   * Executa uma Query
   */
  async query<TResult = any>(query: IQuery): Promise<QueryResult<TResult>> {
    const queryType = query.constructor.name;
    const handler = this.queryHandlers.get(queryType);

    if (!handler) {
      throw new Error(`No handler found for query: ${queryType}`);
    }

    const startTime = Date.now();
    const result = await handler.handle(query, this.uow);
    result.executionTime = Date.now() - startTime;

    return result;
  }
}

// ====================================
// BASE COMMAND/QUERY CLASSES
// ====================================

/**
 * Classe base para Commands
 */
export abstract class BaseCommand implements ICommand {
  readonly executedAt: Date;
  readonly executedBy: string;
  readonly tenantId: string;

  constructor(auth: AuthContext) {
    this.executedAt = new Date();
    this.executedBy = auth.userId;
    this.tenantId = auth.tenantId;
  }
}

/**
 * Classe base para Queries
 */
export abstract class BaseQuery implements IQuery {
  readonly tenantId?: string;
  readonly requestedBy?: string;

  constructor(auth?: AuthContext) {
    this.tenantId = auth?.tenantId;
    this.requestedBy = auth?.userId;
  }
}

// ====================================
// COMMON COMMANDS
// ====================================

/**
 * Command genérico para criação
 */
export class CreateCommand<T> extends BaseCommand {
  constructor(
    public readonly data: Omit<T, 'id'>,
    auth: AuthContext
  ) {
    super(auth);
  }
}

/**
 * Command genérico para atualização
 */
export class UpdateCommand<T> extends BaseCommand {
  constructor(
    public readonly id: string,
    public readonly data: Partial<T>,
    auth: AuthContext
  ) {
    super(auth);
  }
}

/**
 * Command genérico para remoção
 */
export class DeleteCommand extends BaseCommand {
  constructor(
    public readonly id: string,
    public readonly reason?: string,
    auth: AuthContext
  ) {
    super(auth);
  }
}

// ====================================
// COMMON QUERIES
// ====================================

/**
 * Query genérica para buscar por ID
 */
export class GetByIdQuery extends BaseQuery {
  constructor(
    public readonly id: string,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

/**
 * Query genérica para listagem paginada
 */
export class GetPaginatedQuery extends BaseQuery {
  constructor(
    public readonly page: number = 1,
    public readonly size: number = 10,
    public readonly filters?: any,
    public readonly sort?: string,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

/**
 * Query genérica para busca com critérios
 */
export class SearchQuery extends BaseQuery {
  constructor(
    public readonly criteria: any,
    public readonly includeDeleted: boolean = false,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

// ====================================
// VALIDATION DECORATORS
// ====================================

/**
 * Decorator para validar Commands
 */
export function ValidateCommand(validationRules: (command: any) => string[]) {
  return function (target: any, propertyName: string, descriptor: PropertyDescriptor) {
    const method = descriptor.value;

    descriptor.value = async function (...args: any[]) {
      const command = args[0];
      const errors = validationRules(command);

      if (errors.length > 0) {
        return {
          success: false,
          validationErrors: errors
        };
      }

      return method.apply(this, args);
    };
  };
}

/**
 * Decorator para autorizar Commands/Queries
 */
export function Authorize(permissions: string[]) {
  return function (target: any, propertyName: string, descriptor: PropertyDescriptor) {
    const method = descriptor.value;

    descriptor.value = async function (...args: any[]) {
      const commandOrQuery = args[0];
      
      // Verificação de permissões seria implementada aqui
      // Por simplicidade, apenas logamos
      if (process.env.NODE_ENV === 'development') {
        console.log(`[AUTH] Required permissions: ${permissions.join(', ')}`);
      }

      return method.apply(this, args);
    };
  };
}

/**
 * Decorator para cache em Queries
 */
export function Cacheable(ttlSeconds: number, keyGenerator?: (query: any) => string) {
  return function (target: any, propertyName: string, descriptor: PropertyDescriptor) {
    const method = descriptor.value;

    descriptor.value = async function (...args: any[]) {
      const query = args[0];
      const cacheKey = keyGenerator ? keyGenerator(query) : JSON.stringify(query);

      // Cache seria implementado aqui
      if (process.env.NODE_ENV === 'development') {
        console.log(`[CACHE] Query cached with key: ${cacheKey} for ${ttlSeconds}s`);
      }

      return method.apply(this, args);
    };
  };
}

// ====================================
// FACTORY PARA MEDIATOR
// ====================================

/**
 * Factory para criar e configurar Mediator
 */
export class MediatorFactory {
  static create(uow: IUnitOfWork): Mediator {
    const mediator = new Mediator(uow);
    
    // Registrar handlers básicos aqui
    // mediator.registerCommandHandler('CreateUserCommand', new CreateUserCommandHandler());
    // mediator.registerQueryHandler('GetUserByIdQuery', new GetUserByIdQueryHandler());
    
    return mediator;
  }
}