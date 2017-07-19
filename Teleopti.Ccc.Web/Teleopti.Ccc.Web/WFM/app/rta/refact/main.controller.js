(function () {
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
		vm.organization = [];
		vm.siteCards = [];
		vm.urlParams = $stateParams;
		vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
		vm.organizationSelection = false;
		vm.skillSelected = vm.skillIds.length;
		vm.noSiteCards = !vm.siteCards.length;
		vm.goToAgentsView = function () { rtaRouteService.goToSelectSkill(); };

		vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };

		(function fetchDataForFilterComponent() {
			if (vm.skillIds.length > 0) {
				rtaService.getOrganizationForSkills({ skillIds: vm.skillIds }).then(function (result) {
					vm.organization = result;
				});
			} else {
				rtaService.getOrganization().then(function (result) {
					vm.organization = result;
				});
			}
		})();

		(function OverviewComponentHandler() {
			var teamPolling;
			var sitePolling;
			var sitePollingWithSkills;
			var pollingIntervals = [];

			vm.setInitialSelectionsAndState = function () {
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
			}

			vm.setInitialSelectionsAndState();

			if (angular.isDefined(vm.skillAreaId)) {
				vm.skillIds = getSkillIdsFromSkillAreaId(vm.skillAreaId);
				getSiteCards(vm.skillIds);
			} else if (vm.skillIds.length) {
				getSiteCards(vm.skillIds);
			} else {
				getSiteCards();
			}

			function getSiteCards(ids) {
				if (angular.isDefined(ids)) {
					rtaService.getSiteCardsFor(ids).then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				} else {
					rtaService.getSiteCardsFor().then(function (result) {
						vm.siteCards = buildSiteCards(result);
					});
				}
			}

			function buildSiteCards(sites) {
				return sites.map(function (site) {
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
						if (card.isSelected) {
							card.teams.forEach(function (team) {
								team.isSelected = true;
							});
						}
					});
				} else {
					rtaService.getTeamCardsFor({ siteIds: card.site.Id }).then(function (teams) {
						card.teams = teams;
						if (card.isSelected) {
							card.teams.forEach(function (team) {
								team.isSelected = true;
							});
						}
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
				var temp = [];
				var match = vm.skillAreas.find(function (sa) {
					return sa.Id === id;
				});

				match.Skills.forEach(function (skill) {
					temp.push(skill.Id);
				});

				return temp;
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
				function (newValue, oldValue) {
					if (angular.isDefined(sitePollingWithSkills)) {
						$interval.cancel(sitePollingWithSkills);
					}
					if (pollingIntervals.length) {
						pollingIntervals.forEach(function (i) {
							$interval.cancel(i.interval);
						});
						pollingIntervals = [];
					}
					if (newValue.length) {
						vm.skillSelected = true;
						sitePollingWithSkills = $interval(function () {
							rtaService.getSiteCardsFor(vm.skillIds).then(function (result) {
								result.forEach(function (r) {
									updateSiteCard(r);
								})
							});
						}, 5000);
						$interval.cancel(sitePolling);
					} else {
						vm.skillSelected = false;
						sitePolling = $interval(function () {
							rtaService.getSiteCardsFor().then(function (result) {
								result.forEach(function (r) {
									updateSiteCard(r);
								})
							});
						}, 5000);
						$interval.cancel(sitePollingWithSkills);
					}
				});

			function updateSiteCard(site) {
				var match = vm.siteCards.find(function (card) {
					return card.site.Id === site.Id;
				});
				match.site.Color = translateSiteColors(site);
				match.site.InAlarmCount = site.InAlarmCount;
			}

			$scope.$on('$destroy', function () {
				if (pollingIntervals.length) {
					pollingIntervals.forEach(function (i) {
						$interval.cancel(i.interval);
					});
					pollingIntervals = [];
				}

				$interval.cancel(sitePolling);
				$interval.cancel(sitePollingWithSkills);
			});

			vm.filterOutput = function (selectedItem) {
				if (!angular.isDefined(selectedItem)) {
					vm.skillIds = [];
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
					getSiteCards();
					vm.urlParams.skillIds = undefined;
					vm.urlParams.skillAreaId = undefined;
					vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };
					vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
				} else if (selectedItem.hasOwnProperty('Skills')) {
					vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: selectedItem.Id };
					vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillAreaId: "' + selectedItem.Id + '"})';
					$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
					vm.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
					getSiteCards(vm.skillIds);
				} else {
					vm.skillIds = [selectedItem.Id];
					vm.selectedItems = { siteIds: [], teamIds: [], skillIds: vm.skillIds, skillAreaId: undefined };
					vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillIds: ["' + selectedItem.Id + '"]})';
					$state.go($state.current.name, { skillAreaId: undefined, skillIds: vm.skillIds }, { notify: false });
					getSiteCards(vm.skillIds);
				}
			}

			vm.getSelectedItems = function (item) {
				var itemIsSite = angular.isDefined(item.site);
				if (itemIsSite) {
					var indexOfSite = vm.selectedItems.siteIds.indexOf(item.site.Id);
					var siteAlreadySelected = indexOfSite > -1;
					if (!siteAlreadySelected) {
						vm.selectedItems.siteIds.push(item.site.Id);
						if (angular.isDefined(item.teams)) {
							item.teams.forEach(function (team) {
								var teamIsSelected = vm.selectedItems.teamIds.indexOf(team.Id);
								if (teamIsSelected > -1) vm.selectedItems.teamIds.splice(teamIsSelected, 1);
							});
						}
					}
					else {
						vm.selectedItems.siteIds.splice(indexOfSite, 1);
					}
				}
				else {
					var indexOfTeam = vm.selectedItems.teamIds.indexOf(item.Id);
					var teamAlreadySelected = indexOfTeam > -1;
					var match = vm.siteCards.find(function (card) {
						return card.site.Id === item.SiteId;
					});
					var siteIndex = vm.selectedItems.siteIds.indexOf(match.site.Id);

					if (match.isSelected) {
						match.teams.forEach(function (team) {
							var index = vm.selectedItems.teamIds.indexOf(team.Id);
							vm.selectedItems.teamIds.splice(index, 1);
						});

						vm.selectedItems.siteIds.push(match.site.Id);
					}
					else if (!match.isSelected && siteIndex > -1) {
						vm.selectedItems.siteIds.splice(siteIndex, 1);
						match.teams.forEach(function (team) {
							if (team.Id !== item.Id) {
								vm.selectedItems.teamIds.push(team.Id);
							}
						});

					}
					else {
						if (!teamAlreadySelected) {
							vm.selectedItems.teamIds.push(item.Id);
						}
						else {
							vm.selectedItems.teamIds.splice(indexOfTeam, 1);
						}
					}
				}
				vm.organizationSelection = vm.selectedItems.siteIds.length || vm.selectedItems.teamIds.length;
			}

			vm.goToAgents = function () {
				$state.go('rta.agents', vm.selectedItems);
			}

			vm.openTeam = function (team) {
				vm.selectedItems.teamIds = [team.Id];
				$state.go('rta.agents', vm.selectedItems);
			}
		})();
	}
})();
