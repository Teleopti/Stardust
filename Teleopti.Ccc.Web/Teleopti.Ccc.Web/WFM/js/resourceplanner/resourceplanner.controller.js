(function() {
	'use strict';
	angular.module('wfm.resourceplanner', [])
		.controller('ResourceplannerCtrl', [
			'$scope', 'ResourcePlannerSvrc', function($scope, ResourcePlannerSvrc) {
				ResourcePlannerSvrc.getPlanningPeriod.query({}).$promise.then(function(result) {
					$scope.planningPeriod = {
						StartDate: result.StartDate,
						EndDate: result.EndDate,
						Id: result.Id,
						Skills: result.Skills
					};
				});
			}
		]);
})();