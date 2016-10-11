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
			var phoneStates = [];
			var siteAdherencesForSkillArea = [];
			var teamAdherencesForSkillArea =[];
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
				function(params) {
					var result = skills
						.filter(function(s) {
							return params.skillId === s.Id
						})
						.map(function(s) {
							return s.Name;
						});
					return [200, {
						Name: result[0]
					}];
				});

			fake(/\.\.\/api\/SkillArea\/For(.*)/,
				function(params) {
					var result = skillAreas
						.filter(function(s) {
							return params.skillAreaId === s.Id
						});
					return [200, result[0]];
				});

			fake(/\.\.\/api\/SkillAreas(.*)/,
				function() {
					return [200, {
						SkillAreas: skillAreas
					}];
				});

			fake(/\.\.\/api\/Skills(.*)/,
				function() {
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
					return [200, agentsIn(params.teamIds, function(a) {
						return a.TeamId
					})];
				});

			fake(/\.\.\/api\/Agents\/ForSites(.*)/,
				function(params) {
					return [200, agentsIn(params.siteIds, function(a) {
						return a.SiteId;
					})];
				});

			fake(/\.\.\/api\/Agents\/ForSkills(.*)/,
				function(params) {
					return [200, agentsIn(params.skillIds, function(a) {
						return a.SkillId;
					})];
				});

			function agentsIn(collection, map) {
				return agents.filter(function(a) {
					return collection.indexOf(map(a)) >= 0
				});
			}


			fake(/\.\.\/api\/Agents\/GetStatesForTeams(.*)/,
				function(params) {
					var statesForTeams = statesFor(params.ids, function(a) {
						return a.TeamId;
					});
					return [200, {
						Time: serverTime,
						States: statesForTeams
					}];
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSites(.*)/,
				function(params) {
					var statesForSites = statesFor(params.ids, function(a) {
						return a.SiteId;
					});
					return [200, {
						Time: serverTime,
						States: statesForSites
					}];
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSkills(.*)/,
				function(params) {
					var statesForSkills = statesFor(params.ids, function(a) {
						return a.SkillId;
					});
					return [200, {
						Time: serverTime,
						States: statesForSkills
					}];
				});

			function statesFor(collection, map) {
				return states.filter(function(s) {
					var a = agents.find(function(a) {
						return a.PersonId === s.PersonId;
					});
					return a != null && collection.indexOf(map(a)) >= 0;
				});
			}

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeams\?(.*)/,
				function(params) {
					var alarmStatesForTeams = alarmStatesFor(params.ids, function(a) {
						return a.TeamId;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForTeams
					}];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeamsExcludingStates(.*)/,
				function(params) {
					var alarmStatesForTeams = alarmStatesFor(params.ids, function(a) {
						return a.TeamId;
					}).filter(function(s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForTeams
					}];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSites\?(.*)/,
				function(params) {
					var alarmStatesForSites = alarmStatesFor(params.ids, function(a) {
						return a.SiteId;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForSites
					}];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSitesExcludingStates(.*)/,
				function(params) {
					var alarmStatesForSites = alarmStatesFor(params.ids, function(a) {
						return a.SiteId;
					}).filter(function(s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForSites
					}];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSkills\?(.*)/,
				function(params) {
					var alarmStatesForSkills = alarmStatesFor(params.ids, function(a) {
						return a.SkillId;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForSkills
					}];
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSkillsExcludingStates(.*)/,
				function(data) {
					var alarmStatesForSkills = alarmStatesFor(data.ids, function(a) {
						return a.SkillId;
					}).filter(function(s) {
						return data.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return [200, {
						Time: serverTime,
						States: alarmStatesForSkills
					}];
				});

			function alarmStatesFor(collection, map) {
				return states.filter(function(s) {
					var a = agents.find(function(a) {
						return a.PersonId === s.PersonId;
					});
					return a != null && collection.indexOf(map(a)) >= 0;
				}).filter(function(s) {
					return s.TimeInAlarm > 0;
				}).sort(function(s1, s2) {
					return s2.TimeInAlarm - s1.TimeInAlarm;
				});
			}

			fake(/\.\.\/api\/PhoneState\/InfoFor(.*)/,
				function(data) {
					if (data.ids.indexOf(null) > -1 || data.ids.indexOf("noState") > -1)
						throw new Error('Nope, dont ask server for that')

					var result = phoneStates.filter(function(s) {
						return data.ids.indexOf(s.Id) > -1
					});

					if (result.length === 0) {
						result = states
							.filter(function(s) {
								if (data.ids.indexOf(s.StateId) > -1)
									return true;
							})
							.map(function(s) {
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

			fake(/ToggleHandler\/AllToggles(.*)/,
				function(params) {
					return [200, toggles];
				});

			fake(/\.\.\/api\/Agents\/PersonDetails(.*)/,
				function(params) {
					return [200, personDetails.find(function(p) {
						return p.PersonId === params.personId;
					})];
				});

			fake(/\.\.\/api\/Adherence\/ForDetails(.*)/,
				function(params) {
					return [200, activityAdherences.filter(function(a) {
						return a.PersonId === params.personId;
					})];
				});

			fake(/\.\.\/api\/Sites$/,
				function(params) {
					return [200, sites];
				});
			function sitesOrTeamsForSkillOrSkillArea(adherences,adherenceKey,paramId,sitesOrTeams){
				var siteOrTeamIdsBySkillOrSkillAreaId = adherences.filter(function(a){
						return a[adherenceKey] ===  paramId;
					}).map(function(s){
				return sitesOrTeams.filter(function(s){
					return siteOrTeamIdsBySkillOrSkillAreaId.indexOf(s.Id) >-1;
				});
					var filteredSites = sitesOrTeamsForSkillOrSkillArea(siteAdherencesForSkillArea,'SkillAreaId',params.skillAreaId,sites);
					var filteredSites = sitesOrTeamsForSkillOrSkillArea(siteAdherencesForSkill,'SkillId',params.skillId,sites);
				function(params) {	
					var teamsBySites = teams.filter(function(t){
						return params.siteIds.indexOf(t.SiteId) > -1;
					var filteredTeams = sitesOrTeamsForSkillOrSkillArea(teamAdherencesForSkillArea,'SkillAreaId',params.skillAreaId,teamsBySites);
					var teamsBySites = teams.filter(function(t){
						return params.siteIds.indexOf(t.SiteId) > -1;
					var filteredTeams = sitesOrTeamsForSkillOrSkillArea(teamAdherencesForSkill,'SkillId',params.skillId,teamsBySites);
					var siteAdherencesBySkillAreaId = siteAdherencesForSkillArea.filter(function(sa){
						return sa.SkillAreaId ===  params.skillAreaId;
					});
					return [200, siteAdherencesBySkillAreaId];
					var siteAdherencesBySkillId = siteAdherencesForSkill.filter(function(s){
						return s.SkillId ===  params.skillId;
					});
					return [200, siteAdherencesBySkillId];

			function objToArray(obj) {
				return Object.keys(obj).map(function(key) {
					return obj[key];
				});
			}

			function skillIdsFromTheSkillArea(skillAreacollection, skillAreaId) {
				return skillAreacollection.find(function(sa) {
						return sa.Id === skillAreaId;
					})
					.Skills
					.map(function(s) {
						return s.Id;
					});
			}

			fake(/\.\.\/api\/SitesForSkillArea(.*)/,
				function(params) {
					var skillIdsForSelectedSkillArea = skillIdsFromTheSkillArea(skillAreas, params.skillAreaId);
					var filteredSiteIds = getSiteOrTeamAdherenceForSkillOrSkillArea(siteAdherencesForSkill, skillIdsForSelectedSkillArea)
						.map(function(fsa) {
							return fsa.Id;
						});
					var filteredSites = sites.filter(function(s) {
						return filteredSiteIds.indexOf(s.Id) > -1;
					}).map(function(s) {
						return {
							Id: s.Id,
							Name: s.Name
						};
					});
					return [200, filteredSites];
				});

			fake(/\.\.\/api\/SitesForSkill(.*)/,
				function(params) {
					var filteredSiteIds = getSiteOrTeamAdherenceForSkillOrSkillArea(siteAdherencesForSkill, params.skillId)
						.map(function(fsa) {
							return fsa.Id;
						});
					var filteredSites = sites.filter(function(s) {
						return filteredSiteIds.indexOf(s.Id) > -1;
					}).map(function(s) {
						return {
							Id: s.Id,
							Name: s.Name
						};
					});
					return [200, filteredSites];
				});
			fake(/\.\.\/api\/TeamsForSitesAndSkillArea(.*)/,
				function(params) {
					var skillIdsForSelectedSkillArea = skillIdsFromTheSkillArea(skillAreas, params.skillAreaId);
					var teamIdsBySite = teams.filter(function(t) {
						return t.SiteId === params.siteIds;
					}).map(function(t) {
						return t.Id;
					});

					var teamAdherencesForSelectedSite = teamAdherencesForSkill.filter(function(ta) {
						return teamIdsBySite.indexOf(ta.Id) > -1;
					});

					var filteredTeamIds = getSiteOrTeamAdherenceForSkillOrSkillArea(teamAdherencesForSelectedSite, skillIdsForSelectedSkillArea)
						.map(function(fsa) {
							return fsa.Id;
						});
					var filteredTeams = teams.filter(function(t) {
						return filteredTeamIds.indexOf(t.Id) > -1;
					}).map(function(t) {
						return {
							Id: t.Id,
							Name: t.Name
						};
					});
					return [200, filteredTeams];
				});

			fake(/\.\.\/api\/TeamsForSitesAndSkill(.*)/,
				function(params) {

					var teamIdsBySite = teams.filter(function(t) {
						return t.SiteId === params.siteIds;
					}).map(function(t) {
						return t.Id;
					});

					var teamAdherencesForSelectedSite = teamAdherencesForSkill.filter(function(ta) {
						return teamIdsBySite.indexOf(ta.Id) > -1;
					});

					var filteredTeamIds = getSiteOrTeamAdherenceForSkillOrSkillArea(teamAdherencesForSelectedSite, params.skillId)
						.map(function(fta) {
							return fta.Id;
						});

					var filteredTeams = teams.filter(function(t) {
						return filteredTeamIds.indexOf(t.Id) > -1;
					}).map(function(t) {
						return {
							Id: t.Id,
							Name: t.Name
						};
					});
					return [200, filteredTeams];
				});


			fake(/\.\.\/api\/Sites\/GetOutOfAdherenceForAllSitesBySkillArea(.*)/,
				function(params) {
					var skillIdsForSelectedSkillArea = skillIdsFromTheSkillArea(skillAreas, params.skillAreaId);
					return [200, getSiteOrTeamAdherenceForSkillOrSkillArea(siteAdherencesForSkill, skillIdsForSelectedSkillArea)];
				});
			fake(/\.\.\/api\/Sites\/GetOutOfAdherenceForAllSitesBySkill(.*)/,
				function(params) {
					return [200, getSiteOrTeamAdherenceForSkillOrSkillArea(siteAdherencesForSkill, params.skillId)];
				});


			fake(/\.\.\/api\/Sites\/GetOutOfAdherenceForAllSites(.*)/,
				function(params) {
					return [200, siteAdherences];
				});

			fake(/\.\.\/api\/Teams\/Build(.*)/,
				function(params) {
					return [200, teams.filter(function(team) {
						return team.SiteId === params.siteId;
					})];
				});
			fake(/\.\.\/api\/Teams\/GetOutOfAdherenceForAllTeamsOnSitesBySkillArea(.*)/,
					var teamAdherencesBySkillAreaId = teamAdherencesForSkillArea.filter(function(sa){
						return sa.SkillAreaId ===  params.skillAreaId && sa.Id ===  params.teamIds;
					});
					return [200, teamAdherencesBySkillAreaId];
			});
					var teamAdherencesBySkillId = teamAdherencesForSkill.filter(function(s){
						return s.SkillId ===  params.skillId && s.Id ===  params.teamIds;
					});
					return [200, teamAdherencesBySkillId];
			fake(/\.\.\/api\/Teams\/GetOutOfAdherenceForTeamsOnSite(.*)/,
				function(params) {
					var result =
						teamAdherences.filter(function(ta) {
							var t = teams.find(function(team) {
								return team.Id === ta.Id;
							});
							return t != null && params.siteId === t.SiteId;
						});
					return [200, result];
				});

			fake(/\.\.\/api\/HistoricalAdherence\/For(.*)/,
				function (params) {
					var result = agents.find(function(agent){
						return params.personId == agent.PersonId;
					});
					if (result != null)
						result.Date = serverTime;

					return [200, result];
				});


			this.clear = function() {
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
				phoneStates = [];
				siteAdherencesForSkillArea =[];
				teamAdherencesForSkillArea = [];
			}

			this.withToggle = function(toggle) {
				toggles[toggle] = true;
				return this;
			}

			this.withTime = function(time) {
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

			this.withSite = function(site) {
				sites.push(site);
				return this;
			};

			this.withSiteAdherence = function(siteAdherence) {
				siteAdherences.push(siteAdherence);
				return this;
			};

			this.withSiteAdherenceForSkillArea = function(siteAdherenceForSkillArea) {
				siteAdherencesForSkillArea.push(siteAdherenceForSkillArea);
				return this;
			};

			this.clearSiteAdherences = function() {
				siteAdherences = [];
				return this;
			};

			this.withTeam = function(team) {
				teams.push(team);
				return this;
			};

				return this;
			};
			
			this.withTeamAdherenceForSkillArea = function(teamAdherenceForSkillArea) {
				teamAdherencesForSkillArea.push(teamAdherenceForSkillArea);
			this.withTeamAdherence = function(teamAdherence) {
				teamAdherences.push(teamAdherence);
				return this;
			};

			this.clearTeamAdherences = function() {
				teamAdherences = [];
				return this;
			};

			this.withSkill = function(skill) {
				skills.push(skill);
				return this;
			}

			this.withSkills = function(newSkills) {
				skills = skills.concat(newSkills);
				return this;
			}

			this.withSkillAreas = function(newSkillAreas) {
				skillAreas = newSkillAreas;
				return this;
			}

			this.withPhoneState = function(phoneState) {
				phoneStates.push(phoneState)
				return this;
			}

		});
})();
