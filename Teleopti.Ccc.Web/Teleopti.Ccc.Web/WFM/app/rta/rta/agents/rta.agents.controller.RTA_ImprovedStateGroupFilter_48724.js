(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController48724', RtaAgentsController);

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
		'rtaDataService',
		'NoticeService',
		'rtaConfigurationValidator',
		'rtaNamesFormatService'
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
								 rtaDataService,
								 NoticeService,
								 rtaConfigurationValidator) {

		var vm = this;

		rtaConfigurationValidator.validate();

		vm.agentStates = [];
		vm.sites = [];
		vm.states = [];
		vm.showInAlarm = !$stateParams.showAllAgents;

		var lastUpdate;
		var notice;
		vm.filterText = null;
		vm.pause = false;
		vm.pausedAt = null;
		vm.loading = true;
		var phoneStates = [];

		rtaStateService.setCurrentState($stateParams);

		rtaDataService.load().then(function (data) {
			vm.loading = false;
			vm.skills = data.skills;
			vm.skillAreas = data.skillAreas;
			buildSites(data.organization);
			buildPhoneStates(data.states);
		});

		var defaultSorting = function () {
			vm.orderBy = vm.showInAlarm ? undefined : 'Name';
			vm.direction = vm.showInAlarm ? undefined : 'asc';
		};
		defaultSorting();

		vm.displayNoAgentsMessage = function () {
			return vm.agentStates.length === 0;
		};
		vm.displayNoAgentsForSkillMessage = rtaStateService.hasSkillSelection;

		vm.changeScheduleUrl = function (personId) {
			return rtaRouteService.urlForChangingSchedule(personId);
		};
		vm.historicalAdherenceUrl = function (personId) {
			return rtaRouteService.urlForHistoricalAdherence(personId);
		};
		vm.goToOverview = rtaRouteService.goToOverview;


		var poller;

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

		function forcePoll() {
			if (poller)
				poller.force();
		}

		function pollAgentStates() {
			return loadAgentStates()
				.then(updateAgentStates)
				.then(updatePhoneStates);
		}

		function loadAgentStates() {
			var o = rtaStateService.pollParams({
				skillIds: true,
				siteIds: true,
				teamIds: true,
				excludedStateIds: true
			});
			o.inAlarm = vm.showInAlarm;
			o.textFilter = vm.filterText || undefined;
			o.orderBy = vm.orderBy;
			o.direction = vm.direction;
			return rtaService.agentStatesFor(o);
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

		vm.statePickerSelectionText = undefined;

		function buildPhoneStates(data) {

			var makeState = function (id, name) {
				return {
					Id: id,
					Name: name,
					get Selected() {
						return rtaStateService.isStateSelected(id);
					},
					set Selected(value) {
						rtaStateService.selectState(id, value);
						vm.statePickerSelectionText = rtaStateService.statePickerSelectionText();
						forcePoll();
					},
				};
			};
			
			data.forEach(function (phoneState) {
				phoneStates.push(makeState(phoneState.Id, phoneState.Name));
			});
		}

		function updatePhoneStates(states) {

			vm.states = phoneStates.filter(function (phoneState) {
				var stateInView = states.States.some(function (agentState) {
					if (agentState.StateId === null)
						return true;
					return agentState.StateId === phoneState.Id;
				});
				return stateInView || !phoneState.Selected;
			});

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
		};

		function buildSites(organization) {

			organization.forEach(function (site) {
				var siteModel = {
					Id: site.Id,
					Name: site.Name,
					FullPermission: site.FullPermission,
					Teams: [],
					get isChecked() {
						return rtaStateService.isSiteSelected(site.Id);
					},
					get isMarked() {
						return rtaStateService.siteHasTeamsSelected(site.Id);
					},
					toggle: function () {
						rtaStateService.toggleSite(site.Id);
						updateOrganizationPicker();
						forcePoll();
					}
				};

				site.Teams.forEach(function (team) {
					siteModel.Teams.push({
						Id: team.Id,
						Name: team.Name,
						get isChecked() {
							return rtaStateService.isTeamSelected(team.Id);
						},
						toggle: function () {
							rtaStateService.toggleTeam(team.Id);
							updateOrganizationPicker();
							forcePoll();
						}
					});
				});

				vm.sites.push(siteModel);
			});

			updateOrganizationPicker();
		}

		vm.clearOrganizationSelection = function () {
			rtaStateService.deselectOrganization();
			updateOrganizationPicker();
			forcePoll();
		};

		function updateOrganizationPicker() {
			vm.organizationPickerSelectionText = rtaStateService.organizationSelectionText();
			vm.organizationPickerClearEnabled = vm.sites.some(function (site) {
				return site.isChecked || site.isMarked;
			});
		}

		Object.defineProperty(vm, 'selectedSkillNew', {
			get: function () {
				return rtaStateService.selectedSkill();
			},
			set: function (value) {
				rtaStateService.selectSkill(value);
				forcePoll();
			}
		});

		Object.defineProperty(vm, 'selectedSkillArea', {
			get: function () {
				return rtaStateService.selectedSkillArea();
			},
			set: function (value) {
				rtaStateService.selectSkillArea(value);
				forcePoll();
			}
		});

		vm.filterSkills = function (query, data) {
			if (!query)
				return data;
			var q = angular.lowercase(query);
			return data.filter(function (item) {
				return angular.lowercase(item.Name).indexOf(q) === 0;
			});
		};

		vm.clearSkillSelection = rtaStateService.deselectSkillAndSkillArea
	}
})();
