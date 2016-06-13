(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$state', '$stateParams', 'intradayStaffingService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $state, $stateParams, intradayStaffingService, $filter, NoticeService, $translate, $q) {
				$scope.intervalDate = $stateParams.intervalDate;
				var chartData = {};
				chartData.Forcast = ['Forcasted'];
				chartData.Staffing = ['Staffing'];
				chartData.Intervals = ['x'];




				intradayStaffingService.resourceCalculate.query().$promise.then(function(response) {
					extractRelevantData(response.Intervals);

				});

				var extractRelevantData = function(data) {
					angular.forEach(data, function(single) {
						chartData.Forcast.push(single.Forecast);
						chartData.Staffing.push(single.StaffingLevel);
						chartData.Intervals.push(new Date(single.StartDateTime));
					});

					generateChart(chartData);

				}

				var generateChart = function(data) {
					console.log(data);
					c3.generate({
						bindto: '#staffingChart',
						data: {
							x: 'x',
							columns: [
								data.Intervals,
								data.Forcast,
								data.Staffing,
							],
							selection: {
								enabled: true,
							},
							types: {
								Forcasted: 'bar'
							},
						},
						axis: {
							x: {
								type: 'timeseries',
								tick: {
									format: 'T%H:%M:%S'
								}
							}
						},
						zoom: {
							enabled: true,
						},
					});
				}

			}
		]);
})();
