
(function() {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaResourceFactory', rtaResourceFactory);

	rtaResourceFactory.$inject = ['$resource'];

	function rtaResourceFactory($resource) {
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
