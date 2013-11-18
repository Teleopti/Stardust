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
		
		this.AddAbsence = function () {
			navigation.GotoPersonScheduleAddAbsenceForm(groupid, personid, date);
		};
		
		this.RemoveAbsence = function () {
			navigation.GotoPersonScheduleAddAbsenceForm(groupid, personid, date);
		};
	};
});
