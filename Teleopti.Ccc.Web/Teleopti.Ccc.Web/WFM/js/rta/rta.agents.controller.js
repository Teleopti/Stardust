(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', '$sessionStorage', 'RtaService', 'RtaGridService', 'RtaFormatService', 'RtaRouteService', 'FakeTimeService',
			function($scope, $filter, $state, $stateParams, $interval, $sessionStorage, RtaService, RtaGridService, RtaFormatService, RtaRouteService, FakeTimeService) {
				var siteId = $stateParams.siteId;
				var teamId = $stateParams.teamId;
				var propertiesForFiltering = ["Name", "TeamName", "State", "Activity", "NextActivity", "Alarm"];
				$scope.adherence = {};
				$scope.adherencePercent = null;
				$scope.timeStamp = "";
				$scope.agents = [];
				$scope.gridOptions = RtaGridService.createAgentsGridOptions();
				$scope.selectAgent = RtaGridService.selectAgent;
				$scope.isSelected = RtaGridService.isSelected;
				$scope.format = RtaFormatService.formatDateTime;
				$scope.formatDuration = RtaFormatService.formatDuration;
				$scope.hexToRgb = RtaFormatService.formatHexToRgb;
				$scope.agentDetailsUrl = RtaRouteService.urlForAgentDetails;
				$scope.goBackToRootWithUrl = RtaRouteService.urlForSites;
				$scope.goBackToTeamsWithUrl = RtaRouteService.urlForTeams(siteId);

				$scope.$watch(
					function() {
						return $sessionStorage.buid;
					},
					function(newValue, oldValue) {
						if (oldValue !== undefined && newValue !== oldValue) {
							RtaRouteService.goToSites();
						}
					}
				);
				$scope.getAdherenceForAgent = function(personId) {
					if(!$scope.isSelected(personId)){
					RtaService.forToday.query({
							personId: personId
						}).$promise
						.then(function(data) {
							$scope.adherence = data;
							$scope.adherencePercent = data.AdherencePercent;
							$scope.timeStamp = data.LastTimestamp;
						});
					}
				};
				$scope.filterData = function() {
					$scope.gridOptions.data = $filter('agentFilter')($scope.agents, $scope.filterText, propertiesForFiltering);
				};
				$scope.changeScheduleUrl = function(teamId, personId) {
					return RtaRouteService.urlForChangingSchedule($sessionStorage.buid, teamId, personId);
				};
				$scope.agentDetailsUrl = function(personId) {
					return RtaRouteService.urlForAgentDetails(personId);
				};

				var setStatesInAgents = function(states) {
					$scope.agents.forEach(function(agent) {
						var state = $filter('filter')(states, {
							PersonId: agent.PersonId
						});
						if (state.length > 0) {
							agent.State = state[0].State;
							agent.StateStart = state[0].StateStart;
							agent.Activity = state[0].Activity;
							agent.NextActivity = state[0].NextActivity;
							agent.NextActivityStartTime = state[0].NextActivityStartTime;
							agent.Alarm = state[0].Alarm;
							agent.AlarmStart = state[0].AlarmStart;
							agent.AlarmColor = state[0].AlarmColor;
							agent.TimeInState = state[0].TimeInState;
						}
					});
				};

				var updateStates = function() {
					RtaService.getStates.query({
							teamId: teamId
						}).$promise
						.then(function(states) {
							setStatesInAgents(states);
						});
				};

				var updateGrid = function() {
					if ($scope.filterText === undefined)
						$scope.gridOptions.data = $scope.agents;
					else
						$scope.filterData();
				};

				RtaService.getAgents.query({
						teamId: teamId
					}).$promise
					.then(function(agents) {
						$scope.agents = agents;
						$scope.siteName = agents[0].SiteName;
						$scope.teamName = agents[0].TeamName;
					})
					.then(updateStates)
					.then(updateGrid);

				var polling = $interval(function() {
					updateStates();
					updateGrid();
				}, 5000);

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
