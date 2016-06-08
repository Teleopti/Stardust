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
				$scope.agentsInAlarm = !$stateParams.showAllAgents;
				var options = RtaGridService.makeAllGrid();
				options.data = 'filteredData';
				$scope.allGrid = options;
				var options = RtaGridService.makeInAlarmGrid();
				options.data = 'filteredData';
				$scope.inAlarmGrid = options;

				$scope.getTableHeight = function() {
					var rowHeight = 30;
					var headerHeight = 30;
					var agentMenuHeight = 45;
					return {
						height: ($scope.filteredData.length * rowHeight + headerHeight + agentMenuHeight) + "px"
					};
				};

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

				$scope.$watch('filterText', filterData);
				$scope.$watch('agentsInAlarm', function (newValue, oldValue) {
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

				var getStates = function (inAlarm) {
					if (teamIds.length > 0) {
						if (inAlarm)
							return RtaService.getAlarmStatesForTeams;
						return RtaService.getStatesForTeams;
					}
					if (inAlarm)
						return RtaService.getAlarmStatesForSites;
					return RtaService.getStatesForSites;
				};

				var updateBreadCrumb = function(agentsInfo) {
					if (agentsInfo.length > 0) {
						$scope.siteName = agentsInfo[0].SiteName;
						$scope.teamName = agentsInfo[0].TeamName;
					}
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
						$scope.$watchCollection('agents', filterData);
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
					getStates($scope.agentsInAlarm)({
							siteIds: siteIds,
							teamIds: teamIds
						})
						.then(setStatesInAgents);
				}

				function setStatesInAgents(states) {
					$scope.agents = [];
					fillAgentsWithState(states);
					fillAgentsWithoutState();
					buildTimeline(states);
				}


				function layerPercentage(seconds) {
				    return percentageFromSeconds(seconds, 100);
				}

				function alarmPercentage(seconds) {
				    return percentageFromSeconds(seconds, 25);
				}

				function offsetPercentage(seconds) {
				    var maxPercentage = seconds  < 0 ? 0 : (seconds / 3600 * 25);
				    return maxPercentage + '%';
				}

				function percentageFromSeconds(seconds, max) {
					var maxPercentage = (seconds / 3600 * 25);
					return maxPercentage > max ? max + '%' : maxPercentage + '%';
				}

				function timeToPercent(currentTime, time, toPercent) {
				    var offset = moment(currentTime).add(-1, 'hour');
				    return toPercent(moment(time).diff(offset, 'seconds'));
				}

				function offsetToPercent(currentTime, time, toPercent) {
				    var offset = moment(currentTime).add(-1, 'hour');
				    return toPercent(moment(time).diff(offset, 'seconds'));
				}
                

				function buildTimeline(states) {
					var timeline = function (time) {
						return {
							Time: time.format('HH:mm'),
							Offset: timeToPercent(states.Time, time, layerPercentage)
						};
					};

					var time = moment(states.Time).startOf('hour');
					$scope.timeline = [
						timeline(time),
						timeline(time.add(1, 'hour')),
						timeline(time.add(1, 'hour')),
						timeline(time.add(1, 'hour'))
					];

				}

				function fillAgentsWithState(states) {
					states.States.forEach(function(state, i) {
						var agentInfo = $filter('filter')($scope.agentsInfo, {
							PersonId: state.PersonId
						});
						state.Shift = state.Shift || [];

						var now = moment(states.Time);
						var start = now.clone().add(-1, 'hours');
						var end = now.clone().add(3, 'hours');
						var shift = state.Shift
							.filter(function (layer) {
								return start < moment(layer.EndTime) && end > moment(layer.StartTime);
							})
							.map(function (s) {
								var lengthSeconds = moment(s.EndTime).diff(moment(s.StartTime), 'seconds');
								return {
									Color: s.Color,
									Offset: offsetToPercent(states.Time, s.StartTime, offsetPercentage),
									Width: layerPercentage(lengthSeconds),
									Name: s.Name,
									Class: getClassForActivity(states.Time, s.StartTime, s.EndTime)
								};
							});
						if (agentInfo.length > 0) {
							$scope.agents.push({
								Name: agentInfo[0].Name,
								TeamName: agentInfo[0].TeamName,
								PersonId: agentInfo[0].PersonId,
								TeamId: agentInfo[0].TeamId,
								State: state.State,
								Activity: state.Activity,
								NextActivity: state.NextActivity,
								NextActivityStartTime: state.NextActivityStartTime,
								Alarm: state.Alarm,
								Color: state.Color,
								TimeInState: state.TimeInState,
								TimeInAlarm: state.TimeInAlarm,
								TimeInRule: state.TimeInRule,
								AlarmWidth: alarmPercentage(state.TimeInAlarm),
								Shift: shift
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
								PersonId: agentInfo.PersonId,
								TeamName: agentInfo.TeamName,
								TeamId: agentInfo.TeamId,
							});
					});
				}

				function getClassForActivity(currentTime, startTime, endTime) {
					var now = moment(currentTime).unix(),
						start = moment(startTime).unix(),
						end = moment(endTime).unix();

					if (now < start)
						return 'next-activity';
					else if (now > end)
						return 'previous-activity';
					return 'current-activity';
				}
			}
		]);
})();
