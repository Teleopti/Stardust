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

            this.AbsenceType = ko.observable("");

            var startDateObservable = ko.observable("");

            this.StartDate = ko.computed(function () {
                var value = startDateObservable();
                if (moment.isMoment(value))
                    return startDateObservable().format(resources.MomentShortDatePattern);
                return value;
            });

            var endDateObservable = ko.observable("");
            
            this.EndDate = ko.computed({
                read: function () {
                    var value = endDateObservable();
                    if (moment.isMoment(value))
                        return endDateObservable();
                    return "";
                },
                write: function (value) {
                    endDateObservable(moment.utc(value));
                }
            });

            this.SetData = function(data) {
                startDateObservable(data.Date);
                endDateObservable(data.Date);
            };

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