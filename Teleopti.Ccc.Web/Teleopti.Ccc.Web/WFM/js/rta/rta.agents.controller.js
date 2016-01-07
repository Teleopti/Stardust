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
				$scope.agentsInAlarm = toggleService.Wfm_RTA_ProperAlarm_34975;
				var options = RtaGridService.makeAllGrid();
				options.data = 'filteredData';
				$scope.allGrid = options;
				var options = RtaGridService.makeInAlarmGrid();
				options.data = 'filteredData';
				$scope.inAlarmGrid = options;

				$scope.selectAgent = function(personId) {
					selectedPersonId = $scope.isSelected(personId) ? '' : personId;
				};
				$scope.isSelected = function(personId) {
					return selectedPersonId === personId;
				};
				$scope.showAdherenceUpdates = function() {
					return $scope.adherencePercent !== null;
				};
				$scope.getAdherenceForAgent = function(personId) {
					if (!$scope.isSelected(personId)) {
						RtaService.forToday({
								personId: personId
							})
							.then(function(data) {
								$scope.adherence = data;
								$scope.adherencePercent = data.AdherencePercent;
								$scope.timestamp = data.LastTimestamp;
							});
					}
				};
				$scope.changeScheduleUrl = function(teamId, personId) {
					return RtaRouteService.urlForChangingSchedule($sessionStorage.buid, teamId, personId);
				};
				$scope.agentDetailsUrl = function(personId) {
					return RtaRouteService.urlForAgentDetails(personId);
				};

				$scope.$watch('agents', filterData, true);
				$scope.$watch('filterText', filterData);
				$scope.$watch('agentsInAlarm', function(newValue, oldValue) {
					if (newValue !== oldValue) {
						updateStates();
						filterData();
					}
				});
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

				var getAgents = (function() {
					if (teamIds.length > 0)
						return RtaService.getAgentsForTeams;
					return RtaService.getAgentsForSites;
				})();

				var getStates = (function() {
					if (teamIds.length > 0)
						return RtaService.getStatesForTeams;
					return RtaService.getStatesForSites;
				})();

				var updateBreadCrumb = function(agentsInfo) {
					$scope.siteName = agentsInfo[0].SiteName;
					$scope.teamName = agentsInfo[0].TeamName;
				};
				if (siteIds.length > 1) {
					$scope.multipleSitesName = "Multiple Sites";
					updateBreadCrumb = function() {};
				} else if (teamIds.length > 1) {
					$scope.multipleTeamsName = "Multiple Teams";
					updateBreadCrumb = function() {};
				}

				getAgents({
						siteIds: siteIds,
						teamIds: teamIds,
					})
					.then(function(agentsInfo) {
						$scope.agentsInfo = agentsInfo;
						$scope.agents = agentsInfo;
						updateBreadCrumb(agentsInfo);
					})
					.then(updateStates);

				var polling = $interval(function() {
					updateStates();
				}, 5000);

				$scope.$on('$destroy', function() {
					$interval.cancel(polling);
				});

				function filterData() {
					if ($scope.filterText === undefined)
						$scope.filteredData = $scope.agents;
					else
						$scope.filteredData = $filter('agentFilter')($scope.agents, $scope.filterText, propertiesForFiltering);
					if ($scope.agentsInAlarm) {
						$scope.filteredData = $filter('filter')($scope.filteredData, {
							TimeInAlarm: ''
						});
					}
				}

				function updateStates() {
					getStates({
							siteIds: siteIds,
							teamIds: teamIds,
							inAlarmOnly: $scope.agentsInAlarm === true ? true : null,
							alarmTimeDesc: $scope.agentsInAlarm === true ? true : null,
						})
						.then(setStatesInAgents);
				}

				function setStatesInAgents(states) {
					$scope.agents = [];
					fillAgentsWithState(states);
					fillAgentsWithoutState();
				}

				function fillAgentsWithState(states) {
					states.forEach(function(state) {
						var agentInfo = $filter('filter')($scope.agentsInfo, {
							PersonId: state.PersonId
						});
						if (agentInfo.length > 0) {
							$scope.agents.push({
								Name: agentInfo[0].Name,
								TeamName: agentInfo[0].TeamName,
								PersonId: state.PersonId,
								State: state.State,
								StateStartTime: state.StateStartTime,
								Activity: state.Activity,
								NextActivity: state.NextActivity,
								NextActivityStartTime: state.NextActivityStartTime,
								Alarm: state.Alarm,
								AlarmStart: state.AlarmStart,
								AlarmColor: state.AlarmColor,
								TimeInState: state.TimeInState,
								TimeInAlarm: state.TimeInAlarm
							});
						}
					});
				}

				function fillAgentsWithoutState() {
					$scope.agentsInfo.forEach(function(agentInfo) {
						var agentFilled = $filter('filter')($scope.agents, {
							PersonId: agentInfo.PersonId
						});
						if (agentFilled.length === 0)
							$scope.agents.push({
								Name: agentInfo.Name,
								TeamName: agentInfo.TeamName,
							});
					});
				}
			}
		]);
})();
