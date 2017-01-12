(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('bootstrapCommon', bootstrapCommon);

	bootstrapCommon.$inject = ['$q', 'TeamSchedule', 'teamsPermissions'];

	function bootstrapCommon($q, teamScheduleSvc, teamsPermissions) {
		var self = this;

		var tasks = {
			permissions: teamScheduleSvc.PromiseForloadedPermissions()
		};

		var readyDefer = $q.defer();
		self.ready = function ready() { return readyDefer.promise; }

		$q.all(tasks)
			.then(function(data) {
				teamsPermissions.set(data.permissions);				
				readyDefer.resolve();
			});
	}

})();