define([
		'knockout',
		'lazy',
		'views/realtimeadherencesite/team'
],
	function (ko,
		lazy,
		team) {
	return function () {

		var that = {};
		that.teams = ko.observableArray();
		that.fill = function(data) {
			for (var i = 0; i < data.length; i++) {
				var newTeam = team();
				newTeam.Id = data[i].Id;
				newTeam.Name = data[i].Name;
				newTeam.NumberOfAgents = data[i].NumberOfAgents;
				that.teams.push(newTeam);
			}
		};


		return that;
	};


}
);