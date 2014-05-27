kpensar-tarea-di
================

El [post] que corresponde esta ubicado [aqui].

Hoy, el tema de este post será sobre [Inyección de dependencias] con ASP.NET y mvc.  Voy a asumir que tienes un nivel intermedio+ en .NET y ya sabes por lo menos algo de MVC (Modelo Vista Controlador).  No voy a cubrir los conceptos de MVC, solamente voy a enfocar en cómo podemos utilizar el concepto de [Inyección de dependencias] para organizar el código y para testearlo después.

<img src="http://t3con12de.typo3.org/_Resources/Persistent/ac4442d3779269a07a2a4df835815da2b0748a7c/oli-dependency-injection.jpg" style='max-width:100%' />

De Wikipedia:  
>Inyección de Dependencias (en inglés Dependency Injection, DI) es un patrón de diseño orientado a objetos, en el que se suministran objetos a una clase en lugar de ser la propia clase quien cree el objeto. El término fue acuñado por primera vez por Martin Fowler.

ASP.NET fue construido para aprovechar de este patrón de desarrollo y vas a ver como te puede ayudar.  Hay una lista grande de librerías de DI/IOC con .NET, algunos de los más populares son:

* Autofac - http://autofac.org/
* NINject - http://www.ninject.org/
* Unity - https://unity.codeplex.com/
* Simple Injector - https://simpleinjector.codeplex.com/
* Castle Windosor - http://www.castleproject.org/projects/windsor/
* Structure Map - http://docs.structuremap.net/
* y [mas] y [otros]

Como religión o política, cada persona tiene su favorito y no quiero discutir cuales mejor, peor, más rápido.  El punto de este post es familiarizarte con el concepto y mostrar como te puede ayudar.  Los dos que yo prefiero son Autofac y Simple Injector porque para mi fue lo más facil implementar pero no dejes de averiguar y experimentar con otros.  Lo bueno de DI es que normalmente es fácil cambiar uno para otro sin afectar tu proyecto.

Hablamos un poquito de una arquitectura típica con capas.  Te enseñan que deberías separar tu proyecto entre diferente capas: Negocio, Acceso de datos, Web, etc.  Mi ejemplo de hoy vamos a usar una arquitectura así:

* Web - Nuestra capa de la página web.  Tiene las vistas, controladores, manejar session, etc.
* Negocio - Capa de negocios no de servicios web.
* Modelos - Contiene las entidades del negocio lo que se usa en toda la solución.
* Acceso de datos - Esta capa vamos a usar una librería de [NPoco]. No va existir en la solución como un proyecto como lo demás.   Ellos implementan un IDatabase y para nuestro ejemplo, nos sirve así.  En el futuro si quieres cambiar NPoco por otro, solamente tendrás que implementar IDatabase.   Si tu proyecto es grande, no sería tan inusual crear una capa distinta o usar un patrón de Repository.  *Después* de escribir esta parte, decidí escribir una capa de acceso de negocios usando un patrón de Repository.  
* Prueba - Siempre es importante testear por lo menos tu capa de negocio.  También puedes testear los controladores pero hoy vamos a dejar este para otro post para que no sea tan largo.

Como [el post] anterior vamos a implementar una solución chiquita de tareas.   De hecho, vamos a usar nuestra app de javascript y vamos a conectarlo al servidor por este proyecto MVC.   Para simplificar, usaremos un controlador normal y en el futuro lo cambiaremos por Web Api (una excusa para realizar otro post).  

## Los requisitos

Para recordarte, los requisitos son:

* Listar tareas
* Agregar tarea
* Eliminar tarea

Hoy vamos a ignorar el usuario (un usuario - tu) y nos enfocamos en cómo podemos organizar un proyecto que soluciona nuestros requisitos.  Podemos extender el proyecto para incluir un usuario, login, etc (futuro).

## Empieza con el bd

Creo que solamente necesitamos una tabla (hasta que agreguemos más funcionalidad):

Tarea: Id (int), Nombre(string), Desc(text), Status (string)

En el futuro me imagino que vamos a querer separar Estatus en otra tabla, y tener una tabla de usuarios, etc. Para  hoy, vamos a listar, agregar, y eliminar.  Fácil no?

    CREATE TABLE [dbo].[Tareas] (
        [Id]     INT           IDENTITY (1, 1) NOT NULL,
        [Nombre] VARCHAR (200) NOT NULL,
        [Desc]   TEXT          NULL,
        [Status] VARCHAR (50)  NOT NULL,
        PRIMARY KEY CLUSTERED ([Id] ASC)
    );

