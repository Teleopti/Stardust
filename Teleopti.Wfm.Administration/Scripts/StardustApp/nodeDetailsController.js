﻿(function () {
	'use strict';

	angular
		 .module('adminApp')
		 .controller('nodeDetailsController', nodeDetailsController);

	function nodeDetailsController($http, $routeParams) {
		/* jshint validthis:true */
		var vm = this;
		vm.Id = $routeParams.nodeId;
		$http.get("./Stardust/WorkerNode/" + vm.Id).success(function (data) {
			vm.Node = data;
		}).error(function (xhr, ajaxOptions, thrownError) {
			console.log(xhr.Message + ': ' + xhr.ExceptionMessage);
		});

	}
})();
