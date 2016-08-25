
(function () {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
		'$scope',
		'$state',
		'$stateParams',
		'$interval',
		'$filter',
		'$location',
		'$sessionStorage',
		'RtaOrganizationService',
		'RtaService',
		'RtaRouteService',
		'RtaFormatService',
		'RtaSelectionService',
		function (
			$scope,
			$state,
			$stateParams,
			$interval,
			$filter,
			$location,
			$sessionStorage,
			RtaOrganizationService,
			RtaService,
			RtaRouteService,
			RtaFormatService,
			RtaSelectionService
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
				}).then(updateAdherence);
			}, 5000);

			RtaService.getTeams({
				siteId: $scope.siteId
			}).then(function (teams) {
				$scope.teams = teams;
				return RtaService.getAdherenceForTeamsOnSite({
					siteId: $scope.siteId
				});
			}).then(function (teamAdherence) {
				updateAdherence(teamAdherence);
			});

			function updateAdherence(teamAdherence) {
				teamAdherence.forEach(function (team) {
					var filteredTeam = $filter('filter')($scope.teams, {
						Id: team.Id
					});
					if (filteredTeam.length > 0)
						filteredTeam[0].OutOfAdherence = team.OutOfAdherence ? team.OutOfAdherence : 0;
				});
			};

			$scope.toggleSelection = function (teamId) {
				$scope.selectedTeamIds = RtaSelectionService.toggleSelection(teamId, $scope.selectedTeamIds);
			}

			$scope.openSelectedTeams = function () {
				RtaSelectionService.openSelection($scope.selectedTeamIds, 'rta.agents-teams');
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
