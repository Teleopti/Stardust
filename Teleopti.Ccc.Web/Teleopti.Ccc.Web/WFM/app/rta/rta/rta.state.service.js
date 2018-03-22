(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaStateService', rtaStateService);

	function rtaStateService($state, rtaService, $q, $sessionStorage, rtaDataService) {

		var state = {
			open: undefined,
			siteIds: [],
			teamIds: [],
			skillAreaId: undefined,
			skillIds: [],
			openedSiteIds: []
		};

		var data = {
			organization: [],
			skills: [],
			skillAreas: [],
		};

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
				return rtaDataService
					.load()
					.then(function (d) {
						data = d;
					})
					.then(updateOpenedSites);
			},

			deselectSkillAndSkillArea: function () {
				$state.go($state.current.name, buildState({skillIds: undefined, skillAreaId: undefined}));
			},

			selectSkill: function (skillOrId) {
				if (skillOrId && skillOrId.Id)
					skillOrId = skillOrId.Id;
				var previousSkillId = state.skillIds.length > 0 ? state.skillIds[0] : null;
				if (skillOrId != previousSkillId)
					$state.go($state.current.name, buildState({skillIds: skillOrId, skillAreaId: undefined}), {notify: false});
			},

			selectSkillArea: function (skillAreaOrId) {
				if (skillAreaOrId && skillAreaOrId.Id)
					skillAreaOrId = skillAreaOrId.Id;
				var previousSkillAreaId = state.skillAreaId ? state.skillAreaId : null;
				if (skillAreaOrId != previousSkillAreaId)
					$state.go($state.current.name, buildState({skillIds: undefined, skillAreaId: skillAreaOrId}), {notify: false});
			},

			agentsHrefForTeam: function (siteId, teamId) {
				var site = data.organization.find(function (site) {
					return siteId == site.Id;
				});
				if (site.Teams.length == 1)
					return $state.href('rta-agents', {siteIds: siteId, skillIds: state.skillIds, skillAreaId: state.skillAreaId});
				return $state.href('rta-agents', {teamIds: teamId, skillIds: state.skillIds, skillAreaId: state.skillAreaId});
			},

			agentsHrefForSite: function (siteId) {
				return $state.href('rta-agents', {siteIds: siteId, skillIds: state.skillIds, skillAreaId: state.skillAreaId});
			},

			skillPickerPreselectedItem: function () {
				return state.skillPickerPreselectedItem;
			},

			hasSkillSelection: function () {
				return state.skillIds.length > 0;
			},

			selectedSkill: selectedSkill,
			selectedSkillArea: selectedSkillArea,

			isSiteSelected: isSiteSelected,
			isTeamSelected: isTeamSelected,

			toggleSite: function (id) {
				selectSite(id, !isSiteSelected(id));
			},

			toggleTeam: function (id) {
				selectTeam(id, !isTeamSelected(id));
			},

			selectSite: selectSite,
			selectTeam: selectTeam,

			siteHasTeamsSelected: siteHasTeamsSelected,

			isSiteOpen: function (id) {
				return state.openedSiteIds.some(function (siteId) {
					return siteId == id
				});
			},

			openSite: function (id, opened) {
				if (opened)
					state.openedSiteIds.push(id);
				else
					state.openedSiteIds = state.openedSiteIds.filter(function (siteId) {
						return siteId != id;
					});
			},

			deselectOrganization: function () {
				$state.go($state.current.name, buildState({siteIds: [], teamIds: []}), {notify: false});
			},

			organizationSelectionText: function () {
				return data.organization
					.flatMap(function (site) {
						if (isSiteSelected(site.Id))
							return [site.Name];
						return site.Teams.map(function (team) {
							if (isTeamSelected(team.Id))
								return team.Name;
						})
					})
					.filter(function (x) {
						return x;
					})
					.slice(0, 9)
					.join(', ');
			},

			pollParams: function (o) {
				var r = {};
				if (o.skillIds)
					r.skillIds = selectedSkillIds();
				if (o.openedSiteIds)
					r.siteIds = state.openedSiteIds;
				if (o.siteIds)
					r.siteIds = state.siteIds;
				if (o.teamIds)
					r.teamIds = state.teamIds;
				return r
			},

			goToAgents: function () {
				$state.go('rta-agents', buildState());
			},

			hasSelection: function () {
				return state.siteIds.length > 0 || state.teamIds.length > 0 || state.skillIds.length > 0 || state.skillAreaId;
			}
		};

		function selectSite(id, selected) {
			if (selected)
				state.siteIds.push(id);
			else
				state.siteIds = state.siteIds.filter(function (s) {
					return s != id
				});
			$state.go($state.current.name, buildState(), {notify: false});
		}

		function selectTeam(id, selected) {
			if (selected)
				state.teamIds.push(id);
			else {
				state.teamIds = state.teamIds.filter(function (s) {
					return s != id
				});
				selectOtherTeamsIfSiteIsSelected(id);
			}
			$state.go($state.current.name, buildState(), {notify: false});
		}

		function selectedSkill() {
			if (state.skillIds.length > 0)
				return data.skills.find(function (s) {
					return s.Id === state.skillIds[0];
				});
			return undefined;
		}

		function siteHasTeamsSelected(siteId) {
			return data.organization.find(function (site) {
				return site.Id == siteId;
			}).Teams.some(function (team) {
				return state.teamIds.some(function (teamId) {
					return teamId == team.Id;
				})
			})
		}

		function selectedSkillArea() {
			return data.skillAreas.find(function (s) {
				return s.Id === state.skillAreaId;
			});
		}

		function selectedSkillIds() {
			if (!state.skillAreaId)
				return state.skillIds;
			var skillArea = data.skillAreas
				.find(function (skillArea) {
					return skillArea.Id === state.skillAreaId;
				});
			if (!skillArea)
				return [];
			return skillArea.Skills.map(function (skill) {
				return skill.Id;
			});
		}

		function isSiteSelected(id) {
			return state.siteIds.some(function (siteId) {
				return siteId == id
			})
		}

		function isTeamSelected(id) {
			var teamSelected = state.teamIds.some(function (teamId) {
				return teamId == id
			});
			if (teamSelected)
				return true;
			var site = data.organization.find(function (site) {
				return site.Teams.some(function (team) {
					return team.Id == id
				});
			});

			if (site)
				return isSiteSelected(site.Id);
			return false;
		}

		function cleanState() {
			state.open = state.open === true || state.open === "true";
			state.siteIds = state.siteIds || [];
			state.siteIds = angular.isArray(state.siteIds) ? state.siteIds : [state.siteIds];
			state.teamIds = state.teamIds || [];
			state.teamIds = angular.isArray(state.teamIds) ? state.teamIds : [state.teamIds];

			// remove duplicate sites n teams
			state.siteIds = state.siteIds.filter(function (item, pos) {
				return state.siteIds.indexOf(item) == pos;
			});
			state.teamIds = state.teamIds.filter(function (item, pos) {
				return state.teamIds.indexOf(item) == pos;
			});

			// add sites where all teams are selected
			data.organization.filter(function (site) {
				return site.Teams.length > 0;
			}).filter(function (site) {
				return site.Teams.every(function (team) {
					return state.teamIds.some(function (teamId) {
						return teamId == team.Id
					});
				});
			}).filter(function (site) {
				return site.FullPermission !== false;
			}).forEach(function (site) {
				if (!state.siteIds.some(function (siteId) {
						return siteId == site.Id
					}))
					state.siteIds.push(site.Id);
			});

			// remove teams where site is selected
			var teamIdsSelectedBySite = data.organization
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
			state.skillIds = angular.isArray(state.skillIds) ? state.skillIds : [state.skillIds];
			if (state.skillAreaId)
				state.skillPickerPreselectedItem = {skillAreaId: state.skillAreaId}
			else
				state.skillPickerPreselectedItem = {skillIds: state.skillIds};
		}

		function storeState() {
			$sessionStorage.rtaState = state;
		}

		function selectOtherTeamsIfSiteIsSelected(teamId) {
			var siteOfTeam = data.organization.find(function (site) {
				return site.Teams.some(function (team) {
					return team.Id == teamId
				});
			})

			var siteSelected = state.siteIds.some(function (siteId) {
				return siteId == siteOfTeam.Id
			});
			if (siteSelected) {
				var otherTeamIds = siteOfTeam.Teams
					.filter(function (team) {
						return team.Id != teamId
					})
					.map(function (team) {
						return team.Id
					});

				state.teamIds = state.teamIds.concat(otherTeamIds);
				state.siteIds = state.siteIds.filter(function (s) {
					return s != siteOfTeam.Id
				});
			}
		}

		function updateOpenedSites() {
			if (state.open) {
				state.openedSiteIds = data.organization.map(function (site) {
					return site.Id;
				})
			}
			else {
				state.openedSiteIds = data.organization.filter(function (site) {
					return site.Teams.some(function (team) {
						return state.teamIds.some(function (teamId) {
							return team.Id == teamId;
						})
					});
				})
					.map(function (site) {
						return site.Id;
					})
					.concat($sessionStorage.rtaState.openedSiteIds);
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