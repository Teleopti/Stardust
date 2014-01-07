define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources!r',
	'timepicker'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources,
	timepicker
    ) {

	return function () {

		var self = this;

		this.Activity = ko.observable("");
		this.ActivityTypes = ko.observableArray();
		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();

		this.ShiftStart = ko.observable();
		this.ShiftEnd = ko.observable();
		
		var personId;
		var groupId;
		var startTimeAsMoment;
		var endTimeAsMoment;

		this.SetData = function (data) {
			personId = data.PersonId;
			groupId = data.GroupId;
			self.Date(data.Date);
			self.ActivityTypes(data.Activities);
		};

		this.Apply = function () {
			var requestData = JSON.stringify({
				Date: self.Date().format(),
				StartTime: startTimeAsMoment.format(),
				EndTime: endTimeAsMoment.format(),
				ActivityId: self.Activity(),
				PersonId: personId
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddActivity',
					type: 'POST',
					data: requestData,
					success: function(data, textStatus, jqXHR) {
						navigation.GotoPersonScheduleWithoutHistory(groupId, personId, self.Date());
					}
				}
			);
		};

		var startTimeWithinShift = function () {
			return startTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) < 0;
		};

		var nightShiftWithEndTimeOnNextDay = function () {
			return self.ShiftStart().date() != self.ShiftEnd().date() && startTimeAsMoment.diff(endTimeAsMoment) > 0;
		};
		
		var getMomentFromInput = function (input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		this.SetShiftStartAndEnd = function(data) {
			var layers = data.Projection;
			if (layers && layers.length > 0) {
				self.ShiftStart(layers.length > 0 ? moment(layers[0].Start) : undefined);
				self.ShiftEnd(layers.length > 0 ? moment(layers[layers.length - 1].Start).add('m', layers[layers.length - 1].Minutes) : undefined);
				var now = moment();
				var start;
				if (self.ShiftStart() < now && now < self.ShiftEnd()) {
					var minutes = Math.ceil(now.minute() / 15) * 15;
					start = now.startOf('hour').minutes(minutes);
					self.StartTime(start.format(resources.TimeFormatForMoment));
				} else {
					start = self.ShiftStart();
					self.StartTime(start.format(resources.TimeFormatForMoment));
				}
				self.EndTime(start.clone().add("hours", 1).format(resources.TimeFormatForMoment));
			}
			self.ShiftStart(moment(self.Date()).startOf('d'));
			self.ShiftEnd(moment(self.Date()).startOf('d').add('d', 1));
		};
		
		this.StartTimeWithinShift = ko.computed(function () {
			if (self.ShiftStart() && self.ShiftEnd()) {
				startTimeAsMoment = getMomentFromInput(self.StartTime());
				endTimeAsMoment = getMomentFromInput(self.EndTime());
				if (startTimeWithinShift()) {
					if (nightShiftWithEndTimeOnNextDay()) {
						endTimeAsMoment.add('d', 1);
					}
					return true;
				}
				if (self.ShiftStart().date() != self.ShiftEnd().date()) {
					startTimeAsMoment.add('d', 1);
					endTimeAsMoment.add('d', 1);
					if (startTimeWithinShift())
						return true;
				}
				return false;
			}
			return true;
		}).extend({ throttle: 1 });
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.StartTimeWithinShift()) {
				return resources.CannotCreateSecondShiftWhenAddingActivity;
			}
			return undefined;
		});

	};
});