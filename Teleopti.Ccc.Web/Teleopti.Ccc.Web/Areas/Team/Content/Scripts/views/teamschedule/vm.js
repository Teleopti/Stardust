define([
		'knockout'
	], function (ko) {

	    return function (timeLine, resources) {

	        var self = this;

	        this.TimeLine = timeLine;
	        this.Resources = resources;
	        this.Agents = ko.observableArray();

	        this.AddAgent = function (agent) {
	            self.Agents.push(agent);
	            self.TimeLine.AddAgent(agent);
	        };

	        this.AddAgents = function (agents) {
	            for (var i = 0; i < agents.length; i++) {
	                self.AddAgent(agents[i]);
	            }
	        };
	    };
	});
