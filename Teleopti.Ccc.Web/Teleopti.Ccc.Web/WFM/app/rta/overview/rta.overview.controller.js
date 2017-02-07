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
		var stateForTeamsBySkill = 'rta.teams({siteIds: site.Id, skillIds: vm.selectedSkill.Id})';
		var stateForTeamsBySkillArea = 'rta.teams({siteIds: site.Id, skillAreaId: vm.selectedSkillArea.Id})';
		var stateForTeams = 'rta.teams({siteIds: site.Id})';
		var stateForAgentsByTeamsAndSkill = 'rta.agents({teamIds: team.Id, skillIds: vm.selectedSkill.Id})';
		var stateForAgentsByTeamsAndSkillArea = 'rta.agents({teamIds: team.Id, skillAreaId: vm.selectedSkillArea.Id})';
		/***scoped variables */
		vm.selectedItemIds = [];
		vm.displaySkillOrSkillAreaFilter = false;
		vm.skills = [];
		vm.skillAreas = [];
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.skillId = $stateParams.skillIds || null;
		vm.skillAreaId = $stateParams.skillAreaId || null;
		vm.siteIds = angular.isArray(siteId) ? siteId[0] || null : siteId;
		vm.getAdherencePercent = rtaFormatService.numberToPercent;
		vm.getAdherencePercent = rtaFormatService.numberToPercent;
		vm.sortByLocaleLanguage = rtaLocaleLanguageSortingService.sort;
		vm.goBackToRootWithUrl = rtaRouteService.urlForSites(vm.skillId, vm.skillAreaId);
		/***scoped functions */
		vm.querySearch = querySearch;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedSkillAreaChange = selectedSkillAreaChange;
		vm.urlForSelectSkill = urlForSelectSkill;
		vm.getStateForTeams = getStateForTeams;
		vm.getStateForAgents = getStateForAgents;
		vm.goBackWithUrl = goBackWithUrl;
		vm.goToDashboard = goToDashboard;
		vm.goToSelectSkill = goToSelectSkill;
		vm.toggleSelection = toggleSelection;
		vm.openSelectedItems = openSelectedItems;

		rtaService.getSkills()
			.then(function (skills) {
				vm.skillsLoaded = true;
				vm.skills = skills;
				if (vm.skillId !== null) {
					vm.selectedSkill = skills.find(function (skill) { return skill.Id === vm.skillId });
				}
				var defer = $q.defer();
				defer.resolve();
				return defer.promise;
			})
			.then(function () {
				return rtaService.getSkillAreas();
			})
			.then(function (skillAreas) {
				vm.skillAreasLoaded = true;
				vm.skillAreas = skillAreas.SkillAreas;
				if (vm.skillAreaId !== null) {
					vm.selectedSkillArea = skillAreas.SkillAreas.find(function (skillArea) { return skillArea.Id === vm.skillAreaId; });
				}
				getSitesOrTeams();
			});

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
			if (!skill) return;
			if (skill.Id == vm.skillId) return;
			vm.skillId = skill.Id;
			vm.selectedSkill = skill;
			vm.selectedSkillArea = undefined;
			if ($state.current.name !== "rta.teams" && angular.isDefined($stateParams.siteId))
				rtaRouteService.goToTeams(vm.siteIds, skill.Id, vm.selectedSkillArea);
			else
				rtaRouteService.goToSites(skill.Id, vm.selectedSkillArea);
		};

		function selectedSkillAreaChange(skillArea) {
			if (!skillArea) return;
			if (skillArea.Id == vm.skillAreaId) return;
			vm.skillAreaId = skillArea.Id;
			vm.selectedSkillArea = skillArea;
			vm.selectedSkill = undefined;
			if ($state.current.name !== "rta.teams" && $stateParams.siteId)
				rtaRouteService.goToTeams(vm.siteIds, vm.selectedSkill, skillArea.Id);
			else
				rtaRouteService.goToSites(vm.selectedSkill, skillArea.Id);
		};

		function urlForSelectSkill() { return rtaRouteService.urlForSelectSkill(); };

		function getStateForTeams() {
			if (vm.skillId !== null || vm.skillAreaId !== null) return vm.skillId !== null ? stateForTeamsBySkill : stateForTeamsBySkillArea;
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
						.then(function (teamAdherences) {
							updateAdherence(vm.teams, teamAdherences);
						});
				} else if (angular.isDefined(vm.sites)) {
					getAdherenceForSitesBySkills(vm.skillId)
						.then(function (siteAdherences) {
							updateAdherence(vm.sites, siteAdherences);
						});
				};
			}
			else if (vm.siteIds) {
				rtaService.getAdherenceForTeamsOnSite({ siteId: vm.siteIds })
					.then(function (teamAdherence) { rtaAdherenceService.updateAdherence(vm.teams, teamAdherence); });
			} else {
				rtaService.getAdherenceForAllSites()
					.then(function (siteAdherences) { rtaAdherenceService.updateAdherence(vm.sites, siteAdherences); });
			}
		}, 5000);

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
						.then(function (teamAdherences) {
							updateAdherence(vm.teams, teamAdherences);
						});
				})
		}

		function getSitesBySkillsInfo() {
			return getSitesForSkillsOrSkillArea()
				.then(function (sites) {
					vm.sites = sites;
					return getAdherenceForSitesBySkills(vm.skillIds);
				}).then(function (siteAdherences) {
					updateAdherence(vm.sites, siteAdherences);
				});
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

		function updateAdherence(item, adh) {
			rtaAdherenceService.updateAdherence(item, adh);
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
			if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
				vm.goToDashboard();
			}
		});

		$scope.$watch(function () {
			return vm.selectedSkillArea;
		}, function (newValue, oldValue) {
			if (changed(newValue, oldValue) && toggleService.RTA_SiteAndTeamOnSkillOverview_40817) {
				vm.goToDashboard();
			}
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
