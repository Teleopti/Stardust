'use strict';
(function() {
	angular
		.module('wfm.rta')
		.service('FakeRtaBackend', function($httpBackend) {

			var agents = [];
			var states = [];
			var adherences = [];
			var personDetails = [];
			var activityAdherences = [];
			var sites = [];
			var siteAdherences = [];
			var teams = [];
			var teamAdherences = [];

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

			$httpBackend.whenGET2 = function(url) {
				var r = $httpBackend.whenGET(url);
				return {
					respond: function(fn) {
						return r.respond(function(method, url, data, headers, params) {
							var params2 = paramsOf(url);
							return fn(params2, method, url, data, headers, params);
						});
					}
				}
			};

			$httpBackend.whenGET2(/\.\.\/api\/Adherence\/ForToday(.*)/)
				.respond(function(params) {
					var result = adherences.find(function(a) {
						return a.PersonId === params.personId;
					});
					return [200, result];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Agents\/ForTeams(.*)/)
				.respond(function(params) {
					return [200, agents.filter(function(a) { return params.teamIds.indexOf(a.TeamId) >= 0; })];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Agents\/ForSites(.*)/)
				.respond(function(params) {
					return [200, agents.filter(function(a) { return params.siteIds.indexOf(a.SiteId) >= 0; })];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Agents\/GetStatesForTeams(.*)/)
				.respond(function(params) {
					params.inAlarmOnly = params.inAlarmOnly || false;
					var result =
						states.filter(function(s) {
							var a = agents.find(function(a) { return a.PersonId === s.PersonId; });
							return a != null && params.ids.indexOf(a.TeamId) >= 0;
						}).filter(function(s) {
							return !params.inAlarmOnly || s.TimeInAlarm > 0;
						}).sort(function(s1, s2) {
							if (params.alarmTimeDesc)
								return s1.TimeInAlarm - s2.TimeInAlarm;
							return 0;
						});
					return [200, result];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Agents\/GetStatesForSites(.*)/)
				.respond(function(params) {
					params.inAlarmOnly = params.inAlarmOnly || false;
					var result =
						states.filter(function(s) {
							var a = agents.find(function(a) { return a.PersonId === s.PersonId; });
							return a != null && params.ids.indexOf(a.SiteId) >= 0;
						}).filter(function(s) {
							return !params.inAlarmOnly || s.TimeInAlarm > 0;
						}).sort(function(s1, s2) {
							if (params.alarmTimeDesc)
								return s1.TimeInAlarm - s2.TimeInAlarm;
							return 0;
						});
					return [200, result];
				});

			$httpBackend.whenGET2(/ToggleHandler\/(.*)/)
				.respond(function(params) {
					return [200, { IsEnabled: false }];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Agents\/PersonDetails(.*)/)
				.respond(function (params) {
					return [200, personDetails.find(function(p) { return p.PersonId === params.personId })];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Adherence\/ForDetails(.*)/)
				.respond(function(params) {
					return [200, activityAdherences.filter(function(a) { return a.PersonId === params.personId })];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Sites$/)
				.respond(function(params) {
					return [200, sites];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Sites\/GetOutOfAdherenceForAllSites(.*)/)
				.respond(function (params) {
					return [200, siteAdherences];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Teams\/ForSite(.*)/)
				.respond(function (params) {
					return [200, teams.filter(function (team) { return team.SiteId === params.siteId; })];
				});

			$httpBackend.whenGET2(/\.\.\/api\/Teams\/GetOutOfAdherenceForTeamsOnSite(.*)/)
				.respond(function (params) {
					var result =
						teamAdherences.filter(function(ta) {
							var t = teams.find(function (team) { return team.Id === ta.Id; });
							return t != null && params.siteId === t.SiteId;
						});
					return [200, result];
				});

			this.clear = function() {
				agents = [];
				states = [];
				adherences = [];
				personDetails = [];
				activityAdherences = [];
				sites = [];
				siteAdherences = [];
				teams = [];
				teamAdherences = [];
			}

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

		});
})();