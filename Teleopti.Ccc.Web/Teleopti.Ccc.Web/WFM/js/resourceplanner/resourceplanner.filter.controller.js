(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', 'ResourcePlannerSvrc', function ($scope, ResourcePlannerFilterSrvc, ResourcePlannerSvrc) {
				$scope.maxHits = 5;
				$scope.results = [];
				$scope.selectedResults = [];
				$scope.dayOffsPerWeek = {
					MinDayOffsPerWeek: 2,
					MaxDayOffsPerWeek: 3
				};
				$scope.consecDaysOff = {
					MinConsecDaysOff: 2,
					MaxConsecDaysOff: 3
				};
				$scope.consecWorkDays = {
					MinConsecWorkDays: 2,
					MaxConsecWorkDays:3
				}


				$scope.$watch(function() { return $scope.searchString; },
					function (input) {
						if (input === '' || input === undefined) {
							$scope.results = [];
							return;
						}

						ResourcePlannerFilterSrvc.getData.query({ searchString: input, maxHits: $scope.maxHits }).$promise.then(function (results) {
							$scope.results = results;
						});
					}
				);
				$scope.isValid = function () {
					return $scope.isValidDayOffsPerWeek() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidFilters();
				}

				$scope.isValidDayOffsPerWeek = function () {
					return $scope.dayOffsPerWeek.MinDayOffsPerWeek <= $scope.dayOffsPerWeek.MaxDayOffsPerWeek;
				}

				$scope.isValidConsecDaysOff = function () {
					return $scope.consecDaysOff.MinConsecDaysOff <= $scope.consecDaysOff.MaxConsecDaysOff;
				}

				$scope.isValidConsecWorkDays = function () {
					return $scope.consecWorkDays.MinConsecWorkDays <= $scope.consecWorkDays.MaxConsecWorkDays;
				}

				$scope.isValidFilters = function () {
					return $scope.selectedResults.length > 0;
				}

				$scope.selectResultItem = function (item) {
					if($scope.selectedResults.indexOf(item)<0)
						$scope.selectedResults.push(item);
				}

				$scope.moreResultsExists = function () {
					return $scope.results.length >= $scope.maxHits;
				}

				$scope.persist = function () {
					if ($scope.isValid()) {
						ResourcePlannerSvrc.saveDayoffRules.update(
						{
							MinDayOffsPerWeek: $scope.dayOffsPerWeek.MinDayOffsPerWeek,
							MaxDayOffsPerWeek: $scope.dayOffsPerWeek.MaxDayOffsPerWeek,
							MinConsecutiveWorkdays: $scope.consecWorkDays.MinConsecWorkDays,
							MaxConsecutiveWorkdays: $scope.consecWorkDays.MaxConsecWorkDays,
							MinConsecutiveDayOffs: $scope.consecDaysOff.MinConsecDaysOff,
							MaxConsecutiveDayOffs: $scope.consecDaysOff.MaxConsecDaysOff,
							Name: $scope.name,
							Default: false,
							Filters: $scope.selectedResults
						});
						return true;
					}
					return false;
				}
			}
		]);
})();