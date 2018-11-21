'use strict';
(function() {
	angular
		.module('wfm.forecasting')
		.service('fakeForecastingBackend', fakeForecastBackendService);

	function fakeForecastBackendService($httpBackend) {
		var skills = {};
		var forecastStatus = {};
		var scenarios = [];
		var forecastData = {};

		var paramsOf = function (url) {
			var result = {};
			var queryString = url.split("?")[1];
			if (queryString == null) {
				return result;
			}
			var params = queryString.split("&");
			angular.forEach(params,
				function (t) {
					var kvp = t.split("=");
					if (result[kvp[0]] != null)
						result[kvp[0]] = [].concat(result[kvp[0]], kvp[1]);
					else
						result[kvp[0]] = kvp[1];
				});
			return result;
		};

		var fakeGet = function (url, response) {
			$httpBackend.whenGET(url)
				.respond(function (method, url, data, headers, params) {
					var params2 = paramsOf(url);
					return response(params2, method, url, data, headers, params);
				});
		};

		var fakePost = function(url, response) {
			$httpBackend.whenPOST(url)
				.respond(function(method, url, data, headers, params) {
					return response(JSON.parse(data), params, method, url, headers);
				});
		};

		fakeGet('../api/Forecasting/Skills',
			function () {
				return [200, skills];
			});

		fakeGet('../api/Global/Scenario',
			function () {
				return [200, scenarios];
			});

		fakePost('../api/Forecasting/LoadForecast',
			function() {
				return [201, forecastData];
			});

		this.withForecastData = function (forecastDays) {
			forecastData.WorkloadId = "123";
			forecastData.ForecastDays = forecastDays;
			return this;
		}

		this.withSkill = function (skill) {
			skills = skill;
			return this;
		}

		this.withForecastStatus = function (status) {
			forecastStatus.IsRunning = status;
			return this;
		}

		this.withScenario = function (scenario) {
			scenarios.push(scenario);
			return this;
		}

		this.clear = function () {
			skills = {};
			forecastStatus = {};
			scenarios = [];
			forecastData = {};
		};
	}
})();
