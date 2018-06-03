using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Shootr.Models
{
    public class EditarUsuarioModel
    {
        public Usuario MyUserInfo { get; set; }
        public HttpPostedFileBase File { get; set; }
    }
}