(function() {
  'use strict';
  angular.module('wfm.intraday').service('intradayService', ['$resource', '$http', intradayService]);

  function intradayService($resource, $http) {
    this.getSkills = $resource(
      '../api/intraday/skills',
      {},
      {
        query: {
          method: 'GET',
          params: {},
          isArray: true,
          cancellable: true
        }
      }
    );

    this.getSkillAreaMonitorStatistics = $resource(
      '../api/intraday/monitorskillareastatistics/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getSkillMonitorStatistics = $resource(
      '../api/intraday/monitorskillstatistics/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getSkillAreaMonitorPerformance = $resource(
      '../api/intraday/monitorskillareaperformance/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getSkillMonitorPerformance = $resource(
      '../api/intraday/monitorskillperformance/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getSkillAreaStaffingData = $resource(
      '../api/intraday/monitorskillareastaffing/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getSkillStaffingData = $resource(
      '../api/intraday/monitorskillstaffing/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
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
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getLatestStatisticsTimeForSkillArea = $resource(
      '../api/intraday/lateststatisticstimeforskillarea/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getLatestStatisticsTimeForSkill = $resource(
      '../api/intraday/lateststatisticstimeforskill/:id',
      { id: '@id' },
      {
        query: {
          method: 'GET',
          params: {},
          isArray: false,
          cancellable: true
        }
      }
    );

    this.getIntradayExportForSkillArea = function(data, successCb, errorCb) {
      $http({
        url: '../api/intraday/exportskillareadatatoexcel',
        method: 'POST',
        data: data,
        responseType: 'arraybuffer',
        headers: {
          Accept: 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        }
      })
        .success(successCb)
        .error(errorCb);
    };

    this.getIntradayExportForSkill = function(data, successCb, errorCb) {
      $http({
        url: '../api/intraday/exportskilldatatoexcel',
        method: 'POST',
        data: data,
        responseType: 'arraybuffer',
        headers: {
          Accept: 'application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
        }
      })
        .success(successCb)
        .error(errorCb);
    };
  }
})();
