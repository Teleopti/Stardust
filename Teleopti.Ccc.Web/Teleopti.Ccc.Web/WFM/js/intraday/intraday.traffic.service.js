(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayTrafficService', [
			'$filter', function($filter) {
        var service = {};

        service.setTrafficData = function (result) {
          var trafficData = {
            forecastedCallsObj: {
              Series: result.StatisticsDataSeries.ForecastedCalls
            },
            actualCallsObj: {
            	Series: result.StatisticsDataSeries.OfferedCalls
            },
            forecastedAverageHandleTimeObj: {
            	Series: result.StatisticsDataSeries.ForecastedAverageHandleTime
            },
            actualAverageHandleTimeObj: {
            	Series: result.StatisticsDataSeries.AverageHandleTime
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
            summaryForecastedCalls: $filter('number')(result.StatisticsSummary.ForecastedCalls, 1),
            summaryForecastedAverageHandleTime: $filter('number')(result.StatisticsSummary.ForecastedAverageHandleTime, 1),
            summaryOfferedCalls: $filter('number')(result.StatisticsSummary.OfferedCalls, 1),
            summaryAverageHandleTime: $filter('number')(result.StatisticsSummary.AverageHandleTime, 1),
            forecastActualCallsDifference: $filter('number')(result.StatisticsSummary.ForecastedActualCallsDiff, 1),
            forecastActualAverageHandleTimeDifference: $filter('number')(result.StatisticsSummary.ForecastedActualHandleTimeDiff, 1)
          };
          return summary;
        }

        return service;

			}
		]);
})();
