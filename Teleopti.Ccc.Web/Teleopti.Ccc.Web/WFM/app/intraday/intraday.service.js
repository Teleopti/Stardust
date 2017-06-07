(function() {
    'use strict';
    angular.module('wfm.intraday').service('intradayService', [
        '$resource',
        function($resource) {
            this.getSkills = $resource(
                '../api/intraday/skills',
                {},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: true
                    }
                }
            );

            this.createSkillArea = $resource(
                '../api/intraday/skillarea',
                {},
                {
                    query: {
                        method: 'POST',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreas = $resource(
                '../api/intraday/skillarea',
                {},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.deleteSkillArea = $resource(
                '../api/intraday/skillarea/:id',
                {id: '@id'},
                {
                    remove: {
                        method: 'DELETE',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaMonitorStatistics = $resource(
                '../api/intraday/monitorskillareastatistics/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillMonitorStatistics = $resource(
                '../api/intraday/monitorskillstatistics/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillMonitorStatisticsByDayOffset = $resource(
                '../api/intraday/monitorskillstatistics/:id/:dayOffset',
                {
                            id: '@id',
                            dayOffset: '@dayOffset'
				},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaMonitorStatisticsByDayOffset = $resource(
                '../api/intraday/monitorskillareastatistics/:id/:dayOffset',
                {
                    id: '@id',
                    dayOffset: '@dayOffset'
                },
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaMonitorPerformance = $resource(
                '../api/intraday/monitorskillareaperformance/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillMonitorPerformance = $resource(
                '../api/intraday/monitorskillperformance/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillMonitorPerformanceByDayOffset = $resource(
                '../api/intraday/monitorskillperformance/:id/:dayOffset',
                {
                    id: '@id',
                    dayOffset: '@dayOffset'
                },
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaMonitorPerformanceByDayOffset = $resource(
                '../api/intraday/monitorskillareaperformance/:id/:dayOffset',
                {
                    id: '@id',
                    dayOffset: '@dayOffset'
                },
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaStaffingData = $resource(
                '../api/intraday/monitorskillareastaffing/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillStaffingData = $resource(
                '../api/intraday/monitorskillstaffing/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillStaffingDataByDayOffset = $resource(
                '../api/intraday/monitorskillstaffing/:id/:dayOffset',
                {
                    id: '@id',
                    dayOffset: '@dayOffset'
                },
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getSkillAreaStaffingDataByDayOffset = $resource(
                '../api/intraday/monitorskillareastaffing/:id/:dayOffset',
                {
                    id: '@id',
                    dayOffset: '@dayOffset'
                },
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getLatestStatisticsTimeForSkillArea = $resource(
                '../api/intraday/lateststatisticstimeforskillarea/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );

            this.getLatestStatisticsTimeForSkill = $resource(
                '../api/intraday/lateststatisticstimeforskill/:id',
                {id: '@id'},
                {
                    query: {
                        method: 'GET',
                        params: {},
                        isArray: false
                    }
                }
            );
        }
    ]);
})();
