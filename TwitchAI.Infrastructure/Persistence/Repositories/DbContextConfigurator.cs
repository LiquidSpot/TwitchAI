using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using TwitchAI.Application;
using TwitchAI.Application.Constants;

namespace TwitchAI.Infrastructure.Persistence.Repositories
{
    public partial class DbContextConfigurator
    {
        public static void ConfigureApplicationDbContext(DbContextOptionsBuilder<ApplicationDbContext>? builder, IConfiguration config)
        {
            var connectionString = config.GetConnectionString(Constants.ConnectionString);
            if (builder != null)
                builder.UseNpgsql(connectionString,
                                  x => x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name)
                                        .MigrationsHistoryTable("__EFMigrationsHistory", Constants.Scheme));
        }
    }
}
