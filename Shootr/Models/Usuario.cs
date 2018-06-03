using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public String Nombre { get; set; }
        public String Apellido { get; set; }
        public int Telefono { get; set; }
        [DataType(DataType.MultilineText)]
        public String Descripcion { get; set; }
        public String Facebook { get; set; }
        public String Instagram { get; set; }
        public String Website { get; set; }
        public string URLFotoPerfil { get; set; }

        public virtual ICollection<FotoPorfolio> Fotos { get; set; }
        public Rol Rol { get; set; }
        public String UserName { get; set; }
        public String Email { get; set; }

        [InverseProperty("Creador")]
        public virtual ICollection<Propuesta> Propuestas { get; set; }

        [InverseProperty("Usuario")]
        public virtual ICollection<Postulacion> Postulaciones { get; set; }
    }

}