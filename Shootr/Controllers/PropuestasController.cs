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

        //// GET: Propuestas
        //public ActionResult Index()
        //{
        //    var propuestas = db.Propuestas.Include(p => p.Creador);
        //    return View(propuestas.ToList());
        //}

        // GET: Publications
        public ActionResult Index( string searchString)
        {

            var propuestas = from s in db.Propuestas select s;

            propuestas = propuestas.Where(x => x.Activa == true);

            if (!String.IsNullOrEmpty(searchString))
            {
                propuestas = propuestas.Where(s => s.Nombre.Contains(searchString) || s.Creador.UserName.Contains(searchString) || s.Lugar.Contains(searchString));
            }

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


        public ActionResult Calificar(int? id)
        {
            if (id != null)
            {
                Propuesta propuesta = db.Propuestas.Find(id);
                Usuario usuario = ObtenerUsuarioActual(User);
                if (propuesta.Creador.Id == usuario.Id || propuesta.UsuarioGanador.Id == usuario.Id)
                {
                    Calificacion calificacion = new Calificacion();
                    calificacion.Propuesta = propuesta;
                    calificacion.PropuestaId = propuesta.Id;
                    return View("Calificar", calificacion);
                }

                return View("Index");
            }
            return View("Index");
        }



        //[Route("Usuario/AgregarFoto")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calificar(Calificacion modelo)
        {
            if (ModelState.IsValid)
            {
                Propuesta propuesta = db.Propuestas.Find(modelo.PropuestaId);
                Usuario usuarioActual = ObtenerUsuarioActual(User);

                //modelo.Propuesta = propuesta;
                //modelo.PropuestaId = propuesta.Id;

                db.Calificaciones.Add(modelo);
                db.SaveChanges();


                if (usuarioActual.Id == propuesta.Creador.Id)
                {
                    //soy el dueno
                    propuesta.CalificacionCreador = modelo;
                    propuesta.CalificacionCreadorId = modelo.Id;

                }
                else if (usuarioActual.Id == propuesta.UsuarioGanador.Id)
                {
                    //soy el ganador
                    propuesta.CalificacionGanador = modelo;
                    propuesta.CalificacionGanadorId = modelo.Id;
                }

                modelo.PropuestaId = propuesta.Id;

                db.Entry(propuesta).State = EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
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

        //[Route("Folder/PostComment/{Id}")]
        //[HttpPost, ActionName("PostComment")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostComment(int id, string NewComment)
        {
            try
            {
                if (String.Empty != NewComment)
                {
                    Propuesta propuesta = db.Propuestas.Find(id);

                    Usuario usuarioActual = ObtenerUsuarioActual(User);

                    //si la propuesta no esta ya cerrada
                    if (propuesta.Activa == true) {
                        //si no soy el dueno
                        if (propuesta.Creador.Id != usuarioActual.Id) {

                            Pregunta pregunta = new Pregunta();

                            pregunta.AskerId = usuarioActual.Id;

                            pregunta.PropuestaId = propuesta.Id;

                            pregunta.QuestionString = NewComment;
                            pregunta.QuestionRead = false;
                            pregunta.QuestionRead = false;
                            pregunta.AnswerRead = false;

                            db.Preguntas.Add(pregunta);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
                return RedirectToAction("Details", new { Id = id });
            }
            return RedirectToAction("Details", new { Id = id });
        }



        public ActionResult PostAnswer(int id, string NewComment)
        {
            Pregunta pregunta = db.Preguntas.Where(x => x.Id == id).First();
            Propuesta propuesta = pregunta.Propuesta;
            try
            {
                if (String.Empty != NewComment)
                {
                    
                    Usuario usuarioActual = ObtenerUsuarioActual(User);

                    //si la propuesta no esta ya cerrada
                    if (propuesta.Activa == true)
                    {
                        //si no soy el dueno
                        if (propuesta.Creador.Id == usuarioActual.Id)
                        {

                            if (usuarioActual.Id == propuesta.Creador.Id)
                            {
                                pregunta.AnswerString = NewComment;

                                db.SaveChanges();
                            }

                            db.SaveChanges();
                        }
                    }
                }
            }
            catch
            {
                return RedirectToAction("Details", new { Id = propuesta.Id });
            }
            return RedirectToAction("Details", new { Id = propuesta.Id });

        }



    }
}
