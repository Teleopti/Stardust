(function() {
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

		toggleService.togglesLoaded.then(function() {

			vm.displaySkillOrSkillAreaFilter = toggleService.RTA_SiteAndTeamOnSkillOverview_40817;

			if (vm.displaySkillOrSkillAreaFilter) {
				rtaService.getSkills()
					.then(function(skills) {
						vm.skillsLoaded = true;
						vm.skills = skills;
						if (vm.skillId !== null) {
							vm.selectedSkill = getSelected(skills, vm.skillId);
						}
					});

				rtaService.getSkillAreas()
					.then(function(skillAreas) {
						vm.skillAreasLoaded = true;
						vm.skillAreas = skillAreas.SkillAreas;
						if (vm.skillAreaId !== null) {
							vm.selectedSkillArea = getSelected(skillAreas.SkillAreas, vm.skillAreaId);
						}
					});

				function getSelected(outOf, shouldMatch) {
					return outOf.find(function(o) {
						return o.Id === shouldMatch;
					})
				};

				vm.querySearch = function(query, myArray) {
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

				vm.selectedSkillChange = function(skill) {
					if (!skill) return;
					vm.skillId = skill.Id;
					doWhenSelecting(skill, vm.selectedSkill, teamsBySkillsStateName, goToSitesBySkill);
				};

				vm.selectedSkillAreaChange = function(skillArea) {
					if (!skillArea) return
					vm.skillAreaId = skillArea.Id;
					doWhenSelecting(skillArea, vm.selectedSkillArea, teamsBySkillsStateName, goToSitesBySkillArea);
				};

				function doWhenSelecting(item, selected, teamsStateName, goToSites) {
					selected = item;
					if ($state.current.name !== teamsStateName)
						goToSites(item.Id);
				};

				function goToSitesBySkill(skillId) {
					rtaRouteService.goToSitesBySkill(skillId);
				};

				function goToSitesBySkillArea(skillAreaId) {
					rtaRouteService.goToSitesBySkillArea(skillAreaId);
				};
			}
		});




		rtaOrganizationService.getSiteName(vm.siteId)
			.then(function(name) {
				vm.siteName = name;
			});

		if (vm.siteId === null) {
			var message = $translate.instant('WFMReleaseNotification')
				.replace('{0}', 'RTA')
				.replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx'>")
				.replace('{2}', '</a>')
				.replace('{3}', "<a href='../Anywhere#realtimeadherencesites'>RTA</a>");
			NoticeService.info(message, null, true);
		}

		var polling = $interval(function() {
			if (vm.siteId) {
				getAdherenceForTeamsOnSite(vm.siteId)
					.then(function(teamAdherence) {
						updateAdherence(vm.teams, teamAdherence);
					});
			} else {
				getAdherenceForAllSites()
					.then(function(siteAdherences) {
						updateAdherence(vm.sites, siteAdherences);
					});
			}
		}, 5000);

		vm.siteId ? getTeamsInfo() : getSitesInfo();

		function getTeamsInfo() {
			getTeams(vm.siteId)
				.then(function(teams) {
					vm.teams = teams;
					return getAdherenceForTeamsOnSite(vm.siteId);
				}).then(function(teamAdherence) {
					updateAdherence(vm.teams, teamAdherence);
				});
		};

		function getSitesInfo() {
			getSites().then(function(sites) {
				vm.sites = sites;
				return getAdherenceForAllSites();
			}).then(function(siteAdherences) {
				updateAdherence(vm.sites, siteAdherences);
			});
		}

		function getTeams(siteId) {
			return rtaService.getTeams({
				siteId: siteId
			});
		};

		function getSites() {
			return rtaService.getSites();
		}

		function getAdherenceForTeamsOnSite(siteId) {
			return rtaService.getAdherenceForTeamsOnSite({
				siteId: siteId
			});
		};

		function getAdherenceForAllSites() {
			return rtaService.getAdherenceForAllSites();
		}

		function updateAdherence(level, adherence) {
			rtaAdherenceService.updateAdherence(level, adherence);
		};

		vm.toggleSelection = function(itemId) {
			var index = vm.selectedItemIds.indexOf(itemId);
			if (index > -1) {
				vm.selectedItemIds.splice(index, 1);
			} else {
				vm.selectedItemIds.push(itemId);
			}
		}

		vm.openSelectedItems = function() {
			if (vm.selectedItemIds.length > 0)
				goToAgents(vm.selectedItemIds);
		};

		function goToAgents(selectedItemIds) {
			vm.siteId ? rtaRouteService.goToAgents({
				teamIds: selectedItemIds
			}) : rtaRouteService.goToAgents({
				siteIds: selectedItemIds
			});
		};

		vm.goToSkillSelection = function() {
			rtaRouteService.goToSelectSkill();
		};

		vm.goBackWithUrl = function() {
			return rtaRouteService.urlForSites();
		};

		vm.goToDashboard = function() {
			rtaRouteService.goToSites();
		}

		$scope.$watch(

			function() {
				return $sessionStorage.buid;
			},
			function(newValue, oldValue) {
				if (angular.isDefined(oldValue) && newValue !== oldValue) {
					vm.goToDashboard();
				}
			}
		);

		$scope.$on('$destroy', function() {
			$interval.cancel(polling);
		});
	};
})();
