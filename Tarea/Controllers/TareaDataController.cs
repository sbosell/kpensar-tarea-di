using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using Tarea.Modelos;
using Tarea.Negocio;


namespace Tarea.Web.Controllers
{
    
    public class TareaDataController : ApiController
    {
        public readonly ITareaServicio _tareaServicio;

        public TareaDataController(ITareaServicio tareaServicio)
        {
            _tareaServicio = tareaServicio;
        }


        [HttpGet]
        public HttpResponseMessage Listar()
        {
            var tareas = _tareaServicio.Listar();

            return Request.CreateResponse(tareas);

        }

        [HttpGet]
        public HttpResponseMessage Obtener(int id)
        {
            var tarea = _tareaServicio.Obtener(id);
            return Request.CreateResponse(tarea);
        }

        [HttpPost]
        public HttpResponseMessage Guardar(Tarea.Modelos.Tarea Tarea)
        {
            if (ModelState.IsValid)
            {
                _tareaServicio.Agregar(Tarea);
                return Request.CreateResponse(Tarea);
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, ModelState);
            }
            
        }
        [HttpPost]
        public HttpResponseMessage Eliminar(Tarea.Modelos.Tarea Tarea)
        {
            _tareaServicio.Eliminar(Tarea);
            return Request.CreateResponse(200);
        }
    }
}