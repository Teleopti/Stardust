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
			'rtaService',
			'rtaRouteService'
		];

	function RtaFilterController(
		$scope,
		$state,
		$stateParams,
		$translate,
		rtaService,
		rtaRouteService
	) {
		var fc = this;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var enableWatchOnTeam = false;
		var agentsState = "rta.agents";
		// select skill dependency
		fc.teamsSelected = [];
		fc.selectFieldText = $translate.instant('SelectOrganization');
		fc.searchTerm = "";
		fc.showOrganizationPicker = $state.current.name === agentsState;

		/*******REQUESTS*****/
		(function initialize() {
			if ($state.current.name === agentsState)
				rtaService.getOrganization()
					.then(function (organization) {
						fc.sites = organization;
						if (fc.sites.length > 0)
							keepSelectionForOrganization();
					});
		})();

		function keepSelectionForOrganization() {
			selectSiteAndTeamsUnder();
			if (teamIds.length > 0)
				fc.teamsSelected = teamIds;
			enableWatchOnTeam = true;
			updateSelectFieldText();
		}

		function selectSiteAndTeamsUnder() {
			if (siteIds.length === 0)
				return;
			siteIds.forEach(function (sid) {
				var theSite = fc.sites.find(function (site) {
					return site.Id == sid;
				});
				theSite.isChecked = true;
				theSite.Teams.forEach(function (team) {
					team.isChecked = true;
				});
			});
		}

		function updateSelectFieldText() {
			var howManyTeamsSelected = countTeamsSelected();
			fc.selectFieldText = howManyTeamsSelected === 0 ? fc.selectFieldText : $translate.instant("SeveralTeamsSelected").replace('{0}', howManyTeamsSelected);
		}

		function countTeamsSelected() {
			var checkedTeamsCount = 0;
			fc.sites.forEach(function (site) {
				if (siteIds.indexOf(site.Id) > -1) {
					checkedTeamsCount = checkedTeamsCount + site.Teams.length;
				}
			});
			checkedTeamsCount = checkedTeamsCount + fc.teamsSelected.length;
			return checkedTeamsCount;
		}

		/*********AUTOCOMPLETE*****/
		fc.querySearch = function (query, myArray) {
			if (!query)
				return myArray;
			return myArray.filter(function (query) { return function filterFn(item) { return (item.Name.toUpperCase().indexOf(query.toUpperCase()) === 0); }; });
		};

		fc.selectedSkillChange = function (skill) {
			if ((!skill || (skill.Id != skillIds[0] || $stateParams.skillAreaId)) && $state.current.name === agentsState) {
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
				fc.selectedSkill = skill;
				fc.selectedSkillArea = undefined;
				if ($state.current.name !== "rta.teams" && angular.isDefined($stateParams.siteId))
					rtaRouteService.goToTeams(fc.siteIds, skill.Id, fc.selectedSkillArea);
				else {
					rtaRouteService.goToSites(skill.Id, fc.selectedSkillArea);
				}
			}
		}

		fc.selectedSkillAreaChange = function (skillArea) {
			if ($state.current.name === agentsState) {
				if (!skillArea || !(skillArea.Id == $stateParams.skillAreaId)) {
					stateGoToAgents({
						skillIds: skillArea ? [] : $stateParams.skillIds,
						skillAreaId: skillArea ? skillArea.Id : undefined,
						siteIds: siteIds,
						teamIds: teamIds
					});
				}
			}
			else if (!skillArea) { rtaRouteService.goToSites(); }
			else {
				if (skillArea.Id === $stateParams.skillAreaId) return;
				fc.skillAreaId = skillArea.Id;
				fc.selectedSkillArea = skillArea;
				fc.selectedSkill = undefined;
				if ($state.current.name !== "rta.teams" && $stateParams.siteId)
					rtaRouteService.goToTeams(fc.siteIds, fc.selectedSkill, skillArea.Id);
				else {
					rtaRouteService.goToSites(fc.selectedSkill, skillArea.Id);
				}
			}
		}

		/***********MULTI-SELECT************/
		fc.expandSite = function (site) { site.isExpanded = !site.isExpanded; };

		fc.goToAgents = function () {
			var selection = {};
			var selectedSiteIds = fc.selectedSites();
			var selectedTeamIds = fc.teamsSelected;
			selection['siteIds'] = selectedSiteIds;
			selection['teamIds'] = selectedTeamIds;
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

		fc.selectedSites = function () {
			return fc.sites
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
				var index = fc.teamsSelected.indexOf(team.Id);
				if (index > -1) {
					fc.teamsSelected.splice(index, 1);
				}
			});
		}

		fc.teamChecked = function (site, team) {
			if (site.isChecked)
				return true;
			var selectedTeamsChecked = site.Teams.filter(function (t) { return t.isChecked; });
			var isAllTeamsCheckedForSite = selectedTeamsChecked.length === site.Teams.length;
			site.isMarked = (selectedTeamsChecked.length > 0 && !isAllTeamsCheckedForSite);
			if (isAllTeamsCheckedForSite)
				return false;
			return team.isChecked;
		}

		fc.forTest_selectSite = function (site) {
			site.isChecked = !site.isChecked;
			var selectedSite = fc.sites.find(function (s) { return s.Id === site.Id; });
			selectedSite.Teams.forEach(function (team) {
				team.isChecked = selectedSite.isChecked;
				if (selectedSite.isChecked) {
					fc.teamsSelected.push(team.Id);
				} else {
					var index = fc.teamsSelected.indexOf(team.Id);
					fc.teamsSelected.splice(index, 1);
				}
			});
		}

		fc.updateSite = function (oldTeamsSelected) {
			fc.sites.forEach(function (site) {
				var anyChangeForThatSite = false;
				site.Teams.forEach(function (team) {
					team.isChecked = fc.teamsSelected.indexOf(team.Id) > -1;
					var teamChanged = (team.isChecked === oldTeamsSelected.indexOf(team.Id) < 0);
					if (oldTeamsSelected.length > 0 && teamChanged)
						anyChangeForThatSite = true;
				});
				var checkedTeams = site.Teams.filter(function (team) { return team.isChecked; });
				if (checkedTeams.length > 0 || anyChangeForThatSite)
					site.isChecked = checkedTeams.length === site.Teams.length;
			});
		};

		fc.clearOrgSelection = function () {
			fc.sites.forEach(function (site) { site.isChecked = false; });
			fc.teamsSelected = [];
			updateSelectFieldText();
		};

		fc.clearSearchTerm = function () { fc.searchTerm = ''; }
		fc.onSearchOrganization = function ($event) { $event.stopPropagation(); };

		$scope.$watch(
			function () { return fc.teamsSelected; },
			function (newValue, oldValue) {
				if (angular.toJson(newValue) !== angular.toJson(oldValue) && enableWatchOnTeam) {
					fc.updateSite(oldValue);
				}
			});
	};
})();
