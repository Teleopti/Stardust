(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentDetailsController', RtaAgentDetailsController);

	RtaAgentDetailsController.$inject = ['$state', '$stateParams', 'rtaService'];

	function RtaAgentDetailsController($state, $stateParams, rtaService) {

		var vm = this;

		var personId = $stateParams.personId;
		vm.name = "";
		vm.adherence = [];

		rtaService.getPersonDetails({
				personId: personId
			})
			.then(function(person) {
				vm.name = person.Name;
			});

		rtaService.getAdherenceDetails({
				personId: personId
			})
			.then(function(adherence) {
				vm.adherence = adherence;
			});

		rtaService.forToday({
				personId: personId
			})
			.then(function(data) {
				vm.dailyTotal = data.AdherencePercent;
			});
	};
})();
