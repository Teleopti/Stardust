(function () {
	'use strict';
	angular.module('wfm.intraday')
	.service('intradayMonitorStaffingService', [
		'$filter', 'intradayService', '$translate', function ($filter, intradayService, $translate) {
			var service = {};

			var staffingData = {
				forecastedStaffing: {
					max: {},
					series: [],
					updatedSeries: []
				},
				hasMonitorData: false,
				timeSeries: [],
				actualStaffingSeries: [],
				currentInterval: [],
				scheduledStaffing: []
			};

			var hiddenArray = [];

			service.setStaffingData = function (result, showOptimalStaffing, showScheduledStaffing) {
				staffingData.timeSeries = [];
				staffingData.forecastedStaffing.series = [];
				staffingData.forecastedStaffing.updatedSeries = [];
				staffingData.actualStaffingSeries = [];
				staffingData.scheduledStaffing = [];

				if (result.DataSeries == null)
					return staffingData;
				staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;
				staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;

				if (showOptimalStaffing)
					staffingData.actualStaffingSeries = result.DataSeries.ActualStaffing;

				if (showScheduledStaffing)
					staffingData.scheduledStaffing = result.DataSeries.ScheduledStaffing;

				angular.forEach(result.DataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, staffingData.timeSeries);

				if (staffingData.timeSeries[0] != 'x') {
					staffingData.timeSeries.splice(0, 0, 'x');
				}

				staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');
				staffingData.forecastedStaffing.updatedSeries.splice(0, 0, 'Updated_forecasted_staffing');
				staffingData.actualStaffingSeries.splice(0, 0, 'Actual_staffing');
				staffingData.scheduledStaffing.splice(0, 0, 'Scheduled_staffing');

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

			service.pollSkillData = function (selectedItem, toggles) {
				intradayService.getSkillStaffingData.query(
					{
						id: selectedItem.Id
					})
					.$promise.then(function (result) {
						return service.setStaffingData(result, toggles.showOptimalStaffing, toggles.showScheduledStaffing);
					},
					function (error) {
						staffingData.hasMonitorData = false;
					});
			};

			service.pollSkillAreaData = function (selectedItem, toggles) {
				intradayService.getSkillAreaStaffingData.query(
					{
						id: selectedItem.Id
					})
					.$promise.then(function (result) {
						return service.setStaffingData(result, toggles.showOptimalStaffing, toggles.showScheduledStaffing);
					},
					function (error) {
						staffingData.hasMonitorData = false;
					});
			};

			service.loadStaffingChart = function(staffingData) {
					var staffingChart = c3.generate({
						bindto: '#staffingChart',
						data: {
							x: 'x',
							columns: [
								staffingData.timeSeries,
								staffingData.forecastedStaffing.series,
								staffingData.forecastedStaffing.updatedSeries,
								staffingData.actualStaffingSeries,
								staffingData.scheduledStaffing
							],
							type: 'line',
							groups: [
								['Forecasted_staffing'], ['Updated_forecasted_staffing'], ['Actual_staffing'], ['Scheduled_staffing']
							],
							order: 'asc',
							hide: hiddenArray,
							names: {
								Forecasted_staffing: $translate.instant('ForecastedStaff') + ' ←',
								Updated_forecasted_staffing: $translate.instant('ReforecastedStaff') + ' ←',
								Actual_staffing: $translate.instant('RequiredStaff') + ' ←',
								Scheduled_staffing: $translate.instant('ScheduledStaff') + ' ←'
								},
								colors: {
									Forecasted_staffing: '#0099FF',
									Updated_forecasted_staffing: '#E91E63',
									Actual_staffing: '#FB8C00',
									Scheduled_staffing: '#F488C8'
							}
						},
						axis: {
							x: {
								label: {
									text: $translate.instant('SkillTypeTime'),
									position: 'outer-center'
								},
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
								label: {
									text: $translate.instant('Agents'),
									position: 'outer-middle'
								},
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
					},
					legend: {
						item: {
							onclick: function (id) {
								if (hiddenArray.indexOf(id) > -1) {
									hiddenArray.splice(hiddenArray.indexOf(id), 1);
								} else {
									hiddenArray.push(id);
								}
								service.loadStaffingChart(staffingData);
							}
						}
					}
				});
			}

			return service;

		}
	]);
})();
