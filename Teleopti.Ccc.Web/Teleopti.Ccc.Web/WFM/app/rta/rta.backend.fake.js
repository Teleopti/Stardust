'use strict';
(function () {
	angular
		.module('wfm.rta')
		.factory('FakeRtaBackend', fakeRtaBackend);

	fakeRtaBackend.$inject = ['$httpBackend', 'BackendFaker'];

	function fakeRtaBackend($httpBackend, faker) {

		var serverTime = null;
		var agentStates = [];
		var adherenceForToday = [];
		var sitesWithTeams = [];
		var sitesWithTeamsOnSkills = [];
		var siteAdherences = [];
		var teamAdherences = [];
		var skills = [];
		var skillAreas = [];
		var phoneStates = [];
		var timeline = {};

		var service = {
			clear: clear,
			withToggle: faker.withToggle,
			withTime: withTime,
			withAgentState: withAgentState,
			clearAgentStates: clearAgentStates,
			withAdherence: withAdherence,
			withSite: withSite,
			withSiteAdherence: withSiteAdherence,
			clearSiteAdherences: clearSiteAdherences,
			withTeam: withTeam,
			withTeamAdherence: withTeamAdherence,
			clearTeamAdherences: clearTeamAdherences,
			withSkill: withSkill,
			withSkillAreas: withSkillAreas,
			withPhoneState: withPhoneState,
			withOrganization: withOrganization,
			withOrganizationOnSkills: withOrganizationOnSkills,
			withTimeline: withTimeline,

			get skills() { return skills; },
			get skillAreas() { return skillAreas; }

		};

		faker.fake(/\.\.\/api\/AgentStates\/For(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null && params.skillIds != null)
						return agentStates.filter(function (a) {
							return params.skillIds.indexOf(a.SkillId) >= 0
						}).filter(function (a) {
							return params.siteIds.indexOf(a.SiteId) >= 0
						});
					if (params.siteIds != null)
						return agentStates.filter(function (a) {
							return params.siteIds.indexOf(a.SiteId) >= 0
						});
					if (params.teamIds != null && params.skillIds != null)
						return agentStates.filter(function (a) {
							return params.skillIds.indexOf(a.SkillId) >= 0
						}).filter(function (a) {
							return params.teamIds.indexOf(a.TeamId) >= 0
						});
					if (params.teamIds != null)
						return agentStates.filter(function (a) {
							return params.teamIds.indexOf(a.TeamId) >= 0
						});
					return agentStates.filter(function (a) {
						return params.skillIds.indexOf(a.SkillId) >= 0
					});
				})();
				if (params.inAlarm == 'true')
					ret = agentStatesInAlarm(ret);
				if (params.excludedStateIds)
					ret = ret.filter(function (s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});

				return [200, {
					Time: serverTime,
					States: ret
				}];
			});

		function agentStatesInAlarm(collection) {
			return collection.filter(function (s) {
				return s.TimeInAlarm > 0;
			}).sort(function (s1, s2) {
				return s2.TimeInAlarm - s1.TimeInAlarm;
			});
		}

		faker.fake(/\.\.\/api\/SkillArea\/For(.*)/,
			function (params) {
				var result = skillAreas
					.filter(function (s) {
						return params.skillAreaId === s.Id
					});
				return [200, result[0]];
			});

		faker.fake(/\.\.\/api\/SkillAreas(.*)/,
			function () {
				return [200, {
					SkillAreas: skillAreas
				}];
			});

		faker.fake(/\.\.\/api\/Skills(.*)/,
			function () {
				return [200, skills];
			});

		faker.fake(/\.\.\/api\/Sites\/OrganizationForSkills(.*)/,
			function (params) {
				var uniqueSiteIds = [];
				var returnOrg = [];
				var skillIdsArray = angular.isArray(params.skillIds) ? params.skillIds : params.skillIds.split(",");
				skillIdsArray.forEach(function (key) {
					if (uniqueSiteIds.indexOf(sitesWithTeamsOnSkills[key][0].Id) < 0) {
						uniqueSiteIds = uniqueSiteIds.concat(sitesWithTeamsOnSkills[key][0].Id);
						returnOrg = returnOrg.concat(sitesWithTeamsOnSkills[key]);
					}
				});

				return [200, returnOrg];
			});

		faker.fake(/\.\.\/api\/Sites\/Organization(.*)/,
			function () {
				return [200, sitesWithTeams];
			});

		faker.fake(/\.\.\/api\/Adherence\/ForToday(.*)/,
			function (params) {
				var result = adherenceForToday.find(function (a) {
					return a.PersonId === params.personId;
				});
				return [200, result];
			});

		faker.fake(/\.\.\/api\/PhoneState\/InfoFor(.*)/,
			function (data) {
				if (data.ids.indexOf(null) > -1 || data.ids.indexOf("noState") > -1)
					throw new Error('Nope, dont ask server for that')

				var result = phoneStates.filter(function (s) {
					return data.ids.indexOf(s.Id) > -1
				});

				if (result.length === 0) {
					result = agentStates
						.filter(function (s) {
							if (data.ids.indexOf(s.StateId) > -1)
								return true;
						})
						.map(function (s) {
							return {
								Name: s.State,
								Id: s.StateId
							}
						});
				}
				return [200, {
					PhoneStates: result
				}]
			});

		faker.fake(/\.\.\/api\/Overview\/SiteCards(.*)/,
			function (params) {
				var sites = JSON.parse(JSON.stringify(siteAdherences));

				if (params.skillIds)
					sites = sites.filter(function (sa) {
						return params.skillIds.indexOf(sa.SkillId) > -1;
					});

				var siteIds = params.siteIds || [];
				if (siteIds.length > 0) {
					sites.forEach(function (site) {
						if (siteIds.indexOf(site.Id) > -1) {
							var teams = teamAdherences.filter(function (team) {
								return team.SiteId == site.Id;
							})
							if (params.skillIds)
								teams = teams.filter(function (team) {
									return params.skillIds.indexOf(team.SkillId) > -1;
								});
							site.Teams = teams
						}
					})
				}

				return [200, {
					Sites: sites,
					TotalAgentsInAlarm: sites
						.map(function (s) { return s.InAlarmCount })
						.reduce(function (s, v) { return s + v }, 0)
				}];
			});

		faker.fake(/\.\.\/api\/Overview\/TeamCards(.*)/,
			function (params) {
				var result = teamAdherences;
				result = result.filter(function (ta) {
					return params.siteId == ta.SiteId;
				});
				if (params.skillIds)
					result = result.filter(function (ta) {
						return params.skillIds.indexOf(ta.SkillId) > -1;
					});
				return [200, result];
			});

		faker.fake(/\.\.\/api\/HistoricalAdherence\/For(.*)/,
			function (params) {
				var result = agentStates.find(function (agent) {
					return params.personId == agent.PersonId;
				});
				if (result != null) {
					result.Now = serverTime;
					result.Timeline = timeline;
				}

				return [200, result];
			});


		function clear() {
			serverTime = null;
			adherenceForToday = [];
			siteAdherences = [];
			teamAdherences = [];
			skillAreas = [];
			phoneStates = [];
			timeline = {};
			faker.clear();
		}

		function withTime(time) {
			serverTime = time;
			return this;
		};

		function clearAgentStates() {
			agentStates = [];
			return this;
		};

		function withAgentState(state) {
			agentStates.push(state);
			return this;
		}

		function withAdherence(adherence) {
			adherenceForToday.push(adherence);
			return this;
		}

		function withSiteAdherence(siteAdherence) {
			siteAdherences.push(siteAdherence);
			withSite(siteAdherence);
			return this;
		};

		function clearSiteAdherences() {
			siteAdherences = [];
			return this;
		};

		function withTeamAdherence(teamAdherence) {
			teamAdherences.push(teamAdherence);
			withTeam(teamAdherence);
			return this;
		};

		function clearTeamAdherences() {
			teamAdherences = [];
			return this;
		};

		function withSkill(skill) {
			skills.push(skill);
			return this;
		}

		function withSkillAreas(newSkillAreas) {
			newSkillAreas.forEach(function (e) { skillAreas.push(e) })
			return this;
		}

		function withPhoneState(phoneState) {
			phoneStates.push(phoneState)
			return this;
		}

		function withOrganization(siteWithTeams) {
			withSite(siteWithTeams);
			return this;
		}

		function withSite(site) {
			sitesWithTeams.push(site);
			return this;
		}

		function withTeam(team) {
			var site = sitesWithTeams.find(function (site) { return site.Id == team.SiteId; });
			if (site) {
				site.Teams = site.Teams || [];
				site.Teams.push(team);
			}
			return this;
		}

		function withOrganizationOnSkills(siteWithTeams, skillIds) {
			skillIds.split(",").forEach(function (key) {
				var skillIdAsAKey = key.trim();
				//organizationsOnSkills[skillIdAsAKey] = organization;
				if (angular.isDefined(sitesWithTeamsOnSkills[skillIdAsAKey]))
					sitesWithTeamsOnSkills[skillIdAsAKey] = sitesWithTeamsOnSkills[skillIdAsAKey].concat(siteWithTeams);
				else
					sitesWithTeamsOnSkills[skillIdAsAKey] = [siteWithTeams];
			});
			return this;
		}

		function withTimeline(tl) {
			timeline = tl;
			return this;
		}

		return service;
	};
})();