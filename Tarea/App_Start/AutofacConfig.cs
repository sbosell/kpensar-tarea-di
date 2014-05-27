using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Core;
using System.Web.Http;
using NPoco;

using Tarea.Negocio;
using Tarea.Datos;

namespace Tarea.Web
{
    public static class IocConfig
    {
        public static void RegisterIoc()
        {
            var builder = new ContainerBuilder();

            // registrar autofac para resolver las dependencías de los controladores
              builder.RegisterControllers(typeof(Tarea.Web.Controllers.TareaController).Assembly);
            // registrar para IDatabase - 1 Instancia por request
            builder.Register(c => new Database("Default"))
                .As<IDatabase>()
                .SingleInstance();
            // registar el servicio

            builder.RegisterType<TareaServicio>().
                As<ITareaServicio>().
                InstancePerRequest();

            builder.RegisterGeneric(typeof(Repository<>)).As(typeof(IRepository<>)).InstancePerRequest();
            builder.RegisterType<Uow>().As<IUnitOfWork>().InstancePerRequest();
            
            // registar para los filters (aun no tenemos)
            builder.RegisterFilterProvider();
            // completar la config de autofac
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            
        }
    }
}