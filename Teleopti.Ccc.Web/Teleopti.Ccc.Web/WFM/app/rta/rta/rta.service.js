(function () {
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
            getOverviewModelFor: getOverviewModelFor,
            getTeamCardsFor: getTeamCardsFor,
            getSkillArea: getSkillArea,
            getPhoneStates: getPhoneStates,
            forToday: forToday,
            getAgentHistoricalData: getAgentHistoricalData
        }

        return service;

        function getSkills() {
            return $resource('../api/Skills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise;
        };

        function getSkillAreas() {
            return $resource('../api/SkillGroups', {}, {
                query: {
                    method: 'GET',
					isArray: true
                }
            }).query().$promise;
        };

        function getOrganization() {
            return $resource('../api/Sites/Organization', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query().$promise
        };

        function getOrganizationForSkills(data) {
			data.skillIds = data.skillIds || [];
			// with more than 20 skills in a skill area, display the whole organization
			// not tested. we'r not even sure if skill ids should be filtered on the 
			// server side at all
			data.skillIds = data.skillIds.length > 20 ? [] : data.skillIds;
            return $resource('../api/Sites/OrganizationForSkills', {}, {
                query: {
                    method: 'GET',
                    isArray: true
                }
            }).query({ skillIds: data.skillIds }).$promise;
        };

        function agentStatesFor(data) {
            return $resource('../api/AgentStates/Poll', {}, { query: { method: 'POST' } }).query(data).$promise;
        };

        function getTeamCardsFor(data) {
            return $resource('../api/Overview/TeamCards', {}, {
                query: {
                    method: 'POST',
                    isArray: true
                }
            }).query({
                skillIds: data.skillIds,
                siteId: data.siteIds
            }).$promise;
        };

        function getOverviewModelFor(data) {
            return $resource('../api/Overview/SiteCards', {}, {
                query: {
                    method: 'POST'
                }
            }).query(data).$promise
        }

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