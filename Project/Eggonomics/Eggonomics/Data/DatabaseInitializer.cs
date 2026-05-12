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
            bool isNewDb = Convert.ToInt64(result) == 0;

            await connection.CloseAsync().ConfigureAwait(false);

            if (isNewDb)
            {
                var script = _context.Database.GenerateCreateScript();
                await _context.Database.ExecuteSqlRawAsync(script).ConfigureAwait(false);
            }
            else
            {
                await ApplySchemaUpdatesAsync().ConfigureAwait(false);
            }
        }

        private async Task ApplySchemaUpdatesAsync()
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync().ConfigureAwait(false);
            await using var schemaCommand = connection.CreateCommand();

            schemaCommand.CommandText = @"
                ALTER TABLE ""EggOperations""
                    ADD COLUMN IF NOT EXISTS ""MonetaryValue"" numeric(10,2) NOT NULL DEFAULT 0;

                ALTER TABLE ""EggOperationBatchLinks""
                    ALTER COLUMN ""EggTrayId"" DROP NOT NULL;

                ALTER TABLE ""EggOperationBatchLinks""
                    DROP CONSTRAINT IF EXISTS ""FK_EggOperationBatchLinks_EggTrays_EggTrayId"";

                ALTER TABLE ""EggOperationBatchLinks""
                    ADD CONSTRAINT ""FK_EggOperationBatchLinks_EggTrays_EggTrayId""
                    FOREIGN KEY (""EggTrayId"") REFERENCES ""EggTrays""(""Id"") ON DELETE SET NULL;

                ALTER TABLE ""EggOperations""
                    DROP CONSTRAINT IF EXISTS ""FK_EggOperations_Fridges_FridgeId"";

                ALTER TABLE ""EggOperations""
                    ADD CONSTRAINT ""FK_EggOperations_Fridges_FridgeId""
                    FOREIGN KEY (""FridgeId"") REFERENCES ""Fridges""(""Id"") ON DELETE CASCADE;

                ALTER TABLE ""DebtSettlements""
                    DROP CONSTRAINT IF EXISTS ""FK_DebtSettlements_Fridges_FridgeId"";

                ALTER TABLE ""DebtSettlements""
                    ADD CONSTRAINT ""FK_DebtSettlements_Fridges_FridgeId""
                    FOREIGN KEY (""FridgeId"") REFERENCES ""Fridges""(""Id"") ON DELETE CASCADE;
            ";
            await schemaCommand.ExecuteNonQueryAsync().ConfigureAwait(false);

            await connection.CloseAsync().ConfigureAwait(false);
        }
    }
}
