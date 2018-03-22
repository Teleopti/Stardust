(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsBootstrapData', teamsBootstrapData);

	teamsBootstrapData.$inject = [];

	function teamsBootstrapData() {
		var self = this;

		var isScheduleAuditTrailEnabled;

		self.setScheduleAuditTrailSetting = function (data) {
			isScheduleAuditTrailEnabled = data;
		}

		self.isScheduleAuditTrailEnabled = function () {
			return isScheduleAuditTrailEnabled;
		}
	}

})();