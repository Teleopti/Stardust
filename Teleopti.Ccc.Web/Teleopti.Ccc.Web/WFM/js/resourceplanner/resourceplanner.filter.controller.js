(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', 'ResourcePlannerSvrc', function ($scope, ResourcePlannerFilterSrvc, ResourcePlannerSvrc) {
				var maxHits = 5;

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
				$scope.isSearching=false;

				$scope.$watch(function() { return $scope.searchString; },
					function (input) {
						if (input === '' || input === undefined) {
							$scope.results = [];
							return;
						}

						$scope.isSearching = true;
						ResourcePlannerFilterSrvc.getData.query({ searchString: input, maxHits: maxHits }).$promise.then(function (results) {
							$scope.results = results;
							$scope.isSearching = false;
						}, function() {
							$scope.isSearching = false;
						});
					}
				);
				$scope.isValid = function () {
					return $scope.isValidDayOffsPerWeek() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidConsecDaysOff() &&
						$scope.isValidFilters() &&
						$scope.isValidName();
				}

				$scope.isValidDayOffsPerWeek = function () {
					return $scope.dayOffsPerWeek.MinDayOffsPerWeek <= $scope.dayOffsPerWeek.MaxDayOffsPerWeek;
				}

				$scope.isValidConsecDaysOff = function () {
					return $scope.consecDaysOff.MinConsecDaysOff <= $scope.consecDaysOff.MaxConsecDaysOff;
				}
				$scope.clearInput = function () {
					$scope.searchString = '';
					$scope.results = [];
				}

				$scope.isValidConsecWorkDays = function () {
					return $scope.consecWorkDays.MinConsecWorkDays <= $scope.consecWorkDays.MaxConsecWorkDays;
				}

				$scope.isValidFilters = function () {
					return $scope.selectedResults.length > 0;
				}
				$scope.isValidName = function(){
					return $scope.name.length >0;
				}

				$scope.selectResultItem = function (item) {
					if ($scope.selectedResults.indexOf(item) < 0) {
						$scope.selectedResults.push(item);
					}
					$scope.clearInput();
				}

				$scope.moreResultsExists = function () {
					return $scope.results.length >= maxHits;
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
					}
				}
			}
		]);
})();
