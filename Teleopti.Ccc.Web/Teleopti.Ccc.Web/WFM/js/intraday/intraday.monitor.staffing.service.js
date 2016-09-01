(function() {
	'use strict';
	angular.module('wfm.intraday')
	.service('intradayMonitorStaffingService', [
		'$filter', 'intradayService','$translate', function($filter, intradayService, $translate) {
			var service = {};

			var staffingData = {
				forecastedStaffing: {
					max: {},
					series:[]
				},
				hasMonitorData:false,
				timeSeries:[],
				currentInterval: []
			};

			var hiddenArray = [];
			var interval;

			service.setStaffingData = function (result) {
				staffingData.timeSeries = [];
				staffingData.forecastedStaffing.series = [];

				staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;
				// staffingData.latestActualInterval = $filter('date')(result.LatestActualIntervalStart, 'shortTime') + ' - ' + $filter('date')(result.LatestActualIntervalEnd, 'shortTime');

				staffingData.timeSeries = [];

				angular.forEach(result.DataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, staffingData.timeSeries);

				if (staffingData.timeSeries[0] != 'x' ) {
					staffingData.timeSeries.splice(0, 0, 'x');
				}

				staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');

				staffingData.hasMonitorData = true;

				staffingData.forecastedStaffing.max = Math.max.apply(Math, result.DataSeries.ForecastedStaffing);

				service.loadStaffingChart(staffingData);
				return staffingData;
			};

			service.getData = function () {
				return staffingData;
			}

			service.pollSkillData = function (selectedItem) {
				intradayService.getSkillStaffingData.query(
					{
						id: selectedItem.Id
					})
					.$promise.then(function (result) {
						return service.setStaffingData(result);
					},
					function (error) {
						staffingData.hasMonitorData = false;
					});
				};

				service.pollSkillAreaData = function (selectedItem) {
					intradayService.getSkillAreaStaffingData.query(
						{
							id: selectedItem.Id
						})
						.$promise.then(function (result) {
							return service.setStaffingData(result);
						},
						function (error) {
							staffingData.hasMonitorData = false;
						});
					};

					service.loadStaffingChart = function (staffingData) {
						var staffingChart = c3.generate({
							bindto: '#staffingChart',
							data: {
								x: 'x',
								columns: [
									staffingData.timeSeries,
									staffingData.forecastedStaffing.series
								]
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
									label: $translate.instant('Agents'),
									tick: {
										format: d3.format('.1f')
									}
								}
							}
						});
					}

					return service;

				}
			]);
		})();
