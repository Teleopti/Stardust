
(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('RtaResourceFactory', RtaResourceFactory);

	RtaResourceFactory.$inject = ['$resource'];

	function RtaResourceFactory($resource) {
		var resource = function(url, paramDefaults, actions, options) {
			actions.query.headers = {
				'Cache-Control': 'no-cache',
				'Pragma': 'no-cache'
			};
			return $resource(url, paramDefaults, actions, options);
		}
		return resource;
	};
})();
