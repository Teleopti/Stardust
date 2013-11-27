define([
		'navigation'
], function (
	navigation
	) {

	return function (groupid, personid, date) {

		this.ShowDetails = function() {
			navigation.GotoPersonSchedule(groupid, personid, date);
		};

		this.AddFullDayAbsence = function() {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(groupid, personid, date);
		};

		this.AddActivity = function () {
			navigation.GotoPersonScheduleAddActivityForm(groupid, personid, date);
		};
		
		this.AddIntradayAbsence = function () {
			navigation.GotoPersonScheduleAddIntradayAbsenceForm(groupid, personid, date);
		};
		
		this.RemoveAbsence = function () {
			navigation.GotoPersonSchedule(groupid, personid, date);
		};
	};
});
