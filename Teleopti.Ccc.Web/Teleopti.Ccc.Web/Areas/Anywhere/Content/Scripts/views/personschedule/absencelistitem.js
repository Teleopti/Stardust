define([
    'knockout',
    'moment',
    'navigation',
    'ajax'
], function (
    ko,
    moment,
    navigation,
    ajax
) {

    return function (data) {

        this.StartTime = ko.observable(moment(data.StartTime).format("YYYY-MM-DD HH:mm"));
        this.EndTime = ko.observable(moment(data.EndTime).format("YYYY-MM-DD HH:mm"));

        this.Name = ko.observable(data.Name);

        this.BackgroundColor = ko.observable(data.Color);

        var personId = data.PersonId;
        var date = data.Date;

        var personAbsenceId = data.Id;

        this.ConfirmRemoval = function () {
            var data = JSON.stringify({
                PersonAbsenceId: personAbsenceId
            });
            ajax.ajax(
                {
                    url: 'PersonScheduleCommand/RemoveAbsence',
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
