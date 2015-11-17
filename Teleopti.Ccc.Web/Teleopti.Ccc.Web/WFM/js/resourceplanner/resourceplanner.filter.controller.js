(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope', 'ResourcePlannerFilterSrvc', function($scope, ResourcePlannerFilterSrvc) {
				$scope.results = [];

				$scope.$watch(function() { return $scope.searchString; },
					function(input) {
						if (input === '' || input === undefined) return;

						ResourcePlannerFilterSrvc.getData.query({ searchString: input }).$promise.then(function(results) {
							results.forEach(function(result) {
								result.selected = false;
							});
							$scope.results = results;
						});
					}
				);

				$scope.selectResultItem = function(item) {
					item.selected = true;
				}

				$scope.selectedItems = function () {
					return $scope.results.filter(function(item) {
						return item.selected;
					});
				}
			}
		]);
})();