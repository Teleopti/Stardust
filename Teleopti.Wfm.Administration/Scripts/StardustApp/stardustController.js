(function () {
    'use strict';

    angular
        .module('adminApp')
        .controller('stardustController', stardustController);

    function stardustController($http) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'Stardust Manager';

    	$http.get("./Stardust/JobHistoryList").success(function (data) {
        		vm.Jobs = data;
        	}).error(function (xhr, ajaxOptions, thrownError) {
        		console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        	});

    	$http.get("./Stardust/WorkerNodes").success(function (data) {
    	    vm.WorkerNodes = data;
    	}).error(function (xhr, ajaxOptions, thrownError) {
    	    console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
    	});
    }
})();
