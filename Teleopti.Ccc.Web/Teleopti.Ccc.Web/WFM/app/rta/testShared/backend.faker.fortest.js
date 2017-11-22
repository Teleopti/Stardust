'use strict';

(function () {
	angular
		.module('wfm.rtaTestShared')
		.service('BackendFaker', constructor);

	constructor.$inject = ['$httpBackend'];
	
	function constructor($httpBackend) {

		function fake(url, response) {
			$httpBackend.whenGET(url)
				.respond(function (method, url, data, headers, params) {
					return response(params, data, method, url, headers);
				});
			$httpBackend.whenPOST(url)
				.respond(function (method, url, data, headers, params) {
					return response(JSON.parse(data), params, method, url, headers);
				});
		}

		var toggles = {};

		function withToggle(toggle) {
			toggles[toggle] = true;
			return this;
		}
		
		function clear() {
			toggles = {};
		}

		fake(/ToggleHandler\/AllToggles(.*)/,
			function (params) {
				return [200, toggles];
			});

		return {
			fake: fake,
			withToggle: withToggle,
			clear: clear
		}

	}

})();
