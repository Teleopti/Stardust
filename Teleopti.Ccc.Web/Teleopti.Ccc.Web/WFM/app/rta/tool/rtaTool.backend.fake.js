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
			withPhoneState: withPhoneState
		};

		var agents = [];
		var phoneStates = [];

		faker.fake(/ToggleHandler\/AllToggles(.*)/,
			function () {
				return [200, []];
			});

		faker.fake(/\.\.\/RtaTool\/PhoneStates\/For/,
			function () {
				return [200, phoneStates];
			});

		faker.fake(/\.\.\/RtaTool\/Agents\/For(.*)/,
			function (params) {
				var ret = (function () {
					if (params.siteIds != null) {
						var agentsBySite = agents.filter(function (a) {
							return params.siteIds.indexOf(a.SiteId) >= 0
						});
						return agentsBySite;
					}
					if (params.teamIds != null) {
						var agentsByTeam = agents.filter(function (a) {
							return params.teamIds.indexOf(a.TeamId) >= 0
						});
						return agentsByTeam;
					}
					return agents;
				})();
				return [200, ret];
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