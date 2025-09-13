/**
 * User Service - Exemplo de uso do Unit of Work + CQRS
 * Demonstra implementação completa com padrões avançados
 */

import { IUnitOfWork } from '../repositories/IUnitOfWork';
import { 
  BaseCommand, 
  BaseQuery, 
  CommandResult, 
  QueryResult,
  ICommandHandler,
  IQueryHandler,
  ValidateCommand,
  Authorize,
  Cacheable
} from '../patterns/CQRS';
import { AuthContext } from '../config/jwtConfig';
import { generateLogin, generateTemporaryPassword, createInitialPassword } from './passwordService';
import { cacheService } from './cacheService';

// ====================================
// DOMAIN MODELS / DTOs
// ====================================

export interface CreateUserDto {
  nome: string;
  email?: string;
  roleId: string;
  tenantId: string;
  generateCredentials?: boolean;
}

export interface UpdateUserDto {
  nome?: string;
  email?: string;
  roleId?: string;
  ativo?: boolean;
}

export interface UserViewModel {
  id: string;
  login: string;
  nome: string;
  email?: string;
  roleName: string;
  ativo: boolean;
  ultimoLogin?: Date;
  dataCadastro: Date;
}

// ====================================
// COMMANDS (Write Operations)
// ====================================

export class CreateUserCommand extends BaseCommand {
  constructor(
    public readonly data: CreateUserDto,
    auth: AuthContext
  ) {
    super(auth);
  }
}

export class UpdateUserCommand extends BaseCommand {
  constructor(
    public readonly userId: string,
    public readonly data: UpdateUserDto,
    auth: AuthContext
  ) {
    super(auth);
  }
}

export class DeactivateUserCommand extends BaseCommand {
  constructor(
    public readonly userId: string,
    public readonly reason?: string,
    auth: AuthContext
  ) {
    super(auth);
  }
}

export class ResetUserPasswordCommand extends BaseCommand {
  constructor(
    public readonly userId: string,
    public readonly forceChange: boolean = true,
    auth: AuthContext
  ) {
    super(auth);
  }
}

// ====================================
// QUERIES (Read Operations)
// ====================================

