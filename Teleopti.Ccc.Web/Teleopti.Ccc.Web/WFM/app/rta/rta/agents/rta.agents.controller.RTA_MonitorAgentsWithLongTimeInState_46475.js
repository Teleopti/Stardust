(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController46475', RtaAgentsController);

	RtaAgentsController.$inject = [
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
		'rtaPollingService',
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
								 rtaPollingService,
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

		vm.agentStates = [];

		// duplication of state
		vm.showInAlarm = !$stateParams.showAllAgents;
		vm.showAll = true;

		vm.allGrid = rtaGridService.makeAllGrid();
		vm.inAlarmGrid = rtaGridService.makeInAlarmGrid();
		vm.allGrid.data = 'vm.agentStates';
		vm.inAlarmGrid.data = 'vm.agentStates';

		var selectedPersonId;

		var lastUpdate, notice;

		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var excludedStatesFromUrl = $stateParams.es || [];

		// because angular cant handle an array of null in stateparams
		var nullState = "No State";
		var nullStateId = "noState";
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
		vm.maxNumberOfAgents = 50;
		vm.isLoading = angular.toJson($stateParams) !== '{}';

		vm.displayNoAgentsMessage = function () {
			return vm.agentStates.length == 0;
		};
		vm.displayNoAgentsForSkillMessage = rtaStateService.hasSkillSelection;

		vm.orderBy = vm.showInAlarm ? undefined : 'Name';
		vm.direction = vm.showInAlarm ? undefined : 'asc';

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
					.forToday({
						personId: personId
					})
					.then(function (data) {
						vm.adherence = data;
						vm.adherencePercent = data.AdherencePercent;
						vm.timestamp = data.LastTimestamp;
					});
			}
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
		vm.changeScheduleUrl = function (personId) {
			return rtaRouteService.urlForChangingSchedule(personId);
		};
		vm.historicalAdherenceUrl = function (personId) {
			return rtaRouteService.urlForHistoricalAdherence(personId);
		};
		vm.goToOverview = rtaRouteService.goToOverview;
		vm.goToSelectItem = rtaRouteService.goToSelectSkill;

		vm.rightPanelOptions = {
			panelState: false,
			panelTitle: " ",
			sidePanelTitle: " ",
			showCloseButton: true,
			showBackdrop: true,
			showResizer: true,
			showPopupButton: true
		};

		var poller;
		var pollingInterval = angular.isDefined($stateParams.pollingInterval) ? $stateParams.pollingInterval : 5000;

		$scope.$watch(
			function () {
				return vm.pause;
			},
			function () {
				if (vm.pause) {
					vm.pausedAt = moment(lastUpdate).format('YYYY-MM-DD HH:mm:ss');
					var template = $translate.instant('RtaPauseEnabledNotice');
					var noticeText = template.replace('{0}', vm.pausedAt);
					notice = NoticeService.info(noticeText, null, true);
					poller.destroy();
					poller = null;
				} else {
					vm.pausedAt = null;
					if (notice != null) {
						notice.destroy();
					}
					NoticeService.info($translate.instant('RtaPauseDisableNotice'), 5000, true);
					poller = rtaPollingService.create(pollAgentStates);
					poller.start();
				}
			});

		$scope.$on('$destroy', function () {
			if (poller)
				poller.destroy();
		});

		function pollAgentStates() {
			if (skillAreaId) {
				return rtaService.getSkillArea(skillAreaId)
					.then(skillIdsForSkillArea)
					.then(loadAgentStates)
					.then(updateAgentStates)
					.then(updateStatesFromAgentStates)
					.then(loadExcludedStates);
			} else {
				return loadAgentStates()
					.then(updateAgentStates)
					.then(updateStatesFromAgentStates)
					.then(loadExcludedStates);
			}
		}

		function skillIdsForSkillArea(skillArea) {
			if (skillArea.Skills != null) {
				vm.skillArea = true;
				skillIds = skillArea.Skills.map(function (skill) {
					return skill.Id;
				});
			}
		}

		function loadAgentStates() {
			return rtaService.agentStatesFor({
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: skillAreaId,
				inAlarm: vm.showInAlarm,
				excludedStateIds: excludedStateIds().map(function (s) {
					return s === nullStateId ? null : s;
				}),
				textFilter: vm.filterText,
				orderBy: vm.orderBy,
				direction: vm.direction
			});
		}

		function updateAgentStates(states) {
			vm.agentStates = [];
			lastUpdate = states.Time;
			var now = moment(states.Time);
			states.States.forEach(function (state) {
				vm.agentStates.push(rtaAgentsBuildService.buildAgentState(now, state));
			});
			vm.timeline = rtaFormatService.buildTimeline(states.Time);
			vm.isLoading = false;
			$state.go($state.current.name, {es: excludedStateIds()}, {notify: false});
			return states;
		}

		function updateStatesFromAgentStates(data) {
			data.States.forEach(function (agentState) {
				var stateId = agentState.StateId || nullStateId;
				var addIt = (vm.states
					.map(function (s) {
						return s.Id;
					})
					.indexOf(stateId) === -1);
				if (addIt) {
					vm.states.push({
						Id: stateId,
						Name: agentState.State || nullState,
						Selected: excludedStatesFromUrl.indexOf(agentState.StateId) == -1
					});
				}
			});
			addMissingNoState();
			sortPhoneStatesByName();
		}

		function addMissingNoState() {
			if (excludedStatesFromUrl.indexOf(nullStateId) > -1 &&
				vm.states.filter(function (s) {
					return s.Id === nullStateId;
				}).length === 0) {
				vm.states.push({
					Id: nullStateId,
					Name: nullState,
					Selected: false
				});
			}
		}

		function loadExcludedStates() {
			var stateIdsWithoutNull = excludedStatesFromUrl.filter(function (s) {
				return s !== nullStateId;
			});
			if (stateIdsWithoutNull.length !== 0) {
				rtaService.getPhoneStates(stateIdsWithoutNull)
					.then(function (states) {
						vm.states = vm.states.concat(states.PhoneStates);
						sortPhoneStatesByName();
					});
			}
		}

		function sortPhoneStatesByName() {
			vm.states = $filter('orderBy')(vm.states, function (state) {
				return state.Name;
			});
		}

		$scope.$watch(
			function () {
				return vm.showInAlarm;
			},
			function (newValue, oldValue) {
				if (!poller)
					return;
				if (newValue !== oldValue) {
					if (newValue) {
						vm.orderBy = undefined;
						vm.direction = undefined;
					}
					else {
						vm.orderBy = "Name";
						vm.direction = "asc";
					}
					poller.force();
				}
			});

		$scope.$watch(
			function () {
				return vm.filterText;
			},
			function (newValue, oldValue) {
				if (!poller)
					return;
				if (newValue !== oldValue)
					poller.force();
			});

		$scope.$watch(
			function () {
				return $sessionStorage.buid;
			},
			function (newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue)
					rtaRouteService.goToOverview();
			}
		);

		function excludedStateIds() {
			var included = vm.states
				.filter(function (s) {
					return s.Selected === true;
				})
				.map(function (s) {
					return s.Id;
				});
			var excludedViaUrlAndNotManuallyIncluded = excludedStatesFromUrl
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
			return excludedUnique.concat(excluded);
		}

		vm.sort = function (column) {
			if (vm.orderBy !== column)
				vm.direction = 'asc';
			else
				vm.direction = vm.direction === 'asc' ? 'desc' : 'asc';
			vm.orderBy = column;
			poller.force();
		}
	};
})();
