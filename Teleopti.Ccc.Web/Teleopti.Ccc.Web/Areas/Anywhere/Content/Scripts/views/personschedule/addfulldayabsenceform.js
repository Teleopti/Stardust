define([
        'knockout',
        'moment',
        'navigation',
        'noext!application/resources'
    ], function(
        ko,
        moment,
        navigation,
        resources
    ) {

        return function() {

            var self = this;

            this.AbsenceType = ko.observable("");

            this.StartDate = ko.observable();
            this.EndDate = ko.observable();

            var personId;

            this.SetData = function(data) {
                personId = data.PersonId;
                self.StartDate(data.Date);
                self.EndDate(data.Date);
                self.AbsenceTypes(data.Absences);
            };

            this.StartDateFormatted = ko.computed(function() {
                var value = self.StartDate();
                if (moment.isMoment(value))
                    return value.format(resources.MomentShortDatePattern);
                return value;
            });

            this.AbsenceTypes = ko.observableArray();

            this.Apply = function () {
                $.ajax({
                        url: 'PersonScheduleCommand/AddFullDayAbsence',
                        cache: false,
                        dataType: 'json',
                        data: {
                            StartDate: self.StartDate,
                            EndDate: self.EndDate,
                            Absence: self.AbsenceType,
                            PersonId: self.PersonId
                        },
                        success: function(data, textStatus, jqXHR) {
                            navigation.GotoPersonSchedule(personId, self.StartDate());
                        }
                    }
                );
            };

        };
    });