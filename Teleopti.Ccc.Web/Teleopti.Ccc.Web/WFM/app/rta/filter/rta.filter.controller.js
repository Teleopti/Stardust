(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaFilterController', RtaFilterController);

	RtaFilterController.$inject =
		[
			'$scope',
			'$state',
			'$stateParams',
			'$translate',
			'$timeout',
			'rtaService',
			'rtaRouteService',
			'rtaNamesFormatService',
			'localeLanguageSortingService',
			'$q'
		];

	function RtaFilterController(
		$scope,
		$state,
		$stateParams,
		$translate,
		$timeout,
		rtaService,
		rtaRouteService,
		rtaNamesFormatService,
		localeLanguageSortingService,
		$q
	) {
		var vm = this;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = angular.isArray($stateParams.skillIds) ? $stateParams.skillIds[0] || null : $stateParams.skillIds;
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var agentsState = "rta.agents";
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
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.showOrganization = $state.current.name === agentsState;
		vm.clearSelection = clearSelection;
		vm.sortByLocaleLanguage = localeLanguageSortingService.sort;
		vm.openPicker = false;

		(function initialize() {
			rtaService.getSkills()
				.then(function (skills) {
					var deffered = $q.defer();
					deffered.resolve();
					vm.skillsLoaded = true;
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
							vm.skillAreas = skillAreas.SkillAreas;
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
				var theSkillIds = skillIds != null ? [skillIds] : null;
				var skillIdsForOrganization = skillAreaId != null ? getSkillIdsFromSkillArea(skillAreaId) : theSkillIds;
				return skillIdsForOrganization != null ? rtaService.getOrganizationForSkills({ skillIds: skillIdsForOrganization }) : rtaService.getOrganization();
			}

			function getSkillIdsFromSkillArea(skillAreaId) {
				var theSkillArea = vm.skillAreas.find(function (skillArea) {
					return skillArea.Id === skillAreaId;
				});
				if (theSkillArea.Skills != null) {
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
			//vm.defaultInputText = vm.selectFieldText === $translate.instant('SelectOrganization');
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
				stateGoToAgents({
					skillIds: skill ? skill.Id : undefined,
					skillAreaId: skill ? undefined : skillAreaId,
					siteIds: siteIds,
					teamIds: teamIds
				});
			else if (!skill) { rtaRouteService.goToSites(); }
			else if ((skill.Id != skillIds || $stateParams.skillAreaId) && vm.showOrganization) {
				stateGoToAgents({
					skillIds: skill ? skill.Id : undefined,
					skillAreaId: skill ? undefined : skillAreaId,
					siteIds: [],
					teamIds: []
				});
			}
			else {
				if (skill.Id == skillIds) return;
				vm.selectedSkill = skill;
				vm.selectedSkillArea = undefined;
				if ($state.current.name !== "rta.teams" && angular.isDefined($stateParams.siteId))
					rtaRouteService.goToTeams(vm.siteIds, skill.Id, vm.selectedSkillArea);
				else {
					rtaRouteService.goToSites(skill.Id, vm.selectedSkillArea);
				}
			}
		}

		function selectedSkillAreaChange(skillArea) {
			if (!skillArea && vm.showOrganization) {
				stateGoToAgents({
					skillIds: skillArea ? [] : $stateParams.skillIds,
					skillAreaId: skillArea ? skillArea.Id : undefined,
					siteIds: siteIds,
					teamIds: teamIds
				});
			}
			else if (!skillArea) { rtaRouteService.goToSites(); }
			else if (!(skillArea.Id == $stateParams.skillAreaId) && vm.showOrganization) {
				stateGoToAgents({
					skillIds: skillArea ? [] : $stateParams.skillIds,
					skillAreaId: skillArea ? skillArea.Id : undefined,
					siteIds: [],
					teamIds: []
				});
			}
			else {
				if (skillArea.Id === $stateParams.skillAreaId) return;
				vm.selectedSkillArea = skillArea;
				vm.selectedSkill = undefined;
				if ($state.current.name !== "rta.teams" && $stateParams.siteId)
					rtaRouteService.goToTeams(vm.siteIds, vm.selectedSkill, skillArea.Id);
				else {
					rtaRouteService.goToSites(vm.selectedSkill, skillArea.Id);
				}
			}
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
			if(!vm.openPicker) return;
			shrinkSites();
			var selection = vm.sites.reduce(function (acc, site) {
				if (site.isChecked) {
					acc.siteIds.push(site.Id)
				} else if (site.isMarked) {
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
			stateGoToAgents(selection);
		}

		function stateGoToAgents(selection) {
			var stateName = 'rta.agents';
			var options = {
				reload: true,
				notify: true
			};
			$state.go(stateName, selection, options);
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
	};
})();
