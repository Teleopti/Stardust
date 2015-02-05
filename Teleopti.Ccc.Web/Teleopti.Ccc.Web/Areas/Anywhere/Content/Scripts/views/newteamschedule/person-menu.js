define([
	'navigation'
], function (
	navigation
	) {

	return function () {
		var self = this;

		this.urlForAddFullDayAbsence = function () {
			return navigation.UrlForNewPersonScheduleAddFullDayAbsence(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};

		this.urlForAddIntradayAbsence = function () {
			return navigation.UrlForNewPersonScheduleAddIntradayAbsence(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};

		this.urlForAddActivity = function () {
			return navigation.UrlForNewPersonScheduleAddActivity(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};

		this.urlForRemoveAbsence = function () {
			return navigation.UrlForNewPersonSchedule(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date);
		};

		this.urlForMoveActivity = function () {
			return navigation.UrlForNewPersonScheduleMoveActivity(self.BusinessUnitId, self.GroupId, self.PersonId, self.Date, moment());
		}
	};
});
