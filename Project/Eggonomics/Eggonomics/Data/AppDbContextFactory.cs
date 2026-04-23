using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Eggonomics.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(Environment.GetEnvironmentVariable("ConnectionString")
                    ?? throw new InvalidOperationException("Set the ConnectionString environment variable before running dotnet ef commands.")).Options;

            return new AppDbContext(options);
        }
    }
}
