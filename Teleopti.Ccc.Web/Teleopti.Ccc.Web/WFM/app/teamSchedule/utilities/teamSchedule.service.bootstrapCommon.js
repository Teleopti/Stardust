(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('bootstrapCommon', bootstrapCommon);

	bootstrapCommon.$inject = ['$q', 'TeamSchedule', 'teamsPermissions', 'teamsBootstrapData'];

	function bootstrapCommon($q, teamScheduleSvc, teamsPermissions, teamsBootstrapData) {
		var self = this;

		var tasks = {
			permissions: teamScheduleSvc.PromiseForloadedPermissions(),
			scheduleAuditTrailSetting: teamScheduleSvc.getScheduleAuditTrailSetting()
		};

		var readyDefer = $q.defer();
		self.ready = function ready() {
			return readyDefer.promise;
		}

		$q.all(tasks)
			.then(function (data) {
				teamsPermissions.set(data.permissions);
				teamsBootstrapData.setScheduleAuditTrailSetting(data.scheduleAuditTrailSetting);
				readyDefer.resolve();
			});
	}

})();