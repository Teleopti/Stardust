define([
	'knockout',
	'navigation',
	'ajax',
	'resources',
	'timepicker',
	'lazy',
	'guidgenerator',
	'notifications',
	'shared/timezone-display',
	'shared/timezone-current'
], function (
	ko,
	navigation,
	ajax,
	resources,
	timepicker,
	lazy,
	guidgenerator,
	notificationsViewModel,
	timezoneDisplay,
	timezoneCurrent
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
		
		var personId;
		var personName;
		var groupId;
		var businessUnitId;
		var startTimeAsMoment;
		var endTimeAsMoment;

		this.IsOtherTimezone = ko.computed(function () {
			return timezoneCurrent.IanaTimeZone() !== self.ianaTimeZoneOther();
		});

		this.StartTimeOtherTimeZone = ko.computed(function () {
			if (self.StartTime() && self.ianaTimeZoneOther()) {

				var userTime = timezoneDisplay.FromTimeInput(self.StartTime(), timezoneCurrent.IanaTimeZone(), self.ScheduleDate);
				var otherTime = userTime.tz(self.ianaTimeZoneOther());

				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.EndTimeOtherTimeZone = ko.computed(function () {
			var endTime = self.EndTime();
			var ianaTimeZoneOther = self.ianaTimeZoneOther();
			if (endTime && ianaTimeZoneOther) {
				var userTime = timezoneDisplay.FromTimeInput(endTime, timezoneCurrent.IanaTimeZone(), self.ScheduleDate);
				var otherTime = userTime.tz(ianaTimeZoneOther);
				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.SetData = function (data) {
			personId = data.PersonId;
			groupId = data.GroupId;
			personName = data.PersonName;
			businessUnitId = data.BusinessUnitId;
			self.ScheduleDate(timezoneDisplay.FromDate(data.Date, timezoneCurrent.IanaTimeZone()));
			self.ActivityTypes(data.Activities);
			self.TimeZoneName(data.TimeZoneName);
			self.ianaTimeZoneOther(data.IanaTimeZoneOther);
		};

		this.Apply = function () {
			var trackId = guidgenerator.newGuid();
			var requestData = JSON.stringify({
				Date: self.ScheduleDate().format('YYYY-MM-DD HH:mm'),
				StartTime: startTimeAsMoment.format('YYYY-MM-DD HH:mm'),
				EndTime: endTimeAsMoment.format('YYYY-MM-DD HH:mm'),
				ActivityId: self.Activity(),
				PersonId: personId,
				TrackedCommandInfo: { TrackId: trackId }
			});
			ajax.ajax({
					url: 'api/PersonScheduleCommand/AddActivity',
					type: 'POST',
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					data: requestData,
					success: function (data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(businessUnitId, groupId, self.ScheduleDate());
					},
					statusCode500: function (jqXHR, textStatus, errorThrown) {
						notificationsViewModel.UpdateNotification(trackId, 3);
					}
				}
			);
			notificationsViewModel.AddNotification(trackId, resources.AddingActivityFor + " " + personName + "... ");
		};

		this.visibleLayers = ko.computed(function () {
			var shift = self.WorkingShift();
			if (shift != undefined) {
				return lazy(shift.Layers())
					.filter(function(x) { return x.OverlapsTimeLine(); })
					.toArray();
			}
			return [];
		});

		this.ShiftStart = ko.computed(function () {
			if (self.visibleLayers().length > 0) {
				var visibleLayers = self.visibleLayers();
				return moment(self.ScheduleDate()).add("minutes", visibleLayers[0].StartMinutes());
			}
			return moment(self.ScheduleDate()).startOf('d');
		});
		
		this.ShiftEnd = ko.computed(function () {
			if (self.visibleLayers().length > 0) {
				var visibleLayers = self.visibleLayers();
				return moment(self.ScheduleDate()).add("minutes", visibleLayers[visibleLayers.length - 1].EndMinutes());
			}
			return moment(self.ScheduleDate()).startOf('d').add('d', 1);
		});
		
		this.DefaultStart = ko.computed(function () {
			var now = moment(new Date().getTime());
			
			var nowInUserTimeZone = now.tz(timezoneCurrent.IanaTimeZone());
			var start;
			if (self.ShiftStart() < nowInUserTimeZone && nowInUserTimeZone < self.ShiftEnd()) {
				var minutes = Math.ceil(nowInUserTimeZone.minute() / 15) * 15;
				start = nowInUserTimeZone.startOf('hour').minutes(minutes);
				self.StartTime(start.format(resources.TimeFormatForMoment));
			} else {
				start = self.ShiftStart().clone();
				start.tz(timezoneCurrent.IanaTimeZone());
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
		
		
		this.intersectWithShift = function () {
			return endTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) <= 0;
		};

		this.PossbileIntersectWithShift = ko.computed(function () {
			if (self.StartTime() && self.EndTime()) {
				startTimeAsMoment = timezoneDisplay.FromTimeInput(self.StartTime(), timezoneCurrent.IanaTimeZone(), self.ScheduleDate);
				endTimeAsMoment = timezoneDisplay.FromTimeInput(self.EndTime(), timezoneCurrent.IanaTimeZone(), self.ScheduleDate);
				if (startTimeAsMoment.diff(endTimeAsMoment) < 0) {
					if (self.intersectWithShift())
						return true;
					startTimeAsMoment.add('d', 1);
					endTimeAsMoment.add('d', 1);
					if (self.intersectWithShift())
						return true;
					return false;
				} else {
					endTimeAsMoment.add('d', 1);
					if (self.intersectWithShift())
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