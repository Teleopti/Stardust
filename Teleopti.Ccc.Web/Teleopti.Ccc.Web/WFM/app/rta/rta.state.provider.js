'use strict';

angular
    .module('wfm.rta')
    .provider('RtaState', function () {

        var toggles = {}

        this.$get = function () {
            return function (toggleService) {
                toggleService.togglesLoaded.then(function () {
                    toggles = toggleService
                });
            };
        };

        this.config = function ($stateProvider) {
            $stateProvider
                .state('rta-without-slash', {
                    url: '/rta',
                    controller: function ($state) {
                        $state.go('rta')
                    }
                })
                .state('rta', {
                    url: '/rta/?skillIds?skillAreaId?open',
                    templateUrl: 'app/rta/overview/rta.html',
                    controller: 'RtaMainController as vm',
                    params: {
                        skillIds: {
                            array: true
                        }
                    },
                    resolve: {
                        skills: function (rtaService) {
                            return rtaService.getSkills();
                        },
                        skillAreas: function (rtaService) {
                            return rtaService.getSkillAreas().then(function (result) {
                                return result.SkillAreas;
                            });
                        }
                    }
                })
                .state('rta-agents', {
                    url: '/rta/agents/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
                    templateUrl: 'app/rta/agents/rta-agents.html',
                    controller: 'RtaAgentsController as vm',
                    params: {
                        siteIds: {
                            array: true
                        },
                        teamIds: {
                            array: true
                        },
                        skillIds: {
                            array: true
                        },
                        es: {
                            array: true
                        }
                    }
                })
                .state('rta-historical', {
                    url: '/rta/agent-historical/:personId?open',
                    templateUrl: function () {
                        return 'app/rta/historical/rta-historical-RTA_SolidProofWhenManagingAgentAdherence_39351.html';
                    },
                    controller: 'RtaHistoricalController as vm',
                })


                .state('rta-teams-legacy', {
                    url: '/rta/teams/?siteIds&skillIds&skillAreaId&open',
                    controller: function ($state, $stateParams) {
                        $state.go('rta', $stateParams)
                    }
                })
        };
    });
