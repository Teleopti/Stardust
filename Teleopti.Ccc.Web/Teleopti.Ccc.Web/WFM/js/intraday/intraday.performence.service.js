(function() {
	'use strict';
	angular.module('wfm.intraday')
		.service('intradayPerformanceService', [
			'$filter', function($filter) {
        var service = {};

        service.setPerformanceData = function (result) {
          var performanceData = {
            averageSpeedOfAnswerObj: {
              Series: result.DataSeries.AverageSpeedOfAnswer
            },
            abandonedRateObj: {
              Series: result.DataSeries.AbandonedRate
            },
            serviceLevelObj: {
              Series: result.DataSeries.ServiceLevel
            },
            summary: setPerformanceSummary(result)
          };

  				performanceData.averageSpeedOfAnswerObj.Max = Math.max.apply(Math, performanceData.averageSpeedOfAnswerObj.Series);
  				performanceData.abandonedRateObj.Max = Math.max.apply(Math, performanceData.abandonedRateObj.Series);
  				performanceData.serviceLevelObj.Max = Math.max.apply(Math, performanceData.serviceLevelObj.Series);

  				performanceData.averageSpeedOfAnswerObj.Series.splice(0, 0, 'ASA');
  				performanceData.abandonedRateObj.Series.splice(0, 0, 'Abandoned_rate');
  				performanceData.serviceLevelObj.Series.splice(0, 0, 'Service_level');

          return performanceData;
        };

        function setPerformanceSummary(result) {
          var summary = {
            summaryAbandonedRate: $filter('number')(result.Summary.AbandonRate * 100, 1),
            summaryServiceLevel: $filter('number')(result.Summary.ServiceLevel * 100, 1),
            summaryAsa: $filter('number')(result.Summary.AverageSpeedOfAnswer, 1)
          };
          return summary;
        }

        return service;

			}
		]);
})();
