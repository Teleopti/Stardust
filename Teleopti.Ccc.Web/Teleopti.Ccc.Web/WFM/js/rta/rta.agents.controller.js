(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', '$sessionStorage', 'RtaService', 'RtaGridService', 'RtaFormatService', 'RtaRouteService', 'FakeTimeService', 'Toggle',
			function($scope, $filter, $state, $stateParams, $interval, $sessionStorage, RtaService, RtaGridService, RtaFormatService, RtaRouteService, FakeTimeService, toggleService) {
				var selectedPersonId;
				var siteIds = $stateParams.siteIds || ($stateParams.siteId ? [$stateParams.siteId] : []);
				var teamIds = $stateParams.teamIds || ($stateParams.teamId ? [$stateParams.teamId] : []);
				var propertiesForFiltering = ["Name", "TeamName", "State", "Activity", "Alarm"];

				var getAgents = (function() {
					if (teamIds.length === 1)
						return RtaService.getAgents;
					if (teamIds.length > 1)
						return RtaService.getAgentsForTeams;
					return RtaService.getAgentsForSites;
				})();

				var getStates = (function() {
					if (teamIds.length === 1)
						return RtaService.getStates;
					if (teamIds.length > 1)
						return RtaService.getStatesForTeams;
					return RtaService.getStatesForSites;
				})();

				var updateBreadCrumb = function(agents) {
					$scope.siteName = agents[0].SiteName;
					$scope.teamName = agents[0].TeamName;
				};
				if (siteIds.length > 1) {
					$scope.multipleSitesName = "Multiple Sites";
					updateBreadCrumb = function() {};
				} else if (teamIds.length > 1) {
					$scope.multipleTeamsName = "Multiple Teams";
					updateBreadCrumb = function() {};
				}

				$scope.adherence = {};
				$scope.adherencePercent = null;
				$scope.filterText = "";
				$scope.timestamp = "";
				$scope.agents = [];
				$scope.format = RtaFormatService.formatDateTime;
				$scope.formatDuration = RtaFormatService.formatDuration;
				$scope.hexToRgb = RtaFormatService.formatHexToRgb;
				$scope.agentDetailsUrl = RtaRouteService.urlForAgentDetails;
				$scope.goBackToRootWithUrl = RtaRouteService.urlForSites;
				$scope.goBackToTeamsWithUrl = RtaRouteService.urlForTeams(siteIds[0]);
				$scope.filteredData = [];
				var options = RtaGridService.makeAllGrid();
				options.data = 'filteredData';
				$scope.allGrid = options;
				var options = RtaGridService.makeInAlarmGrid();
				options.data = 'filteredData';
				$scope.inAlarmGrid = options;
				$scope.agentsInAlarm = toggleService.Wfm_RTA_ProperAlarm_34975;

				$scope.selectAgent = function(personId) {
					selectedPersonId = $scope.isSelected(personId) ? '' : personId;
				};
				$scope.isSelected = function(personId) {
					return selectedPersonId === personId;
				};
				$scope.showAdherenceUpdates = function() {
					return $scope.adherencePercent !== null;
				};
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
					if (!$scope.isSelected(personId)) {
						RtaService.forToday.query({
								personId: personId
							}).$promise
							.then(function(data) {
								$scope.adherence = data;
								$scope.adherencePercent = data.AdherencePercent;
								$scope.timestamp = data.LastTimestamp;
							});
					}
				};

				var filterData = function() {
					if ($scope.filterText === undefined)
						$scope.filteredData = $scope.agents;
					else
						$scope.filteredData = $filter('agentFilter')($scope.agents, $scope.filterText, propertiesForFiltering);
					if ($scope.agentsInAlarm){
						$scope.filteredData = $filter('agentFilter')($scope.filteredData, 'Out Adherence', ['Alarm']);
						$scope.filteredData = $filter('orderBy')($scope.filteredData, 'TimeInState', true);
					}
				};

				$scope.$watch('agents', filterData, true);
				$scope.$watchGroup(['filterText', 'agentsInAlarm'], filterData);

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
					getStates({
							teamId: teamIds[0],
							siteIds: siteIds,
							teamIds: teamIds,
						})
						.then(setStatesInAgents);
				};

				getAgents({
						teamId: teamIds[0],
						siteIds: siteIds,
						teamIds: teamIds,
					})
					.then(function(agents) {
						$scope.agents = agents;
						updateBreadCrumb(agents);
					})
					.then(updateStates);

				var polling = $interval(function() {
					updateStates();
				}, 5000);

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});
			}
		]);
})();
