(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentDetailsCtrlRefact', RtaAgentDetailsCtrl);

		RtaAgentDetailsCtrl.$inject = ['$state', '$stateParams', 'RtaService'];

			function RtaAgentDetailsCtrl ($state, $stateParams, RtaService) {

				var vm = this;

				var personId = $stateParams.personId;
				vm.name = "";
				vm.adherence = [];

				RtaService.getPersonDetails({
						personId: personId
					})
					.then(function(person) {
						vm.name = person.Name;
					});

				RtaService.getAdherenceDetails({
						personId: personId
					})
					.then(function(adherence) {
						vm.adherence = adherence;
					});

				RtaService.forToday({
						personId: personId
					})
					.then(function(data) {
						vm.dailyTotal = data.AdherencePercent;
					});
			};
})();
