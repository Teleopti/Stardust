define([
	'navigation'
], function (
	navigation
	) {

	return function () {
		var self = this;

		this.urlForAddFullDayAbsence = function () {
			return navigation.UrlForPersonScheduleAddFullDayAbsence(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		}
		this.urlForAddActivity = function () {
			return navigation.UrlForPersonScheduleAddActivity(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		}
	};
});
