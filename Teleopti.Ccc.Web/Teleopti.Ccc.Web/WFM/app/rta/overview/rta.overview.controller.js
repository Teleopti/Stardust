(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController', RtaOverviewController);

	RtaOverviewController.$inject = [
		'$scope',
		'$stateParams',
		'$interval',
		'$filter',
		'$sessionStorage',
		'$state',
		'$translate',
		'$timeout',
		'$q',
		'rtaService',
		'rtaRouteService',
		'rtaFormatService',
		'rtaAdherenceService',
		'NoticeService',
		'localeLanguageSortingService',
		'Toggle'
	];

	function RtaOverviewController(
		$scope,
		$stateParams,
		$interval,
		$filter,
		$sessionStorage,
		$state,
		$translate,
		$timeout,
		$q,
		rtaService,
		rtaRouteService,
		rtaFormatService,
		rtaAdherenceService,
		NoticeService,
		localeLanguageSortingService,
		toggleService
	) {

		var vm = this;
		var siteId = $stateParams.siteIds || [];
		var stateForTeamsBySkill = 'rta.teams({siteIds: site.Id, skillIds: vm.skillId})';
		var stateForTeamsBySkillArea = 'rta.teams({siteIds: site.Id, skillAreaId: vm.skillAreaId})';
		var stateForTeams = 'rta.teams({siteIds: site.Id})';
		var stateForAgentsByTeamsAndSkill = 'rta.agents({teamIds: team.Id, skillIds: vm.skillId})';
		var stateForAgentsByTeamsAndSkillArea = 'rta.agents({teamIds: team.Id, skillAreaId: vm.skillAreaId})';

		var organizationData = [];

		var pollingInterval = angular.isDefined($stateParams.pollingInterval) ? $stateParams.pollingInterval : 5000;
		var pollingLock = true;

		var previousState = "";
		var currentState = "";
		/***scoped variables */
		vm.selectedItemIds = { siteIds: [], teamIds: [] };
		vm.displaySkillOrSkillAreaFilter = false;
		vm.skillAreas = [];
		vm.skillId = $stateParams.skillIds || null;
		vm.skillAreaId = $stateParams.skillAreaId || null;
		vm.siteIds = angular.isArray(siteId) ? siteId[0] || null : siteId;
		vm.sortByLocaleLanguage = localeLanguageSortingService.sort;
		/***scoped functions */

		vm.goToDashboard = function () { rtaRouteService.goToSites(); };
		vm.goToSelectSkill = function () { rtaRouteService.goToSelectSkill(); };

		if (toggleService.togglesLoaded.then(function () {
			vm.urlForSelectSkill = urlForSelectSkill;
			vm.getStateForTeams = getStateForTeams;
			vm.getStateForAgents = getStateForAgents;
			vm.toggleSelection = toggleSelection;
			vm.openSelectedItems = openSelectedItems;

			(function initialize() {
				pollingLock = false;
				rtaService.getOrganization().then(function (result) {
					organizationData = result;
				});
				rtaService.getSkillAreas().then(function (skillAreas) {
					vm.hasPermission = true;
					vm.skillAreas = skillAreas.SkillAreas;
					getSitesOrTeams();
				});
			})();

			function urlForSelectSkill() { return rtaRouteService.urlForSelectSkill(); };

			function getStateForTeams() {
				if (vm.skillId !== null || vm.skillAreaId !== null)
					return vm.skillAreaId !== null ? stateForTeamsBySkillArea : stateForTeamsBySkill;
				else return stateForTeams;
			};

			function getStateForAgents() { return (vm.skillAreaId !== null) ? stateForAgentsByTeamsAndSkillArea : stateForAgentsByTeamsAndSkill; };

			function toggleSelection(itemId) {
				if (vm.siteIds != null) {
					var indexOfTeam = vm.selectedItemIds.teamIds.indexOf(itemId);
					if (indexOfTeam > -1) vm.selectedItemIds.teamIds.splice(indexOfTeam, 1);
					else vm.selectedItemIds.teamIds.push(itemId);
				}
				else {
					var indexOfSite = vm.selectedItemIds.siteIds.indexOf(itemId);
					if (indexOfSite > -1) vm.selectedItemIds.siteIds.splice(indexOfSite, 1);
					else {
						organizationData.forEach(function (org) {
							if (itemId === org.Id && org.FullPermission) {
								vm.selectedItemIds.siteIds.push(itemId);
							}
							else {
								if(org.Id == itemId)
									org.Teams.forEach(function (team) {
										var indexOfTeam = vm.selectedItemIds.teamIds.indexOf(team.Id);
										if (indexOfTeam > -1) 
										vm.selectedItemIds.teamIds.splice(indexOfTeam, 1);
										else
										vm.selectedItemIds.teamIds.push(team.Id);
									});
							}
						})

					}
				}
			}

			function openSelectedItems() {
				if (vm.selectedItemIds.length === 0) return;
				goToAgents(vm.selectedItemIds);
			};

			var polling = $interval(function () {
				if (!pollingLock) return;
				if (pollingLock)
					pollingLock = false;
				if (vm.skillId !== null || vm.skillAreaId !== null) {
					if (vm.skillAreaId !== null)
						vm.skillId = skillIdsFromSkillArea(vm.skillAreaId);
					if (vm.siteIds !== null && angular.isDefined(vm.teams)) {
						rtaService.getTeamCardsFor({
							skillIds: vm.skillId,
							siteIds: vm.siteIds
						})
							.then(function (teamAdherences) {
								currentState = JSON.stringify(teamAdherences);
								if (previousState != currentState) {
									vm.teams = teamAdherences;
									previousState = currentState;
								}
								pollingLock = true;
							});
					} else if (angular.isDefined(vm.sites)) {
						rtaService.getAdherenceForSitesBySkills(vm.skillId)
							.then(function (siteAdherences) {
								currentState = JSON.stringify(siteAdherences);
								if (previousState != currentState) {
									vm.sites = siteAdherences;
									previousState = currentState;
								}
								pollingLock = true;
							});
					};
				} else if (vm.siteIds) {
					rtaService.getTeamCardsFor({ siteIds: vm.siteIds })
						.then(function (teamAdherences) {
							currentState = JSON.stringify(teamAdherences);
							if (previousState != currentState) {
								vm.teams = teamAdherences;
								previousState = currentState;
							}
							pollingLock = true;
						});
				} else {
					rtaService.getAdherenceForAllSites().then(function (siteAdherences) {
						currentState = JSON.stringify(siteAdherences);
						if (previousState != currentState) {
							vm.sites = siteAdherences;
							previousState = currentState;
						}
						pollingLock = true;
					});
				}
			},
				pollingInterval);

			function cancelPolling() {
				if (polling != null) {
					$interval.cancel(polling);
					pollingLock = true;
				}
			}

			function getSitesOrTeams() {
				if (vm.skillId !== null || vm.skillAreaId !== null) {
					return vm.siteIds !== null ? getTeamsBySkillsInfo() : getSitesBySkillsInfo();
				} else {
					return vm.siteIds ? getTeamsInfo() : getSitesInfo();
				}
			};

			function goToAgents(selectedItemIds) {
				var stateParamsObject = {};
				var skillOrSkillAreaKey = vm.skillIds != null ? 'skillIds' : 'skillAreaId';
				var skillOrSkillAreaValue = vm.skillIds != null ? vm.skillIds : vm.skillAreaId;
				if (selectedItemIds.siteIds.length > 0 && selectedItemIds.teamIds.length > 0) {
					stateParamsObject['siteIds'] = selectedItemIds.siteIds;
					stateParamsObject['teamIds'] = selectedItemIds.teamIds;
				}
				else if (selectedItemIds.teamIds.length > 0) {
					stateParamsObject['teamIds'] = selectedItemIds.teamIds;
				}
				else if (selectedItemIds.siteIds.length > 0) {
					stateParamsObject['siteIds'] = selectedItemIds.siteIds;
				}
				if (vm.skillIds || vm.skillAreaId) {
					stateParamsObject[skillOrSkillAreaKey] = skillOrSkillAreaValue;
				}
				rtaRouteService.goToAgents(stateParamsObject);
			};

			function getTeamsBySkillsInfo() {
				pollingLock = false;
				vm.skillIds = getSkillIds();
				rtaService.getTeamCardsFor({
					skillIds: vm.skillIds,
					siteIds: vm.siteIds
				}).then(function (teamAdherences) {
					vm.teams = teamAdherences;
					pollingLock = true;
				});
			};

			function getSitesBySkillsInfo() {
				pollingLock = false;
				vm.skillIds = getSkillIds();
				return rtaService.getAdherenceForSitesBySkills(vm.skillIds).then(function (sitesBySkill) {
					vm.sites = sitesBySkill;
					pollingLock = true;
				});
			}

			function getTeamsInfo() {
				pollingLock = false;
				rtaService.getTeamCardsFor({ siteIds: vm.siteIds }).then(function (teamAdherences) {
					vm.teams = teamAdherences;
					pollingLock = true;
				});
			};

			function getSitesInfo() {
				pollingLock = false;
				rtaService.getAdherenceForAllSites().then(function (siteAdherences) {
					vm.sites = siteAdherences;
					pollingLock = true;
				});
			};

			function skillIdsFromSkillArea(skillAreaId) {
				return vm.skillAreas.find(function (skillArea) {
					return skillArea.Id === skillAreaId;
				})
					.Skills.map(function (skill) {
						return skill.Id;
					});
			};

			function getSkillIds() {
				return vm.skillAreaId !== null ? skillIdsFromSkillArea(vm.skillAreaId) : [vm.skillId];
			}

			$scope.$watch(
				function () { return $sessionStorage.buid; },
				function (newValue, oldValue) {
					if (angular.isDefined(oldValue) && newValue !== oldValue)
						rtaRouteService.goToSites();
				}
			);

			$scope.$on('$destroy', function () {
				cancelPolling();
			});

		}
		));
	};
})();
