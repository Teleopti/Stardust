'use strict';

(function () {
	angular
		.module('wfm.rtaTestShared')
		.service('BackendFaker', constructor);

	constructor.$inject = ['$httpBackend'];

	function constructor($httpBackend) {

		var service = {};

		function fakeHttpCall(url, response) {
			$httpBackend.whenGET(url)
				.respond(function (method, url, data, headers, params) {
					return response(params, data, method, url, headers);
				});
			$httpBackend.whenPOST(url)
				.respond(function (method, url, data, headers, params) {
					return response(JSON.parse(data), params, method, url, headers);
				});
		}

		function fakeEndpoint(spec) {

			spec.add = spec.add || function (data, item) {
				data.push(item);
				return data;
			};
			spec.clear = spec.clear || function () {
				return [];
			};

			var data = spec.clear();

			var endpoint = {
				with: function (item) {
					data = spec.add(data, item);
				},
				clear: function () {
					data = spec.clear();
				},
				lastParams: undefined
			};

			fakeHttpCall(spec.url, function (params) {
				endpoint.lastParams = params;
				return [200, data];
			});

			service[spec.name] = endpoint;

			endpoints.push(endpoint);
		}

		var endpoints = [];

		service.fake = function fake(specOrUrl, responseMaybe) {
			if (specOrUrl.url) {
				var spec = specOrUrl;
				fakeEndpoint(spec)
			} else {
				var url = specOrUrl;
				fakeHttpCall(url, responseMaybe);
			}
		};

		service.clear = function () {
			endpoints.forEach(function (endpoint) {
				endpoint.clear();
			})
		};

		service.fake({
			name: 'toggles',
			url: /ToggleHandler\/AllToggles(.*)/,
			clear: function () {
				return {}
			},
			add: function (data, item) {
				data[item] = true;
			}
		});

		return service;

	}

})();
