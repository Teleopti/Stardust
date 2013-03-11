define([
        'knockout',
        'navigation',
		'views/personschedule/timeline',
        'noext!application/resources'
    ], function(
        ko,
        navigation,
        timeLineViewModel,
        resources) {

        return function(date) {

            var self = this;
            
            this.Agents = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Agents);

            this.Resources = resources;

            this.Teams = ko.observableArray();
            this.SelectedDateInternal = ko.observable(date);
            this.SelectedTeam = ko.observable();
            this.isLoading = ko.observable(false);

            this.SetAgents = function (agents) {
                self.Agents([]);
                self.Agents.push.apply(self.Agents, agents);
            };

            this.SelectedDate = ko.computed({
                read: function() {
                    return self.SelectedDateInternal().clone();
                },
                write: function(value) {
                    if (value.toDate() == self.SelectedDateInternal().toDate()) return;
                    self.SelectedDateInternal(value);
                }
            });

            this.AddTeams = function(teams) {
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
