namespace TwitchAI.Domain.Entites.Abstract
{
    /// <summary>
    /// Entity for all entities
    /// </summary>
    public class Entity : IEntity<Guid>
    {
        /// <summary>
        /// Identificator of object
        /// </summary>
        public Guid Id{ get; set; }

        /// <summary>
        /// Date of creation
        /// </summary>
        public DateTime CreatedAt{ get; set; } = DateTime.UtcNow;
    }
}
