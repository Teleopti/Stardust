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
			var siteAdherencesForSkill = [];
			var teamAdherencesForSkill = [];
			var onlyAllowOrgAndSkillQueries = false;
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

			fake(/\.\.\/api\/Agents\/GetStatesForSitesAndSkills(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var statesForSitesAndSkills = statesForMultiple(params.siteIds, function(a) {
						return a.SiteId;
					}, params.skillIds, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(statesForSitesAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/GetStatesForTeamsAndSkills(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var statesForTeamsAndSkills = statesForMultiple(params.teamIds, function(a) {
						return a.TeamId;
					}, params.skillIds, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(statesForTeamsAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/ForSitesAndSkills(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var agentsFilteredBySkills = agentsIn(params.skillIds, function(a) {
						return a.SkillId;
					});
					var agentsFilteredBySitesAndSkills = resourceIn(params.siteIds, function(a) {
						return a.SiteId;
					}, agentsFilteredBySkills);
					return returnAgentsIfAllowed(agentsFilteredBySitesAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/ForTeamsAndSkills(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var agentsFilteredBySkills = agentsIn(params.skillIds, function(a) {
						return a.SkillId;
					});
					var agentsFilteredByTeamsAndSkills = resourceIn(params.teamIds, function(a) {
						return a.TeamId;
					}, agentsFilteredBySkills);
					return returnAgentsIfAllowed(agentsFilteredByTeamsAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeamsAndSkills\?(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var alarmStatesForTeamsAndSkills = alarmStatesForMultiple(params.teamIds, function(a) {
						return a.TeamId;
					}, params.skillIds, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(alarmStatesForTeamsAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSitesAndSkills\?(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var alarmStatesForSitesAndSkills = alarmStatesForMultiple(params.siteIds, function(a) {
						return a.SiteId;
					}, params.skillIds, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(alarmStatesForSitesAndSkills, true);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeamsAndSkillsExcludingStates(.*)/,
				function(params) {
					allowOrgAndSkillQueries();
					var alarmStatesForTeamsAndSkillsExcludingStates = alarmStatesForMultiple(params.teamIds, function(a) {
						return a.TeamId;
					}, params.skillIds, function(a) {
						return a.SkillId;
					})
					.filter(function(s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return returnStatesIfAllowed(alarmStatesForTeamsAndSkillsExcludingStates, true);
				});

				fake(/\.\.\/api\/Agents\/GetAlarmStatesForSitesAndSkillsExcludingStates(.*)/,
					function(params) {
						allowOrgAndSkillQueries();
						var alarmStatesForSitesAndSkillsExcludingStates = alarmStatesForMultiple(params.siteIds, function(a) {
							return a.SiteId;
						}, params.skillIds, function(a) {
							return a.SkillId;
						})
						.filter(function(s) {
							return params.excludedStateIds.indexOf(s.StateId) === -1;
						});
						return returnStatesIfAllowed(alarmStatesForSitesAndSkillsExcludingStates, true);
					});

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
					return returnAgentsIfAllowed(agentsIn(params.teamIds, function(a) {
						return a.TeamId
					}));
				});

			fake(/\.\.\/api\/Agents\/ForSites(.*)/,
				function(params) {
					return returnAgentsIfAllowed(agentsIn(params.siteIds, function(a) {
						return a.SiteId;
					}));
				});

			fake(/\.\.\/api\/Agents\/ForSkills(.*)/,
				function(params) {
					return returnAgentsIfAllowed(agentsIn(params.skillIds, function(a) {
						return a.SkillId;
					}));
				});

			function resourceIn(collection, map, resource) {
				return resource.filter(function(a) {
					return collection.indexOf(map(a)) >= 0
				});
			};

			function agentsIn(collection, map) {
				return agents.filter(function(a) {
					return collection.indexOf(map(a)) >= 0;
				});
			}

			function returnAgentsIfAllowed(collection, reset = false) {
				if (onlyAllowOrgAndSkillQueries)
					throw new Error("Only allowed to use org and skill query");
				onlyAllowOrgAndSkillQueries = reset;
				return [200, collection]
			}

			fake(/\.\.\/api\/Agents\/GetStatesForTeams(.*)/,
				function(params) {
					var statesForTeams = statesFor(params.ids, function(a) {
						return a.TeamId;
					});
					return returnStatesIfAllowed(statesForTeams);
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSites(.*)/,
				function(params) {
					var statesForSites = statesFor(params.ids, function(a) {
						return a.SiteId;
					});
					return returnStatesIfAllowed(statesForSites);
				});

			fake(/\.\.\/api\/Agents\/GetStatesForSkills(.*)/,
				function(params) {
					var statesForSkills = statesFor(params.ids, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(statesForSkills);
				});

			function statesFor(collection, map) {
				return states.filter(function(s) {
					var a = agents.find(function(a) {
						return a.PersonId === s.PersonId;
					});
					return a != null && collection.indexOf(map(a)) >= 0;
				});
			};

			function statesForMultiple(collection, map, collectionBySecondCriteria, mapForSecondCriteria) {
				return states.filter(function(s) {
					var a = agents.find(function(a) {
						return a.PersonId === s.PersonId;
					});
					return a != null && collection.indexOf(map(a)) >= 0 && collectionBySecondCriteria.indexOf(mapForSecondCriteria(a)) >= 0;
				});
			}



			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeams\?(.*)/,
				function(params) {
					var alarmStatesForTeams = alarmStatesFor(params.ids, function(a) {
						return a.TeamId;
					});
					return returnStatesIfAllowed(alarmStatesForTeams);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForTeamsExcludingStates(.*)/,
				function(params) {
					var alarmStatesForTeams = alarmStatesFor(params.ids, function(a) {
						return a.TeamId;
					}).filter(function(s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return returnStatesIfAllowed(alarmStatesForTeams);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSites\?(.*)/,
				function(params) {
					var alarmStatesForSites = alarmStatesFor(params.ids, function(a) {
						return a.SiteId;
					});
					return returnStatesIfAllowed(alarmStatesForSites);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSitesExcludingStates(.*)/,
				function(params) {
					var alarmStatesForSites = alarmStatesFor(params.ids, function(a) {
						return a.SiteId;
					}).filter(function(s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return returnStatesIfAllowed(alarmStatesForSites);
				});

			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSkills\?(.*)/,
				function(params) {
					var alarmStatesForSkills = alarmStatesFor(params.ids, function(a) {
						return a.SkillId;
					});
					return returnStatesIfAllowed(alarmStatesForSkills);
				});


			fake(/\.\.\/api\/Agents\/GetAlarmStatesForSkillsExcludingStates(.*)/,
				function(data) {
					var alarmStatesForSkills = alarmStatesFor(data.ids, function(a) {
						return a.SkillId;
					}).filter(function(s) {
						return data.excludedStateIds.indexOf(s.StateId) === -1;
					});
					return returnStatesIfAllowed(alarmStatesForSkills);
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

			function alarmStatesForMultiple(collection, map, collectionBySecondCriteria, mapForSecondCriteria) {
				return states.filter(function(s) {
					var a = agents.find(function(a) {
						return a.PersonId === s.PersonId;
					});
					return a != null && collection.indexOf(map(a)) >= 0 && collectionBySecondCriteria.indexOf(mapForSecondCriteria(a)) >= 0;
				}).filter(function(s) {
					return s.TimeInAlarm > 0;
				}).sort(function(s1, s2) {
					return s2.TimeInAlarm - s1.TimeInAlarm;
				});
			}

			function returnStatesIfAllowed(states, reset = false) {
				if (onlyAllowOrgAndSkillQueries)
					throw new Error("Only allowed to use org and skill query");
				onlyAllowOrgAndSkillQueries = reset;
				return [200, {
					Time: serverTime,
					States: states
				}];
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

			function sitesOrTeamsForSkillOrSkillArea(adherences, adherenceKey, paramId, sitesOrTeams) {
				var siteOrTeamIdsBySkillOrSkillAreaId = adherences.filter(function(a) {
					return paramId.indexOf(a[adherenceKey]) > -1;
				}).map(function(s) {
					return s.Id;
				});
				return sitesOrTeams.filter(function(st) {
					return siteOrTeamIdsBySkillOrSkillAreaId.indexOf(st.Id) > -1;
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

			fake(/\.\.\/api\/Sites\/ForSkills(.*)/,
				function(params) {
					var filteredSites = sitesOrTeamsForSkillOrSkillArea(siteAdherencesForSkill, 'SkillId', params.skillIds, sites);
					return [200, filteredSites];
				});

			fake(/\.\.\/api\/Teams\/ForSkills(.*)/,
				function(params) {
					var teamsBySite = teams.filter(function(t) {
						return params.siteId.indexOf(t.SiteId) > -1;
					});
					var filteredSites = sitesOrTeamsForSkillOrSkillArea(teamAdherencesForSkill, 'SkillId', params.skillIds, teamsBySite);
					return [200, filteredSites];
				});

			fake(/\.\.\/api\/Sites\/InAlarmCountForSkills(.*)/,
				function(params) {
					var adherenceBySiteId = {};
					var sAdherencesForMultipleSkills = [];
					var sAdherencesForSkill = siteAdherencesForSkill.filter(function(sa) {
						return params.skillIds.indexOf(sa.SkillId) > -1;
					});

					if (params.skillIds.length > 1) {
						sAdherencesForSkill.forEach(function(sas) {
							if (adherenceBySiteId[sas.Id] !== undefined)
								adherenceBySiteId[sas.Id] = adherenceBySiteId[sas.Id] + sas.OutOfAdherence;
							else
								adherenceBySiteId[sas.Id] = sas.OutOfAdherence;
							return 0;
						});

						for (var id in adherenceBySiteId) {
							sAdherencesForMultipleSkills.push({
								Id: id,
								OutOfAdherence: adherenceBySiteId[id]
							});
						}
						sAdherencesForSkill = sAdherencesForMultipleSkills;
					}
					return [200, sAdherencesForSkill];
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

			fake(/\.\.\/api\/Teams\/InAlarmCountForSkills(.*)/,
				function(params) {
					var adherenceByTeamId = {};
					var tAdherencesForMultipleSkills = [];

					var teamAdherencesBySkillId = teamAdherencesForSkill.filter(function(ta) {
						return params.skillIds.indexOf(ta.SkillId) > -1 && ta.SiteId === params.siteId;
					});

					if (params.skillIds.length > 1) {
						teamAdherencesBySkillId.forEach(function(tas) {
							if (adherenceByTeamId[tas.Id] !== undefined)
								adherenceByTeamId[tas.Id] = adherenceByTeamId[tas.Id] + tas.OutOfAdherence;
							else
								adherenceByTeamId[tas.Id] = tas.OutOfAdherence;
							return 0;
						});

						for (var id in adherenceByTeamId) {
							tAdherencesForMultipleSkills.push({
								Id: id,
								OutOfAdherence: adherenceByTeamId[id]
							});
						}
						teamAdherencesBySkillId = tAdherencesForMultipleSkills;
					}

					return [200, teamAdherencesBySkillId];
				});

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
				function(params) {
					var result = agents.find(function(agent) {
						return params.personId == agent.PersonId;
					});
					if (result != null)
						result.Now = serverTime;

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
				siteAdherencesForSkill = [];
				teamAdherencesForSkill = [];
				onlyAllowOrgAndSkillQueries = false;
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

			this.withSiteAdherenceForSkill = function(siteAdherenceForSkill) {
				siteAdherencesForSkill.push(siteAdherenceForSkill);
				return this;
			};

			this.clearSiteAdherences = function() {
				siteAdherences = [];
				return this;
			};

			this.clearSiteAdherencesForSkill = function() {
				siteAdherencesForSkill = [];
				return this;
			};

			this.clearTeamAdherencesForSkill = function() {
				teamAdherencesForSkill = [];
				return this;
			};

			this.withTeam = function(team) {
				teams.push(team);
				return this;
			};

			this.withTeamAdherenceForSkill = function(teamAdherenceForSkill) {
				teamAdherencesForSkill.push(teamAdherenceForSkill);
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

			// Yeeeaaaah...
			//  Cant make tests fail without banning use of other API points
			this.onlyAllowGettingStatesByCombiningOrganizationAndSkillQueries = function() {
				onlyAllowOrgAndSkillQueries = true;
				return this;
			}

			function allowOrgAndSkillQueries() {
				onlyAllowOrgAndSkillQueries = false;
			};

		});
})();
