define([
    'knockout',
    'moment'
], function (
    ko,
    moment
) {

    return function (data) {

        this.StartTime = ko.observable(moment(data.Start).format("YYYY-MM-DD HH:mm"));
        this.EndTime = ko.observable(moment(data.Start).add('minutes', data.Minutes).format("YYYY-MM-DD HH:mm"));

        this.Name = ko.observable(data.Title);

        this.BackgroundColor = ko.observable(data.Color);

        this.ConfirmRemoval = function () {

        };
    };
});
