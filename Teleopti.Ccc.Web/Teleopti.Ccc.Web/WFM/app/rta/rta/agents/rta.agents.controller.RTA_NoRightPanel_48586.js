(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaAgentsController48586', RtaAgentsController);

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
								 NoticeService,
								 rtaConfigurationValidator,
								 rtaNamesFormatService) {

		var vm = this;

		rtaConfigurationValidator.validate();

		vm.agentStates = [];

		// duplication of state
		vm.showInAlarm = !$stateParams.showAllAgents;

		var lastUpdate, notice;

		var skillIds = angular.isArray($stateParams.skillIds) ? $stateParams.skillIds[0] || null : $stateParams.skillIds;
		var skillAreaId = $stateParams.skillAreaId || undefined;
		$stateParams.es = $stateParams.es || [];
		vm.sites = [];

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
			showBackdrop: false,
			showResizer: false,
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
			return loadAgentStates()
				.then(updateAgentStates)
				.then(updatePhoneStates);
		}

		function loadAgentStates() {
			var o = rtaStateService.pollParams2();
			o.inAlarm = vm.showInAlarm;
			o.excludedStateIds = excludedPhoneStateIds().map(function (s) {
				return s === nullStateId ? null : s;
			});
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
		};

		////////no right panel stuff /////////////////

		rtaStateService.setCurrentState($stateParams)
			.then(function () {

				vm.skillsLoaded = true;
				vm.placeholderTextForSkillSelection = $translate.instant('SelectSkill');
				vm.skills = rtaStateService.skills();
				vm.selectedSkill = vm.skills.find(function (s) {
					return s.Id === skillIds;
				});

				vm.skillAreasLoaded = true;
				vm.placeholderTextForSkillGroupSelection = $translate.instant('SelectSkillGroup');
				vm.skillAreas = rtaStateService.skillAreas();
				vm.selectedSkillArea = vm.skillAreas.find(function (s) {
					return s.Id === skillAreaId
				});

				var organization = rtaStateService.organization();
				organization.forEach(function (site) {
					var siteModel = {
						Id: site.Id,
						Name: site.Name,
						FullPermission: site.FullPermission,
						Teams: [],
						get isChecked() {
							return rtaStateService.isSiteSelected(site.Id);
						},
						set isChecked(newValue) {
							rtaStateService.selectSite2(site.Id, newValue);
						},
						get isMarked() {
							return rtaStateService.isSiteMarked(site.Id);
						},
						toggle: function () {
							rtaStateService.selectSite2(site.Id, !rtaStateService.isSiteSelected(site.Id));
							updateSelectFieldText();
						}
					};
					updateTeams(siteModel, site.Teams);
					vm.sites.push(siteModel);
				});

				function updateTeams(siteModel, teams) {
					teams.forEach(function (team) {
						siteModel.Teams.push({
							Id: team.Id,
							Name: team.Name,
							get isChecked() {
								return rtaStateService.isTeamSelected(team.Id);
							},
							set isChecked(newValue) {
								rtaStateService.selectTeam2(team.Id, newValue);
							},
							toggle: function () {
								rtaStateService.selectTeam2(team.Id, !rtaStateService.isTeamSelected(team.Id));
								updateSelectFieldText();
							}
						});
					})
				}

				updateSelectFieldText();
			});

		// scoped variables
		vm.selectFieldText;
		vm.searchTerm = "";
		//scoped functions
		vm.querySearch = querySearch;
		vm.expandSite = expandSite;
		vm.clearOrgSelection = clearOrgSelection;
		vm.clearSearchTerm = clearSearchTerm;
		vm.clearSelection = clearSelection;
		vm.openPicker = false;
		vm.placeholderTextForSkillSelection = $translate.instant('LoadingSkills');
		vm.placeholderTextForSkillGroupSelection = $translate.instant('LoadingSkillGroups');
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedSkillAreaChange = selectedSkillAreaChange;

		/*********AUTOCOMPLETE*****/
		function querySearch(query, myArray) {
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

		function selectedSkillChange(skill) {
			var newSkillId = skill ? skill.Id : null;
			if (newSkillId != skillIds)
				rtaStateService.selectSkill(newSkillId);
		}

		function selectedSkillAreaChange(skillArea) {
			var newSkillAreaId = skillArea ? skillArea.Id : null;
			if (newSkillAreaId != skillAreaId)
				rtaStateService.selectSkillArea(newSkillAreaId);
		}

		function clearSelection() {
			rtaStateService.deselectSkillAndSkillArea();
		}

		function clearOrgSelection() {
			rtaStateService.deselectOrganization();
			updateSelectFieldText();
		};

		/***********MULTI-SELECT************/
		function expandSite(site) {
			site.isExpanded = !site.isExpanded;
		};

		function updateSelectFieldText() {
			var selectedSites = rtaStateService.selectedSiteIds();
			var selectedTeams = rtaStateService.selectedTeamIds();
			var selectedFieldText = rtaNamesFormatService.getSelectedFieldText(vm.sites, selectedSites, selectedTeams);
			if (selectedFieldText != null)
				vm.selectFieldText = selectedFieldText;
			vm.siteMarkedOrChecked = vm.sites.some(function (site) {
				return site.isChecked || site.isMarked;
			});
		}

		function clearSearchTerm() {
			vm.searchTerm = '';
		}
	}
})();
