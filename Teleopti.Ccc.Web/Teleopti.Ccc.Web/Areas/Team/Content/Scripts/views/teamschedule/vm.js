define([
		'knockout',
		'moment'
	], function (ko, moment) {

		return function (timeLine, date) {

			var self = this;

			this.TimeLine = timeLine;
			this.Agents = ko.observableArray();
			this.Teams = ko.observableArray();
			this.SelectedDate = ko.observable(date);
			this.SelectedTeam = ko.observable();

			this.AddAgents = function (agents) {
				self.Agents.push.apply(self.Agents, agents);
				self.TimeLine.AddAgents(agents);
			};

			this.AddTeams = function (teams) {
				self.Teams.push.apply(self.Teams, teams);
			};

			this.NextDay = function () {
				var newDate = moment(self.SelectedDate()).add('d', 1);
				self.SelectedDate(newDate.toDate());
			};

			this.PreviousDay = function () {
				var newDate = moment(self.SelectedDate()).add('d', -1);
				self.SelectedDate(newDate.toDate());
			};
		};
	});