## Capa de Modelos

A mi me gusta NPoco por su simplicidad y es muy [rápido].  Te muestro como usar NPoco con nuestro modelo.  Porque solamente tenemos una tabla, vamos a tener una clase en el proyecto:

    [TableName(“Tarea”)]
    [PrimaryKey(“Id”)]
    public class Tarea {
    	public int Id {get;set;}
    	public string Nombre {get;set;}
    	public string Desc {get;set;}
    	public string Status {get;set;}
    }

No necesitamos más para configurar Npoco con nuestro proyecto.  NPoco lee los atributos y si las columnas de tu base de datos son iguales, los “queries” funcionarán.  También, si quieres cambiar el nombre de una columna pero no quieres tocar tu clase, no hay problema:

    [TableName(“Tarea”)]
    [ExplicitColumns]
    [PrimaryKey(“Id”)]
    public class Tarea {
    	[Column(“Id”)]
    	public int Id {get;set;}
    	[Column(“Name”)]  // este es diferente
    	public string Nombre {get;set;}
    	[Column(“Desc”)]
    	public string Desc {get;set;}
    	[Column(“Status”)]
    	public string Status {get;set;}
    }

Ahora estamos introduciendo una dependencia en NPoco en mi modelo.  Yo se que no voy a cambiar la capa de acceso de datos y para nuestro ejemplo voy a ignorar este.  Si quieres una separación total, probablemente sería mejor hacer tu model un partial o usar una estrategia de mapping entre un modelo o un Repositorio.  Esta clase nos sirve como un POCO (Plain Old CLR Object).  

Los Pocos solamente representa datos y no deberían tener lógica.

## Accesso de Datos - Repository

Vamos a utilizar al patrón de repository para crear otra abstracción entre el acceso de datos y la capa de negocio.  Yo puedo discutir que en muchos casos, este es una pérdida de tiempo aunque también es otra oportunidad de demostrar la inyección de dependencias.   Que necesitamos:

* Unit of Work - que tiene la referencias al base de datos
* Un interface/repo para hacer los queries.  Vamos a hacer algo genérico

### Unit of Work
Es lo que vamos a llamar para crear un commit del bd.  Es un patrón común y me imagino que hayas visto algo parecido trabajando con Entity framework ó nHibernate.

    namespace Tarea.Datos
    {
        public interface IUnitOfWork : IDisposable {
            void Commit();
            IDatabase bD { get; }
        }
    
        public class Uow : IUnitOfWork, IDisposable
        {
            private readonly ITransaction _nTransaction;
            public readonly IDatabase _bd;
            private bool disposed = false;
    
            public Uow(IDatabase db)
            {
                _bd = db;
                _nTransaction = _bd.GetTransaction();
            }
    
            protected virtual void Dispose(bool disposing)
            {
                if (!disposed)
                {
                    if (!disposing)
                    {
                        _nTransaction.Dispose();
                    }
                }
                disposed = true;
            }
    
           public void Dispose()
           {
               Dispose(true);
               GC.SuppressFinalize(this);
           }
    
            public IDatabase bD
            {
                get { return _bd; }
            }
    
            public void Commit()
            {
                _nTransaction.Complete();
            }
        }
    }

### IRepository
Escribimos algunos de los métodos que vamos a necesitar.

    namespace Tarea.Datos
    {
        public interface IRepository<TEntity>
         where TEntity : new()
        {
            int Delete(int primaryKey);
            int Delete(TEntity entity);
            int Execute(string sql, params object[] args);
            System.Collections.Generic.List<TEntity> Fetch();
            System.Collections.Generic.List<TEntity> Fetch(long page, long itemsPerPage, string sql, params object[] args);
            System.Collections.Generic.List<TEntity> Fetch(string sql, params object[] args);
            TEntity FirstOrDefault(NPoco.Sql sql);
            TEntity FirstOrDefault(string sql, params object[] args);
            object Insert(TEntity entity);
            bool IsNew(TEntity entity);
            System.Collections.Generic.IEnumerable<TEntity> Query(string sql, params object[] args);
            TEntity SingleOrDefault(NPoco.Sql sql);
            TEntity SingleOrDefault(object primaryKey);
            TEntity SingleOrDefault(string sql, params object[] args);
            System.Collections.Generic.List<TEntity> SkipTake(long skip, long take, string sql, params object[] args);
            int Update(TEntity entity);
        }
    }
    
