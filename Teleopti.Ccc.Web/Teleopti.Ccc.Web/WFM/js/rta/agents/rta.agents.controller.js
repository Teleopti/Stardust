(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsCtrl', [
			'$scope',
			'$filter',
			'$state',
			'$stateParams',
			'$interval',
			'$sessionStorage',
			'$q',
			'$translate',
			'RtaService',
			'RtaGridService',
			'RtaFormatService',
			'RtaRouteService',
			'FakeTimeService',
			'Toggle',
			'NoticeService',
			function($scope,
				$filter,
				$state,
				$stateParams,
				$interval,
				$sessionStorage,
				$q,
				$translate,
				RtaService,
				RtaGridService,
				RtaFormatService,
				RtaRouteService,
				FakeTimeService,
				toggleService,
				NoticeService
			) {
				var selectedPersonId, lastUpdate, notice;
				var polling = null;
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
				$scope.stateGroups = [];
				$scope.format = RtaFormatService.formatDateTime;
				$scope.formatDuration = RtaFormatService.formatDuration;
				$scope.hexToRgb = RtaFormatService.formatHexToRgb;
				$scope.agentDetailsUrl = RtaRouteService.urlForAgentDetails;
				$scope.goBackToRootWithUrl = RtaRouteService.urlForSites;
				$scope.goBackToTeamsWithUrl = RtaRouteService.urlForTeams(siteIds[0]);
				$scope.filteredData = [];
				$scope.agentsInAlarm = !$stateParams.showAllAgents;
				var allGrid = RtaGridService.makeAllGrid();
				allGrid.data = 'filteredData';
				$scope.allGrid = allGrid;
				var inAlarmGrid = RtaGridService.makeInAlarmGrid();
				inAlarmGrid.data = 'filteredData';
				$scope.inAlarmGrid = inAlarmGrid;
				$scope.pause = false;
				$scope.pausedAt = null;
				$scope.showPath = false;
				$scope.notifySwitchDisabled = false;
				$scope.showBreadcrumb = siteIds.length > 0 || teamIds.length > 0 || skillIds === [];
				$scope.skill = false;
				$scope.skillArea = false;
				$scope.skillName = "";
				$scope.skillAreaName = "";
				$scope.openedMaxNumberOfAgents = false;
				$scope.maxNumberOfAgents = 50;
				$scope.isLoading = true;

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

				(function initialize() {
					if (siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId) {
						getAgents()
							.then(function(fn) {
								return fn({
									siteIds: siteIds,
									teamIds: teamIds,
									skillIds: skillIds
								});
							})
							.then(function(agentsInfo) {
								$scope.agentsInfo = agentsInfo;
								$scope.agents = agentsInfo;
								$scope.$watchCollection('agents', filterData);
								updateBreadCrumb(agentsInfo);
							})
							.then(updateStates);
					}
				})();

				function updateStates() {
					if ($scope.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId))
						return;
					getStates($scope.agentsInAlarm)({
							siteIds: siteIds,
							teamIds: teamIds,
							skillIds: skillIds,
							excludedStateGroupIds : excludedStateGroupIds()
						})
						.then(setStatesInAgents);
				}

				function getStates(inAlarm) {
					if (skillIds.length > 0) {
						if (inAlarm)
							return RtaService.getAlarmStatesForSkills;
						return RtaService.getStatesForSkills;
					}
					if (teamIds.length > 0) {
						if (inAlarm) {
							if (excludedStateGroupIds().length >0){
									return RtaService.getAlarmStatesForTeamsExcludingGroups;
								}
							return RtaService.getAlarmStatesForTeams;
						}
						return RtaService.getStatesForTeams;
					}
					if (inAlarm)
						return RtaService.getAlarmStatesForSites;
					return RtaService.getStatesForSites;
				};

				function excludedStateGroupIds(){
					return $scope.stateGroups.filter(function(s) { return s.Selected === false;}).map(function(s){ return s.StateId; });
				}

				function setStatesInAgents(states) {
					$scope.agents = [];
					lastUpdate = states.Time;
					fillAgentsWithState(states);
					fillAgentsWithoutState();
					buildTimeline(states);
					$scope.isLoading = false;
				}

				function fillAgentsWithState(states) {
					states.States.forEach(function(state, i) {
						var agentInfo = $filter('filter')($scope.agentsInfo, {
							PersonId: state.PersonId
						});

						state.Shift = state.Shift || [];
						state.OutOfAdherences = state.OutOfAdherences || [];

						var now = moment(states.Time);
						var timeInfo = {
							time: states.Time,
							windowStart: now.clone().add(-1, 'hours'),
							windowEnd: now.clone().add(3, 'hours')
						};

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
								TimeOutOfAdherence: getTimeOutOfAdherence(state, timeInfo),
								OutOfAdherences: getOutOfAdherences(state, timeInfo),
								ShiftTimeBar: getShiftTimeBar(state),
								Shift: getShift(state, timeInfo)
							});

							if ($scope.stateGroups
								.map(function(sg) {
									return sg.StateId;
								})
								.indexOf(state.StateId) === -1) {
								$scope.stateGroups.push({
									StateId: state.StateId,
									State: state.State,
									Selected: true,
								})
							}
						}
					});
				}

				function getTimeOutOfAdherence(state, timeInfo) {
					if (state.OutOfAdherences.length > 0) {
						var lastOOA = state.OutOfAdherences[state.OutOfAdherences.length - 1];
						if (lastOOA.EndTime == null) {
							var seconds = moment(timeInfo.time).diff(moment(lastOOA.StartTime), 'seconds');
							return $scope.formatDuration(seconds);
						}
					}
				}

				function getOutOfAdherences(state, timeInfo) {
					return state.OutOfAdherences
						.filter(function(t) {
							return t.EndTime == null || moment(t.EndTime) > timeInfo.windowStart;
						})
						.map(function(t) {
							var endTime = t.EndTime || timeInfo.time;
							return {
								Offset: Math.max(timeToPercent(timeInfo.time, t.StartTime), 0) + '%',
								Width: Math.min(timePeriodToPercent(timeInfo.windowStart, t.StartTime, endTime), 100) + "%",
								StartTime: moment(t.StartTime).format('HH:mm:ss'),
								EndTime: t.EndTime ? moment(t.EndTime).format('HH:mm:ss') : null,
							};
						});
				}

				function getShiftTimeBar(state) {
					var percentForTimeBar = function(seconds) {
						return Math.min(secondsToPercent(seconds), 25);
					}
					return (state.TimeInAlarm ? percentForTimeBar(state.TimeInRule) : 0) + "%";
				}

				function getShift(state, timeInfo) {
					return state.Shift
						.filter(function(layer) {
							return timeInfo.windowStart < moment(layer.EndTime) && timeInfo.windowEnd > moment(layer.StartTime);
						})
						.map(function(s) {
							return {
								Color: s.Color,
								Offset: Math.max(timeToPercent(timeInfo.time, s.StartTime), 0) + '%',
								Width: Math.min(timePeriodToPercent(timeInfo.windowStart, s.StartTime, s.EndTime), 100) + "%",
								Name: s.Name,
								Class: getClassForActivity(timeInfo.time, s.StartTime, s.EndTime)
							};
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

				function setupPolling() {
					polling = $interval(function() {
						updateStates();
					}, 5000);
				}

				function cancelPolling() {
					if (polling != null)
						$interval.cancel(polling);
				}

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

				$scope.changeScheduleUrl = function(personId) {
					return RtaRouteService.urlForChangingSchedule(personId);
				};

				$scope.agentDetailsUrl = function(personId) {
					return RtaRouteService.urlForAgentDetails(personId);
				};

				function getAgents() {
					var deferred = $q.defer();
					if (skillAreaId) {
						getSkillAreaInfo()
							.then(function() {
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
				};

				function getSkillAreaInfo() {
					return RtaService.getSkillArea(skillAreaId)
						.then(function(skillArea) {
							$scope.skillAreaName = skillArea.Name || '?';
							$scope.skillArea = true;
							skillIds = skillArea.Skills.map(function(skill) {
								return skill.Id;
							});
						});
				}

				$scope.$watch('filterText', filterData);

				function filterData() {
					if ($scope.filterText === undefined)
						$scope.filteredData = $scope.agents;
					else
						$scope.filteredData = $filter('agentFilter')($scope.agents, $scope.filterText, propertiesForFiltering);
					if ($scope.agentsInAlarm) {
						$scope.filteredData = $filter('filter')($scope.filteredData, {
							TimeInAlarm: ''
						});
						$scope.openedMaxNumberOfAgents = ($scope.filteredData.length === $scope.maxNumberOfAgents);
						if (!$scope.notifySwitchDisabled && $scope.agents.length > $scope.maxNumberOfAgents) {
							NoticeService.warning($translate.instant('Viewing agents out of alarm is not possible due to high number of agents. The switch is enabled for maximum ' + $scope.maxNumberOfAgents + ' agents'), null, true);
							$scope.notifySwitchDisabled = true;
						}

					}
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

				(function getSkillName() {
					if (skillIds.length === 1) {
						RtaService.getSkillName(skillIds[0])
							.then(function(skill) {
								$scope.skillName = skill.Name || '?';
								$scope.skill = true;
							});
					}
				})();

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

				function updateBreadCrumb(agentsInfo) {
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

				$scope.goToOverview = function() {
					$state.go('rta');
				}

				$scope.goToSelectItem = function() {
					$state.go('rta.select-skill');
				}

				$scope.$on('$destroy', function() {
					cancelPolling();
				});

				$scope.rightPanelOptions = {
					panelState: false,
					panelTitle: " ",
					showCloseButton: true,
					showBackdrop: true,
					showResizer: true,
					showPopupButton: true
				};

			}
		]);
})();
