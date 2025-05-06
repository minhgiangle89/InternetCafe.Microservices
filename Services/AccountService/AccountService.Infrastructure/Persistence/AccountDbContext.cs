using AccountService.Domain.Entities;
using InternetCafe.Common.Entities;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Interfaces;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountService.Infrastructure.Persistence
{
    public class AccountDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public AccountDbContext(DbContextOptions<AccountDbContext> options, ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account configuration
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Accounts");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            });

            // Transaction configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transactions");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();

                entity.Property(e => e.Type).HasConversion<string>();
                entity.Property(e => e.PaymentMethod).HasConversion<string>();

                entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);

                // Relationships
                entity.HasOne(t => t.Account)
                    .WithMany(a => a.Transactions)
                    .HasForeignKey(t => t.AccountId);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            HandleAuditInfo();
            return base.SaveChanges();
        }

        private void HandleAuditInfo()
        {
            var entries = ChangeTracker.Entries<BaseEntity>();
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

            foreach (var entry in entries)
            {
                var now = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.Creation_Timestamp = now;
                        entry.Entity.Creation_EmpId = _currentUserService.UserId ?? 0;
                        entry.Entity.LastUpdated_Timestamp = now;
                        entry.Entity.LastUpdated_EmpId = _currentUserService.UserId ?? 0;
                        entry.Entity.Status = (int)Status.Active;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastUpdated_Timestamp = now;
                        entry.Entity.LastUpdated_EmpId = _currentUserService.UserId ?? 0;

                        entry.Property(x => x.Creation_Timestamp).IsModified = false;
                        entry.Property(x => x.Creation_EmpId).IsModified = false;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.Status = (int)Status.Cancelled;
                        entry.Entity.LastUpdated_Timestamp = now;
                        entry.Entity.LastUpdated_EmpId = _currentUserService.UserId ?? 0;
                        break;
                }
            }
        }
    }
}
