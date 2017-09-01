(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaStateService', rtaStateService);

	function rtaStateService($state) {

		var state = {
			open: undefined,
			siteIds: [],
			teamIds: [],
			skillAreaId: undefined,
			skillIds: []
		};

		return {
			setCurrentState: function (newState) {
				state = mutateState(newState);
			},

			deselectSkillAndSkillArea: function () {
				$state.go($state.current.name, state = mutateState({ skillIds: undefined, skillAreaId: undefined }), { reload: true });
			},

			selectSkillArea: function (skillAreaId) {
				$state.go($state.current.name, state = mutateState({ skillIds: undefined, skillAreaId: skillAreaId }), { reload: true });
			},

			selectSkill: function (skillId) {
				$state.go($state.current.name, state = mutateState({ skillIds: skillId, skillAreaId: undefined }), { reload: true });
			},

			agentsHrefForTeam: function (teamId) {
				return $state.href('rta-agents', {
					teamIds: teamId,
					skillIds: state.skillIds,
					skillAreaId: state.skillAreaId
				});
			},

			agentsHrefForSite: function (siteId) {
				return $state.href('rta-agents', {
					siteIds: siteId,
					skillIds: state.skillIds,
					skillAreaId: state.skillAreaId
				});
			},

			skillPickerPreselectedItem: function () {
				return { skillIds: state.skillIds, skillAreaId: state.skillAreaId };
			},

			hasSkillSelection: function () {
				return state.skillIds.length > 0;
			},

			isSiteSelected: function (id) {
				if (!state.siteIds) return false;
				return state.siteIds.indexOf(id) > -1;
			},

			isTeamSelected: function (id) {
				if (!state.teamIds) return false;
				return state.teamIds.indexOf(id) > -1;
			},

			selectSite: function (id, selected) {
				state.siteIds = state.siteIds || [];
				if (selected) {
					if (!state.siteIds.find(function (s) { return s == id }))
						state.siteIds.push(id);
				} else
					state.siteIds = state.siteIds.filter(function (s) { return s != id });
			},

			selectTeam: function (siteCard, id, selected) {
				state.teamIds = state.teamIds || [];
				if (selected) {
					if (!state.teamIds.find(function (s) { return s == id }))
						state.teamIds.push(id);
				} else
					state.teamIds = state.teamIds.filter(function (s) { return s != id });
			},

			goToAgents: function (sites) {
				var siteIds = state.siteIds || [];
				var skillIds = state.skillIds || [];

				if (state.skillAreaId)
					skillIds = angular.isDefined(state.skillIds) ? state.skillIds : [];

				var teamIdsSelectedBySite = sites
					.filter(function (site) {
						return state.siteIds.indexOf(site.Id) > -1;
					})
					.map(function (site) {
						return site.teams;
					})
					.reduce(function (flat, toFlatten) {
						return flat.concat(toFlatten);
					}, [])
					.map(function (team) {
						return team.Id
					});

				var teamIds = state.teamIds.filter(function (teamId) {
					return teamIdsSelectedBySite.indexOf(teamId) == -1
				});

				$state.go('rta-agents', { siteIds: state.siteIds, teamIds: teamIds, skillIds: skillIds, skillAreaId: state.skillAreaId });
			}
		}

		function mutateState(mutations) {
			var mutatedState = JSON.parse(JSON.stringify(state));
			Object.keys(mutations).forEach(function (key) {
				mutatedState[key] = mutations[key];
			});
			return mutatedState;
		}

	}

})();



(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaOverviewController39082', RtaOverviewController);

	RtaOverviewController.$inject = ['rtaService', 'rtaRouteService', 'rtaStateService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope', '$q', '$timeout'];

	function RtaOverviewController(rtaService, rtaRouteService, rtaStateService, skills, skillAreas, $state, $stateParams, $interval, $scope, $q, $timeout) {
		var vm = this;
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;

		rtaStateService.setCurrentState($stateParams);

		$stateParams.open = ($stateParams.open || "false");
		if ($stateParams.skillAreaId)
			$stateParams.skillIds = getSkillIdsFromSkillAreaId($stateParams.skillAreaId);
		else
			$stateParams.skillIds = $stateParams.skillIds || [];
		$stateParams.siteIds = $stateParams.siteIds || [];
		$stateParams.teamIds = $stateParams.teamIds || [];

		vm.skillPickerPreselectedItem = rtaStateService.skillPickerPreselectedItem();

		vm.displayNoSitesMessage = function () { return vm.siteCards.length == 0; };
		vm.displayNoSitesForSkillMessage = rtaStateService.hasSkillSelection;

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
			pollPromise = $timeout(function () {
				getSites().then(pollNext)
			}, 5000);
		}

		function pollStop() {
			if (pollPromise)
				$timeout.cancel(pollPromise);
		}

		pollInitiate();

		$scope.$on('$destroy', pollStop);

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

						site.Teams = site.Teams || [];

						if (!siteCard) {
							siteCard = {
								Id: site.Id,
								Name: site.Name,
								isOpen: $stateParams.open != "false" || site.Teams.length > 0,
								isSelected: rtaStateService.isSiteSelected(site.Id),
								teams: [],
								AgentsCount: site.AgentsCount,
								href: rtaStateService.agentsHrefForSite(site.Id)
							};

							$scope.$watch(function () { return siteCard.isOpen }, function (newValue) { if (newValue) pollNow(); });
							$scope.$watch(function () { return siteCard.isSelected }, function (newValue, oldValue) {
								if (newValue)
									siteCard.teams.forEach(function (t) {
										t.isSelected = true;
									});
								rtaStateService.selectSite(siteCard.Id, newValue);
							});

							vm.siteCards.push(siteCard);
						};

						updateTeams(siteCard, site.Teams);
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
						SiteId: team.SiteId,
						isSelected: rtaStateService.isTeamSelected(team.Id),
						AgentsCount: team.AgentsCount,
						href: rtaStateService.agentsHrefForTeam(team.Id)
					};

					$scope.$watch(function () { return teamCard.isSelected }, function (newValue, oldValue) {
						if (newValue) {
							var areAllTeamsSelected = siteCard.teams.every(function (t) { return t.isSelected });
							if (areAllTeamsSelected) siteCard.isSelected = true;
						}
						else
							siteCard.isSelected = false;
						rtaStateService.selectTeam(siteCard, teamCard.Id, newValue);
					});

					siteCard.teams.push(teamCard);
				};
				teamCard.Color = team.Color;
				teamCard.InAlarmCount = team.InAlarmCount;
			})
			if (siteCard.isSelected) {
				siteCard.teams.forEach(function (team) {
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

		vm.selectSkillOrSkillArea = function (skillOrSkillArea) {
			if (!angular.isDefined(skillOrSkillArea))
				rtaStateService.deselectSkillAndSkillArea();
			else if (skillOrSkillArea.hasOwnProperty('Skills'))
				rtaStateService.selectSkillArea(skillOrSkillArea.Id);
			else
				rtaStateService.selectSkill(skillOrSkillArea.Id);
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

		vm.goToAgents = function () { rtaStateService.goToAgents(vm.siteCards) };

	}
})();
