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
            clearAgentStates: clearAgentStates,
            withAdherence: withAdherence,
            withSiteAdherence: withSiteAdherence,
            clearSiteAdherences: clearSiteAdherences,
            withTeamAdherence: withTeamAdherence,
            clearTeamAdherences: clearTeamAdherences,
            withSkill: withSkill,
            withSkillAreas: withSkillAreas,
            withPhoneState: withPhoneState,
            withOrganization: withOrganization,
            withOrganizationOnSkills: withOrganizationOnSkills,
            withTimeline: withTimeline
        };

        ///////////////////////////////

        var serverTime = null;
        var toggles = {};
        var agentStates = [];
        var adherences = [];
        var siteAdherences = [];
        var teamAdherences = [];
        var skills = [];
        var skillAreas = [];
        var phoneStates = [];
        var organizations = [];
        var organizationsOnSkills = [];
        var timeline = {};

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
                var uniqueSiteIds = [];
                var returnOrg = [];
                var skillIdsArray = angular.isArray(params.skillIds) ? params.skillIds : params.skillIds.split(",");
                skillIdsArray.forEach(function (key) {
                    if (uniqueSiteIds.indexOf(organizationsOnSkills[key][0].Id) < 0) {
                        uniqueSiteIds = uniqueSiteIds.concat(organizationsOnSkills[key][0].Id);
                        returnOrg = returnOrg.concat(organizationsOnSkills[key]);
                    }
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

        fake(/ToggleHandler\/AllToggles(.*)/,
            function (params) {
                return [200, toggles];
            });

        fake(/\.\.\/api\/Overview\/SiteCards(.*)/,
            function (params) {
                var sites = siteAdherences;
                if (params.skillIds)
                    sites = siteAdherences.filter(function (sa) {
                        return params.skillIds.indexOf(sa.SkillId) > -1;
                    });
                var totalAgentsInAlarm = sites.length > 0 ?
                    sites
                        .map(function (s) { return s.InAlarmCount })
                        .reduce(function (s, v) { return s + v })
                    : 0;

                return [200, {
                    Sites: sites,
                    TotalAgentsInAlarm: totalAgentsInAlarm
                }];
            });

        fake(/\.\.\/api\/Overview\/TeamCards(.*)/,
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

        fake(/\.\.\/api\/HistoricalAdherence\/For(.*)/,
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
            toggles = {};
            adherences = [];
            siteAdherences = [];
            teamAdherences = [];
            skillAreas = [];
            phoneStates = [];
            timeline = {};
        }

        function withToggle(toggle) {
            toggles[toggle] = true;
            return this;
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
            adherences.push(adherence);
            return this;
        }

        function withSiteAdherence(siteAdherence) {
            siteAdherences.push(siteAdherence);
            return this;
        };

        function clearSiteAdherences() {
            siteAdherences = [];
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

            skillIds.split(",").forEach(function (key) {
                var skillIdAsAKey = key.trim();
                //organizationsOnSkills[skillIdAsAKey] = organization;
                if (angular.isDefined(organizationsOnSkills[skillIdAsAKey]))
                    organizationsOnSkills[skillIdAsAKey] = organizationsOnSkills[skillIdAsAKey].concat(organization);
                else
                    organizationsOnSkills[skillIdAsAKey] = [organization];
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