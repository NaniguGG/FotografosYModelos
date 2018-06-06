using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public class Calificacion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int Puntuacion { get; set; }

        public int PropuestaId { get; set; }
        [ForeignKey("PropuestaId")]
        public virtual Propuesta Propuesta { get; set; }

        public String Comentario { get; set; }
    }
}