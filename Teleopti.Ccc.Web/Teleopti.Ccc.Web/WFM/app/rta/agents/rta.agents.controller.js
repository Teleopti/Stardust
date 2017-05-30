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
			'rtaRouteService',
			'fakeTimeService',
			'localeLanguageSortingService',
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
		rtaRouteService,
		fakeTimeService,
		localeLanguageSortingService,
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
		var propertiesForFiltering = ["Name", "State", "Activity", "Rule", "SiteAndTeamName"];
		var allGrid = rtaGridService.makeAllGrid();
		var inAlarmGrid = rtaGridService.makeInAlarmGrid();
		// because angular cant handle an array of null in stateparams
		var nullState = "No State";
		var nullStateId = "noState";
		var enableWatchOnTeam = false;
		var updateStatesDelegate = updateStates;
		vm.adherence = {};
		vm.adherencePercent = null;
		vm.filterText = "";
		vm.timestamp = "";

		var agentStates = [];
		vm.agents = [];
		vm.filteredData = [];

		vm.states = [];

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
		vm.openedMaxNumberOfAgents = false;
		vm.maxNumberOfAgents = 50;
		vm.isLoading = angular.toJson($stateParams) !== '{}';
		vm.pollingLock = true;
		vm.sortByLocaleLanguage = localeLanguageSortingService.sort;
		vm.getTableHeight = getTableHeight;
		vm.getAdherenceForAgent = getAdherenceForAgent;
		vm.selectAgent = selectAgent;
		vm.isSelected = isSelected;
		vm.showAdherenceUpdates = showAdherenceUpdates;
		vm.changeScheduleUrl = changeScheduleUrl;
		vm.historicalAdherenceUrl = historicalAdherenceUrl;
		vm.goToOverview = function () { rtaRouteService.goToSites(); }
		vm.goToSelectItem = function () { rtaRouteService.goToSelectSkill(); }
		var pollingInterval = angular.isDefined($stateParams.pollingInterval) ? $stateParams.pollingInterval : 5000;
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
		allGrid.data = 'vm.filteredData';
		inAlarmGrid.data = 'vm.filteredData';

		/*******REQUESTS*****/

		(function initialize() {
			vm.pollingLock = false;
			if (siteIds.length > 0 || teamIds.length > 0 || skillIds.length > 0 || skillAreaId) {
				agentState();
				updateStatesDelegate = agentState;
			}
		})();

		/************AGENTS GRID************/
		function getTableHeight() {
			var rowHeight = 30;
			var headerHeight = 30;
			var agentMenuHeight = 45;
			return {
				height: (vm.filteredData.length * rowHeight + headerHeight + agentMenuHeight + rowHeight / 2) + "px"
			};
		};

		function getAdherenceForAgent(personId) {
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

		function selectAgent(personId) { selectedPersonId = vm.isSelected(personId) ? '' : personId; };
		function isSelected(personId) { return selectedPersonId === personId; };
		function showAdherenceUpdates() { return vm.adherencePercent !== null; };
		function changeScheduleUrl(personId) { return rtaRouteService.urlForChangingSchedule(personId); };
		function historicalAdherenceUrl(personId) { return rtaRouteService.urlForHistoricalAdherence(personId); };

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
				.then(updateStuff)
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

		function updateStuff(data) {
			agentStates = data.States;
			$scope.$watchCollection(
				function () { return vm.agents; },
				filterData);
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
			setStatesAndStuff(agentStates);
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

		function setStatesAndStuff(states) {
			vm.agents = [];
			lastUpdate = states.Time;
			fillAgentState(states);
			vm.timeline = rtaFormatService.buildTimeline(states.Time);
			vm.isLoading = false;
			vm.pollingLock = true;
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

		function setupPolling() {
			polling = $interval(function () {
				if (vm.pollingLock) {
					vm.pollingLock = false;
					updateStatesDelegate();
				}

			}, pollingInterval);
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
			}
		}

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
