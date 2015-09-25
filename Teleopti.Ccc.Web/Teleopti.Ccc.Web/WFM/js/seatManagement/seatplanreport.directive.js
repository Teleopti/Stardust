"use strict";

(function () {

	angular.module('wfm.seatPlan').directive('seatPlanReport', seatPlanReport);

	function seatPlanReport() {
		return {
			controller: 'seatPlanReportCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {
				toggle: '&toggle',
				selectedPeriod: '=selectedPeriod',
				selectedTeams: '=selectedTeams',
				selectedLocations: '=selectedLocations'
			},
			link: linkFunction,
			restrict: "E",
			templateUrl: "js/seatManagement/html/seatplanreport.html"
		};

		function linkFunction() {
			$('#materialcontainer').scrollTop(0);
		};
	};

})();