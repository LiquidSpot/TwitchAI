namespace TwitchAI.Domain.Entites.Abstract;

/// <summary>
/// Interface for all entities
/// </summary>
public interface IEntity
{ }

/// <summary>
/// Generic interface for all entities
/// </summary>
/// <typeparam name="TId"></typeparam>
public interface IEntity<TId> : IEntity
{
    /// <summary>
    /// Identificator of object
    /// </summary>
    public TId Id { get; set; }

    /// <summary>
    /// Date of creation
    /// </summary>
    public DateTime CreatedAt { get; set; }
}