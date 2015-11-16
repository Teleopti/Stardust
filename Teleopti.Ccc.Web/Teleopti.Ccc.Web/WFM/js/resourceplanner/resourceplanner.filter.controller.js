(function () {
	'use strict';
	angular.module('wfm.resourceplanner')
		.controller('ResourceplannerFilterCtrl', [
			'$scope','ResourcePlannerFilterSrvc', function ($scope, ResourcePlannerFilterSrvc) {
				$scope.$watch(function () { return $scope.searchInput; },
						function (input) {
							if (input == "") return;
							ResourcePlannerFilterSrvc.getData.query({searchString:input}).$promise.then(function(result){
								$scope.result = result;
							});
						}
				);
        }
    ])
})();
