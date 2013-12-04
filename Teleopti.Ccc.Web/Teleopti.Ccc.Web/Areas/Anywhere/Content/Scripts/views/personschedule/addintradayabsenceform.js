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
		var startTimeAsMoment;
		var endTimeAsMoment;

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
		});

		this.ValidEndTime = ko.computed(function () {
			if (!self.StartTimeWithinShift())
				return true;
			
			if (startTimeAsMoment && startTimeAsMoment.diff(endTimeAsMoment) >= 0) {
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
			
			startTimeAsMoment = getMomentFromInput(self.StartTime());
			endTimeAsMoment = getMomentFromInput(self.EndTime());
		};

		this.Apply = function() {
			var requestData = JSON.stringify({
				StartTime: startTimeAsMoment.format(),
				EndTime: endTimeAsMoment.format(),
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
		
		var getMomentFromInput = function(input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		var startTimeWithinShift = function () {
			return startTimeAsMoment.diff(self.ShiftStart()) >= 0 && startTimeAsMoment.diff(self.ShiftEnd()) < 0;
		};

		var nightShiftWithEndTimeOnNextDay = function () {
			return self.ShiftStart().date() != self.ShiftEnd().date() && startTimeAsMoment.diff(endTimeAsMoment) > 0;
		};
	};
});