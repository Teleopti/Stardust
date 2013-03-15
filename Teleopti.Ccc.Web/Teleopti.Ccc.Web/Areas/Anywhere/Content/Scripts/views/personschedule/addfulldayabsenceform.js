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
            
            this.InvalidEndDate = ko.observable(false);

            var personId;

            this.SetData = function(data) {
                personId = data.PersonId;
                self.StartDate(data.Date);
                self.EndDate(data.Date.format(resources.MomentShortDatePattern));
                self.AbsenceTypes(data.Absences);
            };

            this.StartDateFormatted = ko.computed(function() {
                var value = self.StartDate();
                if (moment.isMoment(value))
                    return value.format(resources.MomentShortDatePattern);
                return value;
            });
            
            this.AbsenceTypes = ko.observableArray();

            var isInValidEndDate = function (startDate, endDate) {
                var startDate = new Date(startDate).setHours(0,0,0,0);
                var endDate = new Date(endDate).setHours(0,0,0,0);
                return endDate.valueOf() < startDate.valueOf();
            };
            
            this.Apply = function () {
                if (isInValidEndDate(self.StartDate(), self.EndDate())) {
                    self.InvalidEndDate(true);
                    return;
                }
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
                        success: function (data, textStatus, jqXHR) {
                            self.InvalidEndDate(false);
                            navigation.GotoPersonSchedule(personId, self.StartDate());
                        }
                    }
                );
            };

        };
    });