'use strict';
(function () {
	angular
		.module('wfm.rta')
		.factory('FakeRtaBackend', fakeRtaBackend);

	fakeRtaBackend.$inject = ['$httpBackend'];

	function fakeRtaBackend($httpBackend) {

		var service = {
			clear: clear,
			withToggle: withToggle,
			withTime: withTime,
			withAgentState: withAgentState,
			withAgent: withAgent,
			clearStates: clearStates,
			clearAgentStates: clearAgentStates,
			withState: withState,
			withAdherence: withAdherence,
			withPersonDetails: withPersonDetails,
			withActivityAdherence: withActivityAdherence,
			withSite: withSite,
			withSiteAdherence: withSiteAdherence,
			withSiteAdherenceForSkill: withSiteAdherenceForSkill,
			clearSiteAdherences: clearSiteAdherences,
			clearSiteAdherencesForSkill: clearSiteAdherencesForSkill,
			clearTeamAdherencesForSkill: clearTeamAdherencesForSkill,
			withTeam: withTeam,
			withTeamAdherenceForSkill: withTeamAdherenceForSkill,
			withTeamAdherence: withTeamAdherence,
			clearTeamAdherences: clearTeamAdherences,
			withSkill: withSkill,
			withSkills: withSkills,
			withSkillAreas: withSkillAreas,
			withPhoneState: withPhoneState,
			withOrganization: withOrganization,
			withOrganizationOnSkills: withOrganizationOnSkills
		};

		///////////////////////////////

		var serverTime = null;
		var toggles = {};
		var agents = [];
		var states = [];
		var agentStates = [];
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
		var organizations = [];
		var organizationsOnSkills = [];
		var siteAdherencesForSkill = [];
		var teamAdherencesForSkill = [];
		var paramsOf = function (url) {
			var result = {};
			var queryString = url.split("?")[1];
			if (queryString == null) {
				return result;
			}
			var params = queryString.split("&");
			angular.forEach(params, function (t) {
				var kvp = t.split("=");
				if (result[kvp[0]] != null)
					result[kvp[0]] = [].concat(result[kvp[0]], kvp[1]);
				else
					result[kvp[0]] = kvp[1];
			});
			return result;
		};

		var fake = function (url, response) {
			$httpBackend.whenGET(url)
				.respond(function (method, url, data, headers, params) {
					var params2 = paramsOf(url);
					return response(params2, method, url, data, headers, params);
				});
		};

		fake(/\.\.\/api\/Agents\/For(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null && params.skillIds != null)
						return agents.filter(function (a) {
							return params.skillIds.indexOf(a.SkillId) >= 0
						}).filter(function (a) {
							return params.siteIds.indexOf(a.SiteId) >= 0
						});
					if (params.siteIds != null)
						return agents.filter(function (a) {
							return params.siteIds.indexOf(a.SiteId) >= 0
						});
					if (params.teamIds != null && params.skillIds != null)
						return agents.filter(function (a) {
							return params.skillIds.indexOf(a.SkillId) >= 0
						}).filter(function (a) {
							return params.teamIds.indexOf(a.TeamId) >= 0
						});
					if (params.teamIds != null)
						return agents.filter(function (a) {
							return params.teamIds.indexOf(a.TeamId) >= 0
						});
					return agents.filter(function (a) {
						return params.skillIds.indexOf(a.SkillId) >= 0
					});
				})();
				return [200, ret];
			});

		fake(/\.\.\/api\/AgentStates\/For(.*)/,
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
				return [200, {
					Time: serverTime,
					States: ret
				}];
			});

		fake(/\.\.\/api\/AgentStates\/InAlarmFor\?(.*)/,
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
				return [200, {
					Time: serverTime,
					States: agentStatesInAlarm(ret)
				}];
			});

		function agentStatesInAlarm(collection) {
			return collection.filter(function (s) {
				return s.TimeInAlarm > 0;
			}).sort(function (s1, s2) {
				return s2.TimeInAlarm - s1.TimeInAlarm;
			});
		}

		fake(/\.\.\/api\/AgentStates\/InAlarmExcludingPhoneStatesFor\?(.*)/,
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
				return [200, {
					Time: serverTime,
					States: agentStatesInAlarm(ret).filter(function (s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					})
				}];
			});



		fake(/\.\.\/api\/Agents\/StatesFor(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null && params.skillIds != null)
						return statesForMultiple(params.siteIds, function (a) {
							return a.SiteId;
						}, params.skillIds, function (a) {
							return a.SkillId;
						});
					if (params.siteIds != null)
						return statesFor(params.siteIds, function (a) {
							return a.SiteId;
						});
					if (params.teamIds != null && params.skillIds != null)
						return statesForMultiple(params.teamIds, function (a) {
							return a.TeamId;
						}, params.skillIds, function (a) {
							return a.SkillId;
						});
					if (params.teamIds != null)
						return statesFor(params.teamIds, function (a) {
							return a.TeamId;
						});
					return statesFor(params.skillIds, function (a) {
						return a.SkillId;
					});
				})();
				return [200, {
					Time: serverTime,
					States: ret
				}];
			});

		function statesFor(collection, map) {
			return states.filter(function (s) {
				var a = agents.find(function (a) {
					return a.PersonId === s.PersonId;
				});
				return a != null && collection.indexOf(map(a)) >= 0;
			});
		};

		function statesForMultiple(collection1, map1, collection2, map2) {
			return states.filter(function (s) {
				var a = agents.find(function (a) {
					return a.PersonId === s.PersonId;
				});
				return a != null && collection1.indexOf(map1(a)) >= 0 && collection2.indexOf(map2(a)) >= 0;
			});
		}

		fake(/\.\.\/api\/Agents\/InAlarmFor\?(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null && params.skillIds != null)
						return alarmStatesForMultiple(params.siteIds, function (a) {
							return a.SiteId;
						}, params.skillIds, function (a) {
							return a.SkillId;
						});
					if (params.siteIds != null)
						return alarmStatesFor(params.siteIds, function (a) {
							return a.SiteId;
						});
					if (params.teamIds != null && params.skillIds != null)
						return alarmStatesForMultiple(params.teamIds, function (a) {
							return a.TeamId;
						}, params.skillIds, function (a) {
							return a.SkillId;
						});
					if (params.teamIds != null)
						return alarmStatesFor(params.teamIds, function (a) {
							return a.TeamId;
						});
					return alarmStatesFor(params.skillIds, function (a) {
						return a.SkillId;
					});
				})();
				return [200, {
					Time: serverTime,
					States: ret
				}];
			});

		fake(/\.\.\/api\/Agents\/InAlarmExcludingPhoneStatesFor(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null && params.skillIds != null)
						return alarmStatesForMultiple(
							params.siteIds,
							function (a) { return a.SiteId; },
							params.skillIds,
							function (a) { return a.SkillId; }
						).filter(function (s) {
							return params.excludedStateIds.indexOf(s.StateId) === -1;
						});
					if (params.siteIds != null)
						return alarmStatesFor(
							params.siteIds,
							function (a) { return a.SiteId; }
						).filter(function (s) {
							return params.excludedStateIds.indexOf(s.StateId) === -1;
						});
					if (params.teamIds != null && params.skillIds != null)
						return alarmStatesForMultiple(
							params.teamIds,
							function (a) { return a.TeamId; },
							params.skillIds,
							function (a) { return a.SkillId; }
						).filter(function (s) {
							return params.excludedStateIds.indexOf(s.StateId) === -1;
						});
					if (params.teamIds != null)
						return alarmStatesFor(
							params.teamIds,
							function (a) { return a.TeamId; }
						).filter(function (s) {
							return params.excludedStateIds.indexOf(s.StateId) === -1;
						});
					return alarmStatesFor(
						params.skillIds,
						function (a) { return a.SkillId; }
					).filter(function (s) {
						return params.excludedStateIds.indexOf(s.StateId) === -1;
					});
				})();
				return [200, {
					Time: serverTime,
					States: ret
				}];
			});

		function alarmStatesFor(collection, map) {
			return statesFor(collection, map)
				.filter(function (s) {
					return s.TimeInAlarm > 0;
				}).sort(function (s1, s2) {
					return s2.TimeInAlarm - s1.TimeInAlarm;
				});
		}

		function alarmStatesFor2(collection, map) {
			return statesFor(collection, map)
				.filter(function (s) {
					return s.TimeInAlarm > 0;
				}).sort(function (s1, s2) {
					return s2.TimeInAlarm - s1.TimeInAlarm;
				});
		}

		function alarmStatesForMultiple(collection1, map1, collection2, map2) {
			return statesForMultiple(collection1, map1, collection2, map2)
				.filter(function (s) {
					return s.TimeInAlarm > 0;
				}).sort(function (s1, s2) {
					return s2.TimeInAlarm - s1.TimeInAlarm;
				});
		}

		fake(/\.\.\/api\/Skills\/NameFor(.*)/,
			function (params) {
				var result = skills
					.filter(function (s) {
						return params.skillId === s.Id
					})
					.map(function (s) {
						return s.Name;
					});
				return [200, {
					Name: result[0]
				}];
			});

		fake(/\.\.\/api\/SkillArea\/For(.*)/,
			function (params) {
				var result = skillAreas
					.filter(function (s) {
						return params.skillAreaId === s.Id
					});
				return [200, result[0]];
			});

		fake(/\.\.\/api\/SkillAreas(.*)/,
			function () {
				return [200, {
					SkillAreas: skillAreas
				}];
			});

		fake(/\.\.\/api\/Skills(.*)/,
			function () {
				return [200, skills];
			});

		fake(/\.\.\/api\/Sites\/OrganizationForSkills(.*)/,
			function (params) {
				var returnOrg = [];	
				var skillIdsArray = angular.isArray(params.skillIds) ? params.skillIds :params.skillIds.split(",");
				skillIdsArray.forEach(function(key){
					if(returnOrg.length > 0)
						return;
					returnOrg.push(organizationsOnSkills[key]);
				});
				return [200, returnOrg];
			});

		fake(/\.\.\/api\/Sites\/Organization(.*)/,
			function () {
				return [200, organizations];
			});

		fake(/\.\.\/api\/Adherence\/ForToday(.*)/,
			function (params) {
				var result = adherences.find(function (a) {
					return a.PersonId === params.personId;
				});
				return [200, result];
			});

		fake(/\.\.\/api\/PhoneState\/InfoFor(.*)/,
			function (data) {
				if (data.ids.indexOf(null) > -1 || data.ids.indexOf("noState") > -1)
					throw new Error('Nope, dont ask server for that')

				var result = phoneStates.filter(function (s) {
					return data.ids.indexOf(s.Id) > -1
				});

				if (result.length === 0) {
					result = states
						.filter(function (s) {
							if (data.ids.indexOf(s.StateId) > -1)
								return true;
						})
						.concat(agentStates
							.filter(function (s) {
								if (data.ids.indexOf(s.StateId) > -1)
									return true;
							}))
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

		fake(/ToggleHandler\/AllToggles(.*)/,
			function (params) {
				return [200, toggles];
			});

		fake(/\.\.\/api\/Agents\/PersonDetails(.*)/,
			function (params) {
				return [200, personDetails.find(function (p) {
					return p.PersonId === params.personId;
				})];
			});

		fake(/\.\.\/api\/Sites$/,
			function (params) {
				return [200, sites];
			});

		function sitesOrTeamsForSkillOrSkillArea(adherences, adherenceKey, paramId, sitesOrTeams) {
			var siteOrTeamIdsBySkillOrSkillAreaId = adherences.filter(function (a) {
				return paramId.indexOf(a[adherenceKey]) > -1;
			}).map(function (s) {
				return s.Id;
			});
			return sitesOrTeams.filter(function (st) {
				return siteOrTeamIdsBySkillOrSkillAreaId.indexOf(st.Id) > -1;
			});
		}

		function skillIdsFromTheSkillArea(skillAreacollection, skillAreaId) {
			return skillAreacollection.find(function (sa) {
				return sa.Id === skillAreaId;
			})
				.Skills
				.map(function (s) {
					return s.Id;
				});
		}

		fake(/\.\.\/api\/Sites\/ForSkills(.*)/,
			function (params) {
				var filteredSites = sitesOrTeamsForSkillOrSkillArea(siteAdherencesForSkill, 'SkillId', params.skillIds, sites);
				return [200, filteredSites];
			});

		fake(/\.\.\/api\/Teams\/ForSkills(.*)/,
			function (params) {
				var teamsBySite = teams.filter(function (t) {
					return params.siteId.indexOf(t.SiteId) > -1;
				});
				var filteredSites = sitesOrTeamsForSkillOrSkillArea(teamAdherencesForSkill, 'SkillId', params.skillIds, teamsBySite);
				return [200, filteredSites];
			});

		fake(/\.\.\/api\/Sites\/InAlarmCountForSkills(.*)/,
			function (params) {
				var adherenceBySiteId = {};
				var sAdherencesForMultipleSkills = [];
				var sAdherencesForSkill = siteAdherencesForSkill.filter(function (sa) {
					return params.skillIds.indexOf(sa.SkillId) > -1;
				});

				if (params.skillIds.length > 1) {
					sAdherencesForSkill.forEach(function (sas) {
						if (angular.isDefined(adherenceBySiteId[sas.Id]))
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
			function (params) {
				return [200, siteAdherences];
			});

		fake(/\.\.\/api\/Teams\/Build(.*)/,
			function (params) {
				return [200, teams.filter(function (team) {
					return team.SiteId === params.siteId;
				})];
			});

		fake(/\.\.\/api\/Teams\/InAlarmCountForSkills(.*)/,
			function (params) {
				var adherenceByTeamId = {};
				var tAdherencesForMultipleSkills = [];

				var teamAdherencesBySkillId = teamAdherencesForSkill.filter(function (ta) {
					return params.skillIds.indexOf(ta.SkillId) > -1 && ta.SiteId === params.siteId;
				});

				if (params.skillIds.length > 1) {
					teamAdherencesBySkillId.forEach(function (tas) {
						if (angular.isDefined(adherenceByTeamId[tas.Id]))
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
			function (params) {
				var result =
					teamAdherences.filter(function (ta) {
						var t = teams.find(function (team) {
							return team.Id === ta.Id;
						});
						return t != null && params.siteId === t.SiteId;
					});
				return [200, result];
			});

		fake(/\.\.\/api\/HistoricalAdherence\/For(.*)/,
			function (params) {
				var result = agents.find(function (agent) {
					return params.personId == agent.PersonId;
				});
				if (result != null)
					result.Now = serverTime;

				return [200, result];
			});


		function clear() {
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
		}

		function withToggle(toggle) {
			toggles[toggle] = true;
			return this;
		}

		function withTime(time) {
			serverTime = time;
			return this;
		};

		function withAgent(agent) {
			agents.push(agent);
			return this;
		};

		function clearStates() {
			states = [];
			return this;
		};

		function clearAgentStates() {
			agentStates = [];
			return this;
		};

		function withState(state) {
			states.push(state);
			return this;
		};

		function withAgentState(state) {
			agentStates.push(state);
			return this;
		}

		function withAdherence(adherence) {
			adherences.push(adherence);
			return this;
		}

		function withPersonDetails(personDetail) {
			personDetails.push(personDetail);
			return this;
		};

		function withActivityAdherence(activityAdherence) {
			activityAdherences.push(activityAdherence);
			return this;
		}

		function withSite(site) {
			sites.push(site);
			return this;
		};

		function withSiteAdherence(siteAdherence) {
			siteAdherences.push(siteAdherence);
			return this;
		};

		function withSiteAdherenceForSkill(siteAdherenceForSkill) {
			siteAdherencesForSkill.push(siteAdherenceForSkill);
			return this;
		};

		function clearSiteAdherences() {
			siteAdherences = [];
			return this;
		};

		function clearSiteAdherencesForSkill() {
			siteAdherencesForSkill = [];
			return this;
		};

		function clearTeamAdherencesForSkill() {
			teamAdherencesForSkill = [];
			return this;
		};

		function withTeam(team) {
			teams.push(team);
			return this;
		};

		function withTeamAdherenceForSkill(teamAdherenceForSkill) {
			teamAdherencesForSkill.push(teamAdherenceForSkill);
			return this;
		};

		function withTeamAdherence(teamAdherence) {
			teamAdherences.push(teamAdherence);
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

		function withSkills(newSkills) {
			skills = skills.concat(newSkills);
			return this;
		}

		function withSkillAreas(newSkillAreas) {
			skillAreas = newSkillAreas;
			return this;
		}

		function withPhoneState(phoneState) {
			phoneStates.push(phoneState)
			return this;
		}

		function withOrganization(organization) {
			organizations.push(organization)
			return this;
		}

		function withOrganizationOnSkills(organization, skillIds) {
			
			skillIds.split(",").forEach(function(key){
				var skillIdAsAKey = key.trim();
				organizationsOnSkills[skillIdAsAKey] = organization;	
			});
			return this;
		}

		return service;
	};
})();
