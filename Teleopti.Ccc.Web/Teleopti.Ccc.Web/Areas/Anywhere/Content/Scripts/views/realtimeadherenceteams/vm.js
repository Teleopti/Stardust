define([
		'knockout',
		'lazy',
		'views/realtimeadherenceteams/team',
		'resources'
],
	function (ko,
		lazy,
		team,
		resources) {
	return function () {

		var that = {};
		that.resources = resources;
		that.teams = ko.observableArray();
		that.siteName = ko.observable();
		that.fill = function (data) {
			for (var i = 0; i < data.length; i++) {
				var newTeam = team();
				newTeam.Id = data[i].Id;
				newTeam.Name = data[i].Name;
				newTeam.NumberOfAgents = data[i].NumberOfAgents;
				that.teams.push(newTeam);
			}
		};

		var teamForId = function (id) {
			if (!id)
				return undefined;
			var theTeam = lazy(that.teams())
				.filter(function (x) { return x.Id == id; })
				.first();
			return theTeam;
		};

		that.update = function (data) {
			var existingTeam = teamForId(data.Id);
			if (existingTeam)
				existingTeam.OutOfAdherence(data.OutOfAdherence);
		};

		that.updateFromNotification = function(notification) {
			var data = JSON.parse(notification.BinaryData);
			data.Id = notification.DomainId;
			that.update(data);
		};

		that.setSiteName = function(data) {
			that.siteName(data.Name);
		};

		return that;
	};


}
);