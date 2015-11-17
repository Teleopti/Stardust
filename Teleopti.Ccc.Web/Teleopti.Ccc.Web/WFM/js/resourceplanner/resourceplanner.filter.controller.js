(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', function ($scope, ResourcePlannerFilterSrvc) {
				$scope.maxHits = 5;

				$scope.results = [];
				$scope.selectedResults = [];

				$scope.$watch(function() { return $scope.searchString; },
					function (input) {
						if (input === '' || input === undefined) {
							$scope.results = [];
							return;
						}

						ResourcePlannerFilterSrvc.getData.query({ searchString: input, maxHits: $scope.maxHits }).$promise.then(function (results) {
							results.forEach(function(result) {
								result.selected = false;
							});
							$scope.results = results;
						});
					}
				);

				$scope.selectResultItem = function(item) {
					item.selected = true;
					$scope.selectedResults.push(item);
				}


				$scope.moreResultsExists = function () {
					return $scope.results.length >= $scope.maxHits;
				}
			}
		]);
})();