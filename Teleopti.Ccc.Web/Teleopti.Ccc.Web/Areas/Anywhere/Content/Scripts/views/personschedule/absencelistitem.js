define([
    'knockout',
    'moment'
], function (
    ko,
    moment
) {

    return function (data) {

        this.StartTime = ko.observable(moment(data.StartTime));
        this.EndTime = ko.observable(moment(data.EndTime));

        this.BackgroundColor = ko.observable(data.BackgroundColor);

        this.Remove = function () {

        };

        this.ConfirmRemoval = function () {

        };
    };
});
