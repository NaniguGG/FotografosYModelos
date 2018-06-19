using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public class Pregunta
    {
        public int Id { get; set; }

        public int? PropuestaId { get; set; }
        [ForeignKey("PropuestaId")]
        public virtual Propuesta Propuesta { get; set; }

        public int AskerId { get; set; }
        [ForeignKey("AskerId")]
        public virtual Usuario Asker { get; set; }

        //public DateTime QuestionDate { get; set; }
        public String QuestionString { get; set; }


        //public DateTime? AnswerDate { get; set; }
        public String AnswerString { get; set; }

        public bool AnswerRead { get; set; }
        public bool QuestionRead { get; set; }
    }
}