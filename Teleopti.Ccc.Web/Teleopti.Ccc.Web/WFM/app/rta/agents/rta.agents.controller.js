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
			'rtaStateService',
			'fakeTimeService',
			'localeLanguageSortingService',
			'Toggle',
			'NoticeService'
		];

	function RtaAgentsController($scope,
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
								 rtaStateService,
								 fakeTimeService,
								 localeLanguageSortingService,
								 Toggle,
								 NoticeService) {

		var vm = this;

		var allAgentStates = [];
		vm.agentStates = [];
		vm.showInAlarm = !$stateParams.showAllAgents;
		vm.showAll = true;
		vm.allGrid = rtaGridService.makeAllGrid();
		vm.inAlarmGrid = rtaGridService.makeInAlarmGrid();
		vm.allGrid.data = 'vm.agentStates';
		vm.inAlarmGrid.data = 'vm.agentStates';

		var polling = null;
		var pollingInterval = angular.isDefined($stateParams.pollingInterval) ? $stateParams.pollingInterval : 5000;
		var pollingLock = true;

		var selectedPersonId, lastUpdate, notice, selectedSiteId;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var excludedStatesFromUrl = function () { return $stateParams.es || [] };
		var propertiesForFiltering = ["Name", "State", "Activity", "Rule", "SiteAndTeamName"];
		// because angular cant handle an array of null in stateparams
		var nullState = "No State";
		var nullStateId = "noState";
		var enableWatchOnTeam = false;
		vm.adherence = {};
		vm.adherencePercent = null;
		vm.filterText = "";
		vm.timestamp = "";

		vm.states = [];

		vm.format = rtaFormatService.formatDateTime;
		vm.formatDuration = rtaFormatService.formatDuration;
		vm.formatToSeconds = rtaFormatService.formatToSeconds;
		vm.hexToRgb = rtaFormatService.formatHexToRgb;
		vm.pause = false;
		vm.pausedAt = null;
		vm.showPath = false;
		vm.notifySwitchDisabled = false;
		vm.showBreadcrumb = siteIds.length > 0 || teamIds.length > 0 || skillIds === [];
		vm.openedMaxNumberOfAgents = false;
		vm.maxNumberOfAgents = 50;
		vm.isLoading = angular.toJson($stateParams) !== '{}';

		vm.displayNoAgentsMessage = function () { return vm.agentStates.length == 0; };
		vm.displayNoAgentsForSkillMessage = rtaStateService.hasSkillSelection;

		var toggles = {};
		Toggle.togglesLoaded.then(function () {
			toggles = Toggle;
		});

		vm.getTableHeight = function () {
			var rowHeight = 30;
			var headerHeight = 30;
			var agentMenuHeight = 45;
			return {
				height: (vm.agentStates.length * rowHeight + headerHeight + agentMenuHeight + rowHeight / 2) + "px"
			};
		};

		vm.getAdherenceForAgent = function (personId) {
			if (!vm.isSelected(personId)) {
				rtaService
					.forToday({personId: personId})
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
		vm.goToOverview = rtaRouteService.goToOverview;
		vm.goToSelectItem = function () { rtaRouteService.goToSelectSkill(); }

		vm.rightPanelOptions = {
			panelState: false,
			panelTitle: " ",
			sidePanelTitle: " ",
			showCloseButton: true,
			showBackdrop: true,
			showResizer: true,
			showPopupButton: true
		};

		(function initialize() {
			pollingLock = false;
			agentState();
		})();

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

		function setupPolling() {
			polling = $interval(function () {
				if (pollingLock) {
					pollingLock = false;
					agentState();
				}

			}, pollingInterval);
		}

		function cancelPolling() {
			if (polling != null) {
				$interval.cancel(polling);
				pollingLock = true;
			}
		}

		function agentState() {
			getAgentStates()
				.then(getAgentStatesByParams)
				.then(updateStuff)
				.then(updateAgentStates)
				.then(updatePhoneStatesFromStateParams);
		}

		function getAgentStates() {
			var deferred = $q.defer();
			if (skillAreaId) {
				rtaService.getSkillArea(skillAreaId)
					.then(getSkillIdsFromSkillArea)
					.then(function () {
						deferred.resolve(rtaService.agentStatesFor);
					});
			} else {
				deferred.resolve(rtaService.agentStatesFor);
			}
			return deferred.promise;
		};

		function getSkillIdsFromSkillArea(skillArea) {
			if (skillArea.Skills != null) {
				vm.skillArea = true;
				skillIds = skillArea.Skills.map(function (skill) { return skill.Id; });
			}
		}

		function getAgentStatesByParams(fn) {
			return fn({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId,
				inAlarm: vm.showInAlarm,
				excludedStateIds: excludedStateIds().map(function (s) { return s === nullStateId ? null : s; })
			})
		}

		function updateStuff(data) {
			$scope.$watchCollection(
				function () { return allAgentStates; },
				filterData);
			pollingLock = true;
			return data;
		}

		function filterData() {
			if (angular.isUndefined(vm.filterText))
				vm.agentStates = allAgentStates;
			else
				vm.agentStates = $filter('agentFilter')(allAgentStates, vm.filterText, propertiesForFiltering);
			if (vm.showInAlarm) {
				vm.agentStates = $filter('filter')(vm.agentStates, {TimeInAlarm: ''});
				vm.openedMaxNumberOfAgents = (vm.agentStates.length === vm.maxNumberOfAgents);
			}
		}

		function updateAgentStates(agentStates) {
			if (!vm.pause) {
				var excludedStates = excludedStateIds();
				setStatesAndStuff(agentStates);
				updateUrlWithExcludedStateIds(excludedStates);
			}
		}

		function setStatesAndStuff(states) {
			allAgentStates = [];
			lastUpdate = states.Time;
			fillAgentState(states);
			vm.timeline = rtaFormatService.buildTimeline(states.Time);
			vm.isLoading = false;
			pollingLock = true;
		}

		function fillAgentState(states) {
			var now = moment(states.Time);
			states.States.forEach(function (state, i) {
				allAgentStates.push(rtaAgentsBuildService.buildAgentState(now, state));
				if (stateIsNotAdded(vm.states, state))
					vm.states.push(mapState(state));
			});
			sortPhoneStatesByName();
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


		$scope.$watch(
			function () { return vm.showInAlarm; },
			function (newValue, oldValue) {
				if (newValue !== oldValue) {
					agentState();
					filterData();
					if (newValue && vm.pause) {
						vm.agentStates.sort(function (a, b) {
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
					rtaRouteService.goToOverview();
				}
			}
		);


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

		function updateUrlWithExcludedStateIds(excludedStates) {
			$state.go($state.current.name,
				{es: excludedStates},
				{notify: false});
		};

		function sortPhoneStatesByName() {
			vm.states = $filter('orderBy')(vm.states, function (state) { return state.Name; });
		};

		$scope.$on('$destroy', function () {
			cancelPolling();
		});
	};
})();
