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
                .state('rta', {
                    url: '/rta',
                    templateUrl: 'app/rta/rta.html',
                    resolve: {
                        toggles: function (Toggle) {
                            return Toggle;
                        }
                    },
                    controller: function ($state, toggles) {
                        if (toggles.RTA_FrontEndRefactor_44772)
                            $state.go('refact-rta');
                    },

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
                    templateUrl: function () {
                        if (toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)
                            return 'app/rta/historical/rta-historical-RTA_SolidProofWhenManagingAgentAdherence_39351.html'
                        else
                            return 'app/rta/historical/rta-historical-SeeAllOutOfAdherences_39146.html'
                    },
                    controller: 'RtaHistoricalController as vm',
                })
                .state('refact-rta', {
                    url: '/refact-rta/?skillIds?skillAreaId?open',
                    templateUrl: 'app/rta/refact/rta.html',
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
                });
        };
    });
