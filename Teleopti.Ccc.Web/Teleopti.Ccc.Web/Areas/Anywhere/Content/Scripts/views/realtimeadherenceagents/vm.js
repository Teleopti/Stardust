define([
		'knockout',
		'views/realtimeadherenceagents/agent'
],
	function (ko,
		agent) {
	return function () {

		var that = {};

		that.agentStates = ko.observableArray(); 
		
		that.fill = function (data) {
			
			for (var i = 0; i < data.length; i++) {
				var agentState = agent();
				agentState.fill(data[i]);
				that.agentStates.push(agentState);
			}
		};

		that.refreshTimeInState = function() {
			that.agentStates().forEach(function (item) {
				item.refreshTimeInState();
			});
		};

		that.updateFromNotification = function (notification) {
			console.log('notif', notification);
			var data = JSON.parse(notification.BinaryData);
			data.Id = notification.DomainId;
			that.agentStates.removeAll();
			that.fill(data.AgentStates);
		}

		return that;
	};
}
);