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
                return self.StartDate().format(resources.MomentShortDatePattern);
            });

            this.EndDateFormatted = ko.computed({
                read: function () {
                    var value = self.EndDate();
                    if (moment.isMoment(value))
                        return value.format(resources.MomentShortDatePattern);
                    return value;
                },
                write: function (value) {
                    if (moment.isMoment(value))
                        self.EndDate(value);
                    self.EndDate(moment(value));
                }
            });

            this.AbsenceTypes = ko.observableArray();

            this.InvalidEndDate = ko.computed(function () {
                if (!self.EndDate())
                    return true;
                if (self.StartDate() && self.StartDate().diff)
                    return self.StartDate().diff(self.EndDate()) > 0;
                return false;
            });
            
            this.Apply = function () {
                $.ajax({
                        url: 'PersonScheduleCommand/AddFullDayAbsence',
                        type: 'POST',
                        cache: false,
                        data: JSON.stringify({
                            StartDate: self.StartDate().format(),
                            EndDate: self.EndDate().format(),
                            AbsenceId: self.AbsenceType(),
                            PersonId: personId
                        }),
                        success: function(data, textStatus, jqXHR) {
                            navigation.GotoPersonSchedule(personId, self.StartDate());
                        }
                    }
                );
            };

        };
    });