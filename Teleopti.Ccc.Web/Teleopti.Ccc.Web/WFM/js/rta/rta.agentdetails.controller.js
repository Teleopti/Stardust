
(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentDetailsCtrl', ['$scope', '$state', '$stateParams', 'RtaService',
			function($scope, $state, $stateParams, RtaService) {
				var personId = $stateParams.personId;
				$scope.name = "";
				$scope.adherence = [];

				RtaService.getPersonDetails.query({
						personId: personId
					}).$promise
					.then(function(person) {
						$scope.name = person.Name;
					});

				RtaService.getAdherenceDetails.query({
						personId: personId
					}).$promise
					.then(function(adherence) {
						$scope.adherence = adherence;
						$scope.adherencePercent = adherence[0].AdherencePercent;
						$scope.activityName = adherence[0].Name;
						$scope.startTime = adherence[0].StartTime;
						$scope.actualStartTime = adherence[0].ActualStartTime;
					});

				RtaService.forToday.query({
						personId: personId
					}).$promise
					.then(function(data) {
						$scope.dailyTotal = data.AdherencePercent;
					});
			}
		]);
})();
