define([
	'knockout',
	'navigation'
], function (
	ko,
	navigation
	) {

	return function (groupid, personid, date) {
		var self = this;

		this.Date = ko.observable(date);

		this.ShowDetails = function() {
			navigation.GotoPersonSchedule(groupid, personid, self.Date());
		};

		this.AddFullDayAbsence = function() {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(groupid, personid, self.Date());
		};

		this.AddActivity = function () {
			navigation.GotoPersonScheduleAddActivityForm(groupid, personid, self.Date());
		};
		
		this.AddIntradayAbsence = function () {
			navigation.GotoPersonScheduleAddIntradayAbsenceForm(groupid, personid, self.Date());
		};
		
		this.RemoveAbsence = function () {
			navigation.GotoPersonSchedule(groupid, personid, self.Date());
		};
	};
});
