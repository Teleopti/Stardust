define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources!r'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources
    ) {

	return function () {

		var self = this;

		this.Activity = ko.observable("");

		this.Date = ko.observable();
		this.StartTime = ko.observable(moment());
		this.EndTime = ko.observable(moment());

		var personId;

		this.SetData = function (data) {
			personId = data.PersonId;
			self.Date = data.Date;
			self.ActivityTypes([
				{
					Id: 1,
					Name: "Phone"
				}, {
					Id: 2,
					Name: "Lunch"
				}, {
					Id: 3,
					Name: "Break"
				}
			]);
		};

		this.ActivityTypes = ko.observableArray();

		this.Apply = function () {
		};

	};
});