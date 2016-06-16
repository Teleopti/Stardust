(function() {
	'use strict';
	angular.module('wfm.intraday')
		.controller('IntradayStaffingCtrl', [
			'$scope', '$stateParams', 'intradayStaffingService', 'Toggle', 'intradayService', '$filter', 'NoticeService', '$translate', '$q',
			function($scope, $stateParams, intradayStaffingService, toggleService, intradayService, $filter, NoticeService, $translate, $q) {
				var chartData = {};
				$scope.isVisible = true;
				$scope.skills;

				if ($stateParams.date) {
					$scope.intervalDate = $stateParams.date;
				} else {
					$scope.intervalDate = moment();
				};
				if ($stateParams.id) {
					$scope.selectedSkill = {Name:"Loading"};
					 intradayStaffingService.matchSkill($stateParams.id).then(function(response){
						$scope.selectedSkill = response;
					});
				};

			 	toggleService.togglesLoaded.then(function(result) {
					$scope.isVisible = toggleService.Intraday_ResourceCalculateReadModel_39200;
				});

				intradayService.getSkills.query().$promise.then(function(response) {
					$scope.skills = response;
				});

				$scope.TriggerResourceCalculate = function() {
					console.log('triggered!');
					intradayStaffingService.TriggerResourceCalculate.query().$promise.then(function(response) {
						console.log(response);
					})
				};
				$scope.updateSelectedSkill = function(skill){
					$scope.selectedSkill = skill;
				};

				var initArrays = function() {
					chartData.Forcast = ['Forcasted'];
					chartData.Staffing = ['Staffing'];
					chartData.Intervals = ['x'];
				};
				var extractRelevantData = function(data) {
					angular.forEach(data, function(single) {
						chartData.Forcast.push(single.Forecast);
						chartData.Staffing.push(single.StaffingLevel);
						chartData.Intervals.push(new Date(single.StartDateTime));
					});
					generateChart();
				};

				$scope.runIntradayFetch = function() {
					initArrays();
					var actualDate = moment($scope.intervalDate).format('YYYY-MM-DD');
					var actualSkill = $scope.selectedSkill.Id;

					$scope.loading = true;
					intradayStaffingService.resourceCalculate.query({
						date: actualDate,skillId:actualSkill
					}).$promise.then(function(response) {
						extractRelevantData(response.Intervals);
						$scope.loading = false;
					});
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
