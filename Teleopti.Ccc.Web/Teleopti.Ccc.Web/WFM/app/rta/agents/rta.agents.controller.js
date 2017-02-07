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
			'$timeout',
			'rtaService',
			'rtaGridService',
			'rtaFormatService',
			'rtaAgentsBuildService',
			'rtaBreadCrumbService',
			'rtaRouteService',
			'fakeTimeService',
			'rtaLocaleLanguageSortingService',
			'Toggle',
			'NoticeService'
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
		$timeout,
		rtaService,
		rtaGridService,
		rtaFormatService,
		rtaAgentsBuildService,
		rtaBreadCrumbService,
		rtaRouteService,
		fakeTimeService,
		rtaLocaleLanguageSortingService,
		toggleService,
		NoticeService
	) {

		var vm = this;
		var selectedPersonId, lastUpdate, notice, selectedSiteId;;
		var polling = null;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var excludedStatesFromUrl = function () { return $stateParams.es || [] };
		var propertiesForFiltering = ["Name", "State", "Activity", "Alarm", "SiteAndTeamName"];
		var allGrid = rtaGridService.makeAllGrid();
		var inAlarmGrid = rtaGridService.makeInAlarmGrid();
		// because angular cant handle an array of null in stateparams
		var nullState = "No State";
		var nullStateId = "noState";
		var enableWatchOnTeam = false;
		var updateStatesDelegate = updateStates;
		var agentsInfo = [];

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
		vm.agentsInAlarm = !$stateParams.showAllAgents;
		vm.allGrid = allGrid;
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
		vm.pollingLock = true;
		// select skill dependency
		vm.skills = [];
		vm.skillAreas = [];
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.teamsSelected = [];
		vm.selectFieldText = $translate.instant('SelectOrganization');
		vm.searchTerm = "";
		vm.sortByLocaleLanguage = rtaLocaleLanguageSortingService.sort;

		allGrid.data = 'vm.filteredData';
		inAlarmGrid.data = 'vm.filteredData';

		/*******REQUESTS*****/
		toggleService.togglesLoaded.then(function () {
			vm.showOrgSelection = toggleService.RTA_QuicklyChangeAgentsSelection_40610;
			rtaService.getOrganization()
				.then(function (organization) {
					vm.sites = organization;
					if (vm.showOrgSelection)
						keepSelectionForOrganization();
				});
		});

		function keepSelectionForOrganization() {
			selectSiteAndTeamsUnder();
			if (teamIds.length > 0)
				vm.teamsSelected = teamIds;
			enableWatchOnTeam = true;
			updateSelectFieldText();
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

		function updateSelectFieldText() {
			var howManyTeamsSelected = countTeamsSelected();
			vm.selectFieldText = howManyTeamsSelected === 0 ? vm.selectFieldText : $translate.instant("SeveralTeamsSelected").replace('{0}', howManyTeamsSelected);
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

		(function initialize() {
			rtaService.getSkills()
				.then(function (skills) {
					vm.skillsLoaded = true;
					vm.skills = skills;
					if (skillIds.length > 0 && skillAreaId == null)
						vm.selectedSkill = skills.find(function (s) { return s.Id === skillIds[0] });
				});
			rtaService.getSkillAreas()
				.then(function (skillAreas) {
					vm.skillAreasLoaded = true;
					vm.skillAreas = skillAreas.SkillAreas;
					if (skillAreaId != null)
						vm.selectedSkillArea = vm.skillAreas.find(function (s) { return s.Id === skillAreaId });
				});

			vm.pollingLock = false;
			removeMeWithToggle('RTA_QuicklyChangeAgentsSelection_40610');
			if (siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId) {
				toggleService.togglesLoaded.then(function () {
					if (toggleService.RTA_FasterAgentsView_42039) {
						agentState();
						updateStatesDelegate = agentState;
					} else {
						getAgents()
							.then(agentsByParams)
							.then(updateStuff)
							.then(updateStates)
							.then(updatePhoneStatesFromStateParams);
					}
				});

			}
		})();

		function removeMeWithToggle() {
			if (skillIds.length === 1) {
				rtaService.getSkillName(skillIds[0])
					.then(function (skill) {
						vm.skillName = skill.Name || '?';
						vm.skill = true;
					});
			}
		}

		function agentsByParams(fn) {
			return fn({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId
			});
		}

		function updateStuff(data) {
			agentsInfo = data;
			vm.agents = data;
			$scope.$watchCollection(
				function () { return vm.agents; },
				filterData);
			updateBreadCrumb(data);
			vm.pollingLock = true;
		}

		/*********AUTOCOMPLETE*****/
		vm.querySearch = function (query, myArray) {
			if (!query)
				return myArray;
			return myArray.filter(function (query) { return function filterFn(item) { return (item.Name.toUpperCase().indexOf(query.toUpperCase()) === 0); }; });
		};

		vm.selectedSkillChange = function (skill) {
			if (!skill || (skill.Id != skillIds[0] || $stateParams.skillAreaId))
				stateGoToAgents({
					skillIds: skill ? skill.Id : undefined,
					skillAreaId: skill ? undefined : skillAreaId,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		vm.selectedSkillAreaChange = function (skillArea) {
			if (!skillArea || !(skillArea.Id == $stateParams.skillAreaId))
				stateGoToAgents({
					skillIds: skillArea ? [] : $stateParams.skillIds,
					skillAreaId: skillArea ? skillArea.Id : undefined,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		/***********MULTI-SELECT************/
		vm.expandSite = function (site) { site.isExpanded = !site.isExpanded; };

		vm.goToAgents = function () {
			var selection = {};
			var selectedSiteIds = vm.selectedSites();
			var selectedTeamIds = vm.teamsSelected;
			selection['siteIds'] = selectedSiteIds;
			selection['teamIds'] = selectedTeamIds;
			stateGoToAgents(selection);
		}

		function stateGoToAgents(selection) {
			var stateName = vm.showOrgSelection ? 'rta.select-skill' : 'rta.agents';
			var options = vm.showOrgSelection ? {
				reload: true,
				notify: true
			} : {};
			$state.go(stateName, selection, options);
		}

		vm.selectedSites = function () {
			return vm.sites
				.filter(function (site) {
					var selectedTeams = site.Teams.filter(function (team) {
						return team.isChecked == true;
					});
					var noTeamsSelected = selectedTeams.length === 0
					if (noTeamsSelected)
						return false;
					var allTeamsSelected = selectedTeams.length == site.Teams.length;
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
			if (site.isChecked)
				return true;
			var selectedTeamsChecked = site.Teams.filter(function (t) { return t.isChecked; });
			var isAllTeamsCheckedForSite = selectedTeamsChecked.length === site.Teams.length;
			site.isMarked = (selectedTeamsChecked.length > 0 && !isAllTeamsCheckedForSite);
			if (isAllTeamsCheckedForSite)
				return false;
			return team.isChecked;
		}

		vm.forTest_selectSite = function (site) {
			site.isChecked = !site.isChecked;
			var selectedSite = vm.sites.find(function (s) { return s.Id === site.Id; });
			selectedSite.Teams.forEach(function (team) {
				team.isChecked = selectedSite.isChecked;
				if (selectedSite.isChecked) {
					vm.teamsSelected.push(team.Id);
				} else {
					var index = vm.teamsSelected.indexOf(team.Id);
					vm.teamsSelected.splice(index, 1);
				}
			});
		}

		vm.updateSite = function (oldTeamsSelected) {
			vm.sites.forEach(function (site) {
				var anyChangeForThatSite = false;
				site.Teams.forEach(function (team) {
					team.isChecked = vm.teamsSelected.indexOf(team.Id) > -1;
					var teamChanged = (team.isChecked === oldTeamsSelected.indexOf(team.Id) < 0);
					if (oldTeamsSelected.length > 0 && teamChanged)
						anyChangeForThatSite = true;
				});
				var checkedTeams = site.Teams.filter(function (team) { return team.isChecked; });
				if (checkedTeams.length > 0 || anyChangeForThatSite)
					site.isChecked = checkedTeams.length === site.Teams.length;
			});
		};

		vm.clearOrgSelection = function () {
			vm.sites.forEach(function (site) { site.isChecked = false; });
			vm.teamsSelected = [];
			updateSelectFieldText();
		};

		vm.clearSearchTerm = function () { vm.searchTerm = ''; }
		vm.onSearchOrganization = function ($event) { $event.stopPropagation(); };


		/************AGENTS GRID************/
		vm.getTableHeight = function () {
			var rowHeight = 30;
			var headerHeight = 30;
			var agentMenuHeight = 45;
			return {
				height: (vm.filteredData.length * rowHeight + headerHeight + agentMenuHeight + rowHeight / 2) + "px"
			};
		};

		vm.getAdherenceForAgent = function (personId) {
			if (!vm.isSelected(personId)) {
				rtaService
					.forToday({ personId: personId })
					.then(function (data) {
						vm.adherence = data;
						vm.adherencePercent = data.AdherencePercent;
						vm.timestamp = data.LastTimestamp;
					});
			}
		};

		vm.selectAgent = function (personId) { selectedPersonId = vm.isSelected(personId) ? '' : personId; };
		vm.isSelected = function (personId) { return selectedPersonId === personId; };
		vm.showAdherenceUpdates = function () { return vm.adherencePercent !== null; };
		vm.changeScheduleUrl = function (personId) { return rtaRouteService.urlForChangingSchedule(personId); };
		vm.historicalAdherenceUrl = function (personId) { return rtaRouteService.urlForHistoricalAdherence(personId); };

		/****************GO TOS**************/
		vm.goToOverview = function () { rtaRouteService.goToSites(); }
		vm.goToSelectItem = function () { rtaRouteService.goToSelectSkill(); }

		/**************RIGHT PANEL**************/

		vm.rightPanelOptions = {
			panelState: false,
			panelTitle: " ",
			sidePanelTitle: " ",
			showCloseButton: true,
			showBackdrop: true,
			showResizer: true,
			showPopupButton: true
		};

		///////////////////////////////////////////////////////////////////

		/*****************WATCHES*****************/
		$scope.$watch(
			function () { return vm.pause; },
			function () {
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

		$scope.$watch(
			function () { return vm.agentsInAlarm; },
			function (newValue, oldValue) {
				if (newValue !== oldValue) {
					updateStatesDelegate();
					filterData();
					if (newValue && vm.pause) {
						vm.filteredData.sort(function (a, b) {
							return vm.formatToSeconds(b.TimeInAlarm) - vm.formatToSeconds(a.TimeInAlarm);
						});
					}
				}
			});

		$scope.$watch(
			function () { return vm.teamsSelected; },
			function (newValue, oldValue) {
				if (angular.toJson(newValue) !== angular.toJson(oldValue) && enableWatchOnTeam) {
					vm.updateSite(oldValue);
				}
			});

		$scope.$watch(
			function () { return vm.filterText; },
			filterData);

		$scope.$watch(
			function () { return $sessionStorage.buid; },
			function (newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue) {
					rtaRouteService.goToSites();
				}
			}
		);

		///////////////////////////////////////////////////////////////////
		function agentState() {
			getAgentStates()
				.then(getAgentStatesByParams)
				.then(updateStuff2)
				.then(updateAgentStates)
				.then(updatePhoneStatesFromStateParams);
		}

		function getAgentStatesByParams(fn) {
			return fn({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId,
				excludedStateIds: excludedStateIds().map(function (s) { return s === nullStateId ? null : s; })
			})
		}

		function updateStuff2(data) {
			agentsInfo = data.States;
			vm.agents = data.States;
			$scope.$watchCollection(
				function () { return vm.agents; },
				filterData);
			updateBreadCrumb(data.States);
			vm.pollingLock = true;
			return data;
		}

		function getAgentStates() {
			var deferred = $q.defer();
			if (skillAreaId) {
				rtaService.getSkillArea(skillAreaId)
					.then(getSkillIdsFromSkillArea)
					.then(function () {
						serviceCall(deferred);
					});
			} else {
				serviceCall(deferred);
			}
			return deferred.promise;
		};

		function serviceCall(deferred) {
			if (excludedStateIds().length > 0)
				deferred.resolve(rtaService.agentStatesInAlarmExcludingPhoneStatesFor);
			else if (vm.agentsInAlarm)
				deferred.resolve(rtaService.agentStatesInAlarmFor);
			else
				deferred.resolve(rtaService.agentStatesFor);
		}

		function updateAgentStates(agentStates) {
			if (skip()) return;
			var excludedStates = excludedStateIds();
			setStatesAndStuff(fillAgentState, agentStates);
			updateUrlWithExcludedStateIds(excludedStates);
		};

		function fillAgentState(states) {
			var now = moment(states.Time);
			states.States.forEach(function (state, i) {
				vm.agents.push(rtaAgentsBuildService.buildAgentState(now, state));
				if (stateIsNotAdded(vm.states, state))
					vm.states.push(mapState(state));
			});
			sortPhoneStatesByName();
		}

		function getAgents() {
			var deferred = $q.defer();
			if (skillAreaId) {
				rtaService
					.getSkillArea(skillAreaId)
					.then(getSkillIdsFromSkillArea)
					.then(function () {
						deferred.resolve(rtaService.agentsFor);
					});
			} else {
				deferred.resolve(rtaService.agentsFor);
			}
			return deferred.promise;
		};

		function getStates(inAlarm, excludeStates) {
			if (!inAlarm)
				return rtaService.statesFor;
			if (excludeStates)
				return rtaService.inAlarmExcludingPhoneStatesFor;
			return rtaService.inAlarmFor;
		};

		function getSkillIdsFromSkillArea(skillArea) {
			if (skillArea.Skills != null) {
				//remove vm.skillAreaNAme when Quickly toggle is released
				vm.skillAreaName = skillArea.Name || '?';
				vm.skillArea = true;
				skillIds = skillArea.Skills.map(function (skill) { return skill.Id; });
			}
		}

		function skip() {
			return vm.pause || !(siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId)
		}

		function updateStates() {
			if (skip()) return;
			var excludedStates = excludedStateIds();
			getStates(vm.agentsInAlarm, excludedStates.length > 0)({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId,
				excludedStateIds: excludedStates.map(function (s) { return s === nullStateId ? null : s; })
			})
				.then(setStatesInAgents)
				.then(updateUrlWithExcludedStateIds(excludedStates));
		}

		function updatePhoneStatesFromStateParams() {
			var stateIds = excludedStatesFromUrl();
			addNoStateIfNeeded(stateIds);
			getStateNamesForAnyExcludedStates(stateIds);
		}

		function addNoStateIfNeeded(stateIds) {
			if (stateIds.indexOf(nullStateId) > -1 &&
				vm.states.filter(function (s) { return s.Id === nullStateId; }).length === 0) {
				vm.states.push({
					Id: nullStateId,
					Name: nullState,
					Selected: false
				});
				sortPhoneStatesByName();
			}
		}

		function getStateNamesForAnyExcludedStates(stateIds) {
			var stateIdsWithoutNull = stateIds.filter(function (s) { return s !== nullStateId; })
			if (stateIdsWithoutNull.length !== 0) {
				rtaService.getPhoneStates(stateIdsWithoutNull)
					.then(function (states) {
						vm.states = vm.states.concat(states.PhoneStates);
						sortPhoneStatesByName();
					});
			}
		}

		function excludedStateIds() {
			var included = vm.states
				.filter(function (s) { return s.Selected === true; })
				.map(function (s) { return s.Id; });
			var excludedViaUrlAndNotManuallyIncluded = excludedStatesFromUrl()
				.filter(function (s) { return included.indexOf(s) === -1; });
			var excluded = vm.states
				.filter(function (s) { return s.Selected === false; })
				.map(function (s) { return s.Id; });
			var excludedUnique = excludedViaUrlAndNotManuallyIncluded
				.filter(function (s) { return excluded.indexOf(s) === -1; });
			return excludedUnique.concat(excluded);
		}

		function setStatesInAgents(states) {
			setStatesAndStuff(
				function (data) {
					fillAgentsWithState(data);
					fillAgentsWithoutState();
				},
				states);
		}

		function setStatesAndStuff(fillFunction, states) {
			vm.agents = [];
			lastUpdate = states.Time;
			fillFunction(states)
			vm.timeline = rtaFormatService.buildTimeline(states.Time);
			vm.isLoading = false;
			vm.pollingLock = true;
		}


		function fillAgentsWithState(states) {
			var now = moment(states.Time);
			states.States.forEach(function (state, i) {
				var agentInfo = $filter('filter')(agentsInfo, {
					PersonId: state.PersonId
				});

				if (agentInfo.length > 0) {
					state.Name = agentInfo[0].Name;
					state.SiteAndTeamName = agentInfo[0].SiteName + '/' + agentInfo[0].TeamName;
					state.TeamName = agentInfo[0].TeamName;
					state.SiteName = agentInfo[0].SiteName;
					state.PersonId = agentInfo[0].PersonId;
					state.TeamId = agentInfo[0].TeamId;

					vm.agents.push(rtaAgentsBuildService.buildAgentState(now, state));
					if (stateIsNotAdded(vm.states, state))
						vm.states.push(mapState(state));
				}
			});
			sortPhoneStatesByName();
		}

		function stateIsNotAdded(existingStates, state) {
			state.StateId = state.StateId || nullStateId;
			state.State = state.State || nullState;
			return (existingStates
				.map(function (s) { return s.Id; })
				.indexOf(state.StateId) === -1)
		}

		function mapState(state) {
			return {
				Id: state.StateId,
				Name: state.State,
				Selected: excludedStatesFromUrl().indexOf(state.StateId) == -1
			};
		}

		function fillAgentsWithoutState() {
			agentsInfo.forEach(function (agentInfo) {
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

		function setupPolling() {
			polling = $interval(function () {
				if (vm.pollingLock) {
					vm.pollingLock = false;
					updateStatesDelegate();
				}

			}, 5000);
		}

		function cancelPolling() {
			if (polling != null) {
				$interval.cancel(polling);
				vm.pollingLock = true;
			}
		}

		function filterData() {
			if (angular.isUndefined(vm.filterText))
				vm.filteredData = vm.agents;
			else
				vm.filteredData = $filter('agentFilter')(vm.agents, vm.filterText, propertiesForFiltering);
			if (vm.agentsInAlarm) {
				vm.filteredData = $filter('filter')(vm.filteredData, { TimeInAlarm: '' });
				vm.openedMaxNumberOfAgents = (vm.filteredData.length === vm.maxNumberOfAgents);
				if (!vm.notifySwitchDisabled && vm.agents.length > vm.maxNumberOfAgents) {
					NoticeService.warning($translate.instant('RTAMaxNumberOfAgentsNotice')
						.replace('{0}', vm.maxNumberOfAgents)
						.replace('{1}', vm.maxNumberOfAgents), null, true);
					vm.notifySwitchDisabled = true;
				}
			}
		}

		function updateBreadCrumb(data) {
			var breadCrumbInfo = rtaBreadCrumbService.getBreadCrumb({
				organization: vm.sites,
				skillAreaId: skillAreaId,
				skillIds: skillIds,
				siteIds: siteIds,
				teamIds: teamIds,
				agentsInfo: data
			});
			vm.goBackToRootWithUrl = breadCrumbInfo.goBackToRootWithUrl;
			vm.siteName = breadCrumbInfo.siteName;
			vm.teamName = breadCrumbInfo.teamName;
			vm.goBackToTeamsWithUrl = breadCrumbInfo.goBackToTeamsWithUrl;
			vm.showPath = breadCrumbInfo.showPath;
		};

		function updateUrlWithExcludedStateIds(excludedStates) {
			$state.go($state.current.name,
				{ es: excludedStates },
				{ notify: false });
		};

		function sortPhoneStatesByName() {
			vm.states = $filter('orderBy')(vm.states, function (state) { return state.Name; });
		};

		$scope.$on('$destroy', function () {
			cancelPolling();
		});
	};
})();
