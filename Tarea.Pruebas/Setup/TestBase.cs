using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Web;
using System.Web.Mvc;

using Moq;
using NPoco;
using Tarea.Datos;
using Tarea.Negocio;
using Tarea.Modelos;

namespace Tarea.Pruebas.Setup
{
    public static class WebExtensions
    {
        public static T ToJson<T>(this JsonResult actionResult)
        {
            var jsonResult = (JsonResult)actionResult;

            return (T)jsonResult.Data;
        }
    }

    [TestClass]
    public abstract class TestBase
    {
        protected IUnitOfWork _uow;
        protected IDatabase _bd;
        protected Mock<HttpContextBase> _fakeHttpContext;
        protected Mock<ControllerContext> _controllerContext;

        [AssemblyInitializeAttribute]
        public static void Initialize(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\App_Data")));
        }

        public void Init()
        {
            _bd = new Database("Default");
            _uow = new Uow(_bd);
            _fakeHttpContext = new Mock<HttpContextBase>();
            _controllerContext = new Mock<ControllerContext>();
            _controllerContext.Setup(t => t.HttpContext).Returns(_fakeHttpContext.Object);
        }
    }
}
