define([
	'knockout',
	'navigation',
	'helpers'
], function (
	ko,
	navigation,
	helpers
	) {

	return function (data) {
		var self = this;

		this.ShowDetails = function() {
			navigation.GotoPersonSchedule(self.GroupId, self.PersonId, self.Date);
		};
		
		this.AddIntradayAbsence = function () {
			navigation.GotoPersonScheduleAddIntradayAbsenceForm(self.GroupId, self.PersonId, self.Date);
		};
		
		this.RemoveAbsence = function () {
			navigation.GotoPersonSchedule(self.GroupId, self.PersonId, self.Date);
		};
		
		this.AddActivity = function () {
			navigation.GotoPersonScheduleAddActivityForm(self.GroupId, self.PersonId, self.Date);
		};
		
		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(self.GroupId, self.PersonId, self.Date);
		};

		this.moveActivity = function () {
			var layer = this;
			navigation.GotoPersonScheduleMoveActivityForm(self.GroupId, self.PersonId, self.Date, helpers.timeFormatForUrl(layer.StartTime()));
		};
	};
});
