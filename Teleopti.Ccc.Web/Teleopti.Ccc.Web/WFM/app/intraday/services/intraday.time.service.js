(function() {
  'use strict';
  angular
    .module('wfm.intraday')
    .service('intradayLatestTimeService', ['$filter', 'intradayService', intradayLatestTimeService]);

  function intradayLatestTimeService($filter, intradayService) {
    var service = {};
    var startTime;
    var endTime;
    var pollTime = function(selectedItem) {
      if (selectedItem.Skills) {
        intradayService.getLatestStatisticsTimeForSkillArea
          .query({
            id: selectedItem.Id
          })
          .$promise.then(
            function(result) {
              if (result.latestIntervalTime) {
                startTime = $filter('date')(result.latestIntervalTime.StartTime, 'shortTime');
                endTime = $filter('date')(result.latestIntervalTime.EndTime, 'shortTime');
              }
            },
            function(error) {}
          );
      } else {
        intradayService.getLatestStatisticsTimeForSkill
          .query({
            id: selectedItem.Id
          })
          .$promise.then(
            function(result) {
              startTime = $filter('date')(result.StartTime, 'shortTime');
              endTime = $filter('date')(result.EndTime, 'shortTime');
            },
            function(error) {}
          );
      }
    };

    service.getLatestTime = function(selectedItem) {
      pollTime(selectedItem);
      return startTime + ' - ' + endTime;
    };

    return service;
  }
})();
