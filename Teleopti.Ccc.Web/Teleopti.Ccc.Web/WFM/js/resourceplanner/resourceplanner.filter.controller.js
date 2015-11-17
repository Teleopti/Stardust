(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', function ($scope, ResourcePlannerFilterSrvc) {
				$scope.maxHits = 5;
				$scope.results = [];
				$scope.selectedResults = [];

				$scope.dayOffsPerWeek = {
					MinDayOffsPerWeek: 2,
					MaxDayOffsPerWeek: 3
				};

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
					return false;
				}

				$scope.isValidDayOffsPerWeek = function () {
					return $scope.dayOffsPerWeek.MinDayOffsPerWeek <= $scope.dayOffsPerWeek.MaxDayOffsPerWeek;
				}

				$scope.selectResultItem = function (item) {
					$scope.selectedResults.push(item);
				}


				$scope.moreResultsExists = function () {
					return $scope.results.length >= $scope.maxHits;
				}
			}
		]);
})();