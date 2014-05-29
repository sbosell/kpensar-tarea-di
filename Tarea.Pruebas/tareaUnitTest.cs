using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Moq;
using NPoco;
using Tarea.Datos;
using Tarea.Negocio;
using Tarea.Modelos;

using Tarea.Pruebas.Setup;


namespace Tarea.Pruebas
{
    [TestClass]
    public class tareaUnitTest : TestBase
    {
        private Tarea.Web.Controllers.TareaController _tareaController;
        private  IRepository<Tarea.Modelos.Tarea> _repoTarea;
        private ITareaServicio _tareaServicio;

        private int tamanoList = 0;

        #region Setup
        public void SetupPrueba()
        {
            Init();
            _repoTarea = new Repository<Modelos.Tarea>(_uow);
            _tareaServicio = new TareaServicio(_repoTarea, _uow);

            _tareaController = new Tarea.Web.Controllers.TareaController(_tareaServicio);
            _tareaController.ControllerContext = _controllerContext.Object;
        }
        public tareaUnitTest()
        {
            SetupPrueba();
            tamanoList = _tareaServicio.Listar().Count();

        }
        private T GetValueFromJsonResult<T>(JsonResult jsonResult, string propertyName)
        {
            var property =
                jsonResult.Data.GetType().GetProperties()
                .Where(p => string.Compare(p.Name, propertyName) == 0)
                .FirstOrDefault();

            if (null == property)
                throw new ArgumentException("propertyName not found", "propertyName");
            return (T)property.GetValue(jsonResult.Data, null);
        }
        #endregion

        #region Servicios

        [TestMethod]
        public void ObtenerTarea()
        {
            SetupPrueba();
            var tarea = _tareaServicio.Obtener(11);
            Assert.IsTrue(tarea.Nombre == "Saca la basura");
        }

         [TestMethod]
        public void AgregarTarea()
        {
            SetupPrueba();

            Tarea.Modelos.Tarea t = new Modelos.Tarea();
            t.Nombre = "Haz eso";
            t.Desc = "Ahora pues " + DateTime.Now;
            t.Status = "Incompleta";

            _tareaServicio.Agregar(t);

            var t2 = _tareaServicio.Obtener(t.Id);
            tamanoList++;
            Assert.IsTrue(t.Id == t2.Id && t.Nombre == t2.Nombre && t.Desc == t2.Desc);
        }

         [TestMethod]
         public void borrarTarea()
         {
             SetupPrueba();

             var lista = _tareaServicio.Listar();
             var t = lista.Where(ta => ta.Id > 11).FirstOrDefault();
             _tareaServicio.Eliminar(t);
             var t2 = _tareaServicio.Obtener(t.Id);
             tamanoList--;
             Assert.IsNull(t2);
         }

        [TestMethod]
        public void GetList()
        {
            SetupPrueba();
            List<Tarea.Modelos.Tarea> lista;
            lista = _tareaServicio.Listar().ToList();

            Assert.IsTrue(lista.Count == tamanoList);
        }
        #endregion

        #region Controller

        [TestMethod]
        public void Listar()
        {
            SetupPrueba();
            var listaOriginal = _tareaServicio.Listar().ToList();
            var lista = _tareaController.Listar().ToJson<List<Modelos.Tarea>>();

            Assert.AreEqual(lista.Count, listaOriginal.Count);

            for (int i = 0; i < listaOriginal.Count(); i++)
            {
                Assert.AreEqual(listaOriginal[i].Id, lista[i].Id);
            }
        }

        [TestMethod]
        public void Borrar()
        {
            SetupPrueba();
            var lista = _tareaServicio.Listar();
            var t = lista.Where(ta => ta.Id > 11).FirstOrDefault();
            _tareaController.Borrar(t.Id);
            var t2 = _tareaServicio.Obtener(t.Id);
            tamanoList--;
            Assert.IsNull(t2);
        }

        [TestMethod]
        public void Agregar()
        {
            SetupPrueba();

            Tarea.Modelos.Tarea t = new Modelos.Tarea();
            t.Nombre = "Haz eso";
            t.Desc = "Ahora pues " + DateTime.Now.Ticks;
            t.Status = "Incompleta";

            _tareaController.Agregar(t);

            var t2 = _tareaServicio.Listar().ToList().Where(tt => tt.Desc == t.Desc).First();
            tamanoList++;
            Debug.Print(t.Id.ToString());
            Debug.Print(t2.Id.ToString());
            Debug.Print(t.Nombre);
            Debug.Print(t2.Nombre);
            Debug.Print(t.Desc);
            Debug.Print(t2.Desc);

            Assert.IsTrue(t.Id == t2.Id && t.Nombre == t2.Nombre && t.Desc == t2.Desc);
        }


        #endregion

       

    }
}
