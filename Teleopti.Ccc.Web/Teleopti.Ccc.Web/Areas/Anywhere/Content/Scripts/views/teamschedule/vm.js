define([
        'knockout',
        'navigation'
    ], function(
        ko,
        navigation) {

        return function(date) {

            var self = this;

            this.Teams = ko.observableArray();
            this.SelectedDateInternal = ko.observable(date);
            this.SelectedTeam = ko.observable();
            this.isLoading = ko.observable(false);

            this.SelectedDate = ko.computed({
                read: function() {
                    return self.SelectedDateInternal().clone();
                },
                write: function (value) {
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
