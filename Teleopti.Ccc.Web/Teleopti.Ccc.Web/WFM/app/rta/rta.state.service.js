(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaStateService', rtaStateService);

	function rtaStateService($state, rtaService, $q, $sessionStorage) {

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
			gotoLastState: function () {
				if ($sessionStorage.rtaState) {
					state = $sessionStorage.rtaState;
					$state.go($state.current.name, buildState());
				}
			},

			setCurrentState: function (newState) {
				mutate(state, newState);
				cleanState();
				storeState();
				return $q.all(dataLoaded);
			},

			deselectSkillAndSkillArea: function () {
				$state.go($state.current.name, buildState({ skillIds: undefined, skillAreaId: undefined }));
			},

			selectSkillArea: function (skillAreaId) {
				$state.go($state.current.name, buildState({ skillIds: undefined, skillAreaId: skillAreaId }));
			},

			selectSkill: function (skillId) {
				$state.go($state.current.name, buildState({ skillIds: skillId, skillAreaId: undefined }));
			},

			agentsHrefForTeam: function (teamId) {
				return $state.href('rta-agents', { teamIds: teamId, skillIds: state.skillIds, skillAreaId: state.skillAreaId });
			},

			agentsHrefForSite: function (siteId) {
				return $state.href('rta-agents', { siteIds: siteId, skillIds: state.skillIds, skillAreaId: state.skillAreaId });
			},

			skillPickerPreselectedItem: function () {
				if (state.skillAreaId)
					return { skillAreaId: state.skillAreaId }
				return { skillIds: state.skillIds };
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
			state.siteIds = state.siteIds || [];
			state.teamIds = state.teamIds || [];

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

		function storeState() {
			$sessionStorage.rtaState = state;
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
			storeState();

			var gotoState = {
				skillAreaId: undefined,
				skillIds: undefined,
				siteIds: undefined,
				teamIds: undefined
			};
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