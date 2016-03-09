(function () {
    'use strict';

    angular
		 .module('adminApp')
		 .controller('nodeDetailsController', nodeDetailsController);

    function nodeDetailsController($http, $routeParams) {
        /* jshint validthis:true */
        var vm = this;
        vm.NodeId = $routeParams.nodeId;
        $http.get("./Stardust/WorkerNode/" + vm.NodeId).success(function (data) {
            vm.Node = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        });

        $http.get("./Stardust/NodeJobHistoryList/" + vm.NodeId).success(function (data) {
            vm.Jobs = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        });
    }
})();
