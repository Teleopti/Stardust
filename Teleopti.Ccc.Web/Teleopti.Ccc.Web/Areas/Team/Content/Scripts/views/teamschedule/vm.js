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
			this.isLoading = ko.observable(false);

			this.AddAgents = function (agents) {
				self.Agents.push.apply(self.Agents, agents);
				self.TimeLine.AddAgents(agents);
			};

			this.AddTeams = function (teams) {
				self.Teams.removeAll();
				self.Teams.push.apply(self.Teams, teams);
			};

			this.NextDay = function () {
				self.SelectedDate().add('d', 1);
				self.SelectedDate.valueHasMutated();
			};

			this.PreviousDay = function () {
				self.SelectedDate().add('d', -1);
				self.SelectedDate.valueHasMutated();
			};

			this.TeamDateCombination = ko.computed(function () {
				var teamId = '';
				var team = self.SelectedTeam();
				if (team != undefined)
					teamId = team.Id;

				return self.SelectedDate().format('YYYYMMDD') + '_' + teamId;
			});
		};
	});
