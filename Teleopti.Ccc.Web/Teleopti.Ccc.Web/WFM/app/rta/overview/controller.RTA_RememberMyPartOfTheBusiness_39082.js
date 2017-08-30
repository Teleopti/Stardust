(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController39082', RtaOverviewController);

	RtaOverviewController.$inject = ['rtaService', 'rtaRouteService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope', '$q', '$timeout'];

	function RtaOverviewController(rtaService, rtaRouteService, skills, skillAreas, $state, $stateParams, $interval, $scope, $q, $timeout) {
		var vm = this;
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;

		$stateParams.open = ($stateParams.open || "false");
		if ($stateParams.skillAreaId)
			$stateParams.skillIds = getSkillIdsFromSkillAreaId($stateParams.skillAreaId);
		else
			$stateParams.skillIds = $stateParams.skillIds || [];
		$stateParams.siteIds = $stateParams.siteIds || [];
		$stateParams.teamIds = $stateParams.teamIds || [];

		vm.skillPickerPreselectedItem = { skillIds: $stateParams.skillIds, skillAreaId: $stateParams.skillAreaId };

		vm.displayNoSitesMessage = function () { return vm.siteCards.length == 0; };
		vm.displayNoSitesForSkillMessage = function () { return $stateParams.skillIds.length > 0; };

		var pollPromise;

		vm.selectTeamOrSite = function (selectable) {
			selectable.isSelected = !selectable.isSelected;
		};

		function pollInitiate() {
			vm.siteCards = [];
			pollNow();
		}

		function pollNow() {
			pollStop();
			getSites()
				.then(pollNext);
		}

		function pollNext() {
			pollPromise = $timeout(getData, 5000);
		}

		function pollStop() {
			if (pollPromise)
				$timeout.cancel(pollPromise);
		}

		pollInitiate();

		$scope.$on('$destroy', pollStop);

		function getData() {
			$q.all([
				getSites(),
			]).then(pollNext);
		};

		function getSites() {
			var siteIds = [];
			vm.siteCards.forEach(function (siteCard) {
				if (siteCard.isOpen) siteIds.push(siteCard.Id);
			});

			return rtaService.getOverviewModelFor({ skillIds: $stateParams.skillIds, teamIds: $stateParams.teamIds, siteIds: siteIds })
				.then(function (data) {
					data.Sites.forEach(function (site) {
						var siteCard = vm.siteCards.find(function (siteCard) {
							return siteCard.Id === site.Id;
						});
						var teams = data.Teams
							.filter(function (team) {
								return team.SiteId == site.Id;
							});
						if (!siteCard) {
							siteCard = {
								Id: site.Id,
								Name: site.Name,
								isOpen: $stateParams.open != "false" || teams.length > 0,
								isSelected: $stateParams.siteIds.indexOf(site.Id) > -1,
								teams: [],
								AgentsCount: site.AgentsCount,
								href: $state.href('rta-agents', { siteIds: site.Id, skillIds: $stateParams.skillIds, skillAreaId: $stateParams.skillAreaId })
							};
							$scope.$watch(function () { return siteCard.isOpen }, function (newValue) { if (newValue) pollNow(); });
							$scope.$watch(function () { return siteCard.isSelected }, function (newValue, oldValue) {
								if (newValue != oldValue) {
									if (newValue) {
										siteCard.teams.forEach(function (t) {
											t.isSelected = true;
										});
									}
								}
							});
							vm.siteCards.push(siteCard);
						};

						updateTeams(siteCard, teams);
						siteCard.Color = translateSiteColors(site);
						siteCard.InAlarmCount = site.InAlarmCount;

					});

					vm.totalAgentsInAlarm = data.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
				});
		}

		function updateTeams(siteCard, teams) {
			teams.forEach(function (team) {
				var teamCard = siteCard.teams.find(function (t) {
					return team.Id === t.Id;
				});

				if (!teamCard) {

					teamCard = {
						Id: team.Id,
						Name: team.Name,
						SiteId: siteCard.Id,
						isSelected: $stateParams.teamIds.indexOf(team.Id) > -1,
						AgentsCount: team.AgentsCount,
						href: $state.href('rta-agents', { teamIds: team.Id, skillIds: $stateParams.skillIds, skillAreaId: $stateParams.skillAreaId })
					};

					$scope.$watch(function () { return teamCard.isSelected }, function (newValue, oldValue) {
						if (newValue) {
							var areAllTeamsSelected = siteCard.teams.every(function (t) { return t.isSelected });
							if (areAllTeamsSelected) siteCard.isSelected = true;
						}
						else {
							siteCard.isSelected = false;
						}
					});

					siteCard.teams.push(teamCard);
				};
				teamCard.Color = team.Color;
				teamCard.InAlarmCount = team.InAlarmCount;
			})
			setTeamToSelected(siteCard);
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
			pollInitiate();
		}

		function resetOnNoSkills() {
			$stateParams.skillIds = undefined;
			$stateParams.skillAreaId = undefined;
			vm.agentsState = 'rta-agents({siteIds: card.site.Id})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
		}

		function setUpForSkillArea(selectedItem) {
			$stateParams.skillAreaId = selectedItem.Id;
			$stateParams.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
			vm.agentsState = 'rta-agents({siteIds: card.site.Id, skillAreaId: "' + selectedItem.Id + '"})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillAreaId: "' + selectedItem.Id + '"})';
			$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
		}

		function setUpForSkill(selectedItem) {
			$stateParams.skillIds = [selectedItem.Id];
			vm.agentsState = 'rta-agents({siteIds: card.site.Id, skillIds: ["' + selectedItem.Id + '"]})';
			vm.agentsStateForTeam = 'rta-agents({teamIds: team.Id, skillIds: ["' + selectedItem.Id + '"]})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: $stateParams.skillIds }, { notify: false });
		}

		vm.goToAgentsView = function () {
			$state.go('rta-agents');
		};

		vm.displayGoToAgents = function () {
			var match = vm.siteCards.find(function (s) {
				return s.isSelected ||
					s.teams.find(function (t) {
						return t.isSelected;
					});
			});
			return !!match;
		};

		vm.goToAgents = function () {
			var teamIds = [];
			var siteIds = [];
			var skillIds = [];

			if (!$stateParams.skillAreaId)
				skillIds = angular.isDefined($stateParams.skillIds) ? $stateParams.skillIds : [];

			vm.siteCards.forEach(function (siteCard) {
				if (siteCard.isSelected) siteIds.push(siteCard.Id);
				else siteCard.teams.forEach(function (team) {
					if (team.isSelected) teamIds.push(team.Id);
				});
			});

			$state.go('rta-agents', { siteIds: siteIds, teamIds: teamIds, skillIds: skillIds, skillAreaId: $stateParams.skillAreaId });
		}
	}
})();
