define([
        'knockout',
        'navigation',
		'views/personschedule/timeline',
        'resources!r',
        'moment',
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
            
            this.Persons = ko.observableArray();

            this.TimeLine = new timeLineViewModel(this.Persons);

            this.Resources = resources;

            this.Teams = ko.observableArray();
            this.SelectedTeam = ko.observable();
            this.SelectedDate = ko.observable(moment());

            this.SetPersons = function (persons) {
                self.Persons([]);
                self.Persons.push.apply(self.Persons, persons);
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
