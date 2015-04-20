(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('ResourcePlannerSvrc', [
		'$resource', function ($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.launchScheduling = $resource('../api/ResourcePlanner/Schedule/FixedStaff', {}, {
				query: { method: 'POST', params: name, isArray: false }
			});
		}
	]); 
})();