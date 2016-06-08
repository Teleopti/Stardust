(function() {
	'use strict';

	angular.module('wfm.reports').controller('ReportsController', ReportsCtrl);

	ReportsCtrl.$inject = ['ReportsService'];

	function ReportsCtrl(ReportsSvc) {
		var vm = this;
		vm.reports = [];
		init();

		function init () {
			ReportsSvc.getPermittedReports().then(function(reports) {
				vm.reports = reports;
			});
		}
	}


})();