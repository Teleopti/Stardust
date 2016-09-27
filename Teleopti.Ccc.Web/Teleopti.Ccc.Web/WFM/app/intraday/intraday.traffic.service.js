(function() {
	'use strict';
	angular.module('wfm.intraday')
	.service('intradayTrafficService', [
		'$filter', 'intradayService','$translate', function($filter, intradayService, $translate) {
			var service = {};

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
				timeSeries : [],
				currentInterval: []
			};

			var hiddenArray = [];
			var interval;
			var intervalStart;
			var max;

			service.setTrafficData = function (result) {
				clearData();
				trafficData.forecastedCallsObj.series = result.StatisticsDataSeries.ForecastedCalls;
				trafficData.actualCallsObj.series = result.StatisticsDataSeries.OfferedCalls;
				trafficData.forecastedAverageHandleTimeObj.series = result.StatisticsDataSeries.ForecastedAverageHandleTime;
				trafficData.actualAverageHandleTimeObj.series = result.StatisticsDataSeries.AverageHandleTime;

				trafficData.latestActualInterval = $filter('date')(result.LatestActualIntervalStart, 'shortTime') + ' - ' + $filter('date')(result.LatestActualIntervalEnd, 'shortTime');
				intervalStart = $filter('date')(result.LatestActualIntervalStart, 'shortTime');

				trafficData.forecastedCallsObj.max = Math.max.apply(Math, trafficData.forecastedCallsObj.series);
				trafficData.actualCallsObj.max = Math.max.apply(Math, trafficData.actualCallsObj.series);
				trafficData.forecastedAverageHandleTimeObj.max = Math.max.apply(Math, trafficData.forecastedAverageHandleTimeObj.series);
				trafficData.actualAverageHandleTimeObj.max = Math.max.apply(Math, trafficData.actualAverageHandleTimeObj.series);

				trafficData.forecastedCallsObj.series.splice(0, 0, 'Forecasted_calls');
				trafficData.actualCallsObj.series.splice(0, 0, 'Calls');
				trafficData.forecastedAverageHandleTimeObj.series.splice(0, 0, 'Forecasted_AHT');
				trafficData.actualAverageHandleTimeObj.series.splice(0, 0, 'AHT');

				trafficData.summary = {
					summaryForecastedCalls: $filter('number')(result.StatisticsSummary.ForecastedCalls, 1),
					summaryForecastedAverageHandleTime: $filter('number')(result.StatisticsSummary.ForecastedAverageHandleTime, 1),
					summaryOfferedCalls: $filter('number')(result.StatisticsSummary.OfferedCalls, 1),
					summaryAverageHandleTime: $filter('number')(result.StatisticsSummary.AverageHandleTime, 1),
					forecastActualCallsDifference: $filter('number')(result.StatisticsSummary.ForecastedActualCallsDiff, 1),
					forecastActualAverageHandleTimeDifference: $filter('number')(result.StatisticsSummary.ForecastedActualHandleTimeDiff, 1)
				};

				angular.forEach(result.StatisticsDataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, trafficData.timeSeries);

				if (trafficData.timeSeries[0] != 'x' ) {
					trafficData.timeSeries.splice(0, 0, 'x');
				}

				trafficData.hasMonitorData = true;
				getCurrent();
				service.loadTrafficChart(trafficData);
				return trafficData;
			};

			service.getData = function () {
				return trafficData;
			};

			var clearData = function () {
				trafficData.timeSeries = [];
				trafficData.forecastedCallsObj.series = [];
				trafficData.actualCallsObj.series = [];
				trafficData.forecastedAverageHandleTimeObj.series = [];
				trafficData.actualAverageHandleTimeObj.series = [];
			};

			var getCurrent = function () {
				if (trafficData.forecastedCallsObj.max > trafficData.actualCallsObj.max) {
					max = trafficData.forecastedCallsObj.max;
				}else{
					max = trafficData.actualCallsObj.max;
				}
				trafficData.currentInterval = [];

				for (var i = 0; i < trafficData.timeSeries.length; i++) {
					if (trafficData.timeSeries[i] === intervalStart) {
						trafficData.currentInterval[i] = max;
					}else{
						trafficData.currentInterval[i] = null;
					}
					trafficData.currentInterval[0] = 'Current';
				};
			};

			service.pollSkillData = function (selectedItem) {
				intradayService.getSkillMonitorStatistics.query(
					{
						id: selectedItem.Id
					})
					.$promise.then(function (result) {
						service.setTrafficData(result);
					},
					function (error) {
						trafficData.hasMonitorData = false;
					});
				};

				service.pollSkillAreaData = function (selectedItem) {
					intradayService.getSkillAreaMonitorStatistics.query(
						{
							id: selectedItem.Id
						})
						.$promise.then(function (result) {
							service.setTrafficData(result);
						},
						function (error) {
							trafficData.hasMonitorData = false;
						});
					};

					service.loadTrafficChart = function (trafficData) {
						var performanceChart = c3.generate({
							bindto: '#trafficChart',
							data: {
								x: 'x',
								columns: [
									trafficData.timeSeries,
									trafficData.forecastedCallsObj.series,
									trafficData.actualCallsObj.series,
									trafficData.forecastedAverageHandleTimeObj.series,
									trafficData.actualAverageHandleTimeObj.series,
									trafficData.currentInterval
								],
								groups: [
									['Forecasted_calls'], ['Calls'], ['Forecasted_AHT'],['AHT']
								],
								order: 'asc',
								hide: hiddenArray,
								types: {
									Current:'bar'
								},
								colors: {
									Forecasted_calls: '#99D6FF',
									Calls: '#0099FF',
									Forecasted_AHT: '#FFC285',
									AHT: '#FB8C00'
								},
								names: {
									Forecasted_calls: $translate.instant('ForecastedCalls') + ' ←',
									Calls: $translate.instant('Calls') + ' ←',
									Forecasted_AHT: $translate.instant('ForecastedAverageHandleTime') + ' →',
									AHT: $translate.instant('AverageHandlingTime') + ' →'
								},
								axes: {
									Forecasted_AHT: 'y2',
									AHT: 'y2'
								}
							},
							axis: {
								x: {
									label: $translate.instant('SkillTypeTime'),
									type: 'category',
									tick: {
										culling: {
											max: 24
										},
										fit: true,
										centered: true,
										multiline: false
									}
								},
								y: {
									label: $translate.instant('Calls'),
									tick: {
										format: d3.format('.1f')
									}
								},
								y2: {
									label: $translate.instant('AverageHandlingTime'),
									show: true,
									tick: {
										format: d3.format('.1f')
									}
								}
							},
							legend: {
								item: {
									onclick: function (id) {
										if (hiddenArray.indexOf(id) > -1) {
											hiddenArray.splice(hiddenArray.indexOf(id), 1);
										} else {
											hiddenArray.push(id);
										}
										service.loadTrafficChart(trafficData);
									}
								}
							}
						});
					};

					return service;

				}
			]);
		})();
