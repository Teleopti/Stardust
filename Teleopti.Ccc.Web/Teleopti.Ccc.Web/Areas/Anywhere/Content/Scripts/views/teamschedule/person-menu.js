define([
	'navigation'
], function (
	navigation
	) {

	return function () {
		var self = this;

		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceForm(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};
		
		this.AddActivity = function () {
			navigation.GotoPersonScheduleAddActivityForm(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};
	};
});
