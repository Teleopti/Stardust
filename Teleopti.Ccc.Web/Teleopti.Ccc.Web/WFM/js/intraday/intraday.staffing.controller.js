(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$state', '$stateParams', 'intradayStaffingService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $state, $stateParams, intradayStaffingService, $filter, NoticeService, $translate, $q) {
				var chartData = {};

				$scope.intervalDate = moment();

				var initArrays = function(){
					chartData.Forcast = ['Forcasted'];
					chartData.Staffing = ['Staffing'];
					chartData.Intervals = ['x'];
				};

				$scope.$watch('intervalDate', function() {
					var newDate = moment($scope.intervalDate).format("YYYY-MM-DD");
					runIntradayFetch(newDate);
				});

				var runIntradayFetch = function(newDate){
					initArrays();
					$scope.loading = true;
					intradayStaffingService.resourceCalculate.query({
						date:newDate
					}).$promise.then(function(response) {
						extractRelevantData(response.Intervals);
						$scope.loading = false;
					});
				}

				var getIntervalDates = function(data) {
					var date;
					if (data) {
						date = moment(new Date(data[0].StartDateTime))
					}
					return date
				};


				var extractRelevantData = function(data) {

					$scope.intervalDateFormated = getIntervalDates(data);
					angular.forEach(data, function(single) {
						chartData.Forcast.push(single.Forecast);
						chartData.Staffing.push(single.StaffingLevel);
						chartData.Intervals.push(new Date(single.StartDateTime));
					});
					generateChart();
				};
				initArrays();
				var generateChart = function() {
					c3.generate({
						bindto: '#staffingChart',
						data: {
							x: 'x',
							columns: [
								chartData.Intervals,
								chartData.Forcast,
								chartData.Staffing,
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
				};
				generateChart();


			}
		]);
})();
