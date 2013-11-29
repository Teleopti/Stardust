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

		var groupId;
		var personId;

		var shiftStart;
		var shiftEnd;
		
		var momentStartInput;
		var momentEndInput;
		var isNextDay;

		this.SetData = function (data, groupid) {
			personId = data.PersonId;
			self.Date(data.Date);
			if (data.DefaultIntradayAbsenceData) {
				self.StartTime(data.DefaultIntradayAbsenceData.StartTime);
				self.EndTime(data.DefaultIntradayAbsenceData.EndTime);
			}
			self.AbsenceTypes(data.Absences);
			groupId = groupid;
			shiftStart = data.Layers.length > 0 ? moment(data.Layers[0].Start) : undefined;
			shiftEnd = data.Layers.length > 0 ? moment(data.Layers[data.Layers.length - 1].Start).add('m', data.Layers[data.Layers.length - 1].Minutes) : undefined;
		};
		
		this.IsStartTimeWithinShift = ko.computed(function () {
			isNextDay = false;
			if (self.StartTime() && self.EndTime() && shiftStart && shiftEnd) {
				momentStartInput = getMomentFromInput(self.StartTime());
				momentEndInput = getMomentFromInput(self.EndTime());
				
				if (momentStartInput.diff(shiftStart) >= 0 && momentStartInput.diff(shiftEnd) < 0) {
					return true;
				}
				if (shiftStart.date() != shiftEnd.date()) {
					momentStartInput = momentStartInput.add('d', 1);
					momentEndInput = momentEndInput.add('d', 1);
					isNextDay = true;
					if (momentStartInput.diff(shiftStart) >= 0 && momentStartInput.diff(shiftEnd) < 0) {
						return true;
					}
				}
				return false;
			}
			return true;
		});

		var getMomentFromInput = function(input) {
			var momentInput = moment(input, resources.TimeFormatForMoment);
			return moment(self.Date()).add('h', momentInput.hours()).add('m', momentInput.minutes());
		};

		this.ValidEndTime = ko.computed(function () {
			if (!self.IsStartTimeWithinShift())
				return true;
			
			if (momentStartInput && momentStartInput.diff(momentEndInput) == 0)
				return false;
			
			if (momentStartInput && momentStartInput.diff(momentEndInput) > 0) {
				if (!isNextDay && shiftStart && shiftEnd && shiftStart.date() != shiftEnd.date()) {
					return true;
				}
				return false;
			}
			return true;
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.IsStartTimeWithinShift()) {
				return resources.InvalidIntradayAbsenceTimes;
			}
			if (!self.ValidEndTime()) {
				return resources.InvalidEndTime;
			}
			return undefined;
		});

		this.Apply = function() {
			var requestData = JSON.stringify({
				Date: self.Date().format('YYYY-MM-DD'),
				StartTime: self.StartTime(),
				EndTime: self.EndTime(),
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

	};
});