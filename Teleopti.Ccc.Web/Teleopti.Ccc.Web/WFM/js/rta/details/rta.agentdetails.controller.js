(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentDetailsCtrl', ['$scope', '$state', '$stateParams', 'RtaService',
			function($scope, $state, $stateParams, RtaService) {
				var personId = $stateParams.personId;
				$scope.name = "";
				$scope.adherence = [];

				RtaService.getPersonDetails({
						personId: personId
					})
					.then(function(person) {
						$scope.name = person.Name;
					});

				RtaService.getAdherenceDetails({
						personId: personId
					})
					.then(function(adherence) {
						$scope.adherence = adherence;
					});

				RtaService.forToday({
						personId: personId
					})
					.then(function(data) {
						$scope.dailyTotal = data.AdherencePercent;
					});
			}
		]);
})();
