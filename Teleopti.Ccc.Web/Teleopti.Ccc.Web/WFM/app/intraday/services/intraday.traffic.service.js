(function() {
	'use strict';
	angular
		.module('wfm.intraday')
		.service('intradayTrafficService', ['$filter', 'intradayService', '$translate', '$log', trafficService]);

	function trafficService($filter, intradayService, $translate, $log) {
		var service = {
			trafficChart: {}
		};

		var trafficData = {
			forecastedCallsObj: {
				series: [],
				max: {}
			},
			actualCallsObj: {
				series: [],
				max: {}
			},
			forecastedAverageHandleTimeObj: {
				series: [],
				max: {}
			},
			actualAverageHandleTimeObj: {
				series: [],
				max: {}
			},
			summary: {},
			hasMonitorData: false,
			waitingForData: false,
			timeSeries: [],
			currentInterval: []
		};

		var hiddenArray = [];
		var intervalStart;
		var max;

		var roundSeries = function(data, decimals) {
			return data.map(function(x) {
				if (x) {
					return +x.toFixed(decimals);
				}
				return x;
			});
		};

		var setTrafficData = function(result, isToday) {
			clearData();
			trafficData.forecastedCallsObj.series = result.DataSeries.ForecastedCalls;
			trafficData.actualCallsObj.series = result.DataSeries.CalculatedCalls;
			trafficData.forecastedAverageHandleTimeObj.series = roundSeries(
				result.DataSeries.ForecastedAverageHandleTime,
				1
			);
			trafficData.actualAverageHandleTimeObj.series = roundSeries(result.DataSeries.AverageHandleTime, 1);

			trafficData.latestActualInterval =
				$filter('date')(result.LatestActualIntervalStart, 'shortTime') +
				' - ' +
				$filter('date')(result.LatestActualIntervalEnd, 'shortTime');
			intervalStart = $filter('date')(result.LatestActualIntervalStart, 'shortTime');

			trafficData.forecastedCallsObj.max = Math.max.apply(Math, trafficData.forecastedCallsObj.series);
			trafficData.actualCallsObj.max = Math.max.apply(Math, trafficData.actualCallsObj.series);
			trafficData.forecastedAverageHandleTimeObj.max = Math.max.apply(
				Math,
				trafficData.forecastedAverageHandleTimeObj.series
			);
			trafficData.actualAverageHandleTimeObj.max = Math.max.apply(
				Math,
				trafficData.actualAverageHandleTimeObj.series
			);

			trafficData.forecastedCallsObj.series.splice(0, 0, 'Forecasted_calls');
			trafficData.actualCallsObj.series.splice(0, 0, 'Calls');
			trafficData.forecastedAverageHandleTimeObj.series.splice(0, 0, 'Forecasted_AHT');
			trafficData.actualAverageHandleTimeObj.series.splice(0, 0, 'AHT');

			trafficData.summary = {
				summaryForecastedCalls: $filter('number')(result.Summary.ForecastedCalls, 1),
				summaryForecastedAverageHandleTime: $filter('number')(result.Summary.ForecastedAverageHandleTime, 1),
				summaryCalculatedCalls: $filter('number')(result.Summary.CalculatedCalls, 1),
				summaryAverageHandleTime: $filter('number')(result.Summary.AverageHandleTime, 1),
				forecastActualCallsDifference: $filter('number')(result.Summary.ForecastedActualCallsDiff, 1),
				forecastActualAverageHandleTimeDifference: $filter('number')(
					result.Summary.ForecastedActualHandleTimeDiff,
					1
				)
			};

			angular.forEach(
				result.DataSeries.Time,
				function(value, key) {
					this.push($filter('date')(value, 'shortTime'));
				},
				trafficData.timeSeries
			);

			if (trafficData.timeSeries[0] != 'x') {
				trafficData.timeSeries.splice(0, 0, 'x');
			}

			trafficData.hasMonitorData = result.IncomingTrafficHasData;

			trafficData.currentInterval = [];
			if (isToday) {
				getCurrent();
			}

			return trafficData;
		};

		service.getData = function() {
			return trafficData;
		};

		var clearData = function() {
			trafficData.timeSeries = [];
			trafficData.forecastedCallsObj.series = [];
			trafficData.actualCallsObj.series = [];
			trafficData.forecastedAverageHandleTimeObj.series = [];
			trafficData.actualAverageHandleTimeObj.series = [];
		};

		var getCurrent = function() {
			if (trafficData.forecastedCallsObj.max > trafficData.actualCallsObj.max) {
				max = trafficData.forecastedCallsObj.max;
			} else {
				max = trafficData.actualCallsObj.max;
			}

			for (var i = 0; i < trafficData.timeSeries.length; i++) {
				if (trafficData.timeSeries[i] === intervalStart) {
					trafficData.currentInterval[i] = max;
				} else {
					trafficData.currentInterval[i] = null;
				}
				trafficData.currentInterval[0] = 'Current';
			}
		};

		var request;
		function cancelPendingRequest() {
			if (request) {
				request.$cancelRequest('cancel');
			}
		}

		service.pollSkillData = function(selectedItem) {
			trafficData.waitingForData = true;
			cancelPendingRequest();

			request = intradayService.getSkillMonitorStatistics.query({
				id: selectedItem.Id
			});

			request.$promise.then(
				function(result) {
					trafficData.waitingForData = false;
					setTrafficData(result, true);
				},
				function(error) {
					trafficData.hasMonitorData = false;
				}
			);
		};

		service.pollSkillDataByDayOffset = function(selectedItem, toggles, dayOffset, gotData) {
			trafficData.waitingForData = true;
			cancelPendingRequest();

			request = intradayService.getSkillMonitorStatisticsByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});

			request.$promise.then(
				function(result) {
					trafficData.waitingForData = false;
					setTrafficData(result, dayOffset === 0);
					gotData(trafficData);
				},
				function(error) {
					trafficData.hasMonitorData = false;
					gotData(trafficData);
				}
			);
		};

		service.pollSkillAreaDataByDayOffset = function(selectedItem, toggles, dayOffset) {
			trafficData.waitingForData = true;
			cancelPendingRequest();

			request = intradayService.getSkillAreaMonitorStatisticsByDayOffset.query({
				id: selectedItem.Id,
				dayOffset: dayOffset
			});
			request.$promise.then(
				function(result) {
					trafficData.waitingForData = false;
					setTrafficData(result, dayOffset === 0);
					gotData(setTrafficData);
				},
				function(error) {
					trafficData.hasMonitorData = false;
					gotData(setTrafficData);
				}
			);
		};
		return service;
	}
})();
