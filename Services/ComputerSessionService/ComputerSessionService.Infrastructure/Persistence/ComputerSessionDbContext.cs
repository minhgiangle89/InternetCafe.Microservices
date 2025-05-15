using ComputerSessionService.Domain.Entities;
using InternetCafe.Common.Entities;
using InternetCafe.Common.Enums;
using InternetCafe.Common.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace ComputerSessionService.Infrastructure.Persistence
{
    public class ComputerSessionDbContext : DbContext
    {
        private readonly ICurrentUserService _currentUserService;

        public ComputerSessionDbContext(DbContextOptions<ComputerSessionDbContext> options, ICurrentUserService currentUserService) : base(options)
        {
            _currentUserService = currentUserService;
        }

        public DbSet<Computer> Computers { get; set; }
        public DbSet<Session> Sessions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ComputerSessionDbContext).Assembly);
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
            var now = TimeZoneInfo.ConvertTime(DateTime.Now, vietnamTimeZone);
            foreach (var entry in entries)
            {
                if (entry.Entity is BaseEntity baseEntity)
                {
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

}