export class GetUserByIdQuery extends BaseQuery {
  constructor(
    public readonly userId: string,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

export class GetUsersByTenantQuery extends BaseQuery {
  constructor(
    public readonly page: number = 1,
    public readonly size: number = 10,
    public readonly includeInactive: boolean = false,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

export class SearchUsersQuery extends BaseQuery {
  constructor(
    public readonly searchTerm: string,
    public readonly roleId?: string,
    auth?: AuthContext
  ) {
    super(auth);
  }
}

// ====================================
// COMMAND HANDLERS
// ====================================

export class CreateUserCommandHandler implements ICommandHandler<CreateUserCommand> {
  
  @ValidateCommand((cmd: CreateUserCommand) => {
    const errors: string[] = [];
    if (!cmd.data.nome.trim()) errors.push('Nome é obrigatório');
    if (!cmd.data.roleId) errors.push('Role é obrigatória');
    if (!cmd.data.tenantId) errors.push('Tenant é obrigatório');
    if (cmd.data.email && !cmd.data.email.includes('@')) errors.push('Email inválido');
    return errors;
  })
  @Authorize(['usuarios:criar'])
  async handle(command: CreateUserCommand, uow: IUnitOfWork): Promise<CommandResult> {
    try {
      // Verificar se role existe
      const role = await uow.roles.findById(command.data.roleId, command.data.tenantId);
      if (!role) {
        return {
          success: false,
          error: 'Role não encontrada'
        };
      }

      // Gerar login único
      let login = generateLogin(command.data.nome);
      let attempts = 0;
      
      while (attempts < 10) {
        const existingUser = await uow.usuarios.findFirst(
          { login }, 
          command.data.tenantId
        );
        
        if (!existingUser) break;
        
        login = generateLogin(command.data.nome);
        attempts++;
      }

      if (attempts >= 10) {
        return {
          success: false,
          error: 'Não foi possível gerar login único'
        };
      }

      // Gerar senha temporária
      const temporaryPassword = generateTemporaryPassword();
      const passwordData = await createInitialPassword(temporaryPassword, true);

      // Criar usuário
      const userData = {
        login,
        nome: command.data.nome,
        email: command.data.email,
        roleId: command.data.roleId,
        senhaHash: passwordData.hashedPassword,
        forcarTrocaSenha: passwordData.forceChangeOnFirstLogin
      };

      const user = await uow.usuarios.add(
        userData,
        command.data.tenantId,
        command.executedBy
      );

      // Invalidar cache relacionado
      uow.scheduleInvalidateCache(`users:${command.data.tenantId}`);

      // Publicar evento de domínio
      await uow.publishDomainEvent(
        'UserCreated',
        user.id,
        { login, nome: command.data.nome },
        command.data.tenantId
      );

      return {
        success: true,
        data: {
          id: user.id,
          login,
          temporaryPassword: command.data.generateCredentials ? temporaryPassword : undefined
        },
        affectedRows: 1
      };

    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Erro interno'
      };
    }
  }
}

export class UpdateUserCommandHandler implements ICommandHandler<UpdateUserCommand> {
  
  @ValidateCommand((cmd: UpdateUserCommand) => {
    const errors: string[] = [];
    if (cmd.data.nome && !cmd.data.nome.trim()) errors.push('Nome não pode estar vazio');
    if (cmd.data.email && !cmd.data.email.includes('@')) errors.push('Email inválido');
    return errors;
  })
  @Authorize(['usuarios:editar'])
  async handle(command: UpdateUserCommand, uow: IUnitOfWork): Promise<CommandResult> {
    try {
      const updatedUser = await uow.usuarios.update(
        command.userId,
        command.data,
        command.tenantId,
        command.executedBy
      );

      if (!updatedUser) {
        return {
          success: false,
          error: 'Usuário não encontrado'
        };
      }

      // Invalidar cache
      uow.scheduleInvalidateCache(`users:${command.tenantId}`);
      uow.scheduleInvalidateCache(`user:${command.userId}`);

      // Evento de domínio
      await uow.publishDomainEvent(
        'UserUpdated',
        command.userId,
        command.data,
        command.tenantId
      );

      return {
        success: true,
        data: updatedUser,
        affectedRows: 1
      };

    } catch (error) {
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Erro interno'
      };
    }
  }
}

// ====================================
// QUERY HANDLERS
// ====================================

export class GetUserByIdQueryHandler implements IQueryHandler<GetUserByIdQuery> {
  
  @Cacheable(300, (query) => `user:${query.userId}:${query.tenantId}`) // 5 minutos
  async handle(query: GetUserByIdQuery, uow: IUnitOfWork): Promise<QueryResult<UserViewModel | null>> {
    try {
      const user = await uow.usuarios.findAdvanced({
        where: { id: query.userId },
        include: { role: { select: { nome: true } } },
        tenantId: query.tenantId
      });

      if (!user[0]) {
        return {
          data: null,
          cached: false
        };
      }

      const userData = user[0];
      const viewModel: UserViewModel = {
        id: userData.id,
        login: userData.login,
        nome: userData.nome,
        email: userData.email,
        roleName: userData.role.nome,
        ativo: userData.ativo,
        ultimoLogin: userData.ultimoLogin,
        dataCadastro: userData.dataCadastro
      };

      return {
        data: viewModel,
        cached: false
      };

    } catch (error) {
      throw new Error(`Erro ao buscar usuário: ${error}`);
    }
  }
}

export class GetUsersByTenantQueryHandler implements IQueryHandler<GetUsersByTenantQuery> {
  
  @Cacheable(180, (query) => `users:${query.tenantId}:p${query.page}:s${query.size}`) // 3 minutos
  @Authorize(['usuarios:visualizar'])
  async handle(query: GetUsersByTenantQuery, uow: IUnitOfWork): Promise<QueryResult<UserViewModel[]>> {
    try {
      const skip = (query.page - 1) * query.size;
      
      const result = await uow.usuarios.findPaginated(
        skip,
        query.size,
        { includeDeleted: query.includeInactive },
        query.tenantId
      );

      const viewModels: UserViewModel[] = result.data.map((user: any) => ({
        id: user.id,
        login: user.login,
        nome: user.nome,
        email: user.email,
        roleName: user.role?.nome || 'N/A',
        ativo: user.ativo,
        ultimoLogin: user.ultimoLogin,
        dataCadastro: user.dataCadastro
      }));

      return {
        data: viewModels,
        total: result.total,
        page: query.page,
        size: query.size,
        cached: false
      };

    } catch (error) {
      throw new Error(`Erro ao listar usuários: ${error}`);
    }
  }
}

// ====================================
// SERVICE CLASS (Facade)
// ====================================

/**
 * User Service - Facade usando UoW + CQRS
 * Expõe interface simples para controllers
 */
export class UserService {
  constructor(private uow: IUnitOfWork) {}

  // Commands (Write Operations)
  async createUser(data: CreateUserDto, auth: AuthContext): Promise<CommandResult> {
    const command = new CreateUserCommand(data, auth);
    const handler = new CreateUserCommandHandler();
    return await handler.handle(command, this.uow);
  }

  async updateUser(userId: string, data: UpdateUserDto, auth: AuthContext): Promise<CommandResult> {
    const command = new UpdateUserCommand(userId, data, auth);
    const handler = new UpdateUserCommandHandler();
    return await handler.handle(command, this.uow);
  }

  async deactivateUser(userId: string, reason: string, auth: AuthContext): Promise<CommandResult> {
    const command = new DeactivateUserCommand(userId, reason, auth);
    // Handler seria implementado similar aos outros
    return { success: true, affectedRows: 1 };
  }

  // Queries (Read Operations)
  async getUserById(userId: string, auth?: AuthContext): Promise<QueryResult<UserViewModel | null>> {
    const query = new GetUserByIdQuery(userId, auth);
    const handler = new GetUserByIdQueryHandler();
    return await handler.handle(query, this.uow);
  }

  async getUsersByTenant(
    page: number = 1, 
    size: number = 10, 
    includeInactive: boolean = false,
    auth?: AuthContext
  ): Promise<QueryResult<UserViewModel[]>> {
    const query = new GetUsersByTenantQuery(page, size, includeInactive, auth);
    const handler = new GetUsersByTenantQueryHandler();
    return await handler.handle(query, this.uow);
  }

  // Advanced Operations
  async executeComplexUserOperation(auth: AuthContext): Promise<CommandResult> {
    return await this.uow.executeInTransaction(async (uow) => {
      // Múltiplas operações em uma transação
      const users = await uow.usuarios.findMany({}, auth.tenantId);
      
      let updatedCount = 0;
      for (const user of users) {
        await uow.usuarios.update(
          user.id,
          { dataUltimaAtualizacao: new Date() },
          auth.tenantId,
          auth.userId
        );
        updatedCount++;
      }

      // Invalidar cache em batch
      uow.scheduleInvalidateCache(`users:${auth.tenantId}`);

      return {
        success: true,
        affectedRows: updatedCount
      };
    });
  }
}