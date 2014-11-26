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

		ko.utils.arrayForEach(fetchAgents(), function (data) {
			var agent = new agentvm();
			agent.fill(data);

			self.agents().push(agent);
		});
		
	};
});

