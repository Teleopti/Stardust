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
		this.urlForAddIntradayAbsence = function () {
			return navigation.UrlForPersonScheduleAddIntradayAbsence(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};
		this.urlForAddActivity = function () {
			return navigation.UrlForPersonScheduleAddActivity(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		}
		this.urlForRemoveAbsence = function () {
			return navigation.UrlForPersonSchedule(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};
		this.urlForMoveActivity = function () {
			return navigation.UrlForPersonScheduleMoveActivity(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date, 0);
		}
	};
});
