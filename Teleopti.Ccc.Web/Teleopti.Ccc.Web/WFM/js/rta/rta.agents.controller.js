(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsCtrl', [
			'$scope', '$filter', '$state', '$stateParams', '$interval', '$sessionStorage', '$q', 'RtaService', 'RtaGridService', 'RtaFormatService', 'RtaRouteService', 'FakeTimeService', 'Toggle', 'NoticeService', '$translate',
			function($scope, $filter, $state, $stateParams, $interval, $sessionStorage, $q, RtaService, RtaGridService, RtaFormatService, RtaRouteService, FakeTimeService, toggleService, NoticeService, $translate) {
				var polling = null;
				var selectedPersonId;
				var siteIds = $stateParams.siteIds || ($stateParams.siteId ? [$stateParams.siteId] : []);
				var teamIds = $stateParams.teamIds || ($stateParams.teamId ? [$stateParams.teamId] : []);
				var skillIds = ($stateParams.skillId ? [$stateParams.skillId] : []);
				var skillAreaId = $stateParams.skillAreaId || undefined;
				var propertiesForFiltering = ["Name", "State", "Activity", "Alarm", "SiteAndTeamName"];
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
				$scope.showPath = false;

				$scope.noSiteIds = siteIds.length == 0;
				$scope.monitorBySkill = toggleService.RTA_MonitorBySkills_39081;
				$scope.showBreadcrumb = siteIds.length > 0 || teamIds.length > 0  || skillIds === [];
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
				$scope.changeScheduleUrl = function(siteName, teamName, personId) {
					return RtaRouteService.urlForChangingSchedule(siteName, teamName, personId);
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

				var getSkillAreaInfo = function () {
					return RtaService.getSkillArea(skillAreaId)
						.then(function (skillArea) {
							$scope.skillAreaName = skillArea.Name || '?';
							skillIds = skillArea.Skills.map(function (skill) { return skill.Id; });
						});
				}

				var getAgents = (function() {
					var deferred = $q.defer();
					if (skillAreaId) {
						getSkillAreaInfo()
							.then(function () {
								deferred.resolve(RtaService.getAgentsForSkills);
							});
					} else if (skillIds.length > 0) {
						deferred.resolve(RtaService.getAgentsForSkills);
					} else if (teamIds.length > 0) {
						deferred.resolve(RtaService.getAgentsForTeams);
					} else {
						deferred.resolve(RtaService.getAgentsForSites);
					}
					return deferred.promise;
				})();

				var getStates = function (inAlarm) {
					if (skillIds.length > 0) {
						if (inAlarm)
							return RtaService.getAlarmStatesForSkills;
						return RtaService.getStatesForSkills;
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
					if (siteIds.length > 1) {
						$scope.siteName = "Multiple Sites";
					} else if (teamIds.length > 1 || (siteIds.length === 1 && teamIds.length !== 1)) {
						$scope.teamName = "Multiple Teams";
					} else if (agentsInfo.length > 0) {
						$scope.siteName = agentsInfo[0].SiteName;
						$scope.teamName = agentsInfo[0].TeamName;
						$scope.goBackToTeamsWithUrl = RtaRouteService.urlForTeams(agentsInfo[0].SiteId);
						$scope.showPath = true;
					}
				};

				if (skillIds.length === 1) {
					RtaService.getSkillName(skillIds[0])
						.then(function(skill) {
							$scope.skillName = skill.Name || '?';
						});
				}

				if (siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId) {
					getAgents
						.then(function (fn) {
							return fn({
								siteIds: siteIds,
								teamIds: teamIds,
								skillIds: skillIds
							});
						})
						.then(function (agentsInfo) {
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
					if ($scope.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId))
						return;
					getStates($scope.agentsInAlarm)({
							siteIds: siteIds,
							teamIds: teamIds,
							skillIds: skillIds
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
								SiteAndTeamName: agentInfo[0].SiteName + '/' + agentInfo[0].TeamName,
								TeamName: agentInfo[0].TeamName,
								SiteName: agentInfo[0].SiteName,
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
								SiteAndTeamName: agentInfo.SiteName + '/' + agentInfo.TeamName,
								TeamName: agentInfo.TeamName,
								TeamId: agentInfo.TeamId,
								SiteName: agentInfo.SiteName
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
