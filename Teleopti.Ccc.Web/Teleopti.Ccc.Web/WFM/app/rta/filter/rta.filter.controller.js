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
		$q
	) {
		var vm = this;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = angular.isArray($stateParams.skillIds) ? $stateParams.skillIds[0] || null : $stateParams.skillIds;
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var enableWatchOnTeam = false;
		var agentsState = "rta.agents";
		// scoped variables
		vm.teamsSelected = [];
		vm.selectFieldText = $translate.instant('SelectOrganization');
		vm.searchTerm = "";
		//scoped functions
		vm.querySearch = querySearch;
		vm.selectedSkillChange = selectedSkillChange;
		vm.selectedSkillAreaChange = selectedSkillAreaChange;
		vm.expandSite = expandSite;
		vm.goToAgents = goToAgents;
		vm.selectedSites = selectedSites;
		vm.teamChecked = teamChecked;
		vm.updateSite = updateSite;
		vm.clearOrgSelection = clearOrgSelection;
		vm.clearSearchTerm = clearSearchTerm;
		vm.onSearchOrganization = onSearchOrganization;
		vm.forTest_selectSite = forTest_selectSite;
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.showOrganization = $state.current.name === agentsState;

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
									if (vm.sites.length > 0)
										keepSelectionForOrganization();
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


		function keepSelectionForOrganization() {
			selectSiteAndTeamsUnder();
			if (teamIds.length > 0)
				vm.teamsSelected = teamIds;
			enableWatchOnTeam = true;
			updateSelectFieldText();
		}

		function selectSiteAndTeamsUnder() {
			if (siteIds.length === 0)
				return;
			siteIds.forEach(function (sid) {
				var theSite = vm.sites.find(function (site) {
					return site.Id == sid;
				});
				theSite.isChecked = true;
				theSite.Teams.forEach(function (team) {
					team.isChecked = true;
				});
			});
		}

		function updateSelectFieldText() {
			// var howManyTeamsSelected = countTeamsSelected();
			// vm.selectFieldText = howManyTeamsSelected === 0 ? vm.selectFieldText : $translate.instant("SeveralTeamsSelected").replace('{0}', howManyTeamsSelected);
			var howManyTeamsSelected = showTeamSelected();
			vm.selectFieldText = howManyTeamsSelected.length === 0 ? vm.selectFieldText : $translate.instant("SeveralTeamsSelected").replace('{0}', howManyTeamsSelected + "...");
		}
		function countTeamsSelected() {
			var checkedTeamsCount = 0;
			vm.sites.forEach(function (site) {
				if (siteIds.indexOf(site.Id) > -1) {
					checkedTeamsCount = checkedTeamsCount + site.Teams.length;
				}
			});
			checkedTeamsCount = checkedTeamsCount + vm.teamsSelected.length;
			return checkedTeamsCount;
		}
		function showTeamSelected() {
			var checkedTeamsName = "";
			var countTeams = 1;
			var maxCountTeam = 2;

			if ((siteIds.length > 0 || teamIds.length > 0) && countTeams < maxCountTeam) {
				vm.sites.forEach(function (site) {
					if (countTeams > maxCountTeam)
						return;
					if (siteIds.indexOf(site.Id) > -1) {
						site.Teams.forEach(function (t) {
							if (countTeams > maxCountTeam)
								return;
							++countTeams;
							checkedTeamsName = checkedTeamsName + t.Name + ",";
						})
					} else if (teamIds.length > 0 && countTeams < maxCountTeam) {
						site.Teams.forEach(function (t) {
							if (countTeams > maxCountTeam)
								return;
							if (teamIds.indexOf(t.Id) > -1) {
								++countTeams;
								checkedTeamsName = checkedTeamsName + t.Name + ",";
							}
						})
					}
				});
			}
			return checkedTeamsName.slice(0, -1);
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
			if ((!skill || (skill.Id != skillIds || $stateParams.skillAreaId)) && vm.showOrganization) {
				stateGoToAgents({
					skillIds: skill ? skill.Id : undefined,
					skillAreaId: skill ? undefined : skillAreaId,
					siteIds: siteIds,
					teamIds: teamIds
				});
			}
			else if (!skill) { rtaRouteService.goToSites(); }
			else {
				if (skill.Id == skillIds) return;
				skillIds = skill.Id;
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
			if ((!skillArea || !(skillArea.Id == $stateParams.skillAreaId)) && vm.showOrganization) {				stateGoToAgents({
					skillIds: skillArea ? [] : $stateParams.skillIds,
					skillAreaId: skillArea ? skillArea.Id : undefined,
					siteIds: siteIds,
					teamIds: teamIds
				});
			}

			else if (!skillArea) { rtaRouteService.goToSites(); }
			else {
				if (skillArea.Id === $stateParams.skillAreaId) return;
				vm.skillAreaId = skillArea.Id;
				vm.selectedSkillArea = skillArea;
				vm.selectedSkill = undefined;
				if ($state.current.name !== "rta.teams" && $stateParams.siteId)
					rtaRouteService.goToTeams(vm.siteIds, vm.selectedSkill, skillArea.Id);
				else {
					console.log('hej');
					rtaRouteService.goToSites(vm.selectedSkill, skillArea.Id);
				}
			}
		}

		/***********MULTI-SELECT************/
		function expandSite(site) { site.isExpanded = !site.isExpanded; };

		function goToAgents() {
			var selection = {};
			var selectedSiteIds = vm.selectedSites();
			var selectedTeamIds = vm.teamsSelected;
			selection['siteIds'] = selectedSiteIds;
			selection['teamIds'] = selectedTeamIds;
			if($stateParams.skillIds)
				selection['skillIds'] = $stateParams.skillIds;
			else if ($stateParams.skillAreaId)
				selection['skillAreaId'] = $stateParams.skillAreaId;
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

		function selectedSites() {
			return vm.sites
				.filter(function (site) {
					var selectedTeams = site.Teams.filter(function (team) {
						return team.isChecked == true;
					});
					var noTeamsSelected = selectedTeams.length === 0
					if (noTeamsSelected)
						return false;
					var allTeamsSelected = selectedTeams.length == site.Teams.length;
					if (site.isChecked && allTeamsSelected)
						unselectTeamsInSite(site);
					return site.isChecked && allTeamsSelected;
				}).map(function (s) {
					return s.Id;
				});
		}

		function unselectTeamsInSite(site) {
			site.Teams.forEach(function (team) {
				var index = vm.teamsSelected.indexOf(team.Id);
				if (index > -1) {
					vm.teamsSelected.splice(index, 1);
				}
			});
		}

		function teamChecked(site, team) {
			if (site.isChecked)
				return true;
			var selectedTeamsChecked = site.Teams.filter(function (t) { return t.isChecked; });
			var isAllTeamsCheckedForSite = selectedTeamsChecked.length === site.Teams.length;
			site.isMarked = (selectedTeamsChecked.length > 0 && !isAllTeamsCheckedForSite);
			if (isAllTeamsCheckedForSite)
				return false;
			return team.isChecked;
		}

		function updateSite(oldTeamsSelected) {
			vm.sites.forEach(function (site) {
				var anyChangeForThatSite = false;
				site.Teams.forEach(function (team) {
					team.isChecked = vm.teamsSelected.indexOf(team.Id) > -1;
					var teamChanged = (team.isChecked === oldTeamsSelected.indexOf(team.Id) < 0);
					if (oldTeamsSelected.length > 0 && teamChanged)
						anyChangeForThatSite = true;
				});
				var checkedTeams = site.Teams.filter(function (team) { return team.isChecked; });
				if (checkedTeams.length > 0 || anyChangeForThatSite)
					site.isChecked = checkedTeams.length === site.Teams.length;
			});
		};

		function clearOrgSelection() {
			vm.sites.forEach(function (site) { site.isChecked = false; });
			vm.teamsSelected = [];
			updateSelectFieldText();
		};

		function clearSearchTerm() { vm.searchTerm = ''; }
		function onSearchOrganization($event) { $event.stopPropagation(); };

		function forTest_selectSite(site) {
			site.isChecked = !site.isChecked;
			var selectedSite = vm.sites.find(function (s) { return s.Id === site.Id; });
			selectedSite.Teams.forEach(function (team) {
				team.isChecked = selectedSite.isChecked;
				if (selectedSite.isChecked) {
					vm.teamsSelected.push(team.Id);
				} else {
					var index = vm.teamsSelected.indexOf(team.Id);
					vm.teamsSelected.splice(index, 1);
				}
			});
		}

		$scope.$watch(
			function () { return vm.teamsSelected; },
			function (newValue, oldValue) {
				if (angular.toJson(newValue) !== angular.toJson(oldValue) && enableWatchOnTeam) {
					vm.updateSite(oldValue);
				}
			});
	};
})();
