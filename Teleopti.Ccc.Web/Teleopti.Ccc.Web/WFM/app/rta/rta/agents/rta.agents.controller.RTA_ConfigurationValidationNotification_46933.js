(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController46933', RtaAgentsController);

	RtaAgentsController.$inject = [
		'$scope',
		'$filter',
		'$state',
		'$stateParams',
		'$sessionStorage',
		'$translate',
		'$http',
		'rtaService',
		'rtaPollingService',
		'rtaFormatService',
		'rtaAgentsBuildService',
		'rtaRouteService',
		'rtaStateService',
		'NoticeService',
		'rtaConfigurationValidator'
	];

	function RtaAgentsController($scope,
								 $filter,
								 $state,
								 $stateParams,
								 $sessionStorage,
								 $translate,
								 $http,
								 rtaService,
								 rtaPollingService,
								 rtaFormatService,
								 rtaAgentsBuildService,
								 rtaRouteService,
								 rtaStateService,
								 NoticeService,
								 rtaConfigurationValidator) {

		var vm = this;

		rtaConfigurationValidator.validate();

		vm.agentStates = [];

		// duplication of state
		vm.showInAlarm = !$stateParams.showAllAgents;

		var lastUpdate, notice;

		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		$stateParams.es = $stateParams.es || [];

		// because angular cant handle an array of null in stateparams
		var nullState = "No State";
		var nullStateId = "noState";
		vm.filterText = null;
		vm.states = [];
		vm.hexToRgb = rtaFormatService.formatHexToRgb;
		vm.pause = false;
		vm.pausedAt = null;

			
		vm.displayNoAgentsMessage = function () {
			return vm.agentStates.length === 0;
		};
		vm.displayNoAgentsForSkillMessage = rtaStateService.hasSkillSelection;

		var defaultSorting = function () {
			vm.orderBy = vm.showInAlarm ? undefined : 'Name';
			vm.direction = vm.showInAlarm ? undefined : 'asc';
		};
		defaultSorting();

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

		var phoneStates = [];
		var phoneStatesLoaded;
		loadPhoneStates();

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
					if (notice)
						notice.destroy();
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
					.then(updatePhoneStates);
			} else {
				return loadAgentStates()
					.then(updateAgentStates)
					.then(updatePhoneStates);
			}
		}

		function skillIdsForSkillArea(skillArea) {
			if (skillArea.Skills) {
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
				inAlarm: vm.showInAlarm,
				excludedStateIds: excludedPhoneStateIds().map(function (s) {
					return s === nullStateId ? null : s;
				}),
				textFilter: vm.filterText || undefined,
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
			return states;
		}

		function loadPhoneStates() {

			phoneStates.push({
				Id: nullStateId,
				Name: nullState,
				Selected: !$stateParams.es.some(function (id) {
					return id === nullStateId
				})
			});

			$stateParams.es
				.filter(function (id) {
					return id !== nullStateId
				})
				.forEach(function (id) {
					phoneStates.push({
						Id: id,
						Name: '<unknown>',
						Selected: false
					})
				});

			$http.get('../api/PhoneStates')
				.then(function (response) {
					response.data.forEach(function (phoneState) {
						var existing = phoneStates.find(function (s) {
							return s.Id === phoneState.Id;
						});
						if (existing)
							existing.Name = phoneState.Name;
						else {
							phoneStates.push({
								Id: phoneState.Id,
								Name: phoneState.Name,
								Selected: true
							});
						}
					});
					phoneStatesLoaded = true;
				});

		}

		function updatePhoneStates(states) {

			$state.go($state.current.name, {es: excludedPhoneStateIds()}, {notify: false});

			if (!phoneStatesLoaded)
				return;

			vm.states = phoneStates.filter(function (phoneState) {
				var stateInView = states.States.some(function (agentState) {
					if (agentState.StateId === null && phoneState.Id === nullStateId)
						return true;
					return agentState.StateId === phoneState.Id;
				});
				return stateInView || !phoneState.Selected;
			});

			vm.states = $filter('orderBy')(vm.states, function (state) {
				return state.Name;
			});

		}

		function excludedPhoneStateIds() {
			return phoneStates
				.filter(function (phoneState) {
					return !phoneState.Selected
				})
				.map(function (phoneState) {
					return phoneState.Id;
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
					defaultSorting();
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
					poller.forceSoon();
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

		vm.sort = function (column) {
			if (vm.showInAlarm)
				return;

			if (vm.orderBy !== column)
				vm.direction = 'asc';
			else
				vm.direction = vm.direction === 'asc' ? 'desc' : 'asc';
			vm.orderBy = column;
			poller.force();
		}
	}
})();
