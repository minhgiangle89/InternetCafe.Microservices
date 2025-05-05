using ComputerSessionService.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComputerSessionService.Infrastructure.Persistence.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> entity)
        {
            entity.ToTable("Sessions");

            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).UseIdentityColumn();

            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime);

            entity.Property(e => e.TotalCost).HasColumnType("decimal(18,2)").HasDefaultValue(0);

            entity.Property(e => e.Status).HasConversion<string>();

            entity.Property(e => e.Notes).HasMaxLength(500);
        }
    }
}
