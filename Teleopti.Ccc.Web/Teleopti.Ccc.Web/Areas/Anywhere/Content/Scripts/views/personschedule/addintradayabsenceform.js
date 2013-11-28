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
		this.AbsenceTypes = ko.observableArray();

		var groupid;
		var personId;

		this.SetData = function (data, groupId) {
			personId = data.PersonId;
			self.Date(data.Date);
			self.StartTime(data.DefaultIntradayAbsenceData.StartTime);
			self.EndTime(data.DefaultIntradayAbsenceData.EndTime);
			self.AbsenceTypes(data.Absences);
			groupid = groupId;
		};


		this.Apply = function () {
			navigation.GoToTeamSchedule(groupid, self.Date());
		};

	};
});