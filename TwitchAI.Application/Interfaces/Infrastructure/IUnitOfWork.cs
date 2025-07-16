using Microsoft.EntityFrameworkCore.Storage;

using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Application.Interfaces.Infrastructure
{
    /// <summary>
    /// Provides an abstraction for managing transactions and saving changes to the database.
    /// </summary>
    public interface IUnitOfWork
    {
        /// <summary>
        /// Provides a factory method to create or retrieve a repository instance for a specific entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity that the repository will manage. This type must implement <see cref="IEntity"/>.</typeparam>
        /// <typeparam name="TId">The type of the identifier used by <typeparamref name="TEntity"/>.</typeparam>
        /// <returns>A repository instance that can be used to perform operations on entities of type <typeparamref name="TEntity"/>. Returns <c>null</c> if the repository could not be created.</returns>
        IRepository<TEntity, TId>? Factory<TEntity, TId>() where TEntity : class, IEntity<TId>;

        /// <summary>
        /// Asynchronously saves all changes made in this context to the database.
        /// </summary>
        /// <param name="saveChanges">A boolean flag indicating whether changes should be saved immediately.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous save operation.</returns>
        Task SaveChangesAsync(CancellationToken cancellationToken = default, bool saveChanges = true);

        /// <summary>
        /// Starts a new transaction on the database.
        /// </summary>
        /// <param name="saveChanges">A boolean flag indicating whether changes should be saved before starting the transaction.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that results in a database transaction.</returns>
        Task<IDbContextTransaction?> BeginTransaction(CancellationToken cancellationToken, bool saveChanges = true);

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        /// <param name="saveChanges">A boolean flag indicating whether changes should be saved as part of committing the transaction.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous commit operation.</returns>
        Task CommitTransactionAsync(CancellationToken cancellationToken, bool saveChanges = true);

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        /// <param name="saveChanges">A boolean flag indicating whether changes should be saved after rolling back the transaction.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous rollback operation.</returns>
        Task RollbackTransactionAsync(CancellationToken cancellationToken, bool saveChanges = true);

    }
}
