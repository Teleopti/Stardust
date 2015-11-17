(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope','ResourcePlannerFilterSrvc', function ($scope, ResourcePlannerFilterSrvc) {
				$scope.selected = [];

				$scope.$watch(function () { return $scope.searchString; },
						function (input) {
							if (input === '' || input === undefined ) return;
							ResourcePlannerFilterSrvc.getData.query({searchString:input}).$promise.then(function(results){
								$scope.results = results;
							});
						}
				);

				$scope.selectResultItem = function(item){
					$scope.selected.push(item);
				}

        	}
    ])
})();
