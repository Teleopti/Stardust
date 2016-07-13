(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', '$sessionStorage', 'RtaService', 'RtaGridService', 'RtaFormatService', 'RtaRouteService', 'FakeTimeService', 'Toggle', 'NoticeService', '$translate',
			function($scope, $filter, $state, $stateParams, $interval, $sessionStorage, RtaService, RtaGridService, RtaFormatService, RtaRouteService, FakeTimeService, toggleService, NoticeService, $translate) {
				var polling = null;
				var selectedPersonId;
				var siteIds = $stateParams.siteIds || ($stateParams.siteId ? [$stateParams.siteId] : []);
				var teamIds = $stateParams.teamIds || ($stateParams.teamId ? [$stateParams.teamId] : []);
				var skillId = $stateParams.skillId || undefined;
				var skillAreaId = $stateParams.skillAreaId || undefined;
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
				$scope.pause = false;
				$scope.pausedAt = null;
				var lastUpdate, notice;

				$scope.noSiteIds = siteIds.length == 0;
				$scope.monitorBySkill = toggleService.RTA_MonitorBySkills_39081;
				$scope.showBreadcrumb = skillId !== undefined ? false : true;
				$scope.showGrid = !$scope.showBreadcrumb;
				$scope.skillName = "";
				$scope.skillAreaName = "";

				$scope.$watch('pause', function() {
					if ($scope.pause) {
						$scope.pausedAt = moment(lastUpdate).format('YYYY-MM-DD HH:mm:ss');
						var template = $translate.instant('RtaPauseEnabledNotice')
						var noticeText = template.replace('{0}', $scope.pausedAt)
						notice = NoticeService.info(noticeText, null, true);
						cancelPolling();
					} else {
						$scope.pausedAt = null;
						if (notice != null) {
							notice.destroy();
						}
						NoticeService.info($translate.instant('RtaPauseDisableNotice'), 5000, true);
						setupPolling();
					}
				});

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
				$scope.$watch('agentsInAlarm', function(newValue, oldValue) {
					if (newValue !== oldValue) {
						updateStates();
						filterData();
						if (newValue && $scope.pause) {
							$scope.filteredData.sort(function(a, b) {
								return b.TimeInAlarm - a.TimeInAlarm;
							});
						}
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

				var getAgents = (function () {
					if (skillAreaId) {
						return RtaService.getAgentsForSkillArea;
					}
					if (skillId) {
						return RtaService.getAgentsForSkill;
					}
					if (teamIds.length > 0) {
						return RtaService.getAgentsForTeams;
					}
					return RtaService.getAgentsForSites;
				})();

				var getStates = function (inAlarm) {
					
					if (skillId) {
						if (inAlarm)
							return RtaService.getAlarmStatesForSkill;
						return RtaService.getStatesForSkill;
					}
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
				} else if (siteIds.length === 1 && teamIds.length !== 1) {
					$scope.multipleTeamsName = "Multiple Teams";
					updateBreadCrumb = function() {};
				}

				if (skillId) {
					RtaService.getSkillName(skillId)
						.then(function(skill) {
							$scope.skillName = skill.Name || '?';
						});
				}

				if (skillAreaId) {
					RtaService.getSkillAreaName(skillAreaId)
						.then(function (skillArea) {
							$scope.skillAreaName = skillArea.Name || '?';
						});
				}

				if (siteIds.length > 0 || teamIds.length > 0 || skillId || skillAreaId) {
					getAgents({
							siteIds: siteIds,
							teamIds: teamIds,
							skillId: skillId,
							skillAreaId: skillAreaId
						})
						.then(function(agentsInfo) {
							$scope.agentsInfo = agentsInfo;
							$scope.agents = agentsInfo;
							$scope.$watchCollection('agents', filterData);
							updateBreadCrumb(agentsInfo);
						})
						.then(updateStates);
				}

				$scope.$on('$destroy', function() {
					cancelPolling();
				});

				function setupPolling() {
					polling = $interval(function() {
						updateStates();
					}, 5000);
				}

				function cancelPolling() {
					if (polling != null)
						$interval.cancel(polling);
				}

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
					if ($scope.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillId))
						return;
					getStates($scope.agentsInAlarm)({
							siteIds: siteIds,
							teamIds: teamIds,
							skillId: skillId
						})
						.then(setStatesInAgents);
				}

				function setStatesInAgents(states) {
					$scope.agents = [];
					lastUpdate = states.Time;
					fillAgentsWithState(states);
					fillAgentsWithoutState();
					buildTimeline(states);
				}

				function secondsToPercent(seconds) {
					return seconds / 3600 * 25;
				}

				function timeToPercent(currentTime, time) {
					var offset = moment(currentTime).add(-1, 'hour');
					return secondsToPercent(moment(time).diff(offset, 'seconds'));
				}

				function timePeriodToPercent(windowStart, startTime, endTime) {
					var start = moment(startTime) > windowStart ? moment(startTime) : windowStart;
					var lengthSeconds = moment(endTime).diff(start, 'seconds');
					return secondsToPercent(lengthSeconds);
				}

				function buildTimeline(states) {
					var timeline = function(time) {
						var percent = timeToPercent(states.Time, time);
						if (percent <= 94)
							return {
								Time: time.format('HH:mm'),
								Offset: percent + "%"
							};
					};

					var time = moment(states.Time).startOf('hour');
					$scope.timeline = [
						timeline(time),
						timeline(time.add(1, 'hour')),
						timeline(time.add(1, 'hour')),
						timeline(time.add(1, 'hour'))
					].filter(function(tl) {
						return tl != null;
					});

				}

				function fillAgentsWithState(states) {
					states.States.forEach(function(state, i) {
						var agentInfo = $filter('filter')($scope.agentsInfo, {
							PersonId: state.PersonId
						});

						state.Shift = state.Shift || [];
						state.OutOfAdherences = state.OutOfAdherences || [];

						var now = moment(states.Time);
						var windowStart = now.clone().add(-1, 'hours');
						var windowEnd = now.clone().add(3, 'hours');

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
								TimeInRule: state.TimeInAlarm ? state.TimeInRule : null,

								TimeOutOfAdherence: function() {
									if (state.OutOfAdherences.length > 0) {
										var lastOOA = state.OutOfAdherences[state.OutOfAdherences.length - 1];
										if (lastOOA.EndTime == null) {
											var seconds = moment(states.Time).diff(moment(lastOOA.StartTime), 'seconds');
											return $scope.formatDuration(seconds);
										}
									}
								}(),

								OutOfAdherences: function() {
									return state.OutOfAdherences
										.filter(function(t) {
											return t.EndTime == null || moment(t.EndTime) > windowStart;
										})
										.map(function(t) {
											var endTime = t.EndTime || states.Time;
											return {
												Offset: Math.max(timeToPercent(states.Time, t.StartTime), 0) + '%',
												Width: Math.min(timePeriodToPercent(windowStart, t.StartTime, endTime), 100) + "%",
												StartTime: moment(t.StartTime).format('HH:mm:ss'),
												EndTime: t.EndTime ? moment(t.EndTime).format('HH:mm:ss') : null,
											};
										});
								}(),

								ShiftTimeBar: function() {
									var percentForTimeBar = function(seconds) {
										return Math.min(secondsToPercent(seconds), 25);
									}
									if (toggleService.RTA_TotalOutOfAdherenceTime_38702)
										return (state.TimeInAlarm ? percentForTimeBar(state.TimeInRule) : 0) + "%";
									return percentForTimeBar(state.TimeInAlarm) + "%";

								}(),

								Shift: function() {
									return state.Shift
										.filter(function(layer) {
											return windowStart < moment(layer.EndTime) && windowEnd > moment(layer.StartTime);
										})
										.map(function(s) {
											return {
												Color: s.Color,
												Offset: Math.max(timeToPercent(states.Time, s.StartTime), 0) + '%',
												Width: Math.min(timePeriodToPercent(windowStart, s.StartTime, s.EndTime), 100) + "%",
												Name: s.Name,
												Class: getClassForActivity(states.Time, s.StartTime, s.EndTime)
											};
										});
								}()

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
				$scope.goToOverview = function() {
					$state.go('rta');
				}
				$scope.goToSelectItem = function() {
					$state.go('rta.select-skill');
				}


			}
		]);
})();
