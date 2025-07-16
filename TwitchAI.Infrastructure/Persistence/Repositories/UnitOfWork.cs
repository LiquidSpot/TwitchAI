using System.Reflection;

using Common.Packages.Logger.Services.Interfaces;
using Common.Packages.Response.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using TwitchAI.Application;
using TwitchAI.Application.Behaviors;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Attributes;
using TwitchAI.Domain.Entites.Abstract;
using TwitchAI.Domain.Enums.ErrorCodes;

namespace TwitchAI.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly Dictionary<string, object?> _repositories;
        private IDbContextTransaction? _currentTransaction;
        private readonly IExternalLogger<UnitOfWork> _logger;

        public UnitOfWork(ApplicationDbContext context, IExternalLogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _repositories = new Dictionary<string, object?>();
        }

        /// <inheritdoc />
        public IRepository<TEntity, TId>? Factory<TEntity, TId>() where TEntity : class, IEntity<TId>
        {
            //var entityType = typeof(TEntity);
            var typeName = $"{typeof(TEntity).Name}_{typeof(TId).Name}";

            //Провека на уже созданые репозитории в памяти
            if (!_repositories.ContainsKey(typeName))
            {
                //var context = GetContextForType(entityType);
                // Используем метод фабрики для создания нужного экземпляра репозитория и нужного контекста
                var repository =
                    RepositoryFactory.GetRepositoryInstance<TEntity, TId, Repository<TEntity, TId>>(_context);

                // Проверяем, был ли репозиторий успешно создан
                if (repository == null)
                {
                    throw new InvalidOperationException($"Не удалось создать экземпляр репозитория для типа {typeName}.");
                }

                _repositories.Add(typeName, repository);
            }

            return (Repository<TEntity, TId>)_repositories[typeName]!;
        }

       
        /// <inheritdoc />
        public async Task SaveChangesAsync(CancellationToken cancellationToken, bool saveChanges = true)
        {
            _logger.LogInformation();
            try
            {
                if (_context.ChangeTracker.HasChanges())
                    await _context.SaveChangesAsync(saveChanges, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError,
                    new { Message = $"{exception.Message} {exception.InnerException?.Message}" });
                await RollbackTransactionAsync(cancellationToken, saveChanges);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IDbContextTransaction?> BeginTransaction(CancellationToken cancellationToken,
            bool saveChanges = true)
        {
            _logger.LogInformation();
            try
            {
                if (_currentTransaction != null)
                {
                    return _currentTransaction;
                }

                _currentTransaction =
                    await _context.Database.BeginTransactionAsync(cancellationToken).ConfigureAwait(false);
                return _currentTransaction;
            }
            catch (Exception exception)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError,
                    new { Message = $"{exception.Message} {exception.InnerException?.Message}" });
                await RollbackTransactionAsync(cancellationToken, saveChanges);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task CommitTransactionAsync(CancellationToken cancellationToken, bool saveChanges = true)
        {
            _logger.LogInformation();
            try
            {
                if (_context.ChangeTracker.HasChanges())
                    await _context.SaveChangesAsync(saveChanges, cancellationToken);
                await _currentTransaction?.CommitAsync(cancellationToken)!;
            }
            catch (Exception exception)
            {
                _logger.LogError((int)BaseErrorCodes.OperationProcessError,
                    new { Message = $"{exception.Message} {exception.InnerException?.Message}" });
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        /// <inheritdoc />
        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default, bool saveChanges = true)
        {
            _logger.LogInformation();
            try
            {
                if (_currentTransaction != null)
                    _currentTransaction?.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
        
        /// <summary>
        /// ATTENTION Определяет контекст базы данных для заданного типа сущности.
        /// Этот метод является основным! для создания репозиториев, так как он подставляет нужный контекст,
        /// используя кастомный атрибут DbContextNameAttribute, определенный для каждого класса сущности.
        /// </summary>
        /// <param name="entityType">Тип сущности, для которой требуется определить контекст базы данных.</param>
        /// <returns>Экземпляр DbContext, соответствующий сущности.</returns>
        /// <exception cref="ArgumentException">Исключение выбрасывается, если для типа сущности не задан контекст
        /// или если указан неизвестный контекст.</exception>
        private DbContext GetContextForType(Type entityType)
        {
            var contextAttribute = entityType.GetCustomAttribute<DbContextNameAttribute>();

            if (contextAttribute != null)
            {
                switch (contextAttribute.ContextName)
                {
                    case nameof(ApplicationDbContext):
                        return _context;
                    default:
                        throw new LSException(BaseErrorCodes.NotFound,$"Неизвестный контекст: {contextAttribute.ContextName} {nameof(entityType)}");
                }
            }

            throw new LSException(BaseErrorCodes.DatabaseSaveError, $"Неизвестный тип сущности для контекста {nameof(entityType)}");
        }

    }
}