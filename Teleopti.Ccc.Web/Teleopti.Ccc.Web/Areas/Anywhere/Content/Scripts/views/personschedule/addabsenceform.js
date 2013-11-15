define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources!r',
	'timepicker'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources,
	timepicker
    ) {

	return function () {

		var self = this;

		this.Absence = ko.observable("");

		this.Date = ko.observable();
		this.StartTime = ko.observable("17:00");
		this.EndTime = ko.observable("18:00");

		var personId;

		this.SetData = function (data) {
			personId = data.PersonId;
			self.Date(data.Date);
			self.AbsenceTypes([
				{
					Id: 1,
					Name: "Vacation"
				}, {
					Id: 2,
					Name: "AWOL"
				}, {
					Id: 3,
					Name: "Illness"
				}
			]);
		};

		this.AbsenceTypes = ko.observableArray();

		this.Apply = function () {
			navigation.GotoPersonScheduleWithoutHistory(personId, self.Date());
		};

	};
});