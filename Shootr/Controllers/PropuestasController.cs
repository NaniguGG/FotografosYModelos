using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Shootr.DataContexts;
using Shootr.Models;


namespace Shootr.Controllers
{
    [Authorize]
    public class PropuestasController : Controller
    {
        private IdentityDb db = new IdentityDb();

        // GET: Propuestas
        public ActionResult Index()
        {
            var propuestas = db.Propuestas.Include(p => p.Creador);
            return View(propuestas.ToList());
        }

        // GET: Propuestas/Details/5
        public ActionResult Details(int id)
        {
            Propuesta propuesta = db.Propuestas.Find(id);
            if (propuesta == null)
            {
                return HttpNotFound();
            }
            return View(propuesta);
        }


        // GET: Propuestas/Create
        public ActionResult Create()
        {
            try
            {
                var usuarioActual = ObtenerUsuarioActual(User);
                if (usuarioActual != null)
                {
                    return View("Create", new CrearPropuestaModel());
                }

                return RedirectToAction("Index", "Home");

            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Propuestas/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CrearPropuestaModel model)
        {

            if (ModelState.IsValid)
            {

                HttpPostedFileBase file = model.File;

                Propuesta propuesta = model.Propuesta;
                //asignar usuario dueno
                Usuario usuarioActual = ObtenerUsuarioActual(User);

                propuesta.UsuarioId = usuarioActual.Id;
                propuesta.Creador = usuarioActual;
                usuarioActual.Propuestas.Add(propuesta);

                //aca codigo de la imagen
                

                if (file != null && file.ContentLength > 0)
                {
                    Guid guid = Guid.NewGuid();
                    string str = guid.ToString();

                    string path = Path.Combine(Server.MapPath("~/PropuestasPictures"), str + ".jpg");

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

                    propuesta.URLFoto = str;
                    propuesta.Activa = true;

                    db.Propuestas.Add(propuesta);
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }

                ViewBag.UsuarioId = new SelectList(db.Usuarios, "Id", "Nombre", propuesta.UsuarioId);
                return View(propuesta);
            }
            return RedirectToAction("Index");
        }



        public ActionResult VerPostulaciones(int? id) {

            if (id != null) {
                Propuesta propuesta = db.Propuestas.Find(id);
                Usuario usuario = ObtenerUsuarioActual(User);
                if (propuesta.Creador.Id == usuario.Id) {

                    return View("Postulaciones", propuesta);
                }

                return View("Index");
            }
            return View("Index");

        }

       


        // GET: Propuestas/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Propuesta propuesta = db.Propuestas.Find(id);
            if (propuesta == null)
            {
                return HttpNotFound();
            }
            ViewBag.UsuarioId = new SelectList(db.Usuarios, "Id", "Nombre", propuesta.UsuarioId);
            return View(propuesta);
        }

        // POST: Propuestas/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Nombre,Descripccion,FechaCreacion,FechaCierre,URLFoto,UsuarioId,Activa,Lugar")] Propuesta propuesta)
        {
            if (ModelState.IsValid)
            {
                db.Entry(propuesta).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UsuarioId = new SelectList(db.Usuarios, "Id", "Nombre", propuesta.UsuarioId);
            return View(propuesta);
        }

        // GET: Propuestas/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Propuesta propuesta = db.Propuestas.Find(id);
            if (propuesta == null)
            {
                return HttpNotFound();
            }
            return View(propuesta);
        }

        // POST: Propuestas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Propuesta propuesta = db.Propuestas.Find(id);
            db.Propuestas.Remove(propuesta);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public async System.Threading.Tasks.Task<ActionResult> Postularse(int id)
        {

            
            Propuesta propuesta = await db.Propuestas.FindAsync(id);
            Usuario usuario = ObtenerUsuarioActual(User);

            if (usuario.Id != propuesta.Creador.Id)
            {
                if (usuario.Rol != propuesta.Creador.Rol)
                {
                    if (propuesta.Activa == true)
                    {
                        Postulacion postulacion = new Postulacion();
                        postulacion.Propuesta = propuesta;
                        postulacion.PropuestaId = propuesta.Id;
                        postulacion.Usuario = usuario;
                        postulacion.UsuarioId = usuario.Id;

                        propuesta.Postulaciones.Add(postulacion);
                        usuario.Postulaciones.Add(postulacion);


                        db.Entry(propuesta).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }

            int idaux = propuesta.Id;
            return RedirectToAction("Details", new { Id = idaux});
        }

        public async System.Threading.Tasks.Task<ActionResult> AceptarPostulacion(int id) {

            Postulacion postulacion = await db.Postulaciones.FindAsync(id);
            Usuario usuario = ObtenerUsuarioActual(User);

            if (postulacion != null && usuario.Id == postulacion.Propuesta.Creador.Id)
            {
                postulacion.Propuesta.UsuarioGanador = postulacion.Usuario;
                postulacion.Propuesta.UsuarioGanadorId = postulacion.Usuario.Id;
                postulacion.Propuesta.Activa = false;
                db.Entry(postulacion).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }

            //ir al dashboard

            return RedirectToRoute("/Dashboard");
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
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