Y la implementación.  Nota que aquí tenemos `IUnitOfWork` en el constructor.  En el futuro podemos cambiar la tecnología de NPoco por otra y solamente tenemos que implementar esta interfaz.    

    namespace Tarea.Datos
    {
        public class Repository<TEntity> : Datos.IRepository<TEntity> where TEntity : new()
        {
            
            public Repository(IUnitOfWork uow) {
                Database = uow.bD;
            }
    
            public IDatabase Database;
            
    
            public bool IsNew(TEntity entity) { return Database.IsNew<TEntity>(entity); }
    
            public TEntity SingleOrDefault(object primaryKey) { return Database.SingleOrDefaultById<TEntity>(primaryKey); }
            public TEntity SingleOrDefault(string sql, params object[] args) { return Database.SingleOrDefault<TEntity>(sql, args); }
            public TEntity SingleOrDefault(Sql sql) { return Database.SingleOrDefault<TEntity>(sql); }
            public TEntity FirstOrDefault(string sql, params object[] args) { return Database.FirstOrDefault<TEntity>(sql, args); }
            public TEntity FirstOrDefault(Sql sql) { return Database.FirstOrDefault<TEntity>(sql); }
    
            public List<TEntity> Fetch() { return Database.Fetch<TEntity>(""); }
            public List<TEntity> Fetch(string sql, params object[] args) { return Database.Fetch<TEntity>(sql, args); }
            public List<TEntity> Fetch(long page, long itemsPerPage, string sql, params object[] args) { return Database.Fetch<TEntity>(page, itemsPerPage, sql, args); }
            public List<TEntity> SkipTake(long skip, long take, string sql, params object[] args) { return Database.SkipTake<TEntity>(skip, take, sql, args); }
            public IEnumerable<TEntity> Query(string sql, params object[] args) { return Database.Query<TEntity>(sql, args); }
            public  int Execute(string sql, params object[] args) { return Database.Execute(sql, args); }
    
            public object Insert(TEntity entity)
            {
                return Database.Insert(entity);
            }
    
            public  int Update(TEntity entity)
            {
                return Database.Update(entity);
            }
    
            public  int Delete(TEntity entity)
            {
                return Database.Delete(entity);
            }
    
            public  int Delete(int primaryKey)
            {
                var poco = SingleOrDefault(primaryKey);
                if (poco != null)
                    return Delete(poco);
    
                return 0;
            }
        }
    }

## Negociar conmigo

La capa de negocio va a seguir los requisitos con listar, agregar, eliminar, obtener.  Quiero que notes la interfaz y el constructor. Lo has visto?   Eso es todo para sacar datos de la tabla.  

    public interface ITareaServicio
    {
        IEnumerable<Tarea.Modelos.Tarea> Listar();
        Tarea.Modelos.Tarea Obtener(int id);
        Tarea.Modelos.Tarea Eliminar(Tarea.Modelos.Tarea tarea);
        Tarea.Modelos.Tarea Agregar(Tarea.Modelos.Tarea tarea);
    }

    public class TareaServicio : ITareaServicio
    {
        private readonly IRepository<Tarea.Modelos.Tarea> _tareaRepo;
        private readonly IUnitOfWork _uow;

        public TareaServicio(IRepository<Tarea.Modelos.Tarea> tareaRepo, IUnitOfWork uow)
        {
            _tareaRepo = tareaRepo;
            _uow = uow;
        }
        
        public IEnumerable<Tarea.Modelos.Tarea> Listar() {
            return _tareaRepo.Fetch();
        }

        public Tarea.Modelos.Tarea Obtener(int id)
        {
            return _tareaRepo.SingleOrDefault(id);
        }

        public Tarea.Modelos.Tarea Eliminar(Tarea.Modelos.Tarea tarea) {
            _tareaRepo.Delete(tarea);           
           _uow.Commit();
            return tarea;
        }

        public Tarea.Modelos.Tarea Agregar(Tarea.Modelos.Tarea tarea)
        {
            _tareaRepo.Insert(tarea);
            _uow.Commit();
            return tarea;
        }
    }
}

Tu cabeza explotó?  El constructor lista la dependencia (`IUnitOfWork`) y eso va a funcionar? Comó es que tareaRepo funciona?  Huh?  Qué?  No te creo!  Todo eso pasó por mi mente la primera vez que revisé código así que revísalo otra vez y acuérdate que el post es sobre inyección de dependencias (pasa con el constructor en este ejemplo).  No te preocupes, vamos a conectar todo en un rato.

## Saltar por espacio y tiempo

Por un ratito quiero que saltemos por la parte del es]etup de DI.  

    // registrar para IDatabase - 1 Instancia por request
    builder.Register(c => new Database("Default"))
        .As<IDatabase>()
        .InstancePerRequest();
    // registar el servicio
    builder.RegisterType<TareaServicio>().
        As<ITareaServicio>().
        InstancePerRequest();
    builder.RegisterGeneric(typeof(Repository<>)).
        As(typeof(IRepository<>))
        .InstancePerRequest();
    builder.RegisterType<Uow>().
        As<IUnitOfWork>().
        InstancePerRequest();

Este setup ocurre en el proyecto web y ata todo el sistema juntos por el DI.  De hecho, es algo que tiene sentido después de usarlo una vez y te explico.  `builder` es  un `Container` de Autofac donde vamos a registrar las clases y como son implementados.  En este caso estamos registrando unos tipos, `IDatabase` y `ITareaServicio` y `IRepository` y `IUnitOfWork`.  Cada vez que hay un pedido por uno de ellos, el sistema sabrá que instanciar.   En el lado de `IDatabase`, el sistema va a instanciar `Database` y para `ITareaServicio` el sistema va a instanciar `TareaServicio`.  Ya has visto que `TareaServicio` tiene una dependencia en `IUnitOfWork` y estes registros hacen la magia.

Volvimos al presente

## Los Controladores - Volver del futuro

En .NET MVC, vamos a tener algunos controladores que utilizan las capas de negocios.  Quiero que notes el constructor otra vez:

    namespace Tarea.Web.Controllers
    {
        public class TareaController : Controller
        {
            public readonly ITareaServicio _tareaServicio;
    
            public TareaController(ITareaServicio tareaServicio)
            {
                _tareaServicio = tareaServicio;
            }
    
            public ActionResult Index()
            {
                return View();
            }
    
            public JsonResult Listar()
            {
                var tareas = _tareaServicio.Listar();
                return Json(tareas, JsonRequestBehavior.AllowGet);
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

El constructor tiene una dependencia en `ITareaServicio` y ya sabes que el DI va a vincular eso.  Ojalá que te des cuenta ahora que sería super facil cambiar `TareaServicio` por otra implementación.  Solamente tenemos que implementar su interfaz y funcionará.  El contralor es fácil y hace juego con los requisitos otra vez.  Es muy probable que vayas a tener filters y otro código que puede usar el DI y el proceso es igual.  

Quiero mencionar que inyección de dependencias no es algo nuevo y hay teorías en la mejor manera de hacerlo, anti-paternos, etc.  También cada librería mencionado arriba puede tener funcionalidades distintas.   Por ejemplo, en algunos puedes inyectar dependencias con propiedades (sin constructor) y otros no.  A mi me gusta Autofac y Simple Injector porque no he tenido un caso que no funcionaba y hay bastante ejemplos de la configuración y integraciones como lo de MVC.

## DI - Autofac

El setup de autofac ocurre en el proceso de "Startup" de la app.   El global.asax tiene su setup:

     protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            IocConfig.RegisterIoc();
        }
        
Y el registrar es:

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
    
Parece más complicado de lo que es.  Autofac tiene algunas integraciones para ayudar con el registro de Controladores y Filtros (`builder.RegisterControllers`, `builder.RegisterFilterProvider`).  Para vincular todo con MVC tenemos que cambiar el DependencyResolver.  Esas líneas son estandares con Autofac.  Con el registro de controladores estamos pasando uno de los tipos de controladores para que pueda ubicarlos en el assembly.    Los registros de `IDatabase` y `ITareaServicio` van a instanciarse una vez por cada request.  Eso significa si llamamos 3 acciones en el request, solamente vamos a ver uno `IDatabase` lo que significa que la bd es compartido por el request.  Sin `InstancePerRequest` tendríamos una instancia cada vez que hay una dependencia.   Por ejemplo, si tuvieras un caché lo que debería ser static, tendrías `SingleInstance`.  Hay bastante [documentación].

## Front End - HTML, Javascript y mas

Solamente nos falta la parte front end.  La parte los usuario ven.  Tuve que hacer algunos cambios a nuestra app que estaba en Plunker para que funcione con un servidor real.  De verdad los cambios eran muy rápidos y más solamente por la parte de ajax para completar esa parte.  Puedes revisarlos el el github, solamente voy a mostrar el layout and vista lo que son casi igual que el post anterior.

Layout:

    <!DOCTYPE html>
    <html>
    
    <head>
       
        <script src="http://cdnjs.cloudflare.com/ajax/libs/knockout/3.1.0/knockout-min.js"></script>
        <script src="/content/qwest.js"></script>
        <style>
            /* Styles go here */
            #alert {
                background: #66ff66;
                color: white;
                padding: 20px;
                font-size: 1.5em;
            }
    
            #alert.error {
                background: #FF0000;
            }
    
            .loader {
                display: none;
            }
    
            .loading {
                width: 50px;
                height: 50px;
                background: no-repeat url('http://cdnjs.cloudflare.com/ajax/libs/semantic-ui/0.16.1/images/loader-medium.gif');
                display: block;
                padding-left: 40px;
            }
        </style>
    </head>
    
    <body>
        @RenderBody()
    
        <script src="/content/script.js" defer="defer"></script>
    </body>
    
    </html>

Vista Index:

    @{
        Layout = "~/Views/Tarea/_Layout.cshtml";
    }
    <div id='alert' data-bind='visible: mensaje(), css: { error: isError }' style='display: none'>
        <span data-bind='text: mensaje()'></span><a href='#' data-bind='    click: cerrar'>X</a>
    </div>
    <h1>Lista de Tareas</h1>
    <div class='loader' data-bind="css: { loading: isLoading() }">Cargando</div>
    <table>
        <tr>
            <th>&nbsp;</th>
            <th>Id</th>
            <th>Nombre</th>
            <th>Descripcion</th>
            <th>Status</th>
        </tr>
        <!-- ko foreach: tareas -->
        <tr>
            <td><a href="#" data-bind="click: $parent.borrar">x</a>
    
            </td>
            <td data-bind="text: id()"></td>
            <td data-bind="text: nombre()"></td>
            <td data-bind="text: desc()"></td>
            <td data-bind="text: status()"></td>
    
        </tr>
        <!-- /ko -->
    </table>
    
    <div>
        <fieldset>
            <legend>Nuevo</legend>
            <div>
                <label>Nombre</label>
                <input data-bind="value: tarea().nombre" name="Tarea.Nombre">
            </div>
            <div>
                <label>Desc</label>
                <input data-bind="value: tarea().desc" name="Tarea.Desc">
            </div>
            <div>
                <label>Status</label>
                <input data-bind="value: tarea().status" name="Tarea.Status">
            </div>
    
            <button data-bind='click: agregar'>Agregar</button>
          
        </fieldset>
    </div>

