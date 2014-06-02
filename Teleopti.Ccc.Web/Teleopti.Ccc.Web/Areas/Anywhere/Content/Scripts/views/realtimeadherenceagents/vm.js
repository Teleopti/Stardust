define([
		'knockout',
		'views/realtimeadherenceagents/agentstate',
		'views/realtimeadherenceagents/agent',
		'resources'
],
	function (ko,
		agentstate,
		agent,
		resources) {
	return function () {

		var that = {};

		that.agents = []; 
		that.agentStates = ko.observableArray();
		that.resources = resources;
		that.teamName = ko.observable();
		that.siteName = ko.observable();
		that.siteId = ko.observable();
		that.siteURI = ko.observable();
		
		
		that.fillAgents = function (data) {
			for (var i = 0; i < data.length; i++) {
				var a = agent();
				a.fill(data[i]);
				that.agents.push(a);
				that.teamName(data[i].TeamName);
				that.siteName(data[i].SiteName);
				that.siteId(data[i].SiteId);
			}
			that.siteURI('#realtimeadherenceteams/' + that.siteId());
		};

		that.fillAgentsStates = function (data) {
			var newagentState, existingState;

			if (!data) {
				that.agents.forEach(function(agent) {
					existingState = that.getExistingAgentState(agent.PersonId);
					if (existingState.length !== 0) {
						existingState[0].fill({PersonId: agent.PersonId}, agent.Name, agent.TimeZoneOffset);
					} else {
						newagentState = agentstate();
						newagentState.fill({ PersonId: agent.PersonId}, agent.Name, agent.TimeZoneOffset);
						that.agentStates.push(newagentState);
					}
				});
			} else {
				for (var i = 0; i < data.length; i++) {
					var a = that.getAgent(data[i].PersonId);	
					existingState = that.getExistingAgentState(data[i].PersonId);
					if (existingState.length !== 0) {
						existingState[0].fill(data[i], a.Name, a.TimeZoneOffset);
				} else {
						newagentState = agentstate();
						newagentState.fill(data[i], a.Name, a.TimeZoneOffset);
						that.agentStates.push(newagentState);
					}
				}
			}
			that.agentStates.sort(function(left, right) { return left.Name == right.Name ? 0 : (left.Name < right.Name ? -1 : 1); });
		}

		that.getExistingAgentState = function(id) {
			var existingState = that.agentStates().filter(function(obj) {
						return obj.PersonId === id;
				});
			return existingState;
		}

		that.getAgent = function(id) {
			var agent = that.agents.filter(function(item) {
				return item.PersonId === id;
				});
			return agent[0];
		}

		that.refreshTimeInState = function() {
			that.agentStates().forEach(function (item) {
				item.refreshTimeInState();
			});
		};

		that.refreshAlarmTime = function () {
			that.agentStates().forEach(function (item) {
				item.refreshAlarmTime();
			});
		};

		that.updateFromNotification = function (notification) {
			var data = JSON.parse(notification.BinaryData);
			data.Id = notification.DomainId;
			that.fillAgentsStates(data.AgentStates);
		}

		return that;
	};
}
);