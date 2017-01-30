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
		'rtaOrganizationService',
		'rtaService',
		'rtaRouteService',
		'rtaFormatService',
		'rtaAdherenceService',
		'NoticeService',
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
		rtaOrganizationService,
		rtaService,
		rtaRouteService,
		rtaFormatService,
		rtaAdherenceService,
		NoticeService,
		toggleService
	) {

		var vm = this;
		vm.selectedItemIds = [];
		vm.siteId = $stateParams.siteId || null;
		vm.getAdherencePercent = rtaFormatService.numberToPercent;
		vm.displaySkillOrSkillAreaFilter = false;
		var teamsBySkillsStateName = "rta.teams-by-skill";

		toggleService.togglesLoaded.then(function () {

			vm.displaySkillOrSkillAreaFilter = toggleService.RTA_SiteAndTeamOnSkillOverview_40817;

			if (vm.displaySkillOrSkillAreaFilter) {
				rtaService.getSkills()
					.then(function (skills) {
						vm.skillsLoaded = true;
						vm.skills = skills;
						if (vm.skillId !== null)
							vm.selectedSkill = skills.find(function (skill) { return skill.Id === vm.skillId });
					});

				rtaService.getSkillAreas()
					.then(function (skillAreas) {
						vm.skillAreasLoaded = true;
						vm.skillAreas = skillAreas.SkillAreas;
						if (vm.skillAreaId !== null)
							vm.selectedSkillArea = skillAreas.SkillAreas.find(function (skillArea) { return skillArea.Id === vm.skillAreaId; });
					});

				vm.selectedSkillChange = function (skill) {
					if (!skill) return;
					vm.skillId = skill.Id;
					vm.skill = skill;
					if ($state.current.name !== teamsBySkillsStateName)
						rtaRouteService.goToSitesBySkill(vm.skillId);
				};

				vm.selectedSkillAreaChange = function (skillArea) {
					if (!skillArea) return
					vm.skillAreaId = skillArea.Id;
					vm.skillArea = skillArea;
					if ($state.current.name !== teamsBySkillsStateName)
						rtaRouteService.goToSitesBySkillArea(vm.skillAreaId);
				};

				vm.querySearch = function (query, myArray) {
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
			}
		});

		vm.goToSkillSelection = function () { rtaRouteService.goToSelectSkill(); };
		vm.goBackWithUrl = function () { return rtaRouteService.urlForSites(); };
		vm.goToDashboard = function () { rtaRouteService.goToSites(); }

		vm.toggleSelection = function (itemId) {
			var index = vm.selectedItemIds.indexOf(itemId);
			if (index > -1) {
				vm.selectedItemIds.splice(index, 1);
			} else {
				vm.selectedItemIds.push(itemId);
			}
		}

		vm.openSelectedItems = function () {
			if (vm.selectedItemIds.length === 0) return;
			goToAgents(vm.selectedItemIds);
		};

		var polling = $interval(function () {
			if (vm.siteId) {
				rtaService.getAdherenceForTeamsOnSite({ siteId: vm.siteId })
					.then(function (teamAdherence) { rtaAdherenceService.updateAdherence(vm.teams, teamAdherence); });
			} else {
				rtaService.getAdherenceForAllSites()
					.then(function (siteAdherences) { rtaAdherenceService.updateAdherence(vm.sites, siteAdherences); });
			}
		}, 5000);

		rtaOrganizationService.getSiteName(vm.siteId)
			.then(function (name) { vm.siteName = name; });

		vm.siteId ? getTeamsInfo() : getSitesInfo();

		function getTeamsInfo() {
			rtaService.getTeams({ siteId: vm.siteId })
				.then(function (teams) {
					vm.teams = teams;
					return rtaService.getAdherenceForTeamsOnSite({ siteId: vm.siteId });
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

		function goToAgents(selectedItemIds) {
			vm.siteId ? rtaRouteService.goToAgents({ teamIds: selectedItemIds}) : rtaRouteService.goToAgents({ siteIds: selectedItemIds});
		};

		$scope.$watch(
			function () { return $sessionStorage.buid; },
			function (newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue)
					vm.goToDashboard();
			}
		);

		$scope.$on('$destroy', function () { $interval.cancel(polling); });

		if (vm.siteId === null) {
			var message = $translate.instant('WFMReleaseNotification')
				.replace('{0}', 'RTA')
				.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>")
				.replace('{2}', '</a>')
				.replace('{3}', "<a href='../Anywhere#realtimeadherencesites'>RTA</a>");
			NoticeService.info(message, null, true);
		}
	};
})();
