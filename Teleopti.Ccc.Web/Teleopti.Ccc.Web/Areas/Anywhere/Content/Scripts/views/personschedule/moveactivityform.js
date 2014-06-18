define([
	'knockout',
	'resources',
	'ajax'
], function (
	ko,
	resources,
	ajax
    ) {

	return function () {

		var self = this;

		self.PersonId = ko.observable();
		self.ScheduleDate = ko.observable();
		self.OldStartMinutes = ko.observable();
		self.ProjectionLength = ko.observable();
		self.StartTime = ko.observable();
		self.ActivityId = ko.observable();

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
				self.ActivityId(layer.ActivityId);
			}
		}

		this.Apply = function () {
			var requestData = JSON.stringify({
				AgentId: self.PersonId(),
				ScheduleDate: self.ScheduleDate().format(),
				NewStartTime: moment().startOf('day').add('minutes', self.StartTime()).format(),
				OldStartTime: moment().startOf('day').add('minutes', self.OldStartMinutes()).format(),
				ActivityId: self.ActivityId(),
				OldProjectionLayerLength: self.ProjectionLength()
			});
			ajax.ajax({
				url: 'PersonScheduleCommand/MoveActivity',
				type: 'POST',
				data: requestData,
				success: function (data, textStatus, jqXHR) {
					navigation.GotoPersonScheduleWithoutHistory(groupId, personId, self.ScheduleDate());
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