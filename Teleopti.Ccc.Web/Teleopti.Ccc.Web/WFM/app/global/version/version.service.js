(function() {
	'use strict';

	angular.module('wfm.versionService').factory('versionService', versionService);

	function versionService() {
		var version = '';
		var service = {
			setVersion: function setVersion(newVersion) {
				version = newVersion;
			},
			getVersion: function setVersion() {
				return version;
			}
		};
		return service;
	}
})();
