define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources',
	'timepicker',
	'lazy'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources,
	timepicker,
	lazy
    ) {

	return function () {

		var self = this;

		this.Activity = ko.observable("");
		this.ActivityTypes = ko.observableArray();
		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.WorkingShift = ko.observable();
		
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

		this.visibleLayers = ko.computed(function () {
			var shift = self.WorkingShift();
			if (shift) {
				return lazy(shift.Layers())
					.filter(function(x) { return x.OverlapsTimeLine(); })
					.toArray();
			}
			return [];
		});

		this.ShiftStart = ko.computed(function () {
			var visibleLayers = self.visibleLayers();
			if (visibleLayers.length > 0) {
				return moment(self.Date()).add("minutes", visibleLayers[0].StartMinutes());
			}
			return moment(self.Date()).startOf('d');
		});
		
		this.ShiftEnd = ko.computed(function () {
			var visibleLayers = self.visibleLayers();
			if (visibleLayers.length > 0) {
				return moment(self.Date()).add("minutes", visibleLayers[visibleLayers.length - 1].EndMinutes());
			}
			return moment(self.Date()).startOf('d').add('d', 1);
		});
		
		this.DefaultStart = ko.computed(function () {
			var now = moment(new Date(new Date().getTeleoptiTime()));
			var start;
			if (self.ShiftStart() < now && now < self.ShiftEnd()) {
				var minutes = Math.ceil(now.minute() / 15) * 15;
				start = now.startOf('hour').minutes(minutes);
				self.StartTime(start.format(resources.TimeFormatForMoment));
			} else {
				start = self.ShiftStart().clone();
				if (self.visibleLayers().length > 0) {
					self.StartTime(start.format(resources.TimeFormatForMoment));
				} else {
					self.StartTime(start.add("hours", 8).format(resources.TimeFormatForMoment));
				}
			}
			return start;
		});

		this.DefaultEnd = ko.computed(function () {
			self.EndTime(self.DefaultStart().clone().add("hours", 1).format(resources.TimeFormatForMoment));
		});
		
		
		var intersectWithShift = function () {
			return endTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) <= 0;
		};

		var getMomentFromInput = function (input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		this.PossbileIntersectWithShift = ko.computed(function () {
			if (self.StartTime() && self.EndTime()) {
				startTimeAsMoment = getMomentFromInput(self.StartTime());
				endTimeAsMoment = getMomentFromInput(self.EndTime());
				if (startTimeAsMoment.diff(endTimeAsMoment) < 0) {
					if (intersectWithShift())
						return true;
					startTimeAsMoment.add('d', 1);
					endTimeAsMoment.add('d', 1);
					if (intersectWithShift())
						return true;
					return false;
				} else {
					endTimeAsMoment.add('d', 1);
					if (intersectWithShift())
						return true;
					return false;
				}
			}
			return false;
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.PossbileIntersectWithShift()) {
				return resources.CannotCreateSecondShiftWhenAddingActivity;
			}
			return undefined;
		});

	};
});