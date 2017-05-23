(function() {
  'use strict';
  angular.module('wfm.intraday')
  .service('intradayLatestTimeService', [
    '$filter', 'intradayService', function($filter, intradayService) {
      var service = {};
      var startTime;
      var endTime;
      var timeData;

      var pollTime = function (selectedItem) {
        if (selectedItem.Skills) {
          intradayService.getLatestStatisticsTimeForSkillArea.query(
            {
              id: selectedItem.Id
            })
            .$promise.then(function (result) {
			          if (result.latestIntervalTime) {
				          startTime = $filter('date')(result.latestIntervalTime.StartTime, 'shortTime');
				          endTime = $filter('date')(result.latestIntervalTime.EndTime, 'shortTime');
			          }
		          },
            function (error) {
              return;
            });
          }else{
            intradayService.getLatestStatisticsTimeForSkill.query(
              {
                id: selectedItem.Id
              })
              .$promise.then(function (result) {
                startTime = $filter('date')(result.StartTime, 'shortTime');
                endTime = $filter('date')(result.EndTime, 'shortTime');
              },
              function (error) {
                return;
              });
            }
          }

          service.getLatestTime = function(selectedItem) {
            pollTime(selectedItem);
            var timeData = startTime + ' - ' + endTime;
            return timeData;
          }

          service.getLatestTimeByDate = function(selectedItem, utcDate){
          }

          return service;
        }
      ]);
    })();
