using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionAnticiposApp.Models;

namespace GestionAnticipos.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Aprobaciones> Aprobaciones { get; set; }
        public DbSet<Contratos> Contratos { get; set; }
        public DbSet<ProcesosVinculados> ProcesosVinculados { get; set; }
        public DbSet<Notificaciones> Notificaciones { get; set; }
        public DbSet<Documentos> Documentos { get; set; }
        public DbSet<Log> Logs { get; set; }
    }
}