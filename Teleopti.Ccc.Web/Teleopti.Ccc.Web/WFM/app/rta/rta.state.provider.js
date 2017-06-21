'use strict';

angular
    .module('wfm.rta')
    .provider('RtaState', function() {

        var toggles = {}

        this.$get = function() {
            return function(toggleService) {
                toggleService.togglesLoaded.then(function() {
                    toggles = toggleService
                });
            };
        };

        this.config = function($stateProvider) {
            $stateProvider.state('rta', {
                    url: '/rta',
                    templateUrl: 'app/rta/rta.html'
                })
                .state('rta.sites', {
                    url: '/?skillIds&skillAreaId',
                    templateUrl: 'app/rta/overview/rta-sites.html',
                    controller: 'RtaOverviewController as vm',
                    params: {
                        skillIds: {
                            array: false
                        },
                        skillAreaId: {
                            array: false
                        }
                    }
                })
                .state('rta.teams', {
                    url: '/teams/?siteIds&skillIds&skillAreaId',
                    templateUrl: 'app/rta/overview/rta-teams.html',
                    controller: 'RtaOverviewController as vm',
                    params: {
                        siteIds: {
                            array: true
                        },
                        skillIds: {
                            array: false
                        },
                        skillAreaId: {
                            array: false
                        }
                    }
                })
                .state('rta.agents', {
                    url: '/agents/?siteIds&teamIds&skillIds&skillAreaId&showAllAgents&es',
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
                .state('rta.historical', {
                    url: '/agent-historical/:personId?open',
                    templateUrl: function() {
                        if (toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)
                            return 'app/rta/historical/rta-historical-RTA_SolidProofWhenManagingAgentAdherence_39351.html'
                        else
                            return 'app/rta/historical/rta-historical-SeeAllOutOfAdherences_39146.html'
                    },
                    controller: 'RtaHistoricalController as vm',
                })
                .state('refact-rta', {
                    url: '/refact-rta?open',
                    templateUrl: 'app/rta/refact/rta.html',
                    controller: 'RtaMainController as vm'
                })
                .state('refact-rta.skill', {
                    url: '/?skillIds',
                    params: {
                        skillIds: {
                            array: true
                        }
                    }
                })
                ;
        };
    });
