(function () {
    'use strict';

    angular
		 .module('adminApp')
		 .controller('nodeDetailsController', nodeDetailsController, ['tokenHeaderService']);

    function nodeDetailsController($http, $routeParams, tokenHeaderService) {
        /* jshint validthis:true */
        var vm = this;
        vm.NodeId = $routeParams.nodeId;
        $http.get("./Stardust/WorkerNode/" + vm.NodeId, tokenHeaderService.getHeaders()).success(function (data) {
            vm.Node = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        });

        $http.get("./Stardust/JobsByNode/" + vm.NodeId, tokenHeaderService.getHeaders()).success(function (data) {
            vm.Jobs = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        });
    }
})();
