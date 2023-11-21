using Microsoft.EntityFrameworkCore;

namespace SCOP_AppWeb.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<OrdenProduccion> OrdenProduccion { get; set; }
        public DbSet<Requisiciones> Requisiciones { get; set; }
        public DbSet<Devoluciones> Devoluciones { get; set; }
        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<RegistroAuditoria> RegistroAuditoria { get; set; }
    }
}
