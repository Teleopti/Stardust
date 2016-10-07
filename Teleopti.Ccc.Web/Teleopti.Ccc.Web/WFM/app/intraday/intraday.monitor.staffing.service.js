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
				timeSeries: [],
                actualStaffingSeries:[],
				currentInterval: []
			};

			var hiddenArray = [];
			var interval;

			service.setStaffingData = function (result) {
				staffingData.timeSeries = [];
				staffingData.forecastedStaffing.series = [];
				staffingData.forecastedStaffing.updatedSeries = [];
			    staffingData.actualStaffingSeries = [];

				staffingData.forecastedStaffing.series = result.DataSeries.ForecastedStaffing;
				staffingData.forecastedStaffing.updatedSeries = result.DataSeries.UpdatedForecastedStaffing;
			    staffingData.actualStaffingSeries = result.DataSeries.ActualStaffing;


				angular.forEach(result.DataSeries.Time, function (value, key) {
					this.push($filter('date')(value, 'shortTime'));
				}, staffingData.timeSeries);

				if (staffingData.timeSeries[0] != 'x' ) {
					staffingData.timeSeries.splice(0, 0, 'x');
				}

				staffingData.forecastedStaffing.series.splice(0, 0, 'Forecasted_staffing');
				staffingData.forecastedStaffing.updatedSeries.splice(0, 0, 'Updated_forecasted_staffing');
			    staffingData.actualStaffingSeries.splice(0, 0, 'Actual_staffing');

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
									staffingData.forecastedStaffing.updatedSeries,
                                    staffingData.actualStaffingSeries
								],
								groups: [
									['Forecasted_staffing'],['Updated_forecasted_staffing'],['Actual_staffing']
								],
								order: 'asc',
								hide: hiddenArray,
								names: {
									Forecasted_staffing: $translate.instant('ForecastedStaff') + ' ←',
									Updated_forecasted_staffing: $translate.instant('ReforecastedStaff') + ' ←',
									Actual_staffing: 'XXActual Staffing' + ' ←'
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
