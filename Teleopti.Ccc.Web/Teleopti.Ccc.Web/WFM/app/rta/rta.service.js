(function() {
    'use strict';

    angular
        .module('wfm.rta')
        .factory('rtaService', rtaService);

    rtaService.$inject = ['rtaResourceFactory', '$q', 'Toggle'];

    function rtaService($resource, $q, toggles) {

        var service = {
            getSkills: getSkills,
            getSkillAreas: getSkillAreas,
            getOrganization: getOrganization,
            getOrganizationForSkills: getOrganizationForSkills,
            agentStatesFor: agentStatesFor,
            agentStatesInAlarmFor: agentStatesInAlarmFor,
            agentStatesInAlarmExcludingPhoneStatesFor: agentStatesInAlarmExcludingPhoneStatesFor,
            agentsFor: agentsFor,
            statesFor: statesFor,
            inAlarmFor: inAlarmFor,
            inAlarmExcludingPhoneStatesFor: inAlarmExcludingPhoneStatesFor,
            getAlarmStatesForSitesAndSkillsExcludingStates: getAlarmStatesForSitesAndSkillsExcludingStates,
            getAlarmStatesForTeamsAndSkillsExcludingStates: getAlarmStatesForTeamsAndSkillsExcludingStates,
            getAlarmStatesForSkillsExcludingStates: getAlarmStatesForSkillsExcludingStates,
            getAlarmStatesForTeamsExcludingStates: getAlarmStatesForTeamsExcludingStates,
            getAdherenceForAllSites: getAdherenceForAllSites,
            getAdherenceForSitesBySkills: getAdherenceForSitesBySkills,
            getSites: getSites,
            getSitesForSkills: getSitesForSkills,
			getTeamCardsFor: getTeamCardsFor,
            getSkillName: getSkillName,
            getSkillArea: getSkillArea,
            getPhoneStates: getPhoneStates,
            forToday: forToday,
            getPersonDetails: getPersonDetails,
            getAgentHistoricalData: getAgentHistoricalData
        }

        return service;

        function getSkills(data) {
            return $resource('../api/Skills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getSkillAreas(data) {
            return $resource('../api/SkillAreas', {}, {
                query: {
                    method: 'GET'
                }
            }).query().$promise;
        };

        function getOrganization(data) {
            return $resource('../api/Sites/Organization', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getOrganizationForSkills(data) {
            return $resource('../api/Sites/OrganizationForSkills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query({ skillIds: data.skillIds }).$promise;
        };

        function agentStatesFor(data) {
            return $resource('../api/AgentStates/For', {}, { query: { method: 'GET' } }).query(data).$promise;
        };

        function agentStatesInAlarmFor(data) {
            return $resource('../api/AgentStates/InAlarmFor', {}, {
                query: {
                    method: 'GET'
                }
            }).query(data).$promise;
        };

        function agentStatesInAlarmExcludingPhoneStatesFor(data) {
            return $resource('../api/AgentStates/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        }

        function agentsFor(data) {
            return $resource('../api/Agents/For', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query(data).$promise;
        };

        function statesFor(data) {
            return $resource('../api/Agents/StatesFor', {}, {
                query: {
                    method: 'GET'
                }
            }).query(data).$promise;
        };

        function inAlarmFor(data) {
            return $resource('../api/Agents/InAlarmFor', {}, {
                query: {
                    method: 'GET'
                }
            }).query(data).$promise;
        };

        function inAlarmExcludingPhoneStatesFor(data) {
            return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        };

        function getAlarmStatesForSitesAndSkillsExcludingStates(data) {
            return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        };

        function getAlarmStatesForTeamsAndSkillsExcludingStates(data) {
            return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        };

        function getAlarmStatesForSkillsExcludingStates(data) {
            return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        };

        function getAlarmStatesForTeamsExcludingStates(data) {
            return $resource('../api/Agents/InAlarmExcludingPhoneStatesFor', {}, {
                    query: {
                        method: 'GET'
                    }
                }).query(data)
                .$promise;
        };
		
		function getTeamCardsFor(data) {
		    return $resource('../api/Teams/CardsFor', {}, {
			    query: {
				    method: 'GET',
				    isArray: true
			    }
		    }).query({
			    skillIds: data.skillIds,
			    siteId: data.siteIds
		    }).$promise;
	    };

        function getAdherenceForAllSites(data) {
            return $resource('../api/Sites/GetOutOfAdherenceForAllSites', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getAdherenceForSitesBySkills(data) {
            return $resource('../api/Sites/InAlarmCountForSkills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query({
                skillIds: data
            }).$promise;
        };

        function getSites(data) {
            return $resource('../api/Sites', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getSitesForSkills(skillIds) {
            return $resource('../api/Sites/ForSkills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query({
                skillIds: skillIds
            }).$promise;
        };

        function getSkillName(data) {
            return $resource('../api/Skills/NameFor', {}, {
                query: {
                    method: 'GET'
                }
            }).query({
                skillId: data
            }).$promise;
        };

        function getSkillArea(data) {
            return $resource('../api/SkillArea/For', {}, {
                query: {
                    method: 'GET'
                }
            }).query({
                skillAreaId: data
            }).$promise;
        };

        function getPhoneStates(data) {
            return $resource('../api/PhoneState/InfoFor', {}, {
                query: {
                    method: 'GET'
                }
            }).query({
                ids: data
            }).$promise;
        };

        function forToday(data) {
            return $resource('../api/Adherence/ForToday', {}, {
                query: {
                    method: 'GET',
                    isArray: false
                }
            }).query({
                personId: data.personId
            }).$promise;
        };

        function getPersonDetails(data) {
            return $resource('../api/Agents/PersonDetails', {}, {
                query: {
                    method: 'GET',
                    isArray: false
                }
            }).query({
                personId: data.personId
            }).$promise;
        };

        function getAgentHistoricalData(id) {
            return $resource('../api/HistoricalAdherence/For', {}, {
                query: {
                    method: 'GET',
                    isArray: false
                }
            }).query({
                personId: id
            }).$promise;
        };
    };
})();