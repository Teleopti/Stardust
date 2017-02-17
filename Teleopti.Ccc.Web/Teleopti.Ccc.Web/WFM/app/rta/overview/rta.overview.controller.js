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
		'rtaOrganizationService',
		'rtaService',
		'rtaRouteService',
		'rtaFormatService',
		'rtaAdherenceService',
		'NoticeService',
		'rtaLocaleLanguageSortingService',
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
		rtaOrganizationService,
		rtaService,
		rtaRouteService,
		rtaFormatService,
		rtaAdherenceService,
		NoticeService,
		rtaLocaleLanguageSortingService,
		toggleService
	) {

		var vm = this;
		var siteId = $stateParams.siteIds || [];
		var stateForTeamsBySkill = 'rta.teams({siteIds: site.Id, skillIds: vm.skillId})';
		var stateForTeamsBySkillArea = 'rta.teams({siteIds: site.Id, skillAreaId: vm.skillAreaId})';
		var stateForTeams = 'rta.teams({siteIds: site.Id})';
		var stateForAgentsByTeamsAndSkill = 'rta.agents({teamIds: team.Id, skillIds: vm.skillId})';
		var stateForAgentsByTeamsAndSkillArea = 'rta.agents({teamIds: team.Id, skillAreaId: vm.skillAreaId})';
		var pollingInterval = angular.isDefined($stateParams.pollingInterval) ? $stateParams.pollingInterval : 5000;
		/***scoped variables */
		vm.selectedItemIds = [];
		vm.displaySkillOrSkillAreaFilter = false;
		vm.skillAreas = [];
		vm.skillId = $stateParams.skillIds || null;
		vm.skillAreaId = $stateParams.skillAreaId || null;
		vm.siteIds = angular.isArray(siteId) ? siteId[0] || null : siteId;
		vm.getAdherencePercent = rtaFormatService.numberToPercent;
		vm.getAdherencePercent = rtaFormatService.numberToPercent;
		vm.sortByLocaleLanguage = rtaLocaleLanguageSortingService.sort;
		vm.goBackToRootWithUrl = rtaRouteService.urlForSites(vm.skillId, vm.skillAreaId);
		/***scoped functions */
		vm.urlForSelectSkill = urlForSelectSkill;
		vm.getStateForTeams = getStateForTeams;
		vm.getStateForAgents = getStateForAgents;
		vm.goBackWithUrl = goBackWithUrl;
		vm.goToDashboard = goToDashboard;
		vm.goToSelectSkill = goToSelectSkill;
		vm.toggleSelection = toggleSelection;
		vm.openSelectedItems = openSelectedItems;
		(function initialize() {
			rtaService.getSkillAreas().then(function(skillAreas){
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

		function changed(newValue, oldValue) {
			return newValue !== oldValue && angular.isDefined(oldValue) && oldValue != null && newValue == null;
		}

		function goBackWithUrl() { return rtaRouteService.urlForSites(); };
		function goToDashboard() { rtaRouteService.goToSites(); }
		function goToSelectSkill() { rtaRouteService.goToSelectSkill(); };


		function toggleSelection(itemId) {
			var index = vm.selectedItemIds.indexOf(itemId);
			if (index > -1) vm.selectedItemIds.splice(index, 1);
			else vm.selectedItemIds.push(itemId);
		}

		function openSelectedItems() {
			if (vm.selectedItemIds.length === 0) return;
			goToAgents(vm.selectedItemIds);
		};

		var polling = $interval(function () {
			if (vm.skillId !== null || vm.skillAreaId !== null) {
				if (vm.skillAreaId !== null)
					vm.skillId = skillIdsFromSkillArea(vm.skillAreaId);
				if (vm.siteIds !== null && angular.isDefined(vm.teams)) {
					getAdherenceForTeamsBySkills(vm.siteIds, vm.skillId)
						.then(function (teamAdherences) { rtaAdherenceService.updateAdherence(vm.teams, teamAdherences); });
				} else if (angular.isDefined(vm.sites)) {
					getAdherenceForSitesBySkills(vm.skillId)
						.then(function (siteAdherences) { rtaAdherenceService.updateAdherence(vm.sites, siteAdherences); });
				};
			}
			else if (vm.siteIds) {
				rtaService.getAdherenceForTeamsOnSite({ siteId: vm.siteIds })
					.then(function (teamAdherence) { rtaAdherenceService.updateAdherence(vm.teams, teamAdherence); });
			} else {
				rtaService.getAdherenceForAllSites()
					.then(function (siteAdherences) { rtaAdherenceService.updateAdherence(vm.sites, siteAdherences); });
			}
		}, pollingInterval);

		rtaOrganizationService.getSiteName(vm.siteIds)
			.then(function (name) { vm.siteName = name; });

		function getSitesOrTeams() {
			if (vm.skillId !== null || vm.skillAreaId !== null) { return vm.siteIds !== null ? getTeamsBySkillsInfo() : getSitesBySkillsInfo(); }
			else { return vm.siteIds ? getTeamsInfo() : getSitesInfo(); }
		};

		function goToAgents(selectedItemIds) {
			var stateParamsObject = {};
			var skillOrSkillAreaKey = vm.selectedSkill != null ? 'skillIds' : 'skillAreaId';
			var skillOrSkillAreaValue = vm.selectedSkill != null ? vm.skillIds : vm.skillAreaId;
			var sitesOrTeamsKey = vm.sites ? 'siteIds' : 'teamIds';
			var sitesOrTeamsValue = selectedItemIds;
			if (vm.skillIds || vm.skillAreaId) {
				stateParamsObject[skillOrSkillAreaKey] = skillOrSkillAreaValue;
			}
			stateParamsObject[sitesOrTeamsKey] = sitesOrTeamsValue;
			rtaRouteService.goToAgents(stateParamsObject);
		};

		function getTeamsBySkillsInfo() {
			return getTeamsForSkillOrSkillArea()
				.then(function (teams) {
					vm.teams = teams;
					var teamIds = teams.map(function (team) {
						return team.Id;
					});
					return getAdherenceForTeamsBySkills(vm.siteIds, vm.skillIds)
						.then(function (teamAdherences) { rtaAdherenceService.updateAdherence(vm.teams, teamAdherences); });
				})
		}

		function getSitesBySkillsInfo() {
			return getSitesForSkillsOrSkillArea()
				.then(function (sites) {
					vm.sites = sites;
					return getAdherenceForSitesBySkills(vm.skillIds);
				}).then(function (siteAdherences) { rtaAdherenceService.updateAdherence(vm.sites, siteAdherences); });
		}

		function getTeamsInfo() {
			rtaService.getTeams({ siteId: vm.siteIds })
				.then(function (teams) {
					vm.teams = teams;
					return rtaService.getAdherenceForTeamsOnSite({ siteId: vm.siteIds });
				})
				.then(function (teamAdherence) {
					rtaAdherenceService.updateAdherence(vm.teams, teamAdherence);
				});
		};

		function getSitesInfo() {
			rtaService.getSites()
				.then(function (sites) {
					vm.sites = sites;
					return rtaService.getAdherenceForAllSites();
				})
				.then(function (siteAdherences) {
					rtaAdherenceService.updateAdherence(vm.sites, siteAdherences);
				});
		}

		function getSitesForSkillsOrSkillArea() {
			vm.skillIds = getSkillIds();
			return getSitesForSkills(vm.skillIds);
		}

		function getTeamsForSkillOrSkillArea() {
			vm.skillIds = getSkillIds();
			return getTeamsForSiteAndSkills(vm.skillIds, vm.siteIds);
		}

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

		function getSitesForSkills(skillIds) {
			return rtaService.getSitesForSkills(skillIds);
		}

		function getTeamsForSiteAndSkills(skillIds, siteIds) {
			return rtaService.getTeamsForSiteAndSkills({
				skillIds: skillIds,
				siteIds: siteIds
			});
		};

		function getAdherenceForSitesBySkills(skillIds) {
			return rtaService.getAdherenceForSitesBySkills(skillIds);
		}

		function getAdherenceForTeamsBySkills(siteIds, skillIds) {
			return rtaService.getAdherenceForTeamsBySkills({
				skillIds: skillIds,
				siteIds: siteIds
			})
		};

		$scope.$watch(
			function () { return $sessionStorage.buid; },
			function (newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue)
					rtaRouteService.goToSites();
			}
		);

		$scope.$watch(function () {
			return vm.selectedSkill;
		}, function (newValue, oldValue) {
			if (changed(newValue, oldValue))
				vm.goToDashboard();
		});

		$scope.$watch(function () {
			return vm.selectedSkillArea;
		}, function (newValue, oldValue) {
			if (changed(newValue, oldValue))
				vm.goToDashboard();
		});

		$scope.$on('$destroy', function () { $interval.cancel(polling); });

		if (vm.siteIds === null) {
			var message = $translate.instant('WFMReleaseNotification')
				.replace('{0}', 'RTA')
				.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>")
				.replace('{2}', '</a>')
				.replace('{3}', "<a href='../Anywhere#realtimeadherencesites'>RTA</a>");
			NoticeService.info(message, null, true);
		}
	};
})();
