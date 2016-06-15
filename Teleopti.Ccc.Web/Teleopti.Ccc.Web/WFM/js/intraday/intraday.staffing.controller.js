(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$state', '$stateParams', 'intradayStaffingService', 'intradayService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $state, $stateParams, intradayStaffingService, intradayService, $filter, NoticeService, $translate, $q) {
				var chartData = {};

				if ($stateParams.intervalDate) {
					$scope.intervalDate = $stateParams.intervalDate;
				} else {
					$scope.intervalDate = moment();
				};

				$scope.$watch('intervalDate', function() {
					var newDate = moment($scope.intervalDate).format("YYYY-MM-DD");
					runIntradayFetch(newDate);
				});

				$scope.TriggerResourceCalculate = function(){
					console.log('triggered!');
					intradayStaffingService.TriggerResourceCalculate.query().$promise.then(function(response){
						console.log(response);
					})
				};

				var initArrays = function() {
					chartData.Forcast = ['Forcasted'];
					chartData.Staffing = ['Staffing'];
					chartData.Intervals = ['x'];
				};

				intradayService.getSkills.query().$promise.then(function(response) {
					$scope.skills = response;
				});



				var runIntradayFetch = function(newDate) {
					initArrays();
					$scope.loading = true;
					intradayStaffingService.resourceCalculate.query({
						date: newDate
					}).$promise.then(function(response) {
						extractRelevantData(response.Intervals);
						$scope.loading = false;
					});
				};

				var extractRelevantData = function(data) {
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

			}
		]);
})();
