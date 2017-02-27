(function() {
	angular.module('restResourcePlannerService', ['ngResource']).service('PlanningPeriodSvrc', [
		'$resource', '$http', function($resource, $http) {
			//scheduling
			this.launchScheduling = $resource('../api/ResourcePlanner/Schedule/:id', { id: "@id" }, {
			});
			this.launchOptimization = $resource('../api/ResourcePlanner/Optimize/:id', { id: "@id" }, {
			});
			//planning period
			this.getPlanningPeriod = $resource('../api/resourceplanner/planningperiod/:id', {id:"@id"}, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.lastJobStatus = $resource('../api/resourceplanner/planningperiod/status/:id', { id: "@id" }, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.lastJobResult = $resource('../api/resourceplanner/planningperiod/result/:id', { id: "@id" }, {
				query: { method: 'GET', params: {}, isArray: false }
			});
			this.changeRange = $resource('../api/resourceplanner/planningperiod/:id', { id: "@id" }, {
				update: { method: 'PUT', params: {} , isArray: false }
			});
			this.getSuggestions = $resource('../api/resourceplanner/planningperiod/:id/suggestions', { id: "@id" }, {
				query: { method: 'GET', params: {}, isArray: true }
			});
			this.publishPeriod = $resource('../api/resourceplanner/planningperiod/:id/publish', {id:"@id"},{
				query: { method: 'POST', params: {}, isArray: false}
			});
			this.destroyDayOffRule = $resource('../api/resourceplanner/dayoffrules/:id', {id:'@id'}, {
				remove: { method: 'DELETE', params: {}, isArray: false }
			});

			this.getDayOffRules = function() {
				return $http({
					method: 'GET',
					url: '../api/resourceplanner/dayoffrules'
				});
			}

			this.keepAlive = function() {
				return $http({ url: '../api/resourceplanner/keepalive', method: 'POST' });
			}
		}
	]);
})();
