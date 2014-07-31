define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources',
	'timepicker',
	'lazy',
	'guidgenerator',
	'notifications'
], function (
	ko,
	moment,
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
		
		this.Absence = ko.observable("");
		this.AbsenceTypes = ko.observableArray();
		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.WorkingShift = ko.observable();
		
		var groupId;
		var personId;
		var personName;
		var startTimeAsMoment;
		var endTimeAsMoment;

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
			return moment(self.Date()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		var startTimeWithinShift = function () {
			return startTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) < 0;
		};

		var nightShiftWithEndTimeOnNextDay = function () {
			return self.ShiftStart().date() != self.ShiftEnd().date() && startTimeAsMoment.diff(endTimeAsMoment) > 0;
		};
		
		this.PossbileStartTimeWithinShift = ko.computed(function () {
			if (self.StartTime() && self.EndTime()) {
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
			return false;
		});

		this.ValidEndTime = ko.computed(function () {
			if (!self.PossbileStartTimeWithinShift())
				return true;
			
			if (startTimeAsMoment && startTimeAsMoment.diff(endTimeAsMoment) >= 0) {
				return false;
			}
			return true;
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.PossbileStartTimeWithinShift()) {
				return resources.InvalidIntradayAbsenceTimes;
			}
			if (!self.ValidEndTime()) {
				return resources.InvalidEndTime;
			}
			return undefined;
		});
		
		this.SetData = function (data) {
			groupId = data.GroupId;
			personId = data.PersonId;
			personName = data.PersonName;
			groupId = data.GroupId;
			self.Date(data.Date);

			if (data.DefaultIntradayAbsenceData) {
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
				StartTime: startTimeAsMoment.format(),
				EndTime: endTimeAsMoment.format(),
				AbsenceId: self.Absence(),
				PersonId: personId,
				TrackedCommandInfo: { TrackId: trackId }
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddIntradayAbsence',
					type: 'POST',
					data: requestData,
					success: function(data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(groupId, self.Date());
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