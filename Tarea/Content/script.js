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
        if ('tareas' in pageVar) {
            self.tareas([]);
            for (var i = 0, l = tareas.length; i < l; i++) {
                var row = tareas[i];
                self.tareas.push(new Tarea(row.Id, row.Nombre, row.Desc, row.Status))
            }
            self.isLoading(false);

        } else {


            services.tareas().then(function (data) {
                self.tareas([]);
                for (var i = 0, l = data.length; i < l; i++) {
                    var row = data[i];
                    self.tareas.push(new Tarea(row.Id, row.Nombre, row.Desc, row.Status))
                }
                self.isLoading(false);
            });
        }
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

console.log(window.pageVars);





