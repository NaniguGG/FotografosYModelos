using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public enum Categoria
    {
        Moda, Retrato, Cosplay, Desnudo
    }

    public class Propuesta
    {
        public int Id { get; set; }
        public String Nombre { get; set; }
        [DataType(DataType.MultilineText)]
        public String Descripccion { get; set; }
        public String URLFoto { get; set; }

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public virtual Usuario Creador { get; set; }

        public bool Activa { get; set; }
        public String Lugar { get; set; }

        public Categoria Categoria { get; set; }

        [InverseProperty("Propuesta")]
        public virtual ICollection<Postulacion> Postulaciones { get; set; }


        public int? UsuarioGanadorId { get; set; }
        [ForeignKey("UsuarioGanadorId")]
        public virtual Usuario UsuarioGanador { get; set; }

        public int? CalificacionCreadorId { get; set; }
        [ForeignKey("CalificacionCreadorId")]
        public virtual Calificacion CalificacionCreador { get; set; }

        public int? CalificacionGanadorId { get; set; }
        [ForeignKey("CalificacionGanadorId")]
        public virtual Calificacion CalificacionGanador { get; set; }

        [InverseProperty("Propuesta")]
        public virtual ICollection<Pregunta> Preguntas { get; set; }
    }
}