
(function() {
	'use strict';

	angular.module('wfm.rta').controller('RtaTeamsCtrl', [
		'$scope', '$state', '$stateParams', '$interval', '$filter', 'RtaOrganizationService', 'RtaService', '$location', '$sessionStorage',
		function($scope, $state, $stateParams, $interval, $filter, RtaOrganizationService, RtaService, $location, $sessionStorage) {

			var siteId = $stateParams.siteId;
			var selectedTeamIds = [];

			var updateAdherence = function(teamAdherence) {
				teamAdherence.forEach(function(team) {
					var filteredTeam = $filter('filter')($scope.teams, {
						Id: team.Id
					});
					if (filteredTeam.length > 0)
						filteredTeam[0].OutOfAdherence = team.OutOfAdherence ? team.OutOfAdherence : 0;
				})
			};

			RtaService.getTeams.query({siteId: siteId})
				.$promise.then(function(teams) {
					$scope.teams = teams;
					return RtaService.getAdherenceForTeamsOnSite.query({
						siteId: siteId
					}).$promise;
				}).then(function(teamAdherence) {
					updateAdherence(teamAdherence);
					$interval(function() {
						RtaService.getAdherenceForTeamsOnSite.query({
							siteId: siteId
						}).$promise.then(updateAdherence);
					}, 5000);
				});

			RtaOrganizationService.getSiteName(siteId).then(function(name) {
				$scope.siteName = name;
			});

			$scope.toggleSelection = function(teamId) {
				var index = selectedTeamIds.indexOf(teamId);
				if (index > -1) {
					selectedTeamIds.splice(index, 1);
				} else {
					selectedTeamIds.push(teamId);
				}
			};

			$scope.openSelectedTeams = function() {
				if(selectedTeamIds.length === 0)return;
				$state.go('rta-agents-teams', {
					teamIds: selectedTeamIds
				});
			};

			$scope.onTeamSelect = function(team) {
				$state.go('rta-agents', {
					siteId: siteId,
					teamId: team.Id
				});
			};

			$scope.goBack = function() {
				$state.go('rta');
			};

			$scope.$watch(
				function(){return $sessionStorage.buid;},
				function(newValue, oldValue) {
					if (newValue !== oldValue) {
						$scope.goBack();
					}
				}
			);
		}
	]);
})();
