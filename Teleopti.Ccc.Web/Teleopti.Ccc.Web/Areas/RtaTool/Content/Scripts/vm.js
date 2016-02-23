define([
	'knockout',
	'agentvm',
	'rta'
], function(
	ko,
	agentvm,
	rta
) {

	return function () {
		var self = this;

		self.authenticationKey = ko.observable('!#¤atAbgT%');
		self.error = ko.observable();

		self.agents = ko.computed(function () {
			var agents = [];
			ko.utils.arrayForEach(rta.Agents(), function (a) {
				a.authenticationKey = self.authenticationKey;
				a.error = self.error;
				agents.push(new agentvm(a));
			});
			return agents;
		});
		
	};
});

