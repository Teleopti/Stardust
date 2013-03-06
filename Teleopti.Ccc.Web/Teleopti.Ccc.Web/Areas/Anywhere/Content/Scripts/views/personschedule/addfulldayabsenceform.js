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
            };

            this.StartDateFormatted = ko.computed(function () {
                var value = self.StartDate();
                if (moment.isMoment(value))
                    return value.format(resources.MomentShortDatePattern);
                return value;
            });

            this.AbsenceTypes = ko.observableArray([
                {
                    Id: 1,
                    Name: "Holiday"
                },
                {
                    Id: 2,
                    Name: "Ill"
                }
            ]);

        };
    });