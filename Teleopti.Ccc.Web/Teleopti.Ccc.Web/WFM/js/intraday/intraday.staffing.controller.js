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




				intradayStaffingService.resourceCalculate.query().$promise.then(function(response) {
					extractRelevantData(response.Intervals);

				});

				var extractRelevantData = function(data) {
					angular.forEach(data, function(single) {
						chartData.Forcast.push(single.Forecast);
						chartData.Staffing.push(single.StaffingLevel);
					});

					generateChart(chartData);

				}

				var generateChart = function(data) {
					c3.generate({
						bindto: '#staffingChart',
						data: {
							columns: [
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
						zoom: {
							enabled: true,
						},
					});
				}

			}
		]);
})();
