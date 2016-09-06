(function() {
	'use strict';
	angular.module('wfm.intraday')
	.service('intradayMonitorStaffingService', [
		'$filter', 'intradayService','$translate', function($filter, intradayService, $translate) {
			var service = {};

			var staffingData = {
				forecastedStaffing: {
					max: {},
					series: [],
					updatedSeries: []
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
				staffingData.forecastedStaffing.updatedSeries = [];

				staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;
				staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;
				// staffingData.latestActualInterval = $filter('date')(result.LatestActualIntervalStart, 'shortTime') + ' - ' + $filter('date')(result.LatestActualIntervalEnd, 'shortTime');

				staffingData.timeSeries = [];

				angular.forEach(result.DataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, staffingData.timeSeries);

				if (staffingData.timeSeries[0] != 'x' ) {
					staffingData.timeSeries.splice(0, 0, 'x');
				}

				staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');
				staffingData.forecastedStaffing.updatedSeries.splice(0, 0, 'Updated_forecasted_staffing');

				staffingData.hasMonitorData = true;

				var forecastedStaffingMax = Math.max.apply(Math, result.DataSeries.ForecastedStaffing);
				var updatedForecastedStaffingMax = Math.max.apply(Math, result.DataSeries.UpdatedForecastedStaffing);
				if (forecastedStaffingMax > updatedForecastedStaffingMax) {
					staffingData.forecastedStaffing.max = forecastedStaffingMax;
				} else {
					staffingData.forecastedStaffing.max = updatedForecastedStaffingMax;
				}
				

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
									staffingData.forecastedStaffing.series,
									staffingData.forecastedStaffing.updatedSeries
								],
								names: {
									Forecasted_staffing: $translate.instant('ForecastedStaff') + ' ←',
									Updated_forecasted_staffing: 'xxUpdated Forecasted Staffing' + ' ←'
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
