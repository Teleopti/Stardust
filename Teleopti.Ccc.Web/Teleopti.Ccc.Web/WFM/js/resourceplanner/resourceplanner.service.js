(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('ResourcePlannerSvrc', [
		'$resource', function($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.launchScheduling = $resource('../api/ResourcePlanner/Schedule/FixedStaff', {}, {
				query: { method: 'POST', params: name, isArray: false }
			});
			this.updatePlanningPeriod = $resource('../api/resourceplanner/updateplanningperiod', {}, {
				update: { method: 'POST', params: { }, isArray: false }
			});
			this.isEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle', {toggle:"@toggle"}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
		}
	]);
})();