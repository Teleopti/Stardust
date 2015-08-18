﻿"use strict";

(function () {

	angular.module('wfm.seatPlan').directive('seatPlanReport', seatPlanReport);

	function seatPlanReport() {
		return {
			controller: 'seatPlanReportCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: { toggle: '&toggle', selectedPeriod: '=selectedPeriod' },
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreport.html"
		};
	};

})();