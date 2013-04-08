define([
        'knockout',
        'navigation',
		'views/personschedule/timeline',
        'noext!application/resources',
        'moment'
    ], function(
        ko,
        navigation,
        timeLineViewModel,
        resources,
        moment
    ) {

        return function() {

            var self = this;
            
            this.Loading = ko.observable(false);
            
            this.Agents = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Agents);

            this.Resources = resources;

            this.Teams = ko.observableArray();
            this.SelectedTeam = ko.observable();
            this.SelectedDate = ko.observable(moment());

            this.SetAgents = function (agents) {
                self.Agents([]);
                self.Agents.push.apply(self.Agents, agents);
            };
            
            this.SetTeams = function (teams) {
                self.Teams([]);
                self.Teams.push.apply(self.Teams, teams);
            };

            this.NextDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', 1));
            };

            this.PreviousDay = function() {
                self.SelectedDate(self.SelectedDate().add('d', -1));
            };
        };
    });
