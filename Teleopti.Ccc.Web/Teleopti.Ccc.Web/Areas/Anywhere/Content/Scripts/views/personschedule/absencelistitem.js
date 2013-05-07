define([
        'knockout',
        'moment',
        'navigation',
        'noext!application/resources',
        'ajax'
    ], function(
        ko,
        moment,
        navigation,
        resources,
        ajax
    ) {

        return function(data) {

            this.StartTime = ko.observable(moment(data.StartTime).format(resources.DateTimeFormatForMoment));
            this.EndTime = ko.observable(moment(data.EndTime).format(resources.DateTimeFormatForMoment));

            this.Name = ko.observable(data.Name);

            this.BackgroundColor = ko.observable(data.Color);

            var personId = data.PersonId;
            var date = data.Date;

            var personAbsenceId = data.Id;

            this.ConfirmRemoval = function() {
                var data = JSON.stringify({
                    PersonAbsenceId: personAbsenceId
                });
                ajax.ajax(
                    {
                        url: 'PersonScheduleCommand/RemovePersonAbsence',
                        type: 'POST',
                        data: data,
                        success: function(data, textStatus, jqXHR) {
                            navigation.GotoPersonSchedule(personId, date);
                        }
                    }
                );
            };
        };
    });
