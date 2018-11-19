(function() {
	'use strict';

	angular.module('wfm.requests').service('requestsPermissions', requestsPermissions);

	requestsPermissions.$inject = [];

	function requestsPermissions() {
		var self = this;

		var permissions;

		self.set = function setPermissions(data) {
			permissions = data;
		};

		self.all = function getPermissions() {
			return permissions;
		};
	}
})();
