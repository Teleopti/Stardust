﻿(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', 'rtaRouteService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope'];

	function RtaMainController(rtaService, rtaRouteService, skills, skillAreas, $state, $stateParams, $interval, $scope) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		vm.skillAreaId = $stateParams.skillAreaId;
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;
		vm.urlParams = $stateParams;
		vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
		vm.organizationSelection = false;
		vm.skillSelected = vm.skillIds.length;
		vm.goToAgentsView = function () { rtaRouteService.goToSelectSkill(); };
		vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };

		var teamPolling;
		var sitePolling;
		var sitePollingWithSkills;
		var pollingIntervals = [];

		(function () {
			if (angular.isDefined(vm.urlParams.skillAreaId)) {
				vm.selectedItems.skillAreaId = vm.urlParams.skillAreaId;
				vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillAreaId: "' + vm.urlParams.skillAreaId + '"})';
			}
			else if (angular.isDefined(vm.urlParams.skillIds)) {
				vm.selectedItems.skillIds = vm.urlParams.skillIds;
				vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillIds: ["' + vm.urlParams.skillIds[0] + '"]})';
			}
			else {
				vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };
				vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
			}

			if (angular.isDefined(vm.skillAreaId)) {
				vm.skillIds = getSkillIdsFromSkillAreaId(vm.skillAreaId);
				getSiteCards(vm.skillIds);
			} else if (vm.skillIds.length) {
				getSiteCards(vm.skillIds);
			} else {
				getSiteCards();
			}

		})();

		function getSiteCards(ids) {
			if (angular.isDefined(ids)) {
				rtaService.getOverviewModelFor(ids).then(function (result) {
					vm.siteCards = buildSiteCards(result);
					vm.totalAgentsInAlarm = result.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
					fetchTeamsForAllSiteCards();
				});
			} else {
				rtaService.getOverviewModelFor().then(function (result) {
					vm.siteCards = buildSiteCards(result);
					vm.totalAgentsInAlarm = result.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
					fetchTeamsForAllSiteCards();
				});
			}
		}

		function fetchTeamsForAllSiteCards() {
			vm.siteCards.forEach(function (s) {
				if (s.isOpen)
					fetchTeamData(s);
			})
		}

		function buildSiteCards(sites) {
			return sites.Sites.map(function (site) {
				site.Color = translateSiteColors(site);
				return {
					site: site,
					isOpen: $stateParams.open != "false",
					fetchTeamData: fetchTeamData
				}
			});
		}

		function fetchTeamData(card) {
			if (!card.isOpen) {
				var match = pollingIntervals.find(function (interval) {
					return interval.siteId === card.site.Id;
				});
				var index = pollingIntervals.indexOf(match);
				pollingIntervals.splice(index, 1);
				$interval.cancel(match.interval);
			} else {
				fetchTeams(card);
				teamPolling = $interval(function () {
					if (vm.skillIds.length) {
						rtaService.getTeamCardsFor({ siteIds: card.site.Id, skillIds: vm.skillIds }).then(function (teams) {
							updateTeamCards(card, teams);
						})
					}
					else {
						rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
							updateTeamCards(card, teams);
						});
					}
				}, 5000);

				pollingIntervals.push({
					siteId: card.site.Id,
					interval: teamPolling
				});
			}
		}

		function fetchTeams(card) {
			if (vm.skillIds.length) {
				rtaService.getTeamCardsFor({ siteIds: card.site.Id, skillIds: vm.skillIds }).then(function (teams) {
					card.teams = teams;
					setTeamToSelected(card);
				});
			} else {
				rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
					card.teams = teams;
					setTeamToSelected(card);
				});

			}
		}

		function setTeamToSelected(card) {
			var indexOfSite = vm.selectedItems.siteIds.indexOf(card.site.Id);
			if (indexOfSite > -1) {
				card.teams.forEach(function (team) {
					team.isSelected = true;
				});
			}
			else {
				card.teams.forEach(function (team) {
					var indexOfTeam = vm.selectedItems.teamIds.indexOf(team.Id);
					if (indexOfTeam > -1) team.isSelected = true;
				});
			}
		}

		function updateTeamCards(card, teams) {
			card.teams.forEach(function (team) {
				var match = teams.find(function (t) {
					return t.Id === team.Id;
				});
				team.Color = match.Color;
				team.InAlarmCount = match.InAlarmCount;
			});
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

		$scope.$watch(function () { return vm.skillIds; },
			function (newValue) {
				clearSitePolls();

				if (newValue.length) {
					vm.skillSelected = true;
					sitePollingWithSkills = $interval(function () {
						rtaService
							.getOverviewModelFor(vm.skillIds)
							.then(updateSiteCards);
					}, 5000);
				} else {
					vm.skillSelected = false;
					sitePolling = $interval(function () {
						rtaService
							.getOverviewModelFor()
							.then(updateSiteCards)
					}, 5000);
				}
			});

		function clearSitePolls() {
			if (angular.isDefined(sitePollingWithSkills)) {
				$interval.cancel(sitePollingWithSkills);
			}
			if (angular.isDefined(sitePolling)) {
				$interval.cancel(sitePolling);
			}
			if (pollingIntervals.length) {
				pollingIntervals.forEach(function (i) {
					$interval.cancel(i.interval);
				});
				pollingIntervals = [];
			}
		}

		function updateSiteCards(sites) {
			sites.Sites.forEach(function (site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				if (match) {
					match.site.Color = translateSiteColors(site);
					match.site.InAlarmCount = site.InAlarmCount;
				}
			});
		}

		$scope.$on('$destroy', function () {
			clearSitePolls();
		});

		vm.filterOutput = function (selectedItem) {
			if (!angular.isDefined(selectedItem)) {
				resetOnNoSkills();
				getSiteCards();
			} else if (selectedItem.hasOwnProperty('Skills')) {
				setUpForSkillArea(selectedItem);
				getSiteCards(vm.skillIds);
			} else {
				setUpForSkill(selectedItem);
				getSiteCards(vm.skillIds);
			}
		}

		function resetOnNoSkills() {
			vm.skillIds = [];
			vm.urlParams.skillIds = undefined;
			vm.urlParams.skillAreaId = undefined;
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
		}

		function setUpForSkillArea(selectedItem) {
			vm.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: selectedItem.Id };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillAreaId: "' + selectedItem.Id + '"})';
			$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
		}

		function setUpForSkill(selectedItem) {
			vm.skillIds = [selectedItem.Id];
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: vm.skillIds, skillAreaId: undefined };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillIds: ["' + selectedItem.Id + '"]})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: vm.skillIds }, { notify: false });
		}


		vm.getSelectedItems = function (item) {
			var selectedItemsHandler = createSelectedItemsHandler(vm.selectedItems);

			if (angular.isDefined(item.site)) {
				selectSite(selectedItemsHandler, item);
			} else {
				selectTeam(selectedItemsHandler, item);
			}

			vm.organizationSelection = vm.selectedItems.siteIds.length || vm.selectedItems.teamIds.length;
		}

		function selectSite(selectedItemsHandler, site) {
			var siteAlreadySelected = vm.selectedItems.siteIds.indexOf(site.site.Id) > -1;

			if (siteAlreadySelected) {
				selectedItemsHandler.removeSite(site);
			} else {
				selectedItemsHandler.addSite(site);

				if (angular.isDefined(site.teams)) {
					selectedItemsHandler.clearTeams(site);
				}
			}
		}

		function selectTeam(selectedItemsHandler, team) {
			var parentSite = vm.siteCards.find(function (card) { return card.site.Id === team.SiteId; });
			var siteNoLongerHasAllTeamsSelected = !parentSite.isSelected && vm.selectedItems.siteIds.indexOf(parentSite.site.Id) > -1;
			var allTeamsInSiteAreNowSelected = parentSite.isSelected;

			if (allTeamsInSiteAreNowSelected) {
				selectedItemsHandler
					.clearTeams(parentSite)
					.addSite(parentSite);

			} else if (siteNoLongerHasAllTeamsSelected) {
				selectedItemsHandler
					.removeSite(parentSite)
					.addAllTeams(parentSite)
					.toggleTeam(team);

			} else {
				selectedItemsHandler
					.toggleTeam(team);
			}
		}

		function createSelectedItemsHandler(selecteItems) {
			var that = {
				addSite: addSite,
				removeSite: removeSite,
				addAllTeams: addAllTeams,
				clearTeams: clearTeams,
				toggleTeam: toggleTeam
			};

			var _selectedItems = selecteItems;

			function addSite(parentSite) {
				_selectedItems.siteIds.push(parentSite.site.Id);
				return that;
			}

			function removeSite(parentSite) {
				var selectedSiteIndex = _selectedItems.siteIds.indexOf(parentSite.site.Id);
				_selectedItems.siteIds.splice(selectedSiteIndex, 1);
				return that;
			}

			function addAllTeams(parentSite) {
				parentSite.teams.forEach(function (team) {
					_selectedItems.teamIds.push(team.Id);
				});
				return that;
			}

			function clearTeams(parentSite) {
				parentSite.teams.forEach(function (team) {
					var index = _selectedItems.teamIds.indexOf(team.Id);
					_selectedItems.teamIds.splice(index, 1);
				});
				return that;
			}

			function toggleTeam(item) {
				var indexOfTeam = _selectedItems.teamIds.indexOf(item.Id);
				var teamNotSelected = indexOfTeam == -1;
				if (teamNotSelected)
					_selectedItems.teamIds.push(item.Id);
				else
					_selectedItems.teamIds.splice(indexOfTeam, 1);
				return that;
			}

			return that;
		}

		vm.goToAgents = function () {
			$state.go('rta.agents', vm.selectedItems);
		}

		vm.openTeam = function (team) {
			vm.selectedItems.teamIds = [team.Id];
			$state.go('rta.agents', vm.selectedItems);
		}

	}
})();
