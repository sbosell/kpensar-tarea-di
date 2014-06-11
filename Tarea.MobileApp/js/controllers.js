angular.module('starter.controllers', [])

.controller('MainCtrl', function ($scope, tareaService) {
    $scope.reloadData = true;
    })
.controller('DashCtrl', function ($scope, tareaService)
{
    tareaService.getTareas().then(function (data)
    {
        $scope.tareas = data;
    });
})

.controller('TabTest', function ($scope, $state, $location, $ionicViewService) {

    $scope.onTabSelected = function() {
        //$state.go('tab.tareas');
        $ionicViewService.nextViewOptions({
            disableBack: true
        });
        $state.go(this.uiSref);
        
        }
})

.controller('TareaCtrl', function($scope, tareaService, $state) {
    
    var controllerData = function (fromCache) {
        tareaService.getTareas({ useCache: fromCache}).then(function (data) {
            $scope.tareas = data;
            $scope.$broadcast('scroll.refreshComplete');
        });
    }

    $scope.doRefresh = function () {
        controllerData(false);
    }
    
    if ($scope.reloadData)
    {
        controllerData(false);
        $scope.reloadData = false;
    }

    
    $scope.tareaNuevo = function () {
        $state.go('tab.tareanew');
    }

    $scope.tareaEliminar = function (tarea)
    {
        tareaService.eliminar(tarea).then(function ()
        {
            controllerData(false);

        });
    }

    $scope.deleteMode = false;

    
})
    .controller('TareaNuevoCtrl', function ($scope, tareaService, $location) {

        $scope.tarea = {};

        $scope.tareaNuevo = function () {
            tareaService.guardar($scope.tarea).then(function (data)
            {
                $scope.reloadData = true;
                $location.url('/tareas');
            });
        };

    })
.controller('TareaDetailCtrl', function($scope, $stateParams, tareaService) {
    //$scope.friend = Friends.get($stateParams.friendId);
    tareaService.obtener($stateParams.tareaId).then(function (data)
    {
        $scope.tarea = data;
    });
})

.controller('AccountCtrl', function($scope) {
});
