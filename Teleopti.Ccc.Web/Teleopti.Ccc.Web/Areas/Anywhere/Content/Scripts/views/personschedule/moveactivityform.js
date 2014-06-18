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
		self.StartTime = ko.observable();
		self.DisplayedStartTime = ko.computed({
			read: function () {
				return moment().startOf('day').add('minutes', self.StartTime()).format(resources.TimeFormatForMoment);
			},
			write:	function (option) {
				self.StartTime(getMomentFromInput(option).diff(self.ScheduleDate(), 'minutes'));
			}
		});

		this.SetData = function (data) {
			self.PersonId(data.PersonId);
			self.ScheduleDate(data.Date);
		};

		this.update = function(layer) {
			if (layer) {
				self.OldStartMinutes(layer.StartMinutes());
				self.StartTime(layer.StartMinutes());
				self.ProjectionLength(layer.LengthMinutes());
			}
		}

		this.Apply = function () {
		};

		this.ErrorMessage = ko.computed(function () {
			return undefined;
		});

		var getMomentFromInput = function (input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.ScheduleDate()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

	};
});