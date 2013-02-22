define([
		'knockout'
	], function(ko) {

		return function() {

			var self = this;

			this.Agents = ko.observableArray();

			this.AddAgents = function(agentsToAdd) {
				self.Agents.push.apply(self.Agents, agentsToAdd);
			};

		};
		
	});
