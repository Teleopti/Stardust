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
		this.StartTime = ko.observable("16:00");
		this.EndTime = ko.observable("18:00");

		var personId;

		this.SetData = function (data) {
			personId = data.PersonId;
			self.Date(data.Date);
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
			navigation.GotoPersonScheduleWithoutHistory(personId, self.Date());
		};

	};
});