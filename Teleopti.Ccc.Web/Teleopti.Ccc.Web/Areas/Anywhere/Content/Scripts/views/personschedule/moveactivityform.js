define([
	'knockout',
	'resources',
	'ajax',
	'navigation'
], function (
	ko,
	resources,
	ajax,
	navigation
    ) {

	return function () {

		var self = this;

		self.PersonId = ko.observable();
		self.GroupId = ko.observable();
		self.ScheduleDate = ko.observable();
		self.OldStartMinutes = ko.observable();
		self.ProjectionLength = ko.observable();
		self.StartTime = ko.observable();
		self.ActivityId = ko.observable();

		this.SetData = function (data) {
			self.PersonId(data.PersonId);
			self.GroupId(data.GroupId);
			self.ScheduleDate(data.Date);
		};

		this.DisplayedStartTime = ko.computed({
			read: function () {
				if (!self.ScheduleDate() || !self.StartTime()) return '';
				return self.StartTime().format(resources.TimeFormatForMoment);
			},
			write: function (option) {
				self.StartTime(getMomentFromInput(option));
			}
		});



		this.update = function(layer) {
			if (layer) {
				self.OldStartMinutes(layer.StartMinutes());
				self.StartTime(moment(self.ScheduleDate()).add('minutes', self.OldStartMinutes()));
				self.ProjectionLength(layer.LengthMinutes());
				self.ActivityId(layer.ActivityId);
			}
		};

		this.Apply = function () {
			var requestData = JSON.stringify({
				AgentId: self.PersonId(),
				ScheduleDate: self.ScheduleDate().format(),
				NewStartTime: self.StartTime().format(),
				OldStartTime: moment(self.ScheduleDate()).add('minutes', self.OldStartMinutes()).format(),
				ActivityId: self.ActivityId(),
				OldProjectionLayerLength: self.ProjectionLength()
			});
			ajax.ajax({
				url: 'PersonScheduleCommand/MoveActivity',
				type: 'POST',
				data: requestData,
				success: function (data, textStatus, jqXHR) {
					navigation.GotoPersonScheduleWithoutHistory(self.GroupId(), self.PersonId(), self.ScheduleDate());
				}
			}
			);
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