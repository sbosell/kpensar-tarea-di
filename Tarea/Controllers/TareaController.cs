﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Tarea.Modelos;
using Tarea.Negocio;


namespace Tarea.Web.Controllers
{
    public class TareaController : Controller
    {
        public readonly ITareaServicio _tareaServicio;

        public TareaController(ITareaServicio tareaServicio)
        {
            _tareaServicio = tareaServicio;
        }

        public ActionResult Stuff()
        {

            return View();
        }

        public ActionResult Index()
        {
            var tareas = _tareaServicio.Listar();
            TempData.Add("tareas", tareas);
            return View();

        }

        public JsonResult Listar()
        {
            var tareas = _tareaServicio.Listar();
            return Json(tareas, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Obtener(int id)
        {
            var tarea = _tareaServicio.Obtener(id);
            return Json(tarea, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Borrar(int id)
        {
            var tarea = _tareaServicio.Obtener(id);
            _tareaServicio.Eliminar(tarea);
            return RedirectToAction("Listar");
        }

        [HttpPost]
        public ActionResult Agregar(Tarea.Modelos.Tarea tarea)
        {
            _tareaServicio.Agregar(tarea);
            var tareas = _tareaServicio.Listar();
            return RedirectToAction("Listar");
        }
	}
}