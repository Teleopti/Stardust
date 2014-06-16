define([
	'knockout'
], function (
	ko
    ) {

	return function () {

		var self = this;

		self.PersonId = ko.observable();
		self.ScheduleDate = ko.observable();
		self.OldStartTime = ko.observable();
		self.ProjectionLength = ko.observable();

		this.SetData = function (data) {
			self.PersonId(data.PersonId);
			self.ScheduleDate(data.Date);
			self.OldStartTime(data.OldStartTime);
			self.ProjectionLength(data.ProjectionLength);
		};

	};
});