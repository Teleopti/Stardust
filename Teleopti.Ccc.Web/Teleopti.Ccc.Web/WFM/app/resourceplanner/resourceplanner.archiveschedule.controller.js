(function() {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerArchiveScheduleCtrl', [
			'ArchiveScheduleSrvc',
			function (ArchiveScheduleSrvc) {
				var vm = this;
				vm.scenarios = [];
				vm.fromScenario = null;
				vm.toScenario = null;
				vm.dateRangeTemplateType = 'popup';
				vm.period = {
					startDate: moment().utc().startOf('month').toDate(),
					endDate: moment().utc().add(1, 'months').startOf('month').toDate()
				};

				var scenariosPromise = ArchiveScheduleSrvc.scenarios.query().$promise;
				scenariosPromise.then(function (result) {
					vm.scenarios = result;
					vm.fromScenario = result[0];
				});

				vm.runArchiving = function (fromScenario, toScenario, period, peopleSelection) {
					//console.log("From", fromScenario);
					//console.log("To", toScenario);
					//console.log("Period", period);
					//console.log("People", peopleSelection);
				};
			}
		]);
})();
