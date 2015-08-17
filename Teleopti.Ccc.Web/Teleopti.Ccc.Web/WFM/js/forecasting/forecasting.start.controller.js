'use strict';

angular.module('wfm.forecasting')
	.controller('ForecastingStartCtrl', [
		'$scope', '$state',
		function ($scope, $state) {
			var startDate = moment().utc().add(1, 'months').startOf('month').toDate();
			var endDate = moment().utc().add(2, 'months').startOf('month').toDate();
			$scope.period = { startDate: startDate, endDate: endDate }; //use moment to get first day of next month

			var moreThanTwoYears = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					var dateDiff = new Date($scope.period.endDate - $scope.period.startDate);
					dateDiff.setDate(dateDiff.getDate() - 1);
					return dateDiff.getFullYear() - 1970 >= 2;
				} else
					return false;
			};

			$scope.updateStartDate = function () {
				$scope.period.startDate = angular.copy($scope.period.startDate);
			};

			$scope.updateEndDate = function () {
				if ($scope.period && $scope.period.endDate && $scope.period.startDate) {
					if ($scope.period.startDate > $scope.period.endDate) {
						$scope.period.endDate = angular.copy($scope.period.startDate);
						return;
					}
				}

				$scope.period.endDate = angular.copy($scope.period.endDate);
			};

			$scope.nextStepAll = function (period) {
				$state.go('forecasting-runall', { period: period });
			};

			$scope.disalbeNextStepAll = function () {
				return moreThanTwoYears();
			};

			$scope.nextStepAdvanced = function (period) {
				$state.go('forecasting-target', { period: period });
			};

			$scope.disalbeNextStepAdvanced = function () {
				return moreThanTwoYears();
			};

			$scope.setRangeClass = function (date, mode) {
				if (mode === 'day') {
					var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
					var startDay = new Date($scope.period.startDate).setHours(12, 0, 0, 0);
					var endDay = new Date($scope.period.endDate).setHours(12, 0, 0, 0);

					if (dayToCheck >= startDay && dayToCheck <= endDay) {
						return 'in-date-range';
					}
				}
				return '';
			};
		}
	]);
