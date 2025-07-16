using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Application.Interfaces.Infrastructure
{
    public interface IRepository<TEntity, TId> where TEntity : IEntity<TId>
    {
        /// <summary>
        /// Adds an entity to the database context and optionally saves changes.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The added entity.</returns>
        Task<TEntity?> AddAsync(TEntity? entity, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Adds multiple entities to the database context and optionally saves changes.
        /// </summary>
        /// <param name="entities">The collection of entities to add.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The collection of added entities.</returns>
        Task<ICollection<TEntity?>> AddManyAsync(ICollection<TEntity?> entities, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Updates an entity in the database context and optionally saves changes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The updated entity.</returns>
        Task<TEntity?> Update(TEntity? entity, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Updates multiple entities in the database context and optionally saves changes.
        /// </summary>
        /// <param name="entities">The collection of entities to update.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The collection of updated entities.</returns>
        Task<ICollection<TEntity?>> UpdateRange(ICollection<TEntity?> entities, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Retrieves an entity by its identifier from the database context.
        /// </summary>
        /// <param name="id">The identifier of the entity.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>The entity found, or null if no entity is found.</returns>
        Task<TEntity?> GetAsync(TId id, CancellationToken cancellationToken);

        /// <summary>
        /// Finds an entity in the database context based on a predicate.
        /// </summary>
        /// <param name="entity">The entity to find.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns></returns>
        Task<TEntity?> FindAsync(TEntity entity, CancellationToken cancellationToken);

        /// <summary>
        /// Finds an entity in the database context based on a predicate.
        /// </summary>
        /// <param name="predicate">A condition to find the entity.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>A collection of entities matching the predicate.</returns>
        Task<ICollection<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

        /// <summary>
        /// Provides an IQueryable interface to query the entities of type TEntity from the underlying data source.
        /// </summary>
        /// <returns>An IQueryable of type TEntity that allows for building and executing queries against the entities.</returns>
        IQueryable<TEntity?> Query();

        /// <summary>
        /// Provides access to the application database context.
        /// </summary>
        /// <returns>The instance of ApplicationDbContext which can be used to perform database operations directly.</returns>
        DbContext Context();

        /// <summary>
        /// Deletes an entity from the database context and optionally saves changes.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The deleted entity.</returns>
        Task<TEntity?> DeleteAsync(TEntity entity, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Deletes an entity from the database context and optionally saves changes.
        /// </summary>
        /// <param name="id">The entity Id to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns></returns>
        public Task<TEntity?> DeleteAsync(TId id, CancellationToken cancellationToken, bool saveChanges = false);

        /// <summary>
        /// Deletes multiple entities from the database context and optionally saves changes.
        /// </summary>
        /// <param name="entities">The collection of entities to delete.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <param name="saveChanges">True to save changes immediately, false otherwise.</param>
        /// <returns>The collection of deleted entities.</returns>
        Task<ICollection<TEntity?>> DeleteMany(ICollection<TEntity?> entities, CancellationToken cancellationToken, bool saveChanges = false);

        Task<bool> IsEmpty(Expression<Func<TEntity?, bool>> predicate, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if any entity satisfies a condition.
        /// </summary>
        /// <param name="predicate">A condition to check against the entities.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>True if any entity satisfies the condition, false otherwise.</returns>
        Task<bool> AnyAsync(Expression<Func<TEntity?, bool>> predicate, CancellationToken cancellationToken);

        /// <summary>
        /// Finds a single entity that satisfies a condition or returns null if none is found. Throws an exception if more than one entity is found.
        /// </summary>
        /// <param name="predicate">A condition to find the entity.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the request.</param>
        /// <returns>The single entity found, or null.</returns>
        Task<TEntity?> SingleOrDefaultAsync(Expression<Func<TEntity?, bool>> predicate, CancellationToken cancellationToken);
    }
}
