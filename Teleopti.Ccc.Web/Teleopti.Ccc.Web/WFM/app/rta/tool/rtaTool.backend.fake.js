'use strict';
(function () {
	angular
		.module('wfm.rtaTool')
		.factory('FakeRtaToolBackend', FakeRtaToolBackend);

	FakeRtaToolBackend.$inject = ['BackendFaker'];

	function FakeRtaToolBackend(faker) {

		var service = {
			clear: clear,
			withAgent: withAgent,
			withPhoneState: withPhoneState,
			lastAgentsParams: null
		};

		var agents = [];
		var phoneStates = [];

		faker.fake(/ToggleHandler\/AllToggles(.*)/,
			function () {
				return [200, []];
			});

		faker.fake(/\.\.\/api\/RtaTool\/PhoneStates/,
			function () {
				return [200, phoneStates];
			});

		faker.fake(/\.\.\/api\/RtaTool\/Agents(.*)/,
			function (params) {
				service.lastAgentsParams = params;
				return [200, agents];
			});

		function withAgent(agent) {
			agents.push(agent);
			return this;
		}

		function withPhoneState(phoneState) {
			phoneStates.push(phoneState);
			return this;
		}

		function clear() {
			agents = [];
			phoneStates = [];
		}

		return service;
	};
})();