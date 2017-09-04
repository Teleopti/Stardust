(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaStateService', rtaStateService);

	function rtaStateService($state, rtaService) {

		var state = {
			open: undefined,
			siteIds: [],
			teamIds: [],
			skillAreaId: undefined,
			skillIds: []
		};




		var organization = [];

		var dataLoaded = rtaService.getOrganization()
			.then(function (data) {
				data.forEach(function (site) {
					if (!site.Teams) site.Teams = [];
				})
				organization = data;
			});

		return {
			setCurrentState: function (newState) {
				mutate(state, newState);
			},

			deselectSkillAndSkillArea: function () {
				$state.go($state.current.name, buildState({ skillIds: undefined, skillAreaId: undefined }), { reload: true });
			},

			selectSkillArea: function (skillAreaId) {
				$state.go($state.current.name, buildState({ skillIds: undefined, skillAreaId: skillAreaId }), { reload: true });
			},

			selectSkill: function (skillId) {
				$state.go($state.current.name, buildState({ skillIds: skillId, skillAreaId: undefined }), { reload: true });
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
				var result = state.teamIds.some(function (teamId) { return teamId == id });
				if (result)
					return true;

				var siteId = organization.find(function (site) {
					return site.Teams.some(function (team) { return team.Id == id });
				}).Id;

				return state.siteIds.some(function (id) { return id == siteId });
			},

			selectSite: function (id, selected) {
				state.siteIds = state.siteIds || [];
				if (selected)
					state.siteIds.push(id);
				else
					state.siteIds = state.siteIds.filter(function (s) { return s != id });

				$state.go($state.current.name, buildState());
			},

			selectTeam: function (id, selected) {
				state.teamIds = state.teamIds || [];
				if (selected)
					state.teamIds.push(id);
				else {
					state.teamIds = state.teamIds.filter(function (s) { return s != id });

					var siteOfTeam = organization.find(function (site) {
						return site.Teams.some(function (team) { return team.Id == id });
					})

					var siteSelected = state.siteIds.some(function (siteId) { return siteId == siteOfTeam.Id });
					if (siteSelected) {
						var otherTeamIds = siteOfTeam.Teams
							.filter(function (team) { return team.Id != id })
							.map(function (team) { return team.Id });

						state.teamIds = state.teamIds.concat(otherTeamIds);
						state.siteIds = state.siteIds.filter(function (s) { return s != siteOfTeam.Id });
					}
				}

				$state.go($state.current.name, buildState());
			},

			goToAgents: function () {
				$state.go('rta-agents', buildState());
			}
		}

		function cleanState() {
			// remove duplicate sites n teams
			state.siteIds = state.siteIds.filter(function (item, pos) {
				return state.siteIds.indexOf(item) == pos;
			});
			state.teamIds = state.teamIds.filter(function (item, pos) {
				return state.teamIds.indexOf(item) == pos;
			});

			// add sites where all teams are selected
			organization.filter(function (site) {
				if (site.Teams.length == 0) return false;
				return site.Teams.every(function (team) {
					return state.teamIds.some(function (teamId) { return teamId == team.Id });
				});
			}).forEach(function (site) {
				if (!state.siteIds.some(function (siteId) { return siteId == site.Id }))
					state.siteIds.push(site.Id);
			});

			// remove teams where site is selected
			var teamIdsSelectedBySite = organization
				.filter(function (site) {
					return state.siteIds.indexOf(site.Id) > -1;
				})
				.map(function (site) {
					return site.Teams || [];
				})
				.reduce(function (flat, toFlatten) {
					return flat.concat(toFlatten);
				}, [])
				.map(function (team) {
					return team.Id
				});
			state.teamIds = state.teamIds.filter(function (teamId) {
				return teamIdsSelectedBySite.indexOf(teamId) == -1
			});

			// skill stuff
			state.skillIds = state.skillIds || [];

		}

		function mutate(object, mutations) {
			if (!mutations)
				return;
			Object.keys(mutations).forEach(function (key) {
				object[key] = mutations[key];
			});
		}

		function buildState(mutations) {

			mutate(state, mutations);
			cleanState();

			var gotoState = {};
			if (state.siteIds.length > 0)
				gotoState.siteIds = state.siteIds;
			if (state.teamIds.length > 0)
				gotoState.teamIds = state.teamIds;
			if (state.skillAreaId)
				gotoState.skillAreaId = state.skillAreaId;
			else if (state.skillIds.length > 0)
				gotoState.skillIds = state.skillIds;
			if (state.open)
				gotoState.open = true;

			return gotoState;
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
								get isSelected() { return rtaStateService.isSiteSelected(site.Id); },
								set isSelected(newValue) { rtaStateService.selectSite(site.Id, newValue); },
								teams: [],
								AgentsCount: site.AgentsCount,
								href: rtaStateService.agentsHrefForSite(site.Id)
							};

							$scope.$watch(function () { return siteCard.isOpen }, function (newValue) { if (newValue) pollNow(); });
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
						get isSelected() { return rtaStateService.isTeamSelected(team.Id); },
						set isSelected(newValue) { rtaStateService.selectTeam(team.Id, newValue); },
						AgentsCount: team.AgentsCount,
						href: rtaStateService.agentsHrefForTeam(team.Id)
					};
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

		vm.goToAgents = rtaStateService.goToAgents;

	}
})();
