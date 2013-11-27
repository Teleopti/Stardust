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
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();

		var groupid;
		var personId;

		this.SetData = function (data, groupId) {
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
			groupid = groupId;
		};

		this.AbsenceTypes = ko.observableArray();

		this.Apply = function () {
			navigation.GoToTeamSchedule(groupid, self.Date());
		};

	};
});