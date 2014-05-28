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
			if (!data) {
				that.agents.forEach(function(agent) { // TODO refacto 
					var newagentState = agentstate();
					newagentState.fill({PersonId: agent.PersonId}, agent.Name, agent.TimeZoneOffset);
					var newexistingState = that.agentStates().filter(function(obj) {
						return obj.PersonId === agent.PersonId;
					});
					if (newexistingState.length !== 0) {
						that.agentStates.remove(newexistingState[0]);
					}
					that.agentStates.push(newagentState);
				});
			} else {
				for (var i = 0; i < data.length; i++) {
					var agentState = agentstate();
					var a = that.getAgent(data[i].PersonId);
					agentState.fill(data[i], a.Name, a.TimeZoneOffset);
					var existingState = that.agentStates().filter(function (obj) {
						return obj.PersonId === data[i].PersonId;
					});
					if (existingState.length !== 0) {
						that.agentStates.remove(existingState[0]);
					}
					that.agentStates.push(agentState);
				}
			}
			that.agentStates.sort(function(left, right) { return left.Name == right.Name ? 0 : (left.Name < right.Name ? -1 : 1); });
		}

		that.getAgent = function(id) {
			var a = that.agents.filter(function(item) {
				return item.PersonId === id;
				});
			return a[0];
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