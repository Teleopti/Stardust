(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayMonitorStaffingService', [
			'$filter', function($filter) {
        var service = {};

        service.setStaffingData = function (result) {
          var  staffingData = {
            forecastedStaffing: {
              max: {},
              series:result.DataSeries.ForecastedStaffing
            }
          };

					staffingData.forecastedStaffing.max = Math.max.apply(Math, result.DataSeries.ForecastedStaffing);
          staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');
          return staffingData;
        };

        return service;

			}
		]);
})();
