(function () {
    'use strict';

    angular
        .module('adminApp')
        .controller('listJobController', listJobController);

    function listJobController($http) {
        /* jshint validthis:true */
        var vm = this;
        vm.title = 'Stardust Manager - List Jobs';

    	$http.get("../Stardust/JobHistoryList").success(function (data) {
        		vm.Jobs = data;
        	}).error(function (xhr, ajaxOptions, thrownError) {
        		console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
        	});
    }
})();
