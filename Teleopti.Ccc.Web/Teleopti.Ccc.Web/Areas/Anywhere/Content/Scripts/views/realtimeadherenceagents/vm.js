define([
		'knockout',
		'views/realtimeadherenceagents/agentstate',
		'views/realtimeadherenceagents/agent'
],
	function (ko,
		agentstate,
		agent) {
	return function () {

		var that = {};

		that.agents = []; 
		that.agentStates = ko.observableArray(); 
		
		that.fillAgents = function (data) {
			for (var i = 0; i < data.length; i++) {
				var a = agent();
				a.fill(data[i]);
				that.agents.push(a);
				}
			};

		that.fillAgentsStates = function(data) {
			for (var i = 0; i < data.length; i++) {
				var agentState = agentstate();
				agentState.fill(data[i], that.getAgentName(data[i].PersonId));
				that.agentStates.push(agentState);
			}
		}

		that.getAgentName =function(id) {

			var agent = that.agents.filter(function(item) {
				return item.PersonId === id;
				});
			return agent[0].Name;
		}

		that.refreshTimeInState = function() {
			that.agentStates().forEach(function (item) {
				item.refreshTimeInState();
			});
		};

		that.updateFromNotification = function (notification) {
			var data = JSON.parse(notification.BinaryData);
			data.Id = notification.DomainId;
			that.agentStates.removeAll();
			that.fillAgentsStates(data.AgentStates);
		}

		return that;
	};
}
);