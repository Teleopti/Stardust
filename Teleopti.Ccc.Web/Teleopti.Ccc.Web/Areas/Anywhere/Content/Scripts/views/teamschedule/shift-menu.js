define([
	'knockout',
	'navigation'
], function (
	ko,
	navigation
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
	};
});
