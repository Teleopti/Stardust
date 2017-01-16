(function() {
	'use strict';
	angular.module('wfm.requests').service('requestsPermissions', RequestsPermissions);

	RequestsPermissions.$inject = ['$q', 'TeamSchedule', 'teamsPermissions'];

	function RequestsPermissions($q, TeamsService, teamsPermissions) {
		var svc = this;

		svc.loadPermissions = function() {
			return $q(function(resolve, reject) {
				TeamsService.PromiseForloadedPermissions().then(function (data) {
					teamsPermissions.set(data);
					resolve();
				});
			});
		};
		
	}
})();