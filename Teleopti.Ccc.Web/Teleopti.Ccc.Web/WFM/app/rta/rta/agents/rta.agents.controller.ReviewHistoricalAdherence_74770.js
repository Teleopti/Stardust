(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController74770', RtaAgentsController);

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
		'rtaStateService',
		'rtaDataService',
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
								 rtaStateService,
								 rtaDataService,
								 NoticeService,
								 rtaConfigurationValidator) {

		var vm = this;

		rtaConfigurationValidator.validate();
		vm.showInAlarm = !$stateParams.showAllAgents;

		var lastUpdate;
		var notice;
		vm.filterText = null;
		vm.pause = false;
		vm.pausedAt = null;
		var phoneStates;
		var skills;
		var skillGroups;

		rtaStateService.setCurrentState($stateParams)
			.then(function () {
				vm.statePickerSelectionText = rtaStateService.statePickerSelectionText();
				updateSkillPicker();
				updateSkillGroupPicker();
			});

		rtaDataService.load().then(function (data) {
			buildSkills(data.skills);
			buildSkillGroups(data.skillAreas);
			buildSites(data.organization);
			buildPhoneStates(data.states);
			vm.hasHistoricalOverviewPermission = data.permissions.HistoricalOverview;
		});

		var defaultSorting = function () {
			vm.orderBy = vm.showInAlarm ? undefined : 'Name';
			vm.direction = vm.showInAlarm ? undefined : 'asc';
		};
		defaultSorting();

		vm.loading = function() {
			return !(vm.agentStates && skillGroups && skills && phoneStates && vm.sites);
		};

		vm.displayNoAgentsMessage = function () {
			if (!vm.agentStates)
				return false;
			return vm.agentStates.length === 0;
		};

		vm.changeScheduleUrl = function (personId) {
			return $state.href('teams.for', {personId: personId});
		};
		vm.historicalAdherenceUrl = function (personId) {
			return $state.href('rta-historical-without-date', {personId: personId});
		};
		vm.goToOverview = rtaStateService.goToOverview;


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

		function buildPhoneStates(data) {
			phoneStates = [];
			data.forEach(function (phoneState) {
				var id = phoneState.Id;
				phoneStates.push({
					Id: id,
					Name: phoneState.Name,
					get Selected() {
						return rtaStateService.isStateSelected(id);
					},
					set Selected(value) {
						rtaStateService.selectState(id, value);
						vm.statePickerSelectionText = rtaStateService.statePickerSelectionText();
						forcePoll();
					},
				});
			});
		}

		function updatePhoneStates(states) {

			vm.states = (phoneStates || []).filter(function (phoneState) {
				var stateInView = states.States.some(function (agentState) {
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
					rtaStateService.goToOverview();
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
			vm.sites = [];
			
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
			vm.organizationPickerClearEnabled = (vm.sites || []).some(function (site) {
				return site.isChecked || site.isMarked;
			});
		}


		vm.skillPickerOpen = false;

		$scope.$watch(
			function () {
				return vm.skillPickerText;
			},
			function (newValue, oldValue) {
				buildSkills(skills);
			}
		);

		Object.defineProperty(vm, 'selectedSkill', {
			get: function () {
				return rtaStateService.selectedSkill();
			},
			set: function (value) {
				vm.skillGroupPickerText = undefined;
				vm.skillPickerOpen = false;
				rtaStateService.selectSkill(value);
				updateSkillPicker();
				forcePoll();
			}
		});

		function buildSkills(s) {
			skills = s;
			vm.skills = $filter('filter')(s, vm.skillPickerText);
			vm.displayNoSkillsMessage = angular.isDefined(vm.skills) && vm.skills.length === 0;
		}

		function updateSkillPicker() {
			vm.skillPickerText = rtaStateService.skillPickerSelectionText();
		}


		vm.skillGroupPickerOpen = false;

		$scope.$watch(
			function () {
				return vm.skillGroupPickerText;
			},
			function (newValue, oldValue) {
				buildSkillGroups(skillGroups);
			}
		);

		Object.defineProperty(vm, 'selectedSkillGroup', {
			get: function () {
				return rtaStateService.selectedSkillArea();
			},
			set: function (value) {
				vm.skillPickerText = undefined;
				vm.skillGroupPickerOpen = false;
				rtaStateService.selectSkillArea(value);
				updateSkillGroupPicker();
				forcePoll();
			}
		});

		function buildSkillGroups(s) {
			skillGroups = s;
			vm.skillGroups = $filter('filter')(s, vm.skillGroupPickerText);
			vm.displayNoSkillGroupsMessage = angular.isDefined(vm.skillGroups) && vm.skillGroups.length === 0;
		}

		function updateSkillGroupPicker() {
			vm.skillGroupPickerText = rtaStateService.skillGroupPickerSelectionText();
		}

		
		vm.clearSkillSelection = function () {
			rtaStateService.deselectSkillAndSkillArea();
			updateSkillPicker();
			updateSkillGroupPicker();
			forcePoll();
		};

		vm.goToHistorical = function () {
			$state.go('rta-historical-overview', {});
		};
	}
})();