okay, okay, aquí es la parte javascript (script.js): 

    // los services utilizan ajax sin jquery para llamar el servidor
    // este ejemplo solamente estamos cargando un archivo plano con datos
    // los borrar y agregar llaman el tarea.json solamente para simular
    // una llamada al servidor.  En un proyecto real, esos llamarian 
    // un endpoint para borrar o agregar items al bd.  
    
    var services = {
        tareas: function () {
    
            return reqwest({
                url: '/tarea/listar',
                method: 'get',
                type: 'json'
            });
        },
        eliminarTarea: function (id) {
            // una app real llamaria un endpoint
            // nostros vamos a llamar nuestra json
            // para simularlo
    
            return reqwest({
                url: '/tarea/borrar/' + id,
                method: 'post',
                type: 'json'
            });
        },
        agregarTarea: function (tarea) {
            // una app real llamaria un endpoint
            // nostros vamos a llamar nuestra json
            // para simularlo
            var d = {
                tarea: {
                    Nombre: tarea.nombre(),
                    Desc: tarea.desc(),
                    Status: tarea.status()
                }
            };
    
            return reqwest({
                url: '/tarea/agregar/',
                method: 'post',
                contentType: 'application/json',
                type: 'json',
                data: JSON.stringify(d)
            });
        }
    };
    
    function Tarea(id, nombre, desc, status) {
        var o = this;
        o.id = ko.observable(id);
        o.nombre = ko.observable(nombre);
        o.desc = ko.observable(desc);
        o.status = ko.observable(status);
    
        return o;
    
    }
    
    function TareaModel() {
        var self = this;
    
        self.tareas = ko.observableArray([]);
        self.isLoading = ko.observable(false);
        self.tarea = ko.observable(new Tarea(0, '', '', ''));
        self.mensaje = ko.observable();
        self.isError = ko.observable(false);
    
        self.cerrar = function () {
            self.mensaje('');
        }
    
        self.mostrarAlert = function (isError) {
            isError = isError !== undefined ? isError : false;
            self.isLoading(false);
            self.isError(isError);
            if (isError) {
                self.mensaje('Error Error Error!  Llena todas las cajas.');
            } else {
                self.mensaje('Operacion exitosa');
            }
    
        }
    
        self.getTareas = function () {
            self.isLoading(true);
            services.tareas().then(function (data) {
                self.tareas([]);
                for (var i = 0, l = data.length; i < l; i++) {
                    var row = data[i];
                    self.tareas.push(new Tarea(row.Id, row.Nombre, row.Desc, row.Status))
                }
                self.isLoading(false);
            });
        };
    
        self.borrar = function (tarea) {
            self.isLoading(true);
            services.eliminarTarea(tarea.id()).then(function (data) {
                self.getTareas();
            });
    
        };
    
        self.agregarConError = function () {
            self.isLoading(true);
            setTimeout(function () {
                self.mostrarAlert(true);
            }, 2000);
    
        };
    
        self.agregar = function () {
            self.isLoading(true);
    
            services.agregarTarea(self.tarea()).then(function () {
                self.tarea(new Tarea(0, '', '', ''));
                self.getTareas();
                self.mostrarAlert();
    
            }).fail(function (err, msg) {
                self.mostrarAlert(true);
            });
    
        };
    
        self.init = function () {
            self.getTareas();
        }
    }
    var tm = new TareaModel();
    tm.init();
    ko.applyBindings(tm);

## Fin

Hoy vimos como implementar inyección de dependencias con .NET MVC.  Te ayuda en organizar tu proyecto, probar código,  y en hacer cambios de la arquitectura en el futuro.   Ojalá que en tu próximo proyecto tengas el chance de utilizarlo y que te sirva bien.  El próximo post vamos a agregar un proyecto de pruebas para probar la capa de negocios y cambiar el controlador para Web Api.

Todo el código fuente incluyendo la bd esta en [github].  

Notas:
* Casi nunca vamos a cambiar la capa de acceso de datos.  Tenemos que pensar bien si necesitamos usar un Repository o si podemos inyectar IDatabase en la capa de negocios.  Es más rápido (aunque rompe la separación).
* Cambié qwest para [Reqwest] porque no funcionó con los model binders de .NET.  Me demoré 2 minutos en hacer el cambio.
* Web api sería mucho más útil para las partes ajax.  En el futuro te muestro Web Api con el DI.
* El proyecto en GitHub es ASP.NET MVC 5 con NPoco y Autofac.  Utiliza un bd local (app_data).  

[documentación]:https://github.com/autofac/Autofac/wiki/Instance-Scope
[NPoco T4]:https://github.com/Rhysling/NPoco.T4
[Reqwest]:https://github.com/ded/Reqwest
[Inyección de dependencias]:http://es.wikipedia.org/wiki/Inyecci%C3%B3n_de_dependencias
[mas]:http://www.hanselman.com/blog/ListOfNETDependencyInjectionContainersIOC.aspx
[otros]:https://github.com/danielpalme/IocPerformance
[NPoco]:https://github.com/schotime/NPoco
[rápido]: https://github.com/StackExchange/dapper-dot-net#performance-of-select-mapping-over-500-iterations---poco-serialization
[el post]:http://kpensar.com/blog/2014/05/26/ajax-sin-jquery-un-ejemplo-con-knockout-y-qwest/
[post]:http://kpensar.com/blog/2014/05/26/ajax-sin-jquery-un-ejemplo-con-knockout-y-qwest/
[aqui]:http://kpensar.com/blog/2014/05/26/ajax-sin-jquery-un-ejemplo-con-knockout-y-qwest/
[github]:https://github.com/sbosell/kpensar-tarea-di




