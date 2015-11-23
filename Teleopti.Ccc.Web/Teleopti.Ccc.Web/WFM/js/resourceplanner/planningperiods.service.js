(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('PlanningPeriodSvrc', [
		'$resource', '$http', function($resource, $http) {
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
			this.publishPeriod = $resource('../api/resourceplanner/planningperiod/:id/publish', {id:"@id"},{
				query: { method: 'POST', params: {}, isArray: false}
			});
			//toggle
			this.isEnabled = $resource('../ToggleHandler/IsEnabled?toggle=:toggle', { toggle: "@toggle" }, {
				query: { method: 'GET', params: {}, isArray: false }
			});

			this.status = $resource('../api/Status/Scheduling', {}, {
			    get: { method: 'GET', params: {}, isArray: false }
			});

			this.getDayOffRules = function() {
				return $http({
					method: 'GET',
					url: '../api/resourceplanner/dayoffrules'
				});
			}
		}
	]);
})();
