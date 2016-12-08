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
			'$location',
			'RtaService',
			'RtaGridService',
			'RtaFormatService',
			'RtaRouteService',
			'FakeTimeService',
			'Toggle',
			'NoticeService',
			'$timeout',
			function($scope,
				$filter,
				$state,
				$stateParams,
				$interval,
				$sessionStorage,
				$q,
				$translate,
				$location,
				RtaService,
				RtaGridService,
				RtaFormatService,
				RtaRouteService,
				FakeTimeService,
				toggleService,
				NoticeService,
				$timeout
			) {
				var selectedPersonId, lastUpdate, notice;
				var polling = null;
				var siteIds = $stateParams.siteIds || [];
				var teamIds = $stateParams.teamIds || [];
				var skillIds = $stateParams.skillIds || [];
				var skillAreaId = $stateParams.skillAreaId || undefined;
				var excludedStatesFromUrl = function() {
					return $stateParams.es || []
				};
				var propertiesForFiltering = ["Name", "State", "Activity", "Alarm", "SiteAndTeamName"];
				$scope.adherence = {};
				$scope.adherencePercent = null;
				$scope.filterText = "";
				$scope.timestamp = "";
				$scope.agents = [];
				$scope.states = [];
				$scope.filteredData = [];
				$scope.format = RtaFormatService.formatDateTime;
				$scope.formatDuration = RtaFormatService.formatDuration;
				$scope.hexToRgb = RtaFormatService.formatHexToRgb;
				$scope.agentDetailsUrl = RtaRouteService.urlForAgentDetails;
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
				$scope.pollingLock = true;
				// because angular cant handle an array of null in stateparams
				var nullStateId = "noState";

				// select skill dependency
				$scope.skills = [];
				$scope.skillAreas = [];
				$scope.skillsLoaded = false;
				$scope.skillAreasLoaded = false;
				$scope.teamsSelected = [];
				var enableWatchOnTeam = false;
				toggleService.togglesLoaded.then(function() {
					$scope.showOrgSelection = toggleService.RTA_QuicklyChangeAgentsSelection_40610;
					if ($scope.showOrgSelection)
						RtaService.getOrganization()
						.then(function(organization) {
							$scope.sites = organization;
							keepSelectionForOrganization();
						});
				});

				function stateGoToAgents(selection) {
					var stateName = $scope.showOrgSelection ? 'rta.select-skill' : 'rta.agents';
					var options = $scope.showOrgSelection ? {
						reload: true,
						notify: true
					} : {};
					$state.go(stateName, selection, options);
				}

				RtaService.getSkills()
					.then(function(skills) {
						$scope.skillsLoaded = true;
						$scope.skills = skills;
						if (skillIds.length > 0 && skillAreaId == null)
							$scope.selectedSkill = getSelected(skills, skillIds[0]);
					});

					RtaService.getSkillAreas()
						.then(function(skillAreas) {
							$scope.skillAreasLoaded = true;
							$scope.skillAreas = skillAreas.SkillAreas;
							if (skillAreaId != null && skillIds.length == 0)
								$scope.selectedSkillArea = getSelected($scope.skillAreas, skillAreaId);
						});

				function getSelected(outOf, shouldMatch) {
					return outOf.find(function(o) {
						return o.Id === shouldMatch;
					})
				};

				$scope.querySearch = function(query, myArray) {
					var results = query ? myArray.filter(createFilterFor(query)) : myArray;
					return results;
				};

				function createFilterFor(query) {
					var lowercaseQuery = angular.lowercase(query);
					return function filterFn(item) {
						var lowercaseName = angular.lowercase(item.Name);
						return (lowercaseName.indexOf(lowercaseQuery) === 0);
					};
				};

				$scope.selectedSkillChange = function(skill) {
					if (!skill) return;
					$scope.skillId = skill.Id;
					stateGoToAgents({
						skillIds: skill.Id,
						skillAreaId: undefined
					});
				}

				$scope.selectedSkillAreaChange = function(skillArea) {
					if (!skillArea) return;
						stateGoToAgents({
							skillAreaId: skillArea.Id,
							skillIds: []
						});
				}

				var selectedSiteId;

				$scope.expandSite = function(siteId) {
					selectedSiteId = $scope.isSiteToBeExpanded(siteId) ? '' : siteId;
				};

				$scope.isSiteToBeExpanded = function(siteId) {
					return selectedSiteId === siteId;
				};


				$scope.goToAgents = function() {
					var selection = {};
					var selectedSiteIds = $scope.selectedSites();
					var selectedTeamIds = $scope.teamsSelected;
					selection['siteIds'] = selectedSiteIds;
					selection['teamIds'] = selectedTeamIds;
					stateGoToAgents(selection);
				}

				function keepSelectionForOrganization() {
					if (!$scope.showOrgSelection)
						return;
					selectSiteAndTeamsUnder();

					if (teamIds.length > 0)
						$scope.teamsSelected = teamIds;
					enableWatchOnTeam = true;
				}

				function selectSiteAndTeamsUnder() {
					if(siteIds.length === 0)
						return;
					siteIds.forEach(function(sid) {
						var theSite = $scope.sites.find(function(site) {
							return site.Id == sid;
						});
						theSite.isChecked = true;
						theSite.Teams.forEach(function(team) {
							team.isChecked = true;
						});
					});
				}

				$scope.selectedSites = function() {
					return $scope.sites
						.filter(function(site) {
							var selectedTeams = site.Teams.filter(function(team) {
								return team.isChecked == true;
							});
							var noTeamsSelected = selectedTeams.length === 0
							var allTeamsSelected = selectedTeams.length == site.Teams.length;
							if (noTeamsSelected)
								return false;
							if (site.isChecked && allTeamsSelected)
								unselectTeamsInSite(site);
							return site.isChecked && allTeamsSelected;
						}).map(function(s) {
							return s.Id;
						});
				}

				function unselectTeamsInSite(site) {
					site.Teams.forEach(function(team) {
						var index = $scope.teamsSelected.indexOf(team.Id);
						if (index > -1) {
							$scope.teamsSelected.splice(index, 1);
						}
					});
				}

				$scope.teamChecked = function(site, team) {
					var selectedTeamsChecked = site.Teams.filter(function(t) {
						return t.isChecked;
					});
					var isAllTeamsCheckedForSite = selectedTeamsChecked.length === site.Teams.length;
					site.isMarked = selectedTeamsChecked.length > 0 && !isAllTeamsCheckedForSite && !site.isChecked ? true : false;
					if (site.isChecked)
						return true;
					if (isAllTeamsCheckedForSite)
						return false;
					return team.isChecked;
				}

				$scope.forTest_selectSite = function(site) {
					site.isChecked = !site.isChecked;
					var selectedSite = $scope.sites.find(function(s) {
						return s.Id === site.Id;
					});

					selectedSite.Teams.forEach(function(team) {
						team.isChecked = selectedSite.isChecked;
					});
				}

				$scope.forTest_selectTeam = function(teams) {
					setTimeout(function(){
						$scope.teamsSelected = teams;
					}, 1000);
				}

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

				$scope.updateSite = function(oldTeamsSelected) {
					$scope.sites.forEach(function(site) {
						var anyChangeForThatSite = false;
						site.Teams.forEach(function(team) {
							team.isChecked = $scope.teamsSelected.indexOf(team.Id) > -1;
							var teamChanged = ($scope.teamsSelected.indexOf(team.Id) > -1 !== oldTeamsSelected.indexOf(team.Id) > -1);
							if(oldTeamsSelected.length > 0 && teamChanged){
								anyChangeForThatSite = true;
							}
						});

						var checkedTeams = site.Teams.filter(function(team) {
							return team.isChecked;
						});

						if(checkedTeams.length > 0 || anyChangeForThatSite)
								site.isChecked = checkedTeams.length === site.Teams.length;
					});
				};

				$scope.$watch('teamsSelected', function(newValue, oldValue) {
					if (JSON.stringify(newValue) !== JSON.stringify(oldValue) && enableWatchOnTeam) {
						$scope.updateSite(oldValue);
					}
				});

				(function initialize() {
					$scope.pollingLock = false;
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
								$scope.pollingLock = true;
							})
							.then(updateStates)
							.then(updatePhoneStatesFromStateParams);
					}
				})();

				function updateStates() {
					if ($scope.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId))
						return;
					var excludedStates = excludedStateIds();
					var excludeStates = excludedStates.length > 0;
					getStates($scope.agentsInAlarm, excludeStates)({
							siteIds: siteIds,
							teamIds: teamIds,
							skillIds: skillIds,
							skillAreaId: skillAreaId,
							excludedStateIds: excludedStates.map(function(s) {
								return s === nullStateId ? null : s;
							})
						})
						.then(setStatesInAgents)
						.then(updateUrlWithExcludedStateIds(excludedStates));
				}


				function getStates(inAlarm, excludeStates) {
					if (!inAlarm)
						return RtaService.statesFor;
					if (excludeStates)
						return RtaService.inAlarmExcludingPhoneStatesFor;
					return RtaService.inAlarmFor;
				};

				function updatePhoneStatesFromStateParams() {
					var stateIds = excludedStatesFromUrl();
					addNoStateIfNeeded(stateIds);
					getStateNamesForAnyExcludedStates(stateIds)
				}

				function addNoStateIfNeeded(stateIds) {
					if (stateIds.indexOf(nullStateId) > -1 &&
						$scope.states.filter(function(s) {
							return s.Id === nullStateId;
						}).length === 0) {
						$scope.states.push({
							Id: nullStateId,
							Name: "No State",
							Selected: false
						});
						sortStatesByName();
					}
				}

				function getStateNamesForAnyExcludedStates(stateIds) {
					var stateIdsWithoutNull = stateIds.filter(function(s) {
						return s !== nullStateId;
					})
					if (stateIdsWithoutNull.length !== 0) {
						RtaService.getPhoneStates(stateIdsWithoutNull)
							.then(function(states) {
								$scope.states = $scope.states.concat(states.PhoneStates);
								sortStatesByName();
							});
					}
				}

				function excludedStateIds() {
					var included = $scope.states
						.filter(function(s) {
							return s.Selected === true;
						})
						.map(function(s) {
							return s.Id;
						});
					var excludedViaUrlAndNotManuallyIncluded = excludedStatesFromUrl()
						.filter(function(s) {
							return included.indexOf(s) === -1;
						});
					var excluded = $scope.states
						.filter(function(s) {
							return s.Selected === false;
						})
						.map(function(s) {
							return s.Id;
						});
					var excludedUnique = excludedViaUrlAndNotManuallyIncluded
						.filter(function(s) {
							return excluded.indexOf(s) === -1;
						});
					var excludedStateIds = excludedUnique.concat(excluded);
					return excludedStateIds;
				}

				function setStatesInAgents(states) {
					$scope.agents = [];
					lastUpdate = states.Time;
					fillAgentsWithState(states);
					fillAgentsWithoutState();
					buildTimeline(states);
					$scope.isLoading = false;
					$scope.pollingLock = true;
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

							state.StateId = state.StateId || nullStateId;
							state.State = state.State || "No State";
							if ($scope.states
								.map(function(s) {
									return s.Id;
								})
								.indexOf(state.StateId) === -1) {
								$scope.states.push({
									Id: state.StateId,
									Name: state.State,
									Selected: excludedStatesFromUrl().indexOf(state.StateId) == -1
								})
							}
						}
					});
					sortStatesByName();
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
						if ($scope.pollingLock) {
							$scope.pollingLock = false;
							updateStates();
						}

					}, 5000);
				}

				function cancelPolling() {
					if (polling != null) {
						$interval.cancel(polling);
						$scope.pollingLock = true;
					}
				}

				$scope.getTableHeight = function() {
					var rowHeight = 30;
					var headerHeight = 30;
					var agentMenuHeight = 45;
					return {
						height: ($scope.filteredData.length * rowHeight + headerHeight + agentMenuHeight + rowHeight / 2) + "px"
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
				$scope.historicalAdherenceUrl = function(personId) {
					return RtaRouteService.urlForHistoricalAdherence(personId);
				};

				$scope.agentDetailsUrl = function(personId) {
					return RtaRouteService.urlForAgentDetails(personId);
				};

				function getAgents() {
					var deferred = $q.defer();
					if (skillAreaId) {
						getSkillAreaInfo()
							.then(function() {
								deferred.resolve(RtaService.agentsFor);
							});
					} else {

						deferred.resolve(RtaService.agentsFor);
					}
					return deferred.promise;
				};

				function getSkillAreaInfo() {
					return RtaService.getSkillArea(skillAreaId)
						.then(function(skillArea) {
							if (skillArea.Skills != null) {
								$scope.skillAreaName = skillArea.Name || '?';
								$scope.skillArea = true;
								skillIds = skillArea.Skills.map(function(skill) {
									return skill.Id;
								});
							}
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
							NoticeService.warning($translate.instant('It is possible to view maximum ' + $scope.maxNumberOfAgents + ' agents. The "In alarm" switch is enabled if the number of agents does not exceed ' + $scope.maxNumberOfAgents + '.'), null, true);
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
						$scope.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
						$scope.siteName = "Multiple Sites";
					} else if (teamIds.length > 1 || (siteIds.length === 1 && teamIds.length !== 1)) {
						$scope.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
						$scope.teamName = "Multiple Teams";
					} else if (agentsInfo.length > 0) {
						$scope.siteName = agentsInfo[0].SiteName;
						$scope.teamName = agentsInfo[0].TeamName;
						$scope.goBackToTeamsWithUrl = urlForTeamsInBreadcrumbs(agentsInfo);
						$scope.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
						$scope.showPath = true;
					}
				};

				function updateUrlWithExcludedStateIds(excludedStates) {
					$state.go($state.current.name, {
						es: excludedStates
					}, {
						notify: false
					});
				};

				function urlForTeamsInBreadcrumbs(agentsInfo) {
					if (skillAreaId != null)
						return RtaRouteService.urlForTeamsBySkillArea(agentsInfo[0].SiteId, skillAreaId);
					if (skillIds.length > 0)
						return RtaRouteService.urlForTeamsBySkills(agentsInfo[0].SiteId, skillIds[0]);
					return RtaRouteService.urlForTeams(agentsInfo[0].SiteId);
				}

				function urlForRootInBreadcrumbs(agentsInfo) {
					if (skillAreaId != null)
						return RtaRouteService.urlForSitesBySkillArea(skillAreaId);
					if (skillIds.length > 0)
						return RtaRouteService.urlForSitesBySkills(skillIds[0]);
					return RtaRouteService.urlForSites();
				}

				function sortStatesByName() {
					$scope.states = $filter('orderBy')($scope.states, function(state) {
						return state.Name;
					});
				};

				$scope.goToOverview = function() {
					RtaRouteService.goToSites();
				}

				$scope.goToSelectItem = function() {
					RtaRouteService.goToSelectSkill();
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
