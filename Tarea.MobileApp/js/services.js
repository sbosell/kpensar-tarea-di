var url = 'http://localhost:52643';

angular.module('starter.services', ['ngStorage'])
.factory('tareaService', function ($http, $q, $localStorage)
{
    var srv = {
        getTareas: function (obj)
        {
            var useCache = obj && obj.useCache && $localStorage.hasOwnProperty('listar') ? true : false;

            var d = $q.defer();

            if (useCache)
            {
                d.resolve($localStorage['listar']);
            } else
            {

                $http.get(url + '/api/tareadata/listar').success(function (data)
                {
                    $localStorage['listar'] = data;
                    d.resolve(data);
                }).error(function (data, error)
                {
                    console.log(error);
                    d.reject();
                });

            }

            return d.promise;
        },
        obtener: function (id)
        {
            var d = $q.defer();
            $http.get(url + '/api/tareadata/obtener/' + id).success(function (data)
            {
                d.resolve(data);

            }).error(function (data, error)
            {
                d.reject();
            });
            return d.promise;
        },
        guardar: function (tarea)
        {
            var d = $q.defer();

            $http({
                url: url + '/api/tareadata/guardar',
                method: 'POST',
                data: tarea

            }).success(function (data)
            {
                d.resolve(data);

            });

            return d.promise;
        },
        eliminar: function (tarea)
        {
            var d = $q.defer();

            $http({
                url: url + '/api/tareadata/eliminar',
                method: 'POST',
                data: tarea

            }).success(function (data)
            {
                d.resolve(data);

            });
            return d.promise;
        }
    };

    return srv;
});
