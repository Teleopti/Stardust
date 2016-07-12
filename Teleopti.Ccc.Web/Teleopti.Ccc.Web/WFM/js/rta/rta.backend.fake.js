'use strict';
(function() {
	angular
		.module('wfm.rta')
		.service('FakeRtaBackend', function($httpBackend) {

			var serverTime = null;
			var toggles = {};
			var agents = [];
			var states = [];
			var adherences = [];
			var personDetails = [];
			var activityAdherences = [];
			var sites = [];
			var siteAdherences = [];
			var teams = [];
			var teamAdherences = [];
			var skills = [];
			var skillAreas = [];

			var paramsOf = function(url) {
				var result = {};
				var queryString = url.split("?")[1];
				if (queryString == null) {
					return result;
				}
				var params = queryString.split("&");
				angular.forEach(params, function(t) {
					var kvp = t.split("=");
					if (result[kvp[0]] != null)
						result[kvp[0]] = [].concat(result[kvp[0]], kvp[1]);
					else
						result[kvp[0]] = kvp[1];
				});
				return result;
			};

			var fake = function(url, response) {
				$httpBackend.whenGET(url)
					.respond(function(method, url, data, headers, params) {
						var params2 = paramsOf(url);
						return response(params2, method, url, data, headers, params);
					});
			};


			fake(/\.\.\/api\/Skills\/NameFor(.*)/,
			function (params) {
				var result = skills
				.filter(function(s) { return params.skillId === s.Id })
				.map(function (s) { return s.Name; });
				return [200, { Name: result[0] }];
			});

			fake(/\.\.\/api\/SkillAreas(.*)/,
				function () {
					return [200, skillAreas];
				});

			fake(/\.\.\/api\/Skills(.*)/,
				function () {
					return [200, skills];
				});
			

			fake(/\.\.\/api\/Adherence\/ForToday(.*)/,
				function(params) {
					var result = adherences.find(function(a) {
						return a.PersonId === params.personId;
					});
					return [200, result];
				});

			fake(/\.\.\/api\/Agents\/ForTeams(.*)/,
				function(params) {
					return [200, agents.filter(function(a) { return params.teamIds.indexOf(a.TeamId) >= 0; })];
				});

			fake(/\.\.\/api\/Agents\/ForSites(.*)/,
				function (params) {
					return [200, agents.filter(function(a) { return params.siteIds.indexOf(a.SiteId) >= 0; })];
				});

			fake(/\.\.\/api\/Agents\/ForSkill(.*)/,
				function (params) {
					return [200, agents.filter(function (a) { return params.skillId === a.SkillId; })];
				});

			fake(/\.\.\/api\/Agents\/GetStatesForTeams(.*)/,
				function (params) {
					var result = states.filter(function(s) {
						var a = agents.find(function(a) { return a.PersonId === s.PersonId; });
						return a != null && params.ids.indexOf(a.TeamId) >= 0;
					});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeams(.*)/,
				function (params) {
					var result =
						states.filter(function (s) {
							var a = agents.find(function (a) { return a.PersonId === s.PersonId; });
							return a != null && params.ids.indexOf(a.TeamId) >= 0;
						}).filter(function (s) {
							return s.TimeInAlarm > 0;
						}).sort(function (s1, s2) {
							return s2.TimeInAlarm - s1.TimeInAlarm;
						});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSites(.*)/,
				function (params) {
					var result = states.filter(function(s) {
							var a = agents.find(function(a) { return a.PersonId === s.PersonId; });
							return a != null && params.ids.indexOf(a.SiteId) >= 0;
						});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSites(.*)/,
				function (params) {
					var result =
						states.filter(function(s) {
							var a = agents.find(function(a) { return a.PersonId === s.PersonId; });
							return a != null && params.ids.indexOf(a.SiteId) >= 0;
						}).filter(function(s) {
							return s.TimeInAlarm > 0;
						}).sort(function(s1, s2) {
							return s2.TimeInAlarm - s1.TimeInAlarm;
						});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSkill(.*)/,
				function (params) {
					var result = states.filter(function (s) {
						var a = agents.find(function (a) { return a.PersonId === s.PersonId; });
						return a != null && params.skillId === a.SkillId;
					});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSkill(.*)/,
				function (params) {
					var result =
						states.filter(function (s) {
							var a = agents.find(function (a) { return a.PersonId === s.PersonId; });
							return a != null && params.skillId === a.SkillId;
						}).filter(function (s) {
							return s.TimeInAlarm > 0;
						}).sort(function (s1, s2) {
							return s2.TimeInAlarm - s1.TimeInAlarm;
						});
					return [200, { Time: serverTime, States: result }];
				});

			fake(/ToggleHandler\/AllToggles(.*)/,
				function (params) {
					return [200, toggles];
				});

			fake(/\.\.\/api\/Agents\/PersonDetails(.*)/,
				function (params) {
					return [200, personDetails.find(function(p) { return p.PersonId === params.personId })];
				});

			fake(/\.\.\/api\/Adherence\/ForDetails(.*)/,
				function (params) {
					return [200, activityAdherences.filter(function(a) { return a.PersonId === params.personId })];
				});

			fake(/\.\.\/api\/Sites$/,
				function (params) {
					return [200, sites];
				});

			fake(/\.\.\/api\/Sites\/GetOutOfAdherenceForAllSites(.*)/,
				function (params) {
					return [200, siteAdherences];
				});

			fake(/\.\.\/api\/Teams\/Build(.*)/,
				function (params) {
					return [200, teams.filter(function (team) { return team.SiteId === params.siteId; })];
				});

			fake(/\.\.\/api\/Teams\/GetOutOfAdherenceForTeamsOnSite(.*)/,
				function (params) {
					var result =
						teamAdherences.filter(function(ta) {
							var t = teams.find(function (team) { return team.Id === ta.Id; });
							return t != null && params.siteId === t.SiteId;
						});
					return [200, result];
				});

			this.clear = function () {
				serverTime = null;
				toggles = {};
				agents = [];
				states = [];
				adherences = [];
				personDetails = [];
				activityAdherences = [];
				sites = [];
				siteAdherences = [];
				teams = [];
				teamAdherences = [];
				skillAreas = [];
			}

			this.withToggle = function(toggle) {
				toggles[toggle] = true;
				return this;
			}

			this.withTime = function (time) {
				serverTime = time;
				return this;
			};

			this.withAgent = function(agent) {
				agents.push(agent);
				return this;
			};

			this.clearStates = function() {
				states = [];
				return this;
			};

			this.withState = function(state) {
				states.push(state);
				return this;
			};

			this.withAdherence = function(adherence) {
				adherences.push(adherence);
				return this;
			}

			this.withPersonDetails = function(personDetail) {
				personDetails.push(personDetail);
				return this;
			};

			this.withActivityAdherence = function(activityAdherence) {
				activityAdherences.push(activityAdherence);
				return this;
			}

			this.withSite = function (site) {
				sites.push(site);
				return this;
			};

			this.withSiteAdherence = function (siteAdherence) {
				siteAdherences.push(siteAdherence);
				return this;
			};

			this.clearSiteAdherences = function () {
				siteAdherences = [];
				return this;
			};

			this.withTeam = function(team) {
				teams.push(team);
				return this;
			};

			this.withTeamAdherence = function(teamAdherence) {
				teamAdherences.push(teamAdherence);
				return this;
			};

			this.clearTeamAdherences = function() {
				teamAdherences = [];
				return this;
			};

			this.withSkill = function (skill) {
				skills.push(skill);
				return this;
			}

			this.withSkills = function (newSkills) {
				skills = skills.concat(newSkills);
				return this;
			}

			this.withSkillAreas = function (newSkillAreas) {
				skillAreas = newSkillAreas;
				return this;
			}
			
		});
})();
