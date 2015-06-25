﻿(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('PlanningPeriodSvrc', [
		'$resource', function($resource) {
			//scheduling
			this.launchScheduling = $resource('../api/ResourcePlanner/Schedule/FixedStaff', {}, {
				query: { method: 'POST', params: name, isArray: false }
			});
			this.launchOptimization = $resource('../api/ResourcePlanner/optimize/FixedStaff/:id', { id: "@id" }, {
				query: { method: 'POST', params: name, isArray: false }
			});
			//planning period
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod/:id', {id:"@id"}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.changeRange = $resource('../api/resourceplanner/changeplanningperiod/:id', { id: "@id" }, {
				update: { method: 'PUT', params: {} , isArray: false }
			});
			this.getSuggestions = $resource('../api/resourceplanner/planningperiod/:id/suggestions', { id: "@id" }, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			//toggle
			this.isEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle', { toggle: "@toggle" }, {
				query: { method: 'GET', params: {}, isArray: false }
			});
		}
	]);
})();