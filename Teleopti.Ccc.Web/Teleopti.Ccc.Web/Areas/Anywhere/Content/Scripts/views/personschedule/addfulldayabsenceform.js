define([
        'knockout',
        'moment',
        'navigation',
        'ajax',
        'resources!r'
    ], function(
        ko,
        moment,
        navigation,
        ajax,
        resources
    ) {

        return function() {

            var self = this;

            this.AbsenceType = ko.observable("");

            this.StartDate = ko.observable(moment());
            this.EndDate = ko.observable(moment());

            var personId;

            this.SetData = function(data) {
                personId = data.PersonId;
                self.StartDate(data.Date);
                self.EndDate(data.Date);
                self.AbsenceTypes(data.Absences);
            };

            this.StartDateFormatted = ko.computed(function() {
                return self.StartDate().format(resources.DateFormatForMoment);
            });

            this.AbsenceTypes = ko.observableArray();

            this.InvalidEndDate = ko.computed(function() {
                if (!self.EndDate())
                    return true;
                if (self.StartDate() && self.StartDate().diff)
                    return self.StartDate().diff(self.EndDate()) > 0;
                return false;
            });

            this.Apply = function() {
                var data = JSON.stringify({
                    StartDate: self.StartDate().format(),
                    EndDate: self.EndDate().format(),
                    AbsenceId: self.AbsenceType(),
                    PersonId: personId
                });
                ajax.ajax({
                        url: 'PersonScheduleCommand/AddFullDayAbsence',
                        type: 'POST',
                        data: data,
                        success: function(data, textStatus, jqXHR) {
                        	navigation.GotoPersonScheduleWithoutHistory(personId, self.StartDate());
                        }
                    }
                );
            };

        };
    });