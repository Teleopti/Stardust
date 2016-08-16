(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayTrafficService', [
			'$filter', function($filter) {
        var service = {};

        service.setTrafficData = function (result) {
          var trafficData = {
            forecastedCallsObj: {
              Series: result.DataSeries.ForecastedCalls
            },
            actualCallsObj: {
              Series: result.DataSeries.OfferedCalls
            },
            forecastedAverageHandleTimeObj: {
              Series: result.DataSeries.ForecastedAverageHandleTime
            },
            actualAverageHandleTimeObj: {
              Series: result.DataSeries.AverageHandleTime
            },
            summary: setTrafficSummary(result)
          };


					trafficData.forecastedCallsObj.Max = Math.max.apply(Math, trafficData.forecastedCallsObj.Series);
					trafficData.actualCallsObj.Max = Math.max.apply(Math, trafficData.actualCallsObj.Series);
					trafficData.forecastedAverageHandleTimeObj.Max = Math.max.apply(Math, trafficData.forecastedAverageHandleTimeObj.Series);
					trafficData.actualAverageHandleTimeObj.Max = Math.max.apply(Math, trafficData.actualAverageHandleTimeObj.Series);

          trafficData.forecastedCallsObj.Series.splice(0, 0, 'Forecasted_calls');
          trafficData.actualCallsObj.Series.splice(0, 0, 'Calls');
          trafficData.forecastedAverageHandleTimeObj.Series.splice(0, 0, 'Forecasted_AHT');
          trafficData.actualAverageHandleTimeObj.Series.splice(0, 0, 'AHT');

          return trafficData;
        };

        function setTrafficSummary(result) {
          var summary = {
            summaryForecastedCalls: $filter('number')(result.Summary.ForecastedCalls, 1),
            summaryForecastedAverageHandleTime: $filter('number')(result.Summary.ForecastedAverageHandleTime, 1),
            summaryOfferedCalls:  $filter('number')(result.Summary.OfferedCalls, 1),
            summaryAverageHandleTime:  $filter('number')(result.Summary.AverageHandleTime, 1),
            forecastActualCallsDifference:  $filter('number')(result.Summary.ForecastedActualCallsDiff, 1),
            forecastActualAverageHandleTimeDifference: $filter('number')(result.Summary.ForecastedActualHandleTimeDiff, 1)
          };
          return summary;
        }

        return service;

			}
		]);
})();
