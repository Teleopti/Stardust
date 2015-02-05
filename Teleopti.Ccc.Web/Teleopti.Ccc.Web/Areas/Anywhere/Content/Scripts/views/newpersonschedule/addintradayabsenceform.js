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
		
		this.Absence = ko.observable("");
		this.AbsenceTypes = ko.observableArray();
		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.WorkingShift = ko.observable();

		this.TimeZoneName = ko.observable();
		this.ianaTimeZoneOther = ko.observable();
		
		var groupId;
		var businessUnitId;
		var personId;
		var personName;
		var startTimeAsMoment;
		var endTimeAsMoment;

		this.IsOtherTimezone = ko.computed(function () {
			return timezoneCurrent.IanaTimeZone() !== self.ianaTimeZoneOther();
		});

		this.StartTimeOtherTimeZone = ko.computed(function () {
			if (self.StartTime() && self.ianaTimeZoneOther()) {
				var userTime = timezoneDisplay.FromTimeInput(self.StartTime(), timezoneCurrent.IanaTimeZone(), self.Date);
				var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.EndTimeOtherTimeZone = ko.computed(function () {
			if (self.EndTime() && self.ianaTimeZoneOther()) {
				var userTime = timezoneDisplay.FromTimeInput(self.EndTime(), timezoneCurrent.IanaTimeZone(), self.Date);
				var otherTime = userTime.clone().tz(self.ianaTimeZoneOther());
				return otherTime.format('HH:mm');
			}
			return undefined;
		});

		this.visibleLayers = ko.computed(function () {
			var shift = self.WorkingShift();
			if (shift) {
				return lazy(shift.Layers())
					.filter(function (x) { return x.OverlapsTimeLine(); })
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
		
		var getMomentFromInput = function (input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			if (!self.Date())
				return moment().add('h', momentInput.hours()).add('m', momentInput.minutes());
			return self.Date().clone().add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		var startTimeWithinShift = function () {
			return startTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) < 0;
		};

		var nightShiftWithEndTimeOnNextDay = function () {
			return self.ShiftStart().date() != self.ShiftEnd().date() && startTimeAsMoment.diff(endTimeAsMoment) > 0;
		};
		
		this.PossbileStartTimeWithinShift = ko.computed(function () {
			if (self.StartTime() && self.EndTime()) {
				startTimeAsMoment = timezoneDisplay.FromTimeInput(self.StartTime(), timezoneCurrent.IanaTimeZone(), self.Date);
				endTimeAsMoment = timezoneDisplay.FromTimeInput(self.EndTime(), timezoneCurrent.IanaTimeZone(), self.Date);
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
			return false;
		});

		this.ValidEndTime = ko.computed(function() {
			if (self.StartTime() && self.EndTime()) {
				//when start time is not within shift period, no need to validate end time since the start time 
				//error message has been shown on screen
				if (!self.PossbileStartTimeWithinShift())
					return true;

				if (startTimeAsMoment && startTimeAsMoment.diff(endTimeAsMoment) >= 0) {
					return false;
				}
				return true;
			}
			return true;
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.PossbileStartTimeWithinShift()) {
				return resources.InvalidIntradayAbsenceTimes;
			}
			if (!self.ValidEndTime()) {
				return resources.StartDateMustBeSmallerThanEndDate;
			}
			return undefined;
		});
		
		this.SetData = function (data) {
			groupId = data.GroupId;
			personId = data.PersonId;
			businessUnitId = data.BusinessUnitId;
			personName = data.PersonName;
			self.Date(timezoneDisplay.FromDate(data.Date, timezoneCurrent.IanaTimeZone()));
			self.ianaTimeZoneOther(data.IanaTimeZoneOther);

			if (data.DefaultIntradayAbsenceData) {
				self.TimeZoneName(data.TimeZoneName);
				self.StartTime(data.DefaultIntradayAbsenceData.StartTime);
				self.EndTime(data.DefaultIntradayAbsenceData.EndTime);
				startTimeAsMoment = getMomentFromInput(self.StartTime());
				endTimeAsMoment = getMomentFromInput(self.EndTime());
			}
			self.AbsenceTypes(data.Absences);
		};
		
		this.Apply = function() {
			var trackId = guidgenerator.newGuid();
			var requestData = JSON.stringify({
				StartTime: startTimeAsMoment.format('YYYY-MM-DD HH:mm'),
				EndTime: endTimeAsMoment.format('YYYY-MM-DD HH:mm'),
				AbsenceId: self.Absence(),
				PersonId: personId,
				TrackedCommandInfo: { TrackId: trackId }
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddIntradayAbsence',
					type: 'POST',
					headers: { 'X-Business-Unit-Filter': businessUnitId },
					data: requestData,
					success: function(data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(businessUnitId, groupId, self.Date());
					},
					statusCode500: function (jqXHR, textStatus, errorThrown) {
						notificationsViewModel.UpdateNotification(trackId, 3);
					}
				}
			);
			notificationsViewModel.AddNotification(trackId, resources.AddingIntradayAbsenceFor + " " + personName + "... ");
		};
	};
});