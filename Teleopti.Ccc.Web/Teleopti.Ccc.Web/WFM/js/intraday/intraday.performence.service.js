(function() {
	'use strict';
	angular.module('wfm.intraday')
	.service('intradayPerformanceService', [
		'$filter', 'intradayService','$translate', function($filter, intradayService, $translate) {
			var service = {};

			var performanceData = {
				averageSpeedOfAnswerObj: {
					series: {},
					max : {}
				},
				abandonedRateObj: {
					series: {},
					max : {}
				},
				serviceLevelObj: {
					series: {},
					max : {}
				},
				summary: {},
				hasMonitorData: false,
				timeSeries: [],
				currentInterval: []
			};

			var hiddenArray = [];
			var interval;
			var intervalStart;
			service.setPerformanceData = function (result) {
				resetData();

				performanceData.averageSpeedOfAnswerObj.series = result.StatisticsDataSeries.AverageSpeedOfAnswer;
				performanceData.abandonedRateObj.series = result.StatisticsDataSeries.AbandonedRate;
				performanceData.serviceLevelObj.series = result.StatisticsDataSeries.ServiceLevel;

				performanceData.latestActualInterval = $filter('date')(result.LatestActualIntervalStart, 'shortTime') + ' - ' + $filter('date')(result.LatestActualIntervalEnd, 'shortTime');
				intervalStart = $filter('date')(result.LatestActualIntervalStart, 'shortTime');

				performanceData.averageSpeedOfAnswerObj.max = Math.max.apply(Math, performanceData.averageSpeedOfAnswerObj.series);
				performanceData.abandonedRateObj.max = Math.max.apply(Math, performanceData.abandonedRateObj.series);
				performanceData.serviceLevelObj.max = Math.max.apply(Math, performanceData.serviceLevelObj.series);

				performanceData.averageSpeedOfAnswerObj.series.splice(0, 0, 'ASA');
				performanceData.abandonedRateObj.series.splice(0, 0, 'Abandoned_rate');
				performanceData.serviceLevelObj.series.splice(0, 0, 'Service_level');

				performanceData.summary = {
					summaryAbandonedRate: $filter('number')(result.StatisticsSummary.AbandonRate * 100, 1),
					summaryServiceLevel: $filter('number')(result.StatisticsSummary.ServiceLevel * 100, 1),
					summaryAsa: $filter('number')(result.StatisticsSummary.AverageSpeedOfAnswer, 1)
				};

				angular.forEach(result.StatisticsDataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, performanceData.timeSeries);

				if (performanceData.timeSeries[0] != 'x' ) {
					performanceData.timeSeries.splice(0, 0, 'x');
				}

				performanceData.hasMonitorData = true;
				getCurrent();
				service.loadPerformanceChart(performanceData);
				return performanceData;
			};

			service.getData = function () {
				return performanceData;
			};

			var getCurrent = function () {
				performanceData.currentInterval = [];
				for (var i = 0; i < performanceData.timeSeries.length; i++) {
					if (performanceData.timeSeries[i] === intervalStart) {
						performanceData.currentInterval[i] = performanceData.serviceLevelObj.max;
					}else{
						performanceData.currentInterval[i] = null;
					}
				};
				performanceData.currentInterval[0] = 'Current';
			};

			var resetData = function () {
				performanceData.timeSeries = [];
				performanceData.averageSpeedOfAnswerObj.series = [];
				performanceData.abandonedRateObj.series = [];
				performanceData.serviceLevelObj.series = [];
			};

			service.pollSkillData = function (selectedItem) {
				intradayService.getSkillMonitorStatistics.query(
					{
						id: selectedItem.Id
					})
					.$promise.then(function (result) {
						service.setPerformanceData(result);
					},
					function (error) {
						performanceData.hasMonitorData = false;
					});
				};

				service.pollSkillAreaData = function (selectedItem) {
					intradayService.getSkillAreaMonitorStatistics.query(
						{
							id: selectedItem.Id
						})
						.$promise.then(function (result) {
							service.setPerformanceData(result);
						},
						function (error) {
							performanceData.hasMonitorData = false;
						});
					};

					service.loadPerformanceChart = function (performanceData) {
						var performanceChart = c3.generate({
							bindto: '#performanceChart',
							data: {
								x: 'x',
								columns: [
									performanceData.timeSeries,
									performanceData.serviceLevelObj.series,
									performanceData.abandonedRateObj.series,
									performanceData.averageSpeedOfAnswerObj.series,
									performanceData.currentInterval
								],
								types: {
									Current:'bar'
								},
								colors: {
									ASA: '#0099FF',
									Abandoned_rate: '#E91E63',
									Service_level: '#FB8C00'
								},
								names: {
									ASA: $translate.instant('AverageSpeedOfAnswer') + ' ←',
									Abandoned_rate: $translate.instant('AbandonedRate') + ' ←',
									Service_level: $translate.instant('ServiceLevel') + ' →',
								},
								axes: {
									Service_level: 'y2'
								}
							},
							axis: {
								x : {
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
								y:{
									tick: {
										format: d3.format('.1f')
									}
								},
								y2: {
									show: true,
									tick: {
										format: d3.format('.1f')
									}
								}
							}
						});
					};

					return service;

				}
			]);
		})();
