(function () {
	'use strict';

	angular.module('wfm.teamSchedule').service('teamsPermissions', teamsPermissions);

	teamsPermissions.$inject = [];

	function teamsPermissions() {
		var self = this;

		var permissions;

		self.set = function setPermissions(data) {
			permissions = data;
		}

		self.all = function getPermissions() {
			return permissions;
		}
	}

})();