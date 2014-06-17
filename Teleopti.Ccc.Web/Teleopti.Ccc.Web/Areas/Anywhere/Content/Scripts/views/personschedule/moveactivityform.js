define([
	'knockout',
	'resources'
], function (
	ko,
	resources
    ) {

	return function () {

		var self = this;

		self.PersonId = ko.observable();
		self.ScheduleDate = ko.observable();
		self.OldStartMinutes = ko.observable();
		self.ProjectionLength = ko.observable();
		self.NewStartMinutes = ko.observable(self.OldStartMinutes());

		this.SetData = function (data) {
			self.PersonId(data.PersonId);
			self.ScheduleDate(data.Date);
			self.OldStartMinutes(data.OldStartMinutes);
			self.ProjectionLength(data.ProjectionLength);
		};

		this.Apply = function () {
		};

		this.ErrorMessage = ko.computed(function () {
			return undefined;
		});

	};
});