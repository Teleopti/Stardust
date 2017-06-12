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
            getAdherenceForAllSites: getAdherenceForAllSites,
            getAdherenceForSitesBySkills: getAdherenceForSitesBySkills,
			getTeamCardsFor: getTeamCardsFor,
            getSkillArea: getSkillArea,
            getPhoneStates: getPhoneStates,
            forToday: forToday,
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
		
		function getTeamCardsFor(data) {
			return $resource('../api/Overview/TeamCards', {}, {
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
			return $resource('../api/Overview/SiteCards', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getAdherenceForSitesBySkills(data) {
			return $resource('../api/Overview/SiteCards', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query({
                skillIds: data
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