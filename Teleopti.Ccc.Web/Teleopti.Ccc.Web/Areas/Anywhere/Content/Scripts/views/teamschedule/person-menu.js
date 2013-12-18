define([
	'navigation'
], function (
	navigation
	) {

	return function () {
		var self = this;

		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(self.GroupId, self.PersonId, self.Date);
		};
	};
});
