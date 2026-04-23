using Microsoft.EntityFrameworkCore;

namespace Eggonomics.Data
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;

        public DatabaseInitializer(AppDbContext context)
        {
            _context = context;
        }

        public async Task InitializeAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync().ConfigureAwait(false);

            await using var command = connection.CreateCommand();
            command.CommandText =
                "SELECT COUNT(*) FROM information_schema.tables " +
                "WHERE table_schema = 'public' AND table_name = 'Users'";
            var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);

            if (Convert.ToInt64(result) == 0)
            {
                var script = _context.Database.GenerateCreateScript();
                await _context.Database.ExecuteSqlRawAsync(script).ConfigureAwait(false);
            }
        }
    }
}
