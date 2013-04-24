define([
    'knockout',
    'moment'
], function (
    ko,
    moment
) {

    return function (data) {

        var self = this;
        
        this.StartTime = ko.observable(moment(data.StartTime).format("YYYY-MM-DD HH:mm"));
        this.EndTime = ko.observable(moment(data.EndTime).format("YYYY-MM-DD HH:mm"));

        this.Name = ko.observable(data.Name);

        this.BackgroundColor = ko.observable(data.Color);

        var personId = data.PersonId;
        var date = data.Date;

        var personAbsenceId = data.Id;

        this.ConfirmRemoval = function () {
            var data = JSON.stringify({
                PersonAbsenceId: personAbsenceId,
            });
            $.ajax({
                url: 'PersonScheduleCommand/RemoveAbsence',
                type: 'POST',
                cache: false,
                contentType: 'application/json; charset=utf-8',
                data: data,
                success: function (data, textStatus, jqXHR) {
                    navigation.GotoPersonSchedule(personId, date);
                }
            }
            );
        };
    };
});
