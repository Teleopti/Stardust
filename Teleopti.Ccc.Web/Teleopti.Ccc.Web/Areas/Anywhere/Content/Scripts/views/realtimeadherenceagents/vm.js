define([
		'knockout',
		'views/realtimeadherenceagents/agent'
],
	function (ko,
		agent) {
	return function () {

		var that = {};

		that.agents = ko.observableArray(); 
		
		that.fill = function (data) {
			for (var i = 0; i < data.length; i++) {
				var agentState = agent();
				agentState.fill(data[i]);
				that.agents.push(agentState);
			}
		};

		that.update = function (data) {
		};

		return that;
	};
}
);