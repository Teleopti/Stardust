(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaStateService', rtaStateService);

	function rtaStateService($state, rtaService, $q) {

		var state = {
			open: undefined,
			siteIds: [],
			teamIds: [],
			skillAreaId: undefined,
			skillIds: [],
			openedSiteIds: []
		};

		var organization = [];
		var skills = [];
		var skillAreas = [];

		var dataLoaded = [
			rtaService.getOrganization()
				.then(function (data) {
					organization = data;
					organization.forEach(function (site) {
						if (!site.Teams) site.Teams = [];
					});
					updateOpenedSites();
				}),
			rtaService.getSkills()
				.then(function (data) {
					skills = data;
				}),
			rtaService.getSkillAreas()
				.then(function (data) {
					skillAreas = data.SkillAreas;
				})
		];

		return {
			setCurrentState: function (newState) {
				mutate(state, newState);
				cleanState();
				return $q.all(dataLoaded);
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
				return $state.href('rta-agents', { teamIds: teamId, skillIds: state.skillIds, skillAreaId: state.skillAreaId });
			},

			agentsHrefForSite: function (siteId) {
				return $state.href('rta-agents', { siteIds: siteId, skillIds: state.skillIds, skillAreaId: state.skillAreaId });
			},

			skillPickerPreselectedItem: function () {
				return { skillIds: state.skillIds, skillAreaId: state.skillAreaId };
			},

			hasSkillSelection: function () {
				return state.skillIds.length > 0;
			},

			isSiteSelected: isSiteSelected,

			isTeamSelected: isTeamSelected,

			selectSite: function (id, selected) {
				if (selected)
					state.siteIds.push(id);
				else
					state.siteIds = state.siteIds.filter(function (s) { return s != id });
				$state.go($state.current.name, buildState());
			},

			selectTeam: function (id, selected) {
				if (selected)
					state.teamIds.push(id);
				else {
					state.teamIds = state.teamIds.filter(function (s) { return s != id });
					selectOtherTeamsIfSiteIsSelected(id);
				}
				$state.go($state.current.name, buildState());
			},

			isSiteOpen: function (id) {
				return state.openedSiteIds.some(function (siteId) { return siteId == id });
			},

			openSite: function (id, opened) {
				if (opened)
					state.openedSiteIds.push(id);
				else
					state.openedSiteIds = state.openedSiteIds.filter(function (siteId) { return siteId != id; });
			},

			pollParams: function () {
				var skillIds = state.skillIds;
				if (state.skillAreaId)
					skillIds = skillAreas
						.find(function (skillArea) { return skillArea.Id === state.skillAreaId; })
						.Skills.map(function (skill) { return skill.Id; });

				return { skillIds: skillIds, teamIds: state.teamIds, siteIds: state.openedSiteIds }
			},

			hasSelection: function () {
				return state.siteIds.length > 0 || state.teamIds.length > 0;
			},

			goToAgents: function () {
				$state.go('rta-agents', buildState());
			}
		}

		function isSiteSelected(id) {
			return state.siteIds.some(function (siteId) { return siteId == id })
		}

		function isTeamSelected(id) {
			var teamSelected = state.teamIds.some(function (teamId) { return teamId == id });
			if (teamSelected)
				return true;
			var siteId = organization.find(function (site) {
				return site.Teams.some(function (team) { return team.Id == id });
			}).Id;
			return isSiteSelected(siteId);
		}

		function cleanState() {

			state.open = state.open === true || state.open === "true";

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

		function selectOtherTeamsIfSiteIsSelected(teamId) {
			var siteOfTeam = organization.find(function (site) {
				return site.Teams.some(function (team) { return team.Id == teamId });
			})

			var siteSelected = state.siteIds.some(function (siteId) { return siteId == siteOfTeam.Id });
			if (siteSelected) {
				var otherTeamIds = siteOfTeam.Teams
					.filter(function (team) { return team.Id != teamId })
					.map(function (team) { return team.Id });

				state.teamIds = state.teamIds.concat(otherTeamIds);
				state.siteIds = state.siteIds.filter(function (s) { return s != siteOfTeam.Id });
			}
		}

		function updateOpenedSites() {
			if (state.open) {
				state.openedSiteIds = organization.map(function (site) {
					return site.Id;
				})
			}
			else {
				state.openedSiteIds = organization.filter(function (site) {
					return site.Teams.some(function (team) {
						return state.teamIds.some(function (teamId) {
							return team.Id == teamId;
						})
					});
				})
					.map(function (site) {
						return site.Id;
					});
			}
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

		rtaStateService.setCurrentState($stateParams)
			.then(pollInitiate);

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

		$scope.$on('$destroy', pollStop);

		function getSites() {
			return rtaService.getOverviewModelFor(rtaStateService.pollParams())
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
								get isOpen() { return rtaStateService.isSiteOpen(site.Id); },
								set isOpen(newValue) {
									rtaStateService.openSite(site.Id, newValue);
									if (newValue) pollNow();
								},
								get isSelected() { return rtaStateService.isSiteSelected(site.Id); },
								set isSelected(newValue) { rtaStateService.selectSite(site.Id, newValue); },
								teams: [],
								AgentsCount: site.AgentsCount,
								href: rtaStateService.agentsHrefForSite(site.Id)
							};

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

		vm.goToAgentsView = function () { $state.go('rta-agents'); };

		vm.displayGoToAgents = rtaStateService.hasSelection;

		vm.goToAgents = rtaStateService.goToAgents;

	}
})();

