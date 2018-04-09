(function() {
	'use strict';
	angular
		.module('wfm.intraday')
		.service('intradayLatestTimeService', ['$filter', 'intradayService', intradayLatestTimeService]);

	function intradayLatestTimeService($filter, intradayService, gotData) {
		var service = {};

		var getTimeString = function(startTime, endTime) {
			return $filter('date')(startTime, 'shortTime') + ' - ' + $filter('date')(endTime, 'shortTime');
		};

		var pollTime = function(selectedItem) {
			if (selectedItem.Skills) {
				intradayService.getLatestStatisticsTimeForSkillArea
					.query({
						id: selectedItem.Id
					})
					.$promise.then(
						function(result) {
							if (result.latestIntervalTime) {
								gotData(
									getTimeString(
										result.latestIntervalTime.StartTime,
										result.latestIntervalTime.EndTime
									)
								);
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
							gotData(getTimeString(result.StartTime, result.EndTime));
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
