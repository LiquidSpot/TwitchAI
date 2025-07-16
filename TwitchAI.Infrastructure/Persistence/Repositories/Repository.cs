using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common.Packages.Response.Exceptions;
using Microsoft.EntityFrameworkCore;

using TwitchAI.Application.Behaviors;
using TwitchAI.Application.Interfaces.Infrastructure;
using TwitchAI.Domain.Entites.Abstract;
using TwitchAI.Domain.Enums.ErrorCodes;

#pragma warning disable CS8619

namespace TwitchAI.Infrastructure.Persistence.Repositories
{
    [SuppressMessage("ReSharper", "AsyncConverter.ConfigureAwaitHighlighting")]
    public class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class, IEntity<TId>
    {
        private readonly DbContext _context;
        public readonly DbSet<TEntity> _dbSet;

        public Repository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<TEntity>();
        }

        #region Implementation of IRepository<T>

        /// <inheritdoc />
        public async Task<TEntity?> AddAsync(TEntity? entity, CancellationToken cancellationToken,
            bool saveChanges = false)
        {
            if (entity is null)
                throw new LSException(BaseErrorCodes.DataNotFound, errorObjects: new object[]{ typeof(TEntity) });

            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        /// <inheritdoc />
        public async Task<ICollection<TEntity?>> AddManyAsync(ICollection<TEntity?> entities,
            CancellationToken cancellationToken, bool saveChanges = false)
        {
            if (entities.Any() == false)
                throw new LSException(BaseErrorCodes.DataNotFound, errorObjects: new object[] { typeof(TEntity) });

            await _dbSet.AddRangeAsync(entities!, cancellationToken).ConfigureAwait(false);

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return entities;
        }

        /// <inheritdoc />
        public async Task<TEntity?> Update(TEntity? entity, CancellationToken cancellationToken, bool saveChanges = false)
        {
            if (entity is null)
                throw new LSException(BaseErrorCodes.DataNotFound, errorObjects: new object[] { typeof(TEntity) });

            _dbSet.Update(entity);

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return entity;
        }

        /// <inheritdoc />
        public async Task<ICollection<TEntity?>> UpdateRange(ICollection<TEntity?> entities,
            CancellationToken cancellationToken, bool saveChanges = false)
        {
            if (entities.Any() == false) throw new LSException(BaseErrorCodes.DataNotFound, errorObjects: new object[] { typeof(TEntity) });

            _dbSet.UpdateRange(entities!);

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return entities;
        }

        /// <inheritdoc />
        public async Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken)
        {
            return await _dbSet.FirstOrDefaultAsync(x => Equals(x!.Id, id), cancellationToken);
        }

        
        /// <inheritdoc />
        public IQueryable<TEntity?> Query()
        {
            return _dbSet.AsNoTracking().AsQueryable();
        }

        /// <inheritdoc />
        public DbContext Context()
        {
            return _context;
        }

        /// <inheritdoc />
        public async Task<TEntity?> FindAsync(TEntity entity, CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync(entity, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<ICollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        {
            return await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<bool> IsEmpty(Expression<Func<TEntity?, bool>> predicate, CancellationToken cancellationToken)
        {
            var res = await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
            return res == null;
        }

        /// <inheritdoc />
        public async Task<TEntity?> DeleteAsync(TEntity entity, CancellationToken cancellationToken,
            bool saveChanges = false)
        {
            var removed = _dbSet.Remove(entity).Entity;

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return removed;
        }

        /// <inheritdoc />
        public async Task<TEntity?> DeleteAsync(TId id, CancellationToken cancellationToken, bool saveChanges = false)
        {
            var entity = await GetAsync(id, cancellationToken);

            var removed = _dbSet.Remove(entity!).Entity;

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return removed;
        }

        /// <inheritdoc />
        public async Task<ICollection<TEntity?>> DeleteMany(ICollection<TEntity?> entities,
            CancellationToken cancellationToken, bool saveChanges = false)
        {
            if (!entities.Any()) throw new LSException(BaseErrorCodes.DataNotFound, errorObjects: new object[] { typeof(TEntity) });

            _dbSet.RemoveRange(entities!);

            if (saveChanges)
                await _context.SaveChangesAsync(saveChanges, cancellationToken).ConfigureAwait(false);

            return entities;
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity?, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken: cancellationToken);
        }

        public async Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity?, bool>> predicate,
            CancellationToken cancellationToken)
        {
            return await _dbSet.SingleOrDefaultAsync(predicate, cancellationToken: cancellationToken);
        }

        #endregion
    }
}