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

        function getOverviewModelFor(data) {
            if (angular.isArray(data)) // remove with RTA_RememberMyPartOfTheBusiness_39082
                data = { skillIds: data };

            return $resource('../api/Overview/SiteCards', {}, {
                query: {
                    method: 'GET'
                }
            }).query(data)
                .$promise
                // .then(function (result) {
                //     result.Teams = [{
                //         Id: "34590A63-6331-4921-BC9F-9B5E015AB495",
                //         SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                //         Name: "Fajker",
                //         AgentsCount: 183897,
                //         InAlarmCount: 13,
                //         Color: "danger"
                //     }]
                //     return result;
                // })
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