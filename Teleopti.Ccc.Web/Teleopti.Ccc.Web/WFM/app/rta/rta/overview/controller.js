﻿(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController', RtaOverviewController);

	RtaOverviewController.$inject = ['rtaService', 'rtaRouteService', 'rtaPollingService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope', '$q', '$timeout'];

	function RtaOverviewController(rtaService, rtaRouteService, rtaPollingService, skills, skillAreas, $state, $stateParams, $interval, $scope, $q, $timeout) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		vm.skillAreaId = $stateParams.skillAreaId;
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;
		vm.urlParams = $stateParams;
		vm.agentsState = 'rta-agents({siteIds: card.site.Id})';
		vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id})';
		vm.organizationSelection = false;
		vm.skillSelected = vm.skillIds.length;
		vm.goToAgentsView = function () { rtaRouteService.goToSelectSkill(); };
		
		(function () {
			if (angular.isDefined(vm.urlParams.skillAreaId)) {
				vm.agentsState = 'rta-agents({siteId	s: card.site.Id, skillAreaId: "' + vm.urlParams.skillAreaId + '"})';
				vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillAreaId: "' + vm.urlParams.skillAreaId + '"})';
			}
			else if (angular.isDefined(vm.urlParams.skillIds)) {
				vm.agentsState = 'rta-agents({siteIds: card.site.Id, skillIds: ["' + vm.urlParams.skillIds[0] + '"]})';
				vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillIds: ["' + vm.urlParams.skillIds[0] + '"]})';
			}
			else {
				vm.agentsState = 'rta-agents({siteIds: card.site.Id})';
				vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id})';
			}

			if (angular.isDefined(vm.skillAreaId)) {
				vm.skillIds = getSkillIdsFromSkillAreaId(vm.skillAreaId);
			}

		})();
		
		var poller = rtaPollingService.create(function () {
				return getSites()
					.then(getTeamsForSites);
			}
		);
		vm.siteCards = [];
		poller.start();
		$scope.$on('$destroy', poller.destroy);
		
		function getSites() {
			return rtaService.getOverviewModelFor(vm.skillIds)
				.then(function (sites) {
					sites.Sites.forEach(function (site) {
						var siteCard = vm.siteCards.find(function (siteCard) {
							return siteCard.site.Id === site.Id;
						});
						if (!siteCard) {
							siteCard = {
								site: site,
								isOpen: $stateParams.open != "false",
								isSelected: false,
								teams: []
							};
							$scope.$watch(function () { return siteCard.isOpen }, function (newValue) { if (newValue) poller.force(); });
								return siteCard.isOpen
							}, function (newValue) {
								if (newValue) pollNow();
							});
							$scope.$watch(function () {
								return siteCard.isSelected
							}, function (newValue, oldValue) {
								if (newValue != oldValue) {
									toggleOpenButton();
									if (newValue) {
										siteCard.teams.forEach(function (t) {
											t.isSelected = true;
										});
									}
								}
							});
							vm.siteCards.push(siteCard);
						}
						;
						siteCard.site.Color = translateSiteColors(site);
						siteCard.site.InAlarmCount = site.InAlarmCount;
					});
					vm.totalAgentsInAlarm = sites.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
				});
		}

		function toggleOpenButton() {
			var match = vm.siteCards.find(function (s) {
				return s.isSelected || s.teams.find(function (t) { return t.isSelected; });
			});
			vm.organizationSelection = !!match;
		}

		function getTeamsForSites() {
			return $q.all(
				vm.siteCards
					.filter(function (s) { return s.isOpen; })
					.map(function (s) {
						return getTeamsForSite(s);
					})
			);
		}

		function getTeamsForSite(s) {
			return rtaService.getTeamCardsFor({siteIds: s.site.Id, skillIds: vm.skillIds})
				.then(function (teams) {
					teams.forEach(function (team) {
						var teamVm = s.teams.find(function (t) {
							return team.Id === t.Id;
						});

						if (!teamVm) {
							teamVm = team;
							teamVm.isSelected = false;
							$scope.$watch(function () { return teamVm.isSelected }, function (newValue, oldValue) {
								toggleOpenButton();
								if (newValue) {
									var areAllTeamsSelected = s.teams.every(function (t) { return t.isSelected });
									if (areAllTeamsSelected) s.isSelected = true;
								}
								else {
									s.isSelected = false;
								}
							});
							s.teams.push(teamVm);
						}
						;
						teamVm.Color = team.Color;
						teamVm.InAlarmCount = team.InAlarmCount;
					})
					setTeamToSelected(s);
				});
		}

		function setTeamToSelected(card) {
			if (card.isSelected) {
				card.teams.forEach(function (team) {
					team.isSelected = true;
				});
			}
		}

		function getSkillIdsFromSkillAreaId(id) {
			return vm.skillAreas.find(function (sa) { return sa.Id === id; })
				.Skills.map(function (skill) { return skill.Id; });
		}

		function translateSiteColors(site) {
			if (site.Color === 'good') {
				return '#C2E085';
			} else if (site.Color === 'warning') {
				return '#FFC285';
			} else if (site.Color === 'danger') {
				return '#EE8F7D';
			} else {
				return '#fff';
			}
		}

		vm.filterOutput = function (selectedItem) {
			if (!angular.isDefined(selectedItem)) {
				resetOnNoSkills();
			} else if (selectedItem.hasOwnProperty('Skills')) {
				setUpForSkillArea(selectedItem);
			} else {
				setUpForSkill(selectedItem);
			}
			poller.force();
		}

		function resetOnNoSkills() {
			vm.skillIds = [];
			vm.urlParams.skillIds = undefined;
			vm.urlParams.skillAreaId = undefined;
			vm.agentsState = 'rta-agents({siteIds: card.site.Id})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id})';
			$state.go($state.current.name, {skillAreaId: undefined, skillIds: undefined}, {notify: false});
		}

		function setUpForSkillArea(selectedItem) {
			vm.urlParams.skillAreaId = selectedItem.Id;
			vm.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
			vm.agentsState = 'rta-agents({siteIds: card.site.Id, skillAreaId: "' + selectedItem.Id + '"})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillAreaId: "' + selectedItem.Id + '"})';
			$state.go($state.current.name, {skillAreaId: selectedItem.Id, skillIds: undefined}, {notify: false});
		}

		function setUpForSkill(selectedItem) {
			vm.urlParams.skillIds = [selectedItem.Id];
			vm.skillIds = [selectedItem.Id];
			vm.agentsState = 'rta-agents({siteIds: card.site.Id, skillIds: ["' + selectedItem.Id + '"]})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillIds: ["' + selectedItem.Id + '"]})';
			$state.go($state.current.name, {skillAreaId: undefined, skillIds: vm.skillIds}, {notify: false});
		}

		vm.goToAgents = function () {
			var teamIds = [];
			var siteIds = [];
			var skillIds = angular.isDefined(vm.urlParams.skillIds) ? vm.urlParams.skillIds : [];

			vm.siteCards.forEach(function (siteCard) {
				if (siteCard.isSelected) siteIds.push(siteCard.site.Id);
				else siteCard.teams.forEach(function (team) {
					if (team.isSelected) teamIds.push(team.Id);
				});
			});
			$state.go('rta-agents', {
				siteIds: siteIds,
				teamIds: teamIds,
				skillIds: skillIds,
				skillAreaId: vm.urlParams.skillAreaId
			});
		}
	}
})();
