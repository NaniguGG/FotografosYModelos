using Microsoft.AspNet.Identity.EntityFramework;
using Shootr.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Shootr.DataContexts
{
    public class IdentityDb : IdentityDbContext<ApplicationUser>
    {
        public IdentityDb()
            : base("DefaultConnection")
        {
        }

        public static IdentityDb Create()
        {
            return new IdentityDb();
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<FotoPorfolio> FotosPorfolio { get; set; }
        public DbSet<Propuesta> Propuestas { get; set; }
        public DbSet<Postulacion> Postulaciones { get; set; }
        public DbSet<Calificacion> Calificaciones { get; set; }
    }
}