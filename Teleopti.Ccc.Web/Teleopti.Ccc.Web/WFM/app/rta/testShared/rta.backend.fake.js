'use strict';
(function () {
	angular
		.module('wfm.rta')
		.service('FakeRtaBackend', fakeRtaBackend);

	fakeRtaBackend.$inject = ['$httpBackend', 'BackendFaker'];

	function fakeRtaBackend($httpBackend, service) {

		var serverTime = null;
		var agentStates = [];
		var sitesWithTeams = [];
		var sitesWithTeamsOnSkills = [];
		var siteAdherences = [];
		var teamAdherences = [];
		var skills = [];
		var skillGroups = [];
		var phoneStates = [];

		service.withTime = withTime;

		service.withAgentState = withAgentState;
		service.clearAgentStates = clearAgentStates;

		service.withSiteAdherence = withSiteAdherence;
		service.clearSiteAdherences = clearSiteAdherences;
		service.withTeamAdherence = withTeamAdherence;
		service.clearTeamAdherences = clearTeamAdherences;

		service.withSkill = withSkill;
		service.withSkillGroup = withSkillGroup;
		service.withSkillGroups = withSkillGroups;

		service.withPhoneState = withPhoneState;
		service.withOrganization = withOrganization;
		service.withOrganizationOnSkills = withOrganizationOnSkills;
		service.clearOrganization = clearOrganization;

		Object.defineProperty(service, 'skills', {
			get: function () {
				return skills;
			}
		});
		Object.defineProperty(service, 'skillAreas', {
			get: function () {
				return skillGroups;
			}
		});

		var clearAll = service.clear.all;
		service.clear.all = function () {

			serverTime = null;
			siteAdherences = [];
			teamAdherences = [];
			skillGroups = [];
			phoneStates = [];

			service.traceCalledForUserCode = null;
			service.stopCalled = false;
			service.clearCalled = false;
			tracedUsers = [];
			tracers = [];

			clearAll();
		};

		service.fake({
			name: 'configurationValidation',
			url: /\.\.\/Rta\/Configuration\/Validate/
		});

		service.fake({
			name: 'approvePeriod',
			url: /\.\.\/api\/HistoricalAdherence\/ApprovePeriod/
		});

		service.fake({
			name: 'removeApprovedPeriod',
			url: /\.\.\/api\/HistoricalAdherence\/RemoveApprovedPeriod/
		});

        service.fake({
            name: 'adjustPeriod',
            url: /\.\.\/api\/Adherence\/AdjustPeriod/
        });        
        
        service.fake({
            name: 'adjustedPeriods',
            url: /\.\.\/api\/Adherence\/AdjustedPeriods/
        });

		service.fake({
			name: 'removeAdjustedPeriod',
			url: /\.\.\/api\/Adherence\/RemoveAdjustedPeriod/
		});
		
		service.fake({
			name: 'historicalAdherence',
			url: /\.\.\/api\/HistoricalAdherence\/ForPerson(.*)/,
			clear: function () {
				return {}
			},
			add: function (data, item) {
				return item;
			}
		});

		service.fake({
			name: 'historicalOverview',
			url: /\.\.\/api\/HistoricalOverview\/Load(.*)/
		});

		service.fake({
			name: 'permissions',
			url: /\.\.\/api\/Adherence\/Permissions(.*)/,
			clear: function () {
				return {
					HistoricalOverview: true,
					ModifyAdherence: true,
                    AdjustAdherence: true
				};
			},
			add: function (data, item) {
				for (var key in item) {
					data[key] = item[key];
				}
				return data;
			}
		});

		service.fake(/\.\.\/api\/AgentStates\/Poll/,
			function (params) {
				service.lastAgentStatesRequestParams = params;
				params.siteIds = params.siteIds || [];
				params.teamIds = params.teamIds || [];
				params.skillIds = params.skillIds || [];
				params.excludedStateIds = params.excludedStateIds || [];
				params.inAlarm = params.inAlarm === 'true' || params.inAlarm === true;

				var result = agentStates;

				if (params.siteIds.length > 0)
					result = result.filter(function (a) {
						return params.siteIds.indexOf(a.SiteId) >= 0
					});
				else if (params.teamIds.length > 0)
					result = result.filter(function (a) {
						return params.teamIds.indexOf(a.TeamId) >= 0
					});

				if (params.skillIds.length > 0)
					result = result.filter(function (a) {
						return params.skillIds.indexOf(a.SkillId) >= 0
					});

				if (params.inAlarm) {
					result = result.filter(function (s) {
						return s.TimeInAlarm > 0;
					}).sort(function (s1, s2) {
						return s2.TimeInAlarm - s1.TimeInAlarm;
					});
				}

				if (params.excludedStateIds.length > 0)
					result = result.filter(function (s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});

				return [200, {
					Time: serverTime,
					States: result
				}];
			});

		service.fake(/\.\.\/api\/SkillArea\/For(.*)/,
			function (params) {
				var result = skillGroups
					.filter(function (s) {
						return params.skillAreaId === s.Id
					});
				return [200, result[0]];
			});

		service.fake(/\.\.\/api\/SkillGroups(.*)/,
			function () {
				return [200, skillGroups];
			});

		service.fake(/\.\.\/api\/Skills(.*)/,
			function () {
				return [200, skills];
			});

		service.fake(/\.\.\/api\/Sites\/OrganizationForSkills(.*)/,
			function (params) {
				var uniqueSiteIds = [];
				var returnOrg = [];
				var skillIds = angular.isArray(params.skillIds) ? params.skillIds : params.skillIds.split(",");
				skillIds.forEach(function (key) {
					// keeping behavior, although this doesnt seem correct either
					if (!sitesWithTeamsOnSkills[key])
						return;
					if (sitesWithTeamsOnSkills[key].length === 0)
						return;
					if (uniqueSiteIds.indexOf(sitesWithTeamsOnSkills[key][0].Id) < 0) {
						uniqueSiteIds = uniqueSiteIds.concat(sitesWithTeamsOnSkills[key][0].Id);
						returnOrg = returnOrg.concat(sitesWithTeamsOnSkills[key]);
					}
				});

				return [200, returnOrg];
			});

		service.fake(/\.\.\/api\/Sites\/Organization(.*)/,
			function () {
				return [200, sitesWithTeams];
			});

		service.fake(/\.\.\/api\/PhoneStates/,
			function (data) {
				return [200, phoneStates]
			});

		service.fake(/\.\.\/api\/Overview\/SiteCards(.*)/,
			function (params) {
				service.lastOverviewSiteCardsRequestParams = params;
				params.skillIds = params.skillIds || [];
				params.siteIds = params.siteIds || [];
				var sites = JSON.parse(JSON.stringify(siteAdherences));

				if (params.skillIds.length > 0)
					sites = sites.filter(function (sa) {
						return params.skillIds.indexOf(sa.SkillId) > -1;
					});

				if (params.siteIds.length > 0) {
					sites.forEach(function (site) {
						if (params.siteIds.indexOf(site.Id) > -1) {
							var teams = teamAdherences.filter(function (team) {
								return team.SiteId == site.Id;
							})
							if (params.skillIds.length > 0)
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
						.map(function (s) {
							return s.InAlarmCount
						})
						.reduce(function (s, v) {
							return s + v
						}, 0)
				}];
			});

		service.fake(/\.\.\/api\/Overview\/TeamCards(.*)/,
			function (params) {
				params.skillIds = params.skillIds || [];
				var result = teamAdherences;
				result = result.filter(function (ta) {
					return params.siteId == ta.SiteId;
				});
				if (params.skillIds.length > 0)
					result = result.filter(function (ta) {
						return params.skillIds.indexOf(ta.SkillId) > -1;
					});
				return [200, result];
			});


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

		function withSkillGroup(skillGroup) {
			skillGroups.push(skillGroup);
			return this;
		}

		function withSkillGroups(newSkillGroups) {
			newSkillGroups.forEach(function (e) {
				skillGroups.push(e)
			});
			return this;
		}

		function withPhoneState(phoneState) {
			phoneStates.push(phoneState);
			return this;
		}

		function withOrganization(siteWithTeams) {
			withSite(siteWithTeams);
			return this;
		}

		function clearOrganization() {
			sitesWithTeams = [];
			return this;
		}

		function withSite(site) {
			sitesWithTeams.push(site);
			return this;
		}

		function withTeam(team) {
			var site = sitesWithTeams.find(function (site) {
				return site.Id == team.SiteId;
			});
			if (site) {
				site.Teams = site.Teams || [];
				site.Teams.push(team);
			}
			return this;
		}

		function withOrganizationOnSkills(siteWithTeams, skillIds) {
			skillIds.split(",").forEach(function (key) {
				var skillIdAsAKey = key.trim();
				if (angular.isDefined(sitesWithTeamsOnSkills[skillIdAsAKey]))
					sitesWithTeamsOnSkills[skillIdAsAKey] = sitesWithTeamsOnSkills[skillIdAsAKey].concat(siteWithTeams);
				else
					sitesWithTeamsOnSkills[skillIdAsAKey] = [siteWithTeams];
			});
			return this;
		}

		service.withTracer = withTracer;
		service.withTracedUser = withTracedUser;

		var tracers = [];

		function withTracer(tracer) {
			tracers.push(tracer);
			return this;
		}

		var tracedUsers = [];

		function withTracedUser(tracedUser) {
			tracedUsers.push(tracedUser);
			return this;
		}

		service.fake(/\.\.\/api\/RtaTracer\/Traces(.*)/,
			function () {
				return [200, {
					Tracers: tracers,
					TracedUsers: tracedUsers
				}];
			});

		service.fake(/\.\.\/api\/RtaTracer\/Trace(.*)/,
			function (params) {
				service.traceCalledForUserCode = params.userCode;
				return [200];
			});

		service.fake(/\.\.\/api\/RtaTracer\/Stop(.*)/,
			function () {
				service.stopCalled = true;
				return [200];
			});

		service.fake(/\.\.\/api\/RtaTracer\/Clear(.*)/,
			function () {
				service.clearCalled = true;
				return [200];
			});


		return service;
	}
})();
