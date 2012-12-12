define([
		'knockout'
	], function (ko) {

		return function (timeLine, date) {

			var self = this;

			this.TimeLine = timeLine;
			this.Agents = ko.observableArray();
			this.Teams = ko.observableArray();
			this.SelectedDate = ko.observable(date);
			this.SelectedTeam = ko.observable();

			this.AddAgent = function (agent) {
				self.Agents.push(agent);
				self.TimeLine.AddAgent(agent);
			};

			this.AddTeam = function (team) {
				self.Teams.push(team);
			};
		};
	});
