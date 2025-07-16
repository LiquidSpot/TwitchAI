using Microsoft.EntityFrameworkCore;
using TwitchAI.Application.Constants;
using TwitchAI.Domain.Entites.Abstract;

namespace TwitchAI.Infrastructure.Persistence.Repositories
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasDefaultSchema(Constants.Scheme);

            var typesToRegister = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(Entity)))
                .ToList();

            var viewTypeAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var type in typesToRegister)
            {
                builder.Entity(type);
            }

            foreach (var assembly in viewTypeAssemblies)
            {
                builder.ApplyConfigurationsFromAssembly(assembly);
            }

            base.OnModelCreating(builder);

            //builder.ApplyConfiguration(new TopicEntityConfiguration());
        }
    }
}