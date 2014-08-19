define([
	'knockout',
	'navigation',
	'ajax',
	'resources',
	'timepicker',
	'lazy',
	'guidgenerator',
	'notifications'
], function (
	ko,
	navigation,
	ajax,
	resources,
	timepicker,
	lazy,
	guidgenerator,
	notificationsViewModel
    ) {

	return function () {

		var self = this;

		this.Activity = ko.observable("");
		this.ActivityTypes = ko.observableArray();
		this.ScheduleDate = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.WorkingShift = ko.observable();
		this.TimeZoneName = ko.observable();
		this.ianaTimeZone = ko.observable();
		this.ianaTimeZoneOther = ko.observable();
		this.IsOtherTimezone = ko.observable(false);

		var personId;
		var personName;
		var groupId;
		var startTimeAsMoment;
		var endTimeAsMoment;

		this.StartTimeOtherTimeZone = ko.computed(function () {
			if (self.StartTime() && self.ianaTimeZone() && self.ianaTimeZoneOther()) {
				var userTime = getMomentFromInput(self.StartTime()).tz(self.ianaTimeZone());
				var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
				self.IsOtherTimezone(otherTime.format('ha z')!=userTime.format('ha z'));
				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.EndTimeOtherTimeZone = ko.computed(function () {
			if (self.EndTime() && self.ianaTimeZone() && self.ianaTimeZoneOther()) {
				var userTime = getMomentFromInput(self.EndTime()).tz(self.ianaTimeZone());
				var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.SetData = function (data) {
			personId = data.PersonId;
			groupId = data.GroupId;
			personName = data.PersonName;
			self.ScheduleDate(data.Date);
			self.ActivityTypes(data.Activities);
			self.TimeZoneName(data.TimeZoneName);
			self.ianaTimeZone(data.IanaTimeZoneLoggedOnUser);
			self.ianaTimeZoneOther(data.IanaTimeZoneOther);
		};

		this.Apply = function () {
			var trackId = guidgenerator.newGuid();
			var requestData = JSON.stringify({
				Date: self.ScheduleDate().format(),
				StartTime: startTimeAsMoment.format(),
				EndTime: endTimeAsMoment.format(),
				ActivityId: self.Activity(),
				PersonId: personId,
				TrackedCommandInfo: { TrackId: trackId }
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddActivity',
					type: 'POST',
					data: requestData,
					success: function (data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(groupId, self.ScheduleDate());
					},
					statusCode500: function (jqXHR, textStatus, errorThrown) {
						notificationsViewModel.UpdateNotification(trackId, 3);
					}
				}
			);
			notificationsViewModel.AddNotification(trackId, resources.AddingActivityFor + " " + personName + "... ");
		};

		var getMomentFromInput = function (input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.ScheduleDate()).add('h', momentInput.hours()).add('m', momentInput.minutes());
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
				return moment(self.ScheduleDate()).add("minutes", visibleLayers[0].StartMinutes());
			}
			return moment(self.ScheduleDate()).startOf('d');
		});
		
		this.ShiftEnd = ko.computed(function () {
			var visibleLayers = self.visibleLayers();
			if (visibleLayers.length > 0) {
				return moment(self.ScheduleDate()).add("minutes", visibleLayers[visibleLayers.length - 1].EndMinutes());
			}
			return moment(self.ScheduleDate()).startOf('d').add('d', 1);
		});
		
		this.DefaultStart = ko.computed(function () {
			var now = moment((new Date).getTime());
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
			return moment(self.ScheduleDate()).add('h', momentInput.hours()).add('m', momentInput.minutes());
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