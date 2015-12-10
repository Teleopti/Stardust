(function() {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController);

	requestsController.$inject = [];

	function requestsController() {		
		var vm = this;
		vm.period = { startDate: new Date(), endDate: new Date()};
	}

})();