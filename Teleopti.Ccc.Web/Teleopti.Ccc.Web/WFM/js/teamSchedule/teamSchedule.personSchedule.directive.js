'use strict';

(function () {
	angular.module('wfm.teamSchedule').directive('personSchedule', personScheduleDirective);

	function personScheduleDirective() {

		return {
			scope: {
				personId: '=personSchedule'
			},
			restrict: 'A',
			controllerAs: 'vm',
			link: linkFunction,
			bindToController: true,
			require: ['^scheduleListContainer'],
			controller: ['Toggle', personScheduleController],
			templateUrl: "js/teamSchedule/html/personschedule.html"
		};
	};



	function linkFunction(scope, element, attr, ctrls) {
		var parentCtrl = ctrls[0];

		scope.vm.parentCtrl = parentCtrl;
		scope.vm.init();
	};


	function personScheduleController(toggleService) {

		var vm = this;
		vm.isAbsenceReportingEnabled = function(){return toggleService['WfmTeamSchedule_AbsenceReporting_35995'];}

		vm.init = function () {
			getPersonScheduleFromParentScheduleList(vm.personId);
		}

		function getPersonScheduleFromParentScheduleList(personId) {

			vm.parentCtrl.scheduleList.forEach(function (scheduleDetail) {

				if (scheduleDetail.PersonId === personId) {
					vm.personSchedule = scheduleDetail;
				}

			});

			console.log(vm, 'personScheduleController');
		};



	};

}());