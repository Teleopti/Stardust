define([
        'knockout',
        'moment',
        'noext!application/resources'
], function (
        ko,
        moment,
        resources
    ) {

        return function() {

            var self = this;
            
            this.AbsenceType = ko.observable("");

            this.StartDate = ko.observable();
            this.EndDate = ko.observable();

            this.SetData = function (data) {
                self.StartDate(data.Date);
                self.EndDate(data.Date);
                self.PersonId(data.PersonId);

                self.AbsenceTypes(data.Absences);
            };

            this.StartDateFormatted = ko.computed(function () {
                var value = self.StartDate();
                if (moment.isMoment(value))
                    return value.format(resources.MomentShortDatePattern);
                return value;
            });

            this.AbsenceTypes = ko.observableArray();

            this.Apply = function() {
                $.ajax({
                        url: 'PersonSchedule/AddFullDayAbsence',
                        cache: false,
                        dataType: 'json',
                        data: {
                            StartDate: self.StartDate,
                            EndDate: self.EndDate,
                            Absence: self.AbsenceType,
                            PersonId: self.PersonId
                        },
                        success: function (data, textStatus, jqXHR) {
                            navigation.GotoPersonSchedule(self.PersonId(), self.StartDate());
                        }
                    }
                );
            };

        };
    });