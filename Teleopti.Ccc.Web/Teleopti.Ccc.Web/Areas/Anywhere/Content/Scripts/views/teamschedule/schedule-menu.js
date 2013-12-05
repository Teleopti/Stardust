define([
		'navigation'
], function (
	navigation
	) {

	return function (personid, groupid, date) {
		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(groupid, personid, date);
		};
	};
});
