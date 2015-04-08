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

	angular.module('restResourcePlannerService', ['ngResource']).service('ResourcePlannerSvrc', [
		'$resource', function($resource) {
			this.getPlanningPeriod = $resource('../api/PlanningPeriod', {}, {
				query: { method: 'GET', params: {}, isArray: true }
			});
		}
	]);
})();