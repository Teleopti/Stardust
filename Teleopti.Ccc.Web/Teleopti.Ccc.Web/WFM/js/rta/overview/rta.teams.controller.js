
(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
		'$scope',
		'$stateParams',
		'$interval',
		'$filter',
		'$sessionStorage',
		'RtaOrganizationService',
		'RtaService',
		'RtaRouteService',
		'RtaFormatService',
		'RtaSelectionService',
		'RtaAdherenceService',
		function (
			$scope,
			$stateParams,
			$interval,
			$filter,
			$sessionStorage,
			RtaOrganizationService,
			RtaService,
			RtaRouteService,
			RtaFormatService,
			RtaSelectionService,
			RtaAdherenceService
			) {

			$scope.getAdherencePercent = RtaFormatService.numberToPercent;
			$scope.siteId = $stateParams.siteId;
			$scope.selectedTeamIds = [];

			RtaOrganizationService.getSiteName($scope.siteId).then(function (name) {
				$scope.siteName = name;
			});

			var polling = $interval(function () {
				RtaService.getAdherenceForTeamsOnSite({
					siteId: $scope.siteId
				}).then(function(teamAdherence) {
					RtaAdherenceService.updateAdherence($scope.teams, teamAdherence);
				});
			}, 5000);

			RtaService.getTeams({
				siteId: $scope.siteId
			}).then(function (teams) {
				$scope.teams = teams;
				return RtaService.getAdherenceForTeamsOnSite({
					siteId: $scope.siteId
				});
			}).then(function (teamAdherence) {
				RtaAdherenceService.updateAdherence($scope.teams, teamAdherence);
			});

			$scope.toggleSelection = function (teamId) {
				$scope.selectedTeamIds = RtaSelectionService.toggleSelection(teamId, $scope.selectedTeamIds);
			}

			$scope.openSelectedTeams = function () {
				if ($scope.selectedTeamIds.length > 0)
					RtaSelectionService.openSelection('rta.agents-teams', { teamIds: $scope.selectedTeamIds });
			}

			$scope.goBackWithUrl = function () {
				return RtaRouteService.urlForSites();
			};

			$scope.$watch(
				function () {
					return $sessionStorage.buid;
				},
				function (newValue, oldValue) {
					if (oldValue !== undefined && newValue !== oldValue) {
						RtaRouteService.goToSites();
					}
				}
			);

			$scope.$on('$destroy', function () {
				$interval.cancel(polling);
			});
		}
	]);
})();
