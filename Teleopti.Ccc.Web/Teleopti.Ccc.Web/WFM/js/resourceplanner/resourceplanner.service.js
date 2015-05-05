(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('ResourcePlannerSvrc', [
		'$resource', function($resource) {
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod', {}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.launchScheduling = $resource('../api/ResourcePlanner/Schedule/FixedStaff', {}, {
				query: { method: 'POST', params: name, isArray: false }
			});
			this.changeRange = $resource('../api/resourceplanner/planningperiod/:id', { id: "@id" }, {
				update: { method: 'PUT', params: {} , isArray: false }
			});
			this.getSuggestions = $resource('../api/resourceplanner/planningperiod/:id/suggestions', { id: "@id" }, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.isEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle', {toggle:"@toggle"}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
		}
	]);
})();