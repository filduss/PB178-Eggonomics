using Eggonomics.Models;
using Microsoft.EntityFrameworkCore;

namespace Eggonomics.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Fridge> Fridges => Set<Fridge>();
        public DbSet<FridgeUser> FridgeUsers => Set<FridgeUser>();
        public DbSet<EggTray> EggTrays => Set<EggTray>();
        public DbSet<EggOperation> EggOperations => Set<EggOperation>();
        public DbSet<EggOperationBatchLink> EggOperationBatchLinks => Set<EggOperationBatchLink>();
        public DbSet<DebtSettlement> DebtSettlements => Set<DebtSettlement>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // FridgeUser unique constraint — one membership record per user per fridge
            modelBuilder.Entity<FridgeUser>()
                .HasIndex(fu => new { fu.FridgeId, fu.UserId })
                .IsUnique();

            // Fridge → Owner
            modelBuilder.Entity<Fridge>()
                .HasOne(f => f.Owner)
                .WithMany()
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Fridge → Members
            modelBuilder.Entity<FridgeUser>()
                .HasOne(fu => fu.Fridge)
                .WithMany(f => f.Members)
                .HasForeignKey(fu => fu.FridgeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FridgeUser>()
                .HasOne(fu => fu.User)
                .WithMany()
                .HasForeignKey(fu => fu.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EggTray → Fridge
            modelBuilder.Entity<EggTray>()
                .HasOne(et => et.Fridge)
                .WithMany(f => f.EggTrays)
                .HasForeignKey(et => et.FridgeId)
                .OnDelete(DeleteBehavior.Cascade);

            // EggTray → AddedBy
            modelBuilder.Entity<EggTray>()
                .HasOne(et => et.AddedBy)
                .WithMany()
                .HasForeignKey(et => et.AddedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EggOperation → Fridge
            modelBuilder.Entity<EggOperation>()
                .HasOne(eo => eo.Fridge)
                .WithMany()
                .HasForeignKey(eo => eo.FridgeId)
                .OnDelete(DeleteBehavior.Cascade);

            // EggOperation → FridgeUser
            modelBuilder.Entity<EggOperation>()
                .HasOne(eo => eo.FridgeUser)
                .WithMany()
                .HasForeignKey(eo => eo.FridgeUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EggOperation → EditedBy (optional)
            modelBuilder.Entity<EggOperation>()
                .HasOne(eo => eo.EditedBy)
                .WithMany()
                .HasForeignKey(eo => eo.EditedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // EggOperationBatchLink → EggOperation
            modelBuilder.Entity<EggOperationBatchLink>()
                .HasOne(link => link.EggOperation)
                .WithMany(eo => eo.BatchLinks)
                .HasForeignKey(link => link.EggOperationId)
                .OnDelete(DeleteBehavior.Cascade);

            // EggOperationBatchLink → EggTray (nullable — tray may be deleted after link is written)
            modelBuilder.Entity<EggOperationBatchLink>()
                .HasOne(link => link.EggTray)
                .WithMany()
                .HasForeignKey(link => link.EggTrayId)
                .OnDelete(DeleteBehavior.SetNull);

            // DebtSettlement → Fridge
            modelBuilder.Entity<DebtSettlement>()
                .HasOne(ds => ds.Fridge)
                .WithMany()
                .HasForeignKey(ds => ds.FridgeId)
                .OnDelete(DeleteBehavior.Cascade);

            // DebtSettlement → Payer / Receiver (two FKs to User)
            modelBuilder.Entity<DebtSettlement>()
                .HasOne(ds => ds.Payer)
                .WithMany()
                .HasForeignKey(ds => ds.PayerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DebtSettlement>()
                .HasOne(ds => ds.Receiver)
                .WithMany()
                .HasForeignKey(ds => ds.ReceiverUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Decimal precision for monetary fields (numeric is PostgreSQL native)
            modelBuilder.Entity<EggTray>()
                .Property(et => et.Price)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<EggTray>()
                .Property(et => et.PricePerEgg)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<DebtSettlement>()
                .Property(ds => ds.Amount)
                .HasColumnType("numeric(10,2)");

            modelBuilder.Entity<EggOperation>()
                .Property(eo => eo.MonetaryValue)
                .HasColumnType("numeric(10,2)");
        }
    }
}
