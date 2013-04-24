define([
    'knockout',
    'moment'
], function (
    ko,
    moment
) {

    return function (data) {

        this.StartTime = ko.observable(moment(data.StartTime).format("YYYY-MM-DD HH:mm"));
        this.EndTime = ko.observable(moment(data.EndTime).format("YYYY-MM-DD HH:mm"));

        this.Name = ko.observable(data.Name);

        this.BackgroundColor = ko.observable(data.Color);

        var personId = data.PersonId;
        var absenceId = data.Id;

        this.ConfirmRemoval = function () {

        };
    };
});
