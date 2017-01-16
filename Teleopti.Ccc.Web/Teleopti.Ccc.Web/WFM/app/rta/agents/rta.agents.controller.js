(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController', RtaAgentsController);

	RtaAgentsController.$inject =
		[
			'$scope',
			'$filter',
			'$state',
			'$stateParams',
			'$interval',
			'$sessionStorage',
			'$q',
			'$translate',
			'$location',
			'rtaService',
			'rtaGridService',
			'rtaFormatService',
			'rtaRouteService',
			'fakeTimeService',
			'Toggle',
			'NoticeService',
			'$timeout'
		];

	function RtaAgentsController(
		$scope,
		$filter,
		$state,
		$stateParams,
		$interval,
		$sessionStorage,
		$q,
		$translate,
		$location,
		rtaService,
		rtaGridService,
		rtaFormatService,
		rtaRouteService,
		fakeTimeService,
		toggleService,
		NoticeService,
		$timeout
	) {

		var vm = this;

		var selectedPersonId, lastUpdate, notice, selectedSiteId;;
		var polling = null;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var excludedStatesFromUrl = function () {
			return $stateParams.es || []
		};
		var propertiesForFiltering = ["Name", "State", "Activity", "Alarm", "SiteAndTeamName"];
		vm.adherence = {};
		vm.adherencePercent = null;
		vm.filterText = "";
		vm.timestamp = "";
		vm.agents = [];
		vm.states = [];
		vm.filteredData = [];
		vm.format = rtaFormatService.formatDateTime;
		vm.formatDuration = rtaFormatService.formatDuration;
		vm.formatToSeconds = rtaFormatService.formatToSeconds;
		vm.hexToRgb = rtaFormatService.formatHexToRgb;
		vm.agentDetailsUrl = rtaRouteService.urlForAgentDetails;
		vm.agentsInAlarm = !$stateParams.showAllAgents;
		var allGrid = rtaGridService.makeAllGrid();
		allGrid.data = 'vm.filteredData';
		vm.allGrid = allGrid;
		var inAlarmGrid = rtaGridService.makeInAlarmGrid();
		inAlarmGrid.data = 'vm.filteredData';
		vm.inAlarmGrid = inAlarmGrid;
		vm.pause = false;
		vm.pausedAt = null;
		vm.showPath = false;
		vm.notifySwitchDisabled = false;
		vm.showBreadcrumb = siteIds.length > 0 || teamIds.length > 0 || skillIds === [];
		vm.skill = false;
		vm.skillArea = false;
		vm.skillName = "";
		vm.skillAreaName = "";
		vm.openedMaxNumberOfAgents = false;
		vm.maxNumberOfAgents = 50;
		vm.isLoading = angular.toJson($stateParams) !== '{}';
		console.info(vm.isLoading);
		vm.pollingLock = true;
		// because angular cant handle an array of null in stateparams
		var nullStateId = "noState";

		// select skill dependency
		vm.skills = [];
		vm.skillAreas = [];
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.teamsSelected = [];
		var enableWatchOnTeam = false;
		vm.selectFieldText = 'Select organization';
		vm.searchTerm = "";

		var updateStatesDelegate = updateStates;

		toggleService.togglesLoaded.then(function () {
			vm.showOrgSelection = toggleService.RTA_QuicklyChangeAgentsSelection_40610;
			if (vm.showOrgSelection)
				rtaService.getOrganization()
					.then(function (organization) {
						vm.sites = organization;
						keepSelectionForOrganization();
					});
		});

		function stateGoToAgents(selection) {
			var stateName = vm.showOrgSelection ? 'rta.select-skill' : 'rta.agents';
			var options = vm.showOrgSelection ? {
				reload: true,
				notify: true
			} : {};
			$state.go(stateName, selection, options);
		}

		rtaService.getSkills()
			.then(function (skills) {
				vm.skillsLoaded = true;
				vm.skills = skills;
				if (skillIds.length > 0 && skillAreaId == null)
					vm.selectedSkill = getSelected(skills, skillIds[0]);
			});

		rtaService.getSkillAreas()
			.then(function (skillAreas) {
				vm.skillAreasLoaded = true;
				vm.skillAreas = skillAreas.SkillAreas;
				if (skillAreaId != null)
					vm.selectedSkillArea = getSelected(vm.skillAreas, skillAreaId);
			});

		function getSelected(outOf, shouldMatch) {
			return outOf.find(function (o) {
				return o.Id === shouldMatch;
			})
		};

		vm.querySearch = function (query, myArray) {
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

		vm.selectedSkillChange = function (skill) {
			var selectedSkillId = undefined;
			var selectedSkillAreaId = skillAreaId;
			if (skill) {
				selectedSkillId = skill.Id;
				selectedSkillAreaId = undefined
			}

			if (!skill || (skill.Id != skillIds[0] || $stateParams.skillAreaId))
				stateGoToAgents({
					skillIds: selectedSkillId,
					skillAreaId: selectedSkillAreaId,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		vm.selectedSkillAreaChange = function (skillArea) {
			var selectedSkillId = $stateParams.skillIds;
			var selectedSkillAreaId = undefined;
			if (skillArea) {
				selectedSkillId = [];
				selectedSkillAreaId = skillArea.Id
			}

			if (!skillArea || !(skillArea.Id == $stateParams.skillAreaId))
				stateGoToAgents({
					skillAreaId: selectedSkillAreaId,
					skillIds: selectedSkillId,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		vm.expandSite = function (site) {
			site.isExpanded = !site.isExpanded;
		};

		vm.goToAgents = function () {
			var selection = {};
			var selectedSiteIds = vm.selectedSites();
			var selectedTeamIds = vm.teamsSelected;
			selection['siteIds'] = selectedSiteIds;
			selection['teamIds'] = selectedTeamIds;
			stateGoToAgents(selection);
		}

		function keepSelectionForOrganization() {

			if (!vm.showOrgSelection)
				return;
			selectSiteAndTeamsUnder();

			if (teamIds.length > 0)
				vm.teamsSelected = teamIds;
			enableWatchOnTeam = true;
			updateSelectFieldText();
		}

		function countTeamsSelected() {
			var checkedTeamsCount = 0;
			vm.sites.forEach(function (site) {
				if (siteIds.indexOf(site.Id) > -1) {
					checkedTeamsCount = checkedTeamsCount + site.Teams.length;
				}
			});
			checkedTeamsCount = checkedTeamsCount + vm.teamsSelected.length;
			return checkedTeamsCount;
		}

		function selectSiteAndTeamsUnder() {
			if (siteIds.length === 0)
				return;
			siteIds.forEach(function (sid) {
				var theSite = vm.sites.find(function (site) {
					return site.Id == sid;
				});
				theSite.isChecked = true;
				theSite.Teams.forEach(function (team) {
					team.isChecked = true;
				});
			});
		}

		vm.selectedSites = function () {
			return vm.sites
				.filter(function (site) {
					var selectedTeams = site.Teams.filter(function (team) {
						return team.isChecked == true;
					});
					var noTeamsSelected = selectedTeams.length === 0
					var allTeamsSelected = selectedTeams.length == site.Teams.length;
					if (noTeamsSelected)
						return false;
					if (site.isChecked && allTeamsSelected)
						unselectTeamsInSite(site);
					return site.isChecked && allTeamsSelected;
				}).map(function (s) {
					return s.Id;
				});
		}

		function unselectTeamsInSite(site) {
			site.Teams.forEach(function (team) {
				var index = vm.teamsSelected.indexOf(team.Id);
				if (index > -1) {
					vm.teamsSelected.splice(index, 1);
				}
			});
		}

		vm.teamChecked = function (site, team) {
			var selectedTeamsChecked = site.Teams.filter(function (t) {
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

		vm.forTest_selectSite = function (site) {
			site.isChecked = !site.isChecked;
			var selectedSite = vm.sites.find(function (s) {
				return s.Id === site.Id;
			});

			selectedSite.Teams.forEach(function (team) {
				team.isChecked = selectedSite.isChecked;
				if (selectedSite.isChecked) {
					vm.teamsSelected.push(team.Id);
				}
				else {
					var index = vm.teamsSelected.indexOf(team.Id);
					vm.teamsSelected.splice(index, 1);
				}
			});
		}

		$scope.$watch(function () {
			return vm.pause;
		}, function () {
			if (vm.pause) {
				vm.pausedAt = moment(lastUpdate).format('YYYY-MM-DD HH:mm:ss');
				var template = $translate.instant('RtaPauseEnabledNotice')
				var noticeText = template.replace('{0}', vm.pausedAt)
				notice = NoticeService.info(noticeText, null, true);
				cancelPolling();
			} else {
				vm.pausedAt = null;
				if (notice != null) {
					notice.destroy();
				}
				NoticeService.info($translate.instant('RtaPauseDisableNotice'), 5000, true);
				setupPolling();
			}
		});

		$scope.$watch(function () {
			return vm.agentsInAlarm;
		}, function (newValue, oldValue) {
			if (newValue !== oldValue) {
				updateStates();
				filterData();
				if (newValue && vm.pause) {
					vm.filteredData.sort(function (a, b) {
						return vm.formatToSeconds(b.TimeInAlarm) - vm.formatToSeconds(a.TimeInAlarm);
					});
				}
			}
		});

		vm.updateSite = function (oldTeamsSelected) {
			vm.sites.forEach(function (site) {
				var anyChangeForThatSite = false;
				site.Teams.forEach(function (team) {
					team.isChecked = vm.teamsSelected.indexOf(team.Id) > -1;
					var teamChanged = (vm.teamsSelected.indexOf(team.Id) > -1 !== oldTeamsSelected.indexOf(team.Id) > -1);
					if (oldTeamsSelected.length > 0 && teamChanged) {
						anyChangeForThatSite = true;
					}
				});

				var checkedTeams = site.Teams.filter(function (team) {
					return team.isChecked;
				});

				if (checkedTeams.length > 0 || anyChangeForThatSite)
					site.isChecked = checkedTeams.length === site.Teams.length;
			});
		};

		vm.clearOrgSelection = function () {
			vm.sites.forEach(function (site) {
				if (site.isChecked)
					site.isChecked = false;
			});
			vm.teamsSelected = [];
			vm.selectFieldText = 'Select organization';
		};

		vm.clearSearchTerm = function () {
			vm.searchTerm = '';
		}

		$scope.$watch(function () {
			return vm.teamsSelected;
		}, function (newValue, oldValue) {
			if (angular.toJson(newValue) !== angular.toJson(oldValue) && enableWatchOnTeam) {
				vm.updateSite(oldValue);
			}
		});

		function updateSelectFieldText() {
			var howManyTeamsSelected = countTeamsSelected();
			vm.selectFieldText = howManyTeamsSelected === 0 ? 'Select organization' : howManyTeamsSelected + ' teams selected';
		}

		(function initialize() {
			vm.pollingLock = false;
			if (siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId) {
				toggleService.togglesLoaded.then(function () {
					if (toggleService.RTA_FasterAgentsView_42039) {
						getAgentStates()
							.then(function (fn) {
								return fn({
									siteIds: siteIds,
									teamIds: teamIds,
									skillIds: skillIds,
									skillAreaId: skillAreaId
								});
							})
							.then(function (agentsInfo) {
								console.log('agents info', agentsInfo);
								vm.agentsInfo = agentsInfo.States;
								vm.agents = agentsInfo.States;
								$scope.$watchCollection(function () {
									return vm.agents;
								}, filterData);
								updateBreadCrumb(vm.agentsInfo);
								vm.pollingLock = true;
								return agentsInfo;
							})
							.then(function (data) {
								updateStates2(data);
							})
							.then(updatePhoneStatesFromStateParams);

						updateStatesDelegate = function () {
							getAgentStates()
								.then(function (fn) {
									return fn({
										siteIds: siteIds,
										teamIds: teamIds,
										skillIds: skillIds,
										skillAreaId: skillAreaId
									});
								})
								.then(function (agentsInfo) {
									vm.agentsInfo = agentsInfo;
									vm.agents = agentsInfo;
									$scope.$watchCollection(function () {
										return vm.agents;
									}, filterData);
									// REMOVE ME!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11111111111111111112222
									updateBreadCrumb(agentsInfo);
									vm.pollingLock = true;
									return agentsInfo;
								})
								.then(function (data) {
									updateStates2(data);
								})
								.then(updatePhoneStatesFromStateParams);
						}
					}





					else {
				getAgents()
					.then(function (fn) {
						return fn({
							siteIds: siteIds,
							teamIds: teamIds,
							skillIds: skillIds,
							skillAreaId: skillAreaId
						});
					})
					.then(function (agentsInfo) {
						vm.agentsInfo = agentsInfo;
						vm.agents = agentsInfo;
						$scope.$watchCollection(function () {
							return vm.agents;
						}, filterData);
						updateBreadCrumb(agentsInfo);
						vm.pollingLock = true;
					})
					.then(updateStates)
					.then(updatePhoneStatesFromStateParams);
			}
				});

			}
		})();

		function updateStates2(agentStates) {
			if (vm.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId))
				return;
			var excludedStates = excludedStateIds();
			var excludeStates = excludedStates.length > 0;
			setStatesInAgents2(agentStates);
			updateUrlWithExcludedStateIds(excludedStates);
		}

		function updateStates() {
			if (vm.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId))
				return;
			var excludedStates = excludedStateIds();
			var excludeStates = excludedStates.length > 0;
			getStates(vm.agentsInAlarm, excludeStates)({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId,
				excludedStateIds: excludedStates.map(function (s) {
					return s === nullStateId ? null : s;
				})
			})
				.then(setStatesInAgents)
				.then(updateUrlWithExcludedStateIds(excludedStates));
		}


		function getStates(inAlarm, excludeStates) {
			if (!inAlarm)
				return rtaService.statesFor;
			if (excludeStates)
				return rtaService.inAlarmExcludingPhoneStatesFor;
			return rtaService.inAlarmFor;
		};

		function updatePhoneStatesFromStateParams() {
			var stateIds = excludedStatesFromUrl();
			addNoStateIfNeeded(stateIds);
			getStateNamesForAnyExcludedStates(stateIds);
		}

		function addNoStateIfNeeded(stateIds) {
			if (stateIds.indexOf(nullStateId) > -1 &&
				vm.states.filter(function (s) {
					return s.Id === nullStateId;
				}).length === 0) {
				vm.states.push({
					Id: nullStateId,
					Name: "No State",
					Selected: false
				});
				sortStatesByName();
			}
		}

		function getStateNamesForAnyExcludedStates(stateIds) {
			var stateIdsWithoutNull = stateIds.filter(function (s) {
				return s !== nullStateId;
			})
			if (stateIdsWithoutNull.length !== 0) {
				rtaService.getPhoneStates(stateIdsWithoutNull)
					.then(function (states) {
						vm.states = vm.states.concat(states.PhoneStates);
						sortStatesByName();
					});
			}
		}

		function excludedStateIds() {
			var included = vm.states
				.filter(function (s) {
					return s.Selected === true;
				})
				.map(function (s) {
					return s.Id;
				});
			var excludedViaUrlAndNotManuallyIncluded = excludedStatesFromUrl()
				.filter(function (s) {
					return included.indexOf(s) === -1;
				});
			var excluded = vm.states
				.filter(function (s) {
					return s.Selected === false;
				})
				.map(function (s) {
					return s.Id;
				});
			var excludedUnique = excludedViaUrlAndNotManuallyIncluded
				.filter(function (s) {
					return excluded.indexOf(s) === -1;
				});
			var excludedStateIds = excludedUnique.concat(excluded);
			return excludedStateIds;
		}

		function setStatesInAgents2(states) {
			vm.agents = [];
			lastUpdate = states.Time;
			fillAgentsWithState2(states);
			buildTimeline(states);
			vm.isLoading = false;
			vm.pollingLock = true;
		}

		function setStatesInAgents(states) {
			vm.agents = [];
			lastUpdate = states.Time;
			fillAgentsWithState(states);
			fillAgentsWithoutState();
			buildTimeline(states);
			vm.isLoading = false;
			vm.pollingLock = true;
		}


		function fillAgentsWithState2(states) {
			states.States.forEach(function (state, i) {
				state.Shift = state.Shift || [];
				state.OutOfAdherences = state.OutOfAdherences || [];

				var now = moment(states.Time);
				var timeInfo = {
					time: states.Time,
					windowStart: now.clone().add(-1, 'hours'),
					windowEnd: now.clone().add(3, 'hours')
				};

				vm.agents.push({
					PersonId: state.PersonId,
					Name: state.Name,
					SiteAndTeamName: state.SiteName + '/' + state.TeamName,
					TeamName: state.TeamName,
					SiteName: state.SiteName,
					PersonId: state.PersonId,
					TeamId: state.TeamId,
					State: state.State,
					Activity: state.Activity,
					NextActivity: state.NextActivity,
					NextActivityStartTime: state.NextActivityStartTime,
					Alarm: state.Alarm,
					Color: state.Color,
					TimeInState: state.TimeInState,
					TimeInAlarm: getTimeInAlarm(state),
					TimeInRule: state.TimeInAlarm ? state.TimeInRule : null,
					TimeOutOfAdherence: getTimeOutOfAdherence(state, timeInfo),
					OutOfAdherences: getOutOfAdherences(state, timeInfo),
					ShiftTimeBar: getShiftTimeBar(state),
					Shift: getShift(state, timeInfo)
				});

				state.StateId = state.StateId || nullStateId;
				state.State = state.State || "No State";
				if (vm.states
					.map(function (s) {
						return s.Id;
					})
					.indexOf(state.StateId) === -1) {
					vm.states.push({
						Id: state.StateId,
						Name: state.State,
						Selected: excludedStatesFromUrl().indexOf(state.StateId) == -1
					})
				}

			});
			sortStatesByName();
		}

		function fillAgentsWithState(states) {
			states.States.forEach(function (state, i) {
				var agentInfo = $filter('filter')(vm.agentsInfo, {
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
					vm.agents.push({
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
						TimeInAlarm: getTimeInAlarm(state),
						TimeInRule: state.TimeInAlarm ? state.TimeInRule : null,
						TimeOutOfAdherence: getTimeOutOfAdherence(state, timeInfo),
						OutOfAdherences: getOutOfAdherences(state, timeInfo),
						ShiftTimeBar: getShiftTimeBar(state),
						Shift: getShift(state, timeInfo)
					});

					state.StateId = state.StateId || nullStateId;
					state.State = state.State || "No State";
					if (vm.states
						.map(function (s) {
							return s.Id;
						})
						.indexOf(state.StateId) === -1) {
						vm.states.push({
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
					return vm.formatDuration(seconds);
				}
			}
		}

		function getTimeInAlarm(state) {
			if (state.TimeInAlarm !== null)
				return vm.formatDuration(state.TimeInAlarm);
		}

		function getOutOfAdherences(state, timeInfo) {
			return state.OutOfAdherences
				.filter(function (t) {
					return t.EndTime == null || moment(t.EndTime) > timeInfo.windowStart;
				})
				.map(function (t) {
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
			var percentForTimeBar = function (seconds) {
				return Math.min(secondsToPercent(seconds), 25);
			}
			return (state.TimeInAlarm ? percentForTimeBar(state.TimeInRule) : 0) + "%";
		}

		function getShift(state, timeInfo) {
			return state.Shift
				.filter(function (layer) {
					return timeInfo.windowStart < moment(layer.EndTime) && timeInfo.windowEnd > moment(layer.StartTime);
				})
				.map(function (s) {
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
			vm.agentsInfo.forEach(function (agentInfo) {
				var agentFilled = $filter('filter')(vm.agents, {
					PersonId: agentInfo.PersonId
				});
				if (agentFilled.length === 0)
					vm.agents.push({
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
			var timeline = function (time) {
				var percent = timeToPercent(states.Time, time);
				if (percent <= 94)
					return {
						Time: time.format('HH:mm'),
						Offset: percent + "%"
					};
			};

			var time = moment(states.Time).startOf('hour');
			vm.timeline = [
				timeline(time),
				timeline(time.add(1, 'hour')),
				timeline(time.add(1, 'hour')),
				timeline(time.add(1, 'hour'))
			].filter(function (tl) {
				return tl != null;
			});
		}

		function setupPolling() {
			polling = $interval(function () {
				if (vm.pollingLock) {
					vm.pollingLock = false;
					updateStates();
				}

			}, 5000);
		}

		function cancelPolling() {
			if (polling != null) {
				$interval.cancel(polling);
				vm.pollingLock = true;
			}
		}

		vm.getTableHeight = function () {
			var rowHeight = 30;
			var headerHeight = 30;
			var agentMenuHeight = 45;
			return {
				height: (vm.filteredData.length * rowHeight + headerHeight + agentMenuHeight + rowHeight / 2) + "px"
			};
		};

		vm.selectAgent = function (personId) {
			selectedPersonId = vm.isSelected(personId) ? '' : personId;
		};

		vm.isSelected = function (personId) {
			return selectedPersonId === personId;
		};

		vm.showAdherenceUpdates = function () {
			return vm.adherencePercent !== null;
		};

		vm.getAdherenceForAgent = function (personId) {
			if (!vm.isSelected(personId)) {
				rtaService.forToday({
					personId: personId
				})
					.then(function (data) {
						vm.adherence = data;
						vm.adherencePercent = data.AdherencePercent;
						vm.timestamp = data.LastTimestamp;
					});
			}
		};

		vm.changeScheduleUrl = function (personId) {
			return rtaRouteService.urlForChangingSchedule(personId);
		};
		vm.historicalAdherenceUrl = function (personId) {
			return rtaRouteService.urlForHistoricalAdherence(personId);
		};

		vm.agentDetailsUrl = function (personId) {
			return rtaRouteService.urlForAgentDetails(personId);
		};

		function getAgentStates() {
			var deferred = $q.defer();
			if (skillAreaId) {
				getSkillAreaInfo()
					.then(function () {
						deferred.resolve(rtaService.agentStatesFor);
					});
			} else {
				console.log(vm.agentsInAlarm);
				if (vm.agentsInAlarm)
					deferred.resolve(rtaService.agentStatesInAlarmFor);
				else
					deferred.resolve(rtaService.agentStatesFor);
			}
			return deferred.promise;
		};

		function getAgents() {
			var deferred = $q.defer();
			if (skillAreaId) {
				getSkillAreaInfo()
					.then(function () {
						deferred.resolve(rtaService.agentsFor);
					});
			} else {
				deferred.resolve(rtaService.agentsFor);
			}
			return deferred.promise;
		};

		function getSkillAreaInfo() {
			return rtaService.getSkillArea(skillAreaId)
				.then(function (skillArea) {
					if (skillArea.Skills != null) {
						vm.skillAreaName = skillArea.Name || '?';
						vm.skillArea = true;
						skillIds = skillArea.Skills.map(function (skill) {
							return skill.Id;
						});
					}
				});
		}

		$scope.$watch(function () {
			return vm.filterText;
		}, filterData);

		function filterData() {
			if (angular.isUndefined(vm.filterText))
				vm.filteredData = vm.agents;
			else
				vm.filteredData = $filter('agentFilter')(vm.agents, vm.filterText, propertiesForFiltering);
			if (vm.agentsInAlarm) {
				vm.filteredData = $filter('filter')(vm.filteredData, {
					TimeInAlarm: ''
				});
				vm.openedMaxNumberOfAgents = (vm.filteredData.length === vm.maxNumberOfAgents);
				if (!vm.notifySwitchDisabled && vm.agents.length > vm.maxNumberOfAgents) {
					NoticeService.warning($translate.instant('It is possible to view maximum ' + vm.maxNumberOfAgents + ' agents. The "In alarm" switch is enabled if the number of agents does not exceed ' + vm.maxNumberOfAgents + '.'), null, true);
					vm.notifySwitchDisabled = true;
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
				rtaService.getSkillName(skillIds[0])
					.then(function (skill) {
						vm.skillName = skill.Name || '?';
						vm.skill = true;
					});
			}
		})();

		$scope.$watch(
			function () {
				return $sessionStorage.buid;
			},
			function (newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue) {
					rtaRouteService.goToSites();
				}
			}
		);

		function updateBreadCrumb(agentsInfo) {
			if (siteIds.length > 1) {
				vm.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
				vm.siteName = "Multiple Sites";
			} else if (teamIds.length > 1 || (siteIds.length === 1 && teamIds.length !== 1)) {
				vm.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
				vm.teamName = "Multiple Teams";
			} else if (agentsInfo.length > 0) {
				vm.siteName = agentsInfo[0].SiteName;
				vm.teamName = agentsInfo[0].TeamName;
				vm.goBackToTeamsWithUrl = urlForTeamsInBreadcrumbs(agentsInfo);
				vm.goBackToRootWithUrl = urlForRootInBreadcrumbs(agentsInfo);
				vm.showPath = true;
			}
			else if (agentsInfo.length === 0) {
				vm.siteName = siteIds;
				vm.TeamName = teamIds;
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
				return rtaRouteService.urlForTeamsBySkillArea(agentsInfo[0].SiteId, skillAreaId);
			if (skillIds.length > 0)
				return rtaRouteService.urlForTeamsBySkills(agentsInfo[0].SiteId, skillIds[0]);
			return rtaRouteService.urlForTeams(agentsInfo[0].SiteId);
		}

		function urlForRootInBreadcrumbs(agentsInfo) {
			if (skillAreaId != null)
				return rtaRouteService.urlForSitesBySkillArea(skillAreaId);
			if (skillIds.length > 0)
				return rtaRouteService.urlForSitesBySkills(skillIds[0]);
			return rtaRouteService.urlForSites();
		}

		function sortStatesByName() {
			vm.states = $filter('orderBy')(vm.states, function (state) {
				return state.Name;
			});
		};

		vm.goToOverview = function () {
			rtaRouteService.goToSites();
		}

		vm.goToSelectItem = function () {
			rtaRouteService.goToSelectSkill();
		}

		$scope.$on('$destroy', function () {
			cancelPolling();
		});

		vm.rightPanelOptions = {
			panelState: false,
			panelTitle: " ",
			sidePanelTitle: " ",
			showCloseButton: true,
			showBackdrop: true,
			showResizer: true,
			showPopupButton: true
		};

		vm.onSearchOrganization = function ($event) {
			$event.stopPropagation();
		};

	};
})();
