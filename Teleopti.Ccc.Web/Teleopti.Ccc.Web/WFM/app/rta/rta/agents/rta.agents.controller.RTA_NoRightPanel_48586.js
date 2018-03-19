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
		'$q',
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
								 $q,
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

		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = angular.isArray($stateParams.skillIds) ? $stateParams.skillIds[0] || null : $stateParams.skillIds;
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
		
		////////no right panel stuff /////////////////

		rtaStateService.setCurrentState($stateParams);
		var agentsState = "rta-agents";

		// scoped variables
		vm.teamsSelected = [];
		vm.selectFieldText;
		vm.searchTerm = "";
		//scoped functions
		vm.querySearch = querySearch;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedSkillAreaChange = selectedSkillAreaChange;
		vm.expandSite = expandSite;
		vm.goToAgents = goToAgents;	
		vm.clearOrgSelection = clearOrgSelection;
		vm.clearSearchTerm = clearSearchTerm;
		vm.showOrganization = $state.current.name === agentsState;
		vm.clearSelection = clearSelection;
		vm.openPicker = false;
		vm.placeholderTextForSkillSelection = $translate.instant('LoadingSkills');
		vm.placeholderTextForSkillGroupSelection = $translate.instant('LoadingSkillGroups');
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;

		(function initialize() {
			rtaService.getSkills()
				.then(function (skills) {
					var deffered = $q.defer();
					deffered.resolve();
					vm.skillsLoaded = true;
					vm.placeholderTextForSkillSelection = $translate.instant('SelectSkill');
					vm.skills = skills;
					if (skillIds != null && skillAreaId == null) {
						vm.selectedSkill = skills.find(function (s) {
							return s.Id === skillIds;
						});
					}
					return deffered.promise;
				}).then(function () {
				rtaService.getSkillAreas()
					.then(function (skillAreas) {
						var deffered = $q.defer();
						deffered.resolve();
						vm.skillAreasLoaded = true;
						vm.placeholderTextForSkillGroupSelection = $translate.instant('SelectSkillGroup');
						vm.skillAreas = skillAreas;
						if (skillAreaId != null && skillIds == null)
							vm.selectedSkillArea = vm.skillAreas.find(function (s) { return s.Id === skillAreaId });
						return deffered.promise;
					})
					.then(function () {
						getOrganizationCall()
							.then(function (organization) {
								vm.sites = organization;
								vm.sites.forEach(function (site) {
									var isSiteInParams = siteIds.indexOf(site.Id) > -1;
									site.isChecked = isSiteInParams || false;
									site.toggle = function () {
										site.isChecked = !site.isChecked;
										site.isMarked = false;
										site.Teams.forEach(function (team) {
											team.isChecked = site.isChecked;
										})
										vm.siteMarkedOrChecked = vm.sites.filter(function (site) { return site.isChecked || site.isMarked; }).length > 0;
									}
									site.Teams.forEach(function (team) {
										var isTeamInParams = teamIds.indexOf(team.Id) > -1;
										team.isChecked = isTeamInParams || site.isChecked || false;
										site.isMarked = site.Teams.some(function (t) {
											return t.isChecked;
										});
										team.toggle = function () {
											team.isChecked = !team.isChecked;
											site.isChecked = site.Teams.every(function (t) {
												return t.isChecked;
											});
											site.isMarked = site.Teams.some(function (t) {
												return t.isChecked;
											});
											vm.siteMarkedOrChecked = vm.sites.filter(function (site) { return site.isChecked || site.isMarked; }).length > 0;
										}
									})
								});
								vm.siteMarkedOrChecked = vm.sites.filter(function (site) { return site.isChecked || site.isMarked; }).length > 0;
								updateSelectFieldText();
							});
					});
			});

			function getOrganizationCall() {
				var skillIds2 = angular.isArray($stateParams.skillIds) ? $stateParams.skillIds[0] || null : $stateParams.skillIds;
				var theSkillIds = skillIds2 != null ? [skillIds2] : null;
				var skillIdsForOrganization = skillAreaId != null ? getSkillIdsFromSkillArea(skillAreaId) : theSkillIds;
				return skillIdsForOrganization != null ? rtaService.getOrganizationForSkills({ skillIds: skillIdsForOrganization }) : rtaService.getOrganization();
			}

			function getSkillIdsFromSkillArea(skillAreaId) {
				var theSkillArea = vm.skillAreas.find(function (skillArea) {
					return skillArea.Id === skillAreaId;
				});
				if (theSkillArea && theSkillArea.Skills != null) {
					return theSkillArea.Skills.map(function (skill) { return skill.Id; });
				}
				return null;
			}
		})();

		function updateSelectFieldText() {
			var selectedFieldText = rtaNamesFormatService.getSelectedFieldText(vm.sites, siteIds, teamIds);
			if (selectedFieldText.length > 0) {
				vm.selectFieldText = selectedFieldText;
			}
		}

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
			if (!skill && vm.showOrganization)
				rtaStateService.selectSkillArea(skillAreaId);
			else if ((skill.Id != skillIds || $stateParams.skillAreaId) && vm.showOrganization)
				rtaStateService.selectSkill(skill.Id);
		}
		
		function selectedSkillAreaChange(skillArea) {
			if (!skillArea && vm.showOrganization)
				rtaStateService.selectSkill($stateParams.skillIds);
			else if (!(skillArea.Id == $stateParams.skillAreaId) && vm.showOrganization)
				rtaStateService.selectSkillArea(skillArea.Id);
		}

		function clearSelection() {
			vm.selectedSkill ? vm.selectedSkill = null : vm.selectedSkillArea = null;
		}

		/***********MULTI-SELECT************/
		function expandSite(site) { site.isExpanded = !site.isExpanded; };

		function shrinkSites() {
			vm.sites.forEach(function (site) {
				site.isExpanded = false;
			});
		}

		function goToAgents() {
			if (!vm.openPicker) return;
			shrinkSites();
			var selection = vm.sites.reduce(function (acc, site) {
				if (site.isChecked && site.FullPermission) {
					acc.siteIds.push(site.Id);
				}
				else if (site.isChecked) {
					site.Teams.forEach(function (team) {
						acc.teamIds.push(team.Id);
					})
				}
				else if (site.isMarked) {
					acc.teamIds = acc.teamIds.concat(
						site.Teams
							.filter(function (team) {
								return team.isChecked;
							})
							.map(function (team) {
								return team.Id;
							})
					);
				}
				return acc;
			}, {
				siteIds: [],
				teamIds: []
			})

			if ($stateParams.skillIds)
				selection['skillIds'] = $stateParams.skillIds;
			else if ($stateParams.skillAreaId)
				selection['skillAreaId'] = $stateParams.skillAreaId;
			if (angular.toJson(selection.siteIds) === angular.toJson(siteIds) && angular.toJson(selection.teamIds) === angular.toJson(teamIds)) {
				vm.openPicker = false;
				return;
			}

			rtaStateService.setCurrentState(selection);
			rtaStateService.goToAgents();
		}

		function clearOrgSelection() {
			vm.siteMarkedOrChecked = 0;
			vm.sites.forEach(function (site) {
					site.isChecked = false;
					site.isMarked = false;
					site.Teams.forEach(function (team) {
						team.isChecked = false;
					});
				}
			);
		};

		function clearSearchTerm() { vm.searchTerm = ''; }
	}
})();
