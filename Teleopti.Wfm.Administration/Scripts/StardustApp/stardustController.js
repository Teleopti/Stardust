(function () {
    'use strict';

    angular
        .module('adminApp')
        .controller('stardustController', stardustController, ['tokenHeaderService']);

    function stardustController($http, tokenHeaderService) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'Stardust Manager';
        vm.NodeError = '';
        vm.JobError = '';
        $http.get("./Stardust/Jobs", tokenHeaderService.getHeaders()).success(function (data) {
        	vm.Jobs = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
        	    console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
            vm.JobError = ajaxOptions;
        	    if (xhr !== "") {
        	        vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
	            } 
        	});

        $http.get("./Stardust/QueuedJobs", tokenHeaderService.getHeaders()).success(function (data) {
        	vm.QueuedJobs = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
        	console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        	vm.JobError = ajaxOptions;
        	if (xhr !== "") {
        		vm.JobError = vm.JobError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
        	}
        });

        $http.get("./Stardust/WorkerNodes", tokenHeaderService.getHeaders()).success(function (data) {
    	    vm.WorkerNodes = data;
        }).error(function (xhr, ajaxOptions, thrownError) {
            vm.NodeError = ajaxOptions;
            console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
            if (xhr !== "") {
                vm.NodeError = vm.NodeError + " " + xhr.Message + ': ' + xhr.ExceptionMessage;
            }
    	    
    	});
    }
})();
