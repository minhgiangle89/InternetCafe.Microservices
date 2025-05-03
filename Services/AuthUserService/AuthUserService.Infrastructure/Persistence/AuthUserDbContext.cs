using AuthUserService.Application.Interfaces;
using AuthUserService.Domain.Entities;
using InternetCafe.Common.Entities;
using InternetCafe.Common.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace AuthUserService.Infrastructure.Persistence
{
    public class AuthUserDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;
        public AuthUserDbContext(DbContextOptions<AuthUserDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");

                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).UseIdentityColumn();

                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();

                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);
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
