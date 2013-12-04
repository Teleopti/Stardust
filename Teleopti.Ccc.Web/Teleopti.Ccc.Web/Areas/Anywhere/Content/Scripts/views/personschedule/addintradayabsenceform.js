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
		
		this.Absence = ko.observable("");
		this.Date = ko.observable();
		this.StartTime = ko.observable();
		this.EndTime = ko.observable();
		this.AbsenceTypes = ko.observableArray();
		
		this.ShiftStart = ko.observable();
		this.ShiftEnd = ko.observable();

		var groupId;
		var personId;

		this.StartTimeAsMoment = ko.computed(function () {
			var momentStartTime = moment(self.StartTime(), resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentStartTime.hours()).add('m', momentStartTime.minutes());
		});
		
		this.EndTimeAsMoment = ko.computed(function () {
			var momentEndTime = moment(self.EndTime(), resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentEndTime.hours()).add('m', momentEndTime.minutes());
		});
		
		this.StartTimeWithinShift = ko.computed(function () {
			if (self.ShiftStart() && self.ShiftEnd()) {
				if (startTimeWithinShift()) {
					if (nightShiftWithEndTimeOnNextDay()) {
						self.EndTimeAsMoment().add('d', 1);
					}
					return true;
				}
				if (self.ShiftStart().date() != self.ShiftEnd().date()) {
					self.StartTimeAsMoment().add('d', 1);
					self.EndTimeAsMoment().add('d', 1);
					if (startTimeWithinShift())
						return true;
				}
				return false;
			}
			return true;
		});

		this.ValidEndTime = ko.computed(function () {
			if (!self.StartTimeWithinShift())
				return true;
			
			if (self.StartTimeAsMoment() && self.StartTimeAsMoment().diff(self.EndTimeAsMoment()) >= 0) {
				return false;
			}
			return true;
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.StartTimeWithinShift()) {
				return resources.InvalidIntradayAbsenceTimes;
			}
			if (!self.ValidEndTime()) {
				return resources.InvalidEndTime;
			}
			return undefined;
		});
		
		this.SetData = function (data, groupid) {
			personId = data.PersonId;
			self.Date(data.Date);

			if (data.DefaultIntradayAbsenceData) {
				self.StartTime(data.DefaultIntradayAbsenceData.StartTime);
				self.EndTime(data.DefaultIntradayAbsenceData.EndTime);
			}
			self.AbsenceTypes(data.Absences);
			groupId = groupid;

			self.ShiftStart(data.Layers.length > 0 ? moment(data.Layers[0].Start) : undefined);
			self.ShiftEnd(data.Layers.length > 0 ? moment(data.Layers[data.Layers.length - 1].Start).add('m', data.Layers[data.Layers.length - 1].Minutes) : undefined);
		};

		this.Apply = function() {
			var requestData = JSON.stringify({
				StartTime: self.StartTimeAsMoment().format(),
				EndTime: self.EndTimeAsMoment().format(),
				AbsenceId: self.Absence(),
				PersonId: personId
			});
			ajax.ajax({
					url: 'PersonScheduleCommand/AddIntradayAbsence',
					type: 'POST',
					data: requestData,
					success: function(data, textStatus, jqXHR) {
						navigation.GoToTeamSchedule(groupId, self.Date());
					}
				}
			);
		};
		
		var startTimeWithinShift = function () {
			return self.StartTimeAsMoment().diff(self.ShiftStart()) >= 0 && self.StartTimeAsMoment().diff(self.ShiftEnd()) < 0;
		};

		var nightShiftWithEndTimeOnNextDay = function () {
			return self.ShiftStart().date() != self.ShiftEnd().date() && self.StartTimeAsMoment().diff(self.EndTimeAsMoment()) > 0;
		};
	};
});