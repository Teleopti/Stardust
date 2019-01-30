'use strict';

(function () {
	angular
		.module('wfm.rtaTestShared')
		.service('BackendFaker', constructor);

	constructor.$inject = ['$httpBackend'];

	function constructor($httpBackend) {

		var endpoints = [];

		var service = {
			with: {},
			clear: {
				all: function () {
					endpoints.forEach(function (endpoint) {
						endpoint.clear();
					})
				}
			},
			lastParams: {},
			data: {}
		};

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
			var lastParams = undefined;

			var endpoint = {
				with: function (item) {
					data = spec.add(data, item);
					return service.with;
				},
				clear: function () {
					data = spec.clear();
				},
				lastParams: function () {
					return lastParams;
				},
				data: function () {
					return data;
				}
			};

			fakeHttpCall(spec.url, function (params) {
				lastParams = params;
				return [200, data];
			});

			service.with[spec.name] = endpoint.with;
			service.clear[spec.name] = endpoint.clear;
			service.lastParams[spec.name] = endpoint.lastParams;
			service.data[spec.name] = endpoint.data;

			endpoints.push(endpoint);
		}

		service.fake = function fake(specOrUrl, responseMaybe) {
			if (specOrUrl.url) {
				var spec = specOrUrl;
				fakeEndpoint(spec)
			} else {
				var url = specOrUrl;
				fakeHttpCall(url, responseMaybe);
			}
		};

		service.fake({
			name: 'toggles',
			url: /ToggleHandler\/AllToggles(.*)/,
			clear: function () {
				return {}
			},
			add: function (data, item) {
				data[item] = true;
				return data;
			}
		});

		service.fake({
			name: 'translation',
			url: /Global\/Language?lang=en/,
			clear: function () {
				return {}
			},
			add: function (data, item) {
				data[item.Name] = item.Value;
				return data;
			}
		});

        service.fake({
            name: 'currentUserInfo',
			url: /Global\/User\/CurrentUser/,
            clear: function () {
                return { DateFormatLocale: "sv-SE" }
            },
            add: function (data, item) {
                return item;
            }
        });

		return service;

	}

})();
