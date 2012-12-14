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

			this.AddAgent = function (agent) {
				self.Agents.push(agent);
				self.TimeLine.AddAgent(agent);
			};

			this.AddTeam = function (team) {
				self.Teams.push(team);
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
