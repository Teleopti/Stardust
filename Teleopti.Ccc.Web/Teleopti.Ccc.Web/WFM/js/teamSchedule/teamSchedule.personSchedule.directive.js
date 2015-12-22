'use strict';

(function () {
	angular.module('wfm.teamSchedule')
		.directive('personSchedule', personScheduleDirective)
		.controller('personScheduleCtrl', personScheduleController);
	

	function personScheduleDirective() {

		return {
			scope: {
				personSchedule: '=personSchedule'
			},
			restrict: 'A',
			controllerAs: 'vm',
			link: linkFunction,
			bindToController: true,
			controller: 'personScheduleCtrl',
			templateUrl: "js/teamSchedule/html/personschedule.html"
		};
	};

	function linkFunction(scope, element, attr) {
		scope.vm.init();
	};


	personScheduleController.$inject = ['Toggle', 'GroupScheduleFactory'];

	function personScheduleController(toggleService, groupScheduleFactory) {

		var vm = this;
		vm.isAbsenceReportingEnabled = function(){return toggleService['WfmTeamSchedule_AbsenceReporting_35995'];}
		vm.init = function() {
			console.log(vm, 'personScheduleController');
		};
	};

}());