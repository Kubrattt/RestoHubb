using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace RestoHub.Models
{
    public partial class RestoranEntities: DbContext
    {
        public override int SaveChanges()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    ((BaseEntity)entry.Entity).OlusturmaTarihi = DateTime.Now;
                    ((BaseEntity)entry.Entity).Aktif = true;
                }

                if (entry.State == EntityState.Modified)
                {
                    ((BaseEntity)entry.Entity).GuncellemeTarihi = DateTime.Now;
                }
            }

            return base.SaveChanges();
        }
    }
}