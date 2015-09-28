define([
	'knockout',
	'agentvm',
	'rta'
], function(
	ko,
	agentvm,
	rta
) {

	return function (fetchAgents) {
		var self = this;

		fetchAgents = fetchAgents || rta.FetchAgents;

		self.agents = ko.observableArray();
		self.authenticationKey = ko.observable('!#¤atAbgT%');

		ko.utils.arrayForEach(fetchAgents(), function (data) {
			var agent = new agentvm();
			agent.fill(data, self.authenticationKey);

			self.agents().push(agent);
		});
		
	};
});

