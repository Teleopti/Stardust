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
		];

	function RtaFilterController(
		$scope,
		$state,
		$stateParams,
		$translate,
		rtaService
	) {

		var vm = this;
		var siteIds = $stateParams.siteIds || [];
		var teamIds = $stateParams.teamIds || [];
		var skillIds = $stateParams.skillIds || [];
		var skillAreaId = $stateParams.skillAreaId || undefined;
		var enableWatchOnTeam = false;
		var agentsStates = "rta.agents";
		// select skill dependency
		vm.skills = [];
		vm.skillAreas = [];
		vm.skillsLoaded = false;
		vm.skillAreasLoaded = false;
		vm.teamsSelected = [];
		vm.selectFieldText = $translate.instant('SelectOrganization');
		vm.searchTerm = "";
		vm.showOrganizationPicker = $state.current.name === agentsStates;
		/*******REQUESTS*****/

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
			var howManyTeamsSelected = countTeamsSelected();
			vm.selectFieldText = howManyTeamsSelected === 0 ? vm.selectFieldText : $translate.instant("SeveralTeamsSelected").replace('{0}', howManyTeamsSelected);
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

		(function initialize() {
			rtaService.getSkills()
				.then(function (skills) {
					vm.skillsLoaded = true;
					vm.skills = skills;
					if (skillIds.length > 0 && skillAreaId == null)
						vm.selectedSkill = skills.find(function (s) { return s.Id === skillIds[0] });
				});
			rtaService.getSkillAreas()
				.then(function (skillAreas) {
					vm.skillAreasLoaded = true;
					vm.skillAreas = skillAreas.SkillAreas;
					if (skillAreaId != null)
						vm.selectedSkillArea = vm.skillAreas.find(function (s) { return s.Id === skillAreaId });
				});
			if ($state.current.name === agentsStates);
			rtaService.getOrganization()
				.then(function (organization) {
					vm.sites = organization;
					if (vm.sites.length > 0)
						keepSelectionForOrganization();
				});
		})();

		/*********AUTOCOMPLETE*****/
		vm.querySearch = function (query, myArray) {
			if (!query)
				return myArray;
			return myArray.filter(function (query) { return function filterFn(item) { return (item.Name.toUpperCase().indexOf(query.toUpperCase()) === 0); }; });
		};

		vm.selectedSkillChange = function (skill) {
			if (!skill || (skill.Id != skillIds[0] || $stateParams.skillAreaId))
				stateGoToAgents({
					skillIds: skill ? skill.Id : undefined,
					skillAreaId: skill ? undefined : skillAreaId,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		vm.selectedSkillAreaChange = function (skillArea) {
			if (!skillArea || !(skillArea.Id == $stateParams.skillAreaId))
				stateGoToAgents({
					skillIds: skillArea ? [] : $stateParams.skillIds,
					skillAreaId: skillArea ? skillArea.Id : undefined,
					siteIds: siteIds,
					teamIds: teamIds
				});
		}

		/***********MULTI-SELECT************/
		vm.expandSite = function (site) { site.isExpanded = !site.isExpanded; };

		vm.goToAgents = function () {
			var selection = {};
			var selectedSiteIds = vm.selectedSites();
			var selectedTeamIds = vm.teamsSelected;
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

		vm.selectedSites = function () {
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

		vm.teamChecked = function (site, team) {
			if (site.isChecked)
				return true;
			var selectedTeamsChecked = site.Teams.filter(function (t) { return t.isChecked; });
			var isAllTeamsCheckedForSite = selectedTeamsChecked.length === site.Teams.length;
			site.isMarked = (selectedTeamsChecked.length > 0 && !isAllTeamsCheckedForSite);
			if (isAllTeamsCheckedForSite)
				return false;
			return team.isChecked;
		}

		vm.forTest_selectSite = function (site) {
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

		vm.updateSite = function (oldTeamsSelected) {
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

		vm.clearOrgSelection = function () {
			vm.sites.forEach(function (site) { site.isChecked = false; });
			vm.teamsSelected = [];
			updateSelectFieldText();
		};

		vm.clearSearchTerm = function () { vm.searchTerm = ''; }
		vm.onSearchOrganization = function ($event) { $event.stopPropagation(); };

		$scope.$watch(
			function () { return vm.teamsSelected; },
			function (newValue, oldValue) {
				if (angular.toJson(newValue) !== angular.toJson(oldValue) && enableWatchOnTeam) {
					vm.updateSite(oldValue);
				}
			});
	};
})();
