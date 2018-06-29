using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shootr.DataContexts;
using Shootr.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Shootr.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private IdentityDb db = new IdentityDb();

        public UsuarioController()
        {
           
        }

        // GET: Usuario
        public ActionResult Index()
        {
            return View();
        }


        // GET: Usuario/Details/5

        [Route("Usuario/{username}")]
        [Route("Usuario/")]
        public ActionResult Details(string username)
        {
            try
            {
                if (!String.IsNullOrEmpty(username))
                {
                    var usuario = Obtener(username);
                    if (usuario != null)
                    {
                        return View("Usuario", usuario);
                    }
                }

                var usuarioActual = ObtenerUsuarioActual(User);
                if (usuarioActual != null)
                {
                    return View("Usuario", usuarioActual);
                }

                return View();
            }
            catch {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("Dashboard")]
        public ActionResult Dashboard()
        {
            try
            {
                var usuarioActual = ObtenerUsuarioActual(User);
                if (usuarioActual != null)
                {
                    return View("Dashboard", usuarioActual);
                }

                return View();
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [Route("Usuario/Editar")]
        [Authorize]
        public ActionResult ModificarUsuario() {
            try
            {
                var usuarioActual = ObtenerUsuarioActual(User);
                if (usuarioActual != null)
                {
                    return View("UsuarioEditar", new EditarUsuarioModel { MyUserInfo = usuarioActual });
                }

                return RedirectToAction("Index", "Home");

            }
            catch {
                return RedirectToAction("Index", "Home");
            }
        }



        [Route("Usuario/Editar")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ModificarUsuario(EditarUsuarioModel modelo)
        {

            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = modelo.File;
                Usuario userInfo = modelo.MyUserInfo;

                if(userInfo.Fotos == null)
                {
                    userInfo.Fotos = new List<FotoPorfolio>();
                }
                

                if (file != null && file.ContentLength > 0)
                {
                    Guid guid = Guid.NewGuid();
                    string str = guid.ToString();

                    string path = Path.Combine(Server.MapPath("~/ProfilePictures"), str + ".jpg");

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                        {
                            Size size = new Size(200    , 200);
                            ISupportedImageFormat format = new JpegFormat { Quality = 80 };
                            imageFactory.Load(file.InputStream)
                                        .Constrain(size)
                                        .BackgroundColor(Color.White)
                                        .Format(format)
                                        .Save(path);
                        }
                    }

                    userInfo.URLFotoPerfil = str;

                }
                Usuario usuarioActual = ObtenerUsuarioActual(User);

                //copiar una por una las propiedades al usuario actual
                //usuarioActual.Apellido = userInfo.Apellido;
                usuarioActual.Descripcion = userInfo.Descripcion;
                usuarioActual.Facebook = userInfo.Facebook;
                usuarioActual.Instagram = userInfo.Instagram;
                //usuarioActual.Nombre = userInfo.Nombre;
                usuarioActual.Rol = userInfo.Rol;
                usuarioActual.Telefono = userInfo.Telefono;
                usuarioActual.URLFotoPerfil = userInfo.URLFotoPerfil;
                usuarioActual.Website = userInfo.Website;

                db.Entry(usuarioActual).State = EntityState.Modified;
                db.SaveChanges();

                return View("Usuario", ObtenerUsuarioActual(User));

            }
            return View(modelo);
        }



        [Route("Usuario/AgregarFoto")]
        [Authorize]
        public ActionResult AgregarFotoPorfolio()
        {
            try
            {
                var usuarioActual = ObtenerUsuarioActual(User);
                if (usuarioActual != null)
                {
                    return View("UsuarioAgregarFoto", new SubirFotoModel() );
                }

                return RedirectToAction("Index", "Home");

            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }



        [Route("Usuario/AgregarFoto")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarFotoPorfolio(EditarUsuarioModel modelo)
        {

            if (ModelState.IsValid)
            {
                HttpPostedFileBase file = modelo.File;

                Usuario usuarioActual = ObtenerUsuarioActual(User);

                if (file != null && file.ContentLength > 0)
                {
                    Guid guid = Guid.NewGuid();
                    string str = guid.ToString();

                    string path = Path.Combine(Server.MapPath("~/PorfoliosPictures"), str + ".jpg");

                    using (MemoryStream outStream = new MemoryStream())
                    {
                        using (ImageFactory imageFactory = new ImageFactory(preserveExifData: false))
                        {
                            Size size = new Size(1000, 1000);
                            ISupportedImageFormat format = new JpegFormat { Quality = 80 };
                            imageFactory.Load(file.InputStream)
                                        .Constrain(size)
                                        .BackgroundColor(Color.White)
                                        .Format(format)
                                        .Save(path);
                        }
                    }

                    
                    //crear nueva foto porfolio
                    FotoPorfolio foto = new FotoPorfolio();
                    foto.Usuario = usuarioActual;
                    foto.UsuarioId = usuarioActual.Id;
                    foto.Url = str;

                    //agregarla a la lista
                    //if (usuarioActual.Fotos == null)
                    //{
                       // usuarioActual.Fotos = new List<FotoPorfolio>();
                    //}
                    usuarioActual.Fotos.Add(foto);

                    //db.Usuarios.Add(usuarioActual);
                    //db.SaveChanges();

                    db.FotosPorfolio.Add(foto);
                    db.SaveChangesAsync();


                    return RedirectToAction("Index");

                    //return View("Usuario", usuarioActual);
                }
                

            }
            return RedirectToAction("Index");
        }




        public Usuario Obtener(string username)
        {
            return db.Usuarios.Where(x => x.UserName == username).First();
        }

        internal Usuario ObtenerUsuarioActual(IPrincipal User)
        {
            var currentUserId = User.Identity.GetUserId();
            var manager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(db));
            var currentUser = manager.FindById(User.Identity.GetUserId());
            return currentUser.MyUserInfo;
        }
    }
}
