using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Infrastructure.Persistence.Repositories
{
    /// <summary>
    /// Factory for creating repository instances.
    /// This factory is responsible for instantiating repositories with specific constructors.
    /// </summary>
    public class RepositoryFactory
    {
        /// <summary>
        /// Creates an instance of a repository for the specified entity type.
        /// </summary>
        /// <typeparam name="TEntity">The entity type that the repository will manage.</typeparam>
        /// <typeparam name="TId">The type of the identifier that the entity uses.</typeparam>
        /// <typeparam name="TRepository">The type of the repository to create.</typeparam>
        /// <param name="args">An array of parameters that will be passed to the repository constructor.</param>
        /// <returns>An instance of <typeparamref name="TRepository"/> or null if the instantiation fails.</returns>
        public static TRepository? GetRepositoryInstance<TEntity, TId, TRepository>(params object[] args)
            where TRepository : Repository<TEntity, TId>
            where TEntity : class, IEntity<TId>
        {
            return (TRepository)Activator.CreateInstance(typeof(TRepository), args)!;
        }
    }
}