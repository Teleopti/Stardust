define([
		'knockout'
	], function(ko) {

		return function() {

			var self = this;

			this.Agents = ko.observableArray();

			this.SetAgents = function(agents) {
				self.Agents([]);
				self.Agents.push.apply(self.Agents, agents);
			};

		};
		
	});
