(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$state', '$stateParams', 'intradayStaffingService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $state, $stateParams, intradayStaffingService, $filter, NoticeService, $translate, $q) {
				var chartData = {};
				$scope.intervalDate = "loading"
				chartData.Forcast = ['Forcasted'];
				chartData.Staffing = ['Staffing'];
				chartData.Intervals = ['x'];




				intradayStaffingService.resourceCalculate.query().$promise.then(function(response) {
					extractRelevantData(response.Intervals);

				});



				var extractRelevantData = function(data) {
					$scope.intervalDate = getIntervalDates(data);
					angular.forEach(data, function(single) {
						chartData.Forcast.push(single.Forecast);
						chartData.Staffing.push(single.StaffingLevel);
						chartData.Intervals.push(new Date(single.StartDateTime));
					});

					generateChart(chartData);

				};

				var getIntervalDates = function(data){
					return data[0].StartDateTime + " - " + data[data.length-1].EndDateTime;
				};

				var generateChart = function(data) {
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
