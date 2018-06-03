using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public class CrearPropuestaModel
    {
        public Propuesta Propuesta { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
}