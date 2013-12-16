define([
	'knockout',
	'moment',
	'navigation',
	'ajax',
	'resources!r'
], function (
	ko,
	moment,
	navigation,
	ajax,
	resources
    ) {

	return function () {

		var self = this;

		this.Activity = ko.observable("");

		this.Date = ko.observable();
		this.StartTime = ko.observable("16:00");
		this.EndTime = ko.observable("18:00");

		this.ShiftStart = ko.observable();
		this.ShiftEnd = ko.observable();
		
		var personId;
		
		var startTimeAsMoment;
		var endTimeAsMoment;

		this.SetData = function (data) {
			personId = data.PersonId;
			self.Date(data.Date);
			self.ActivityTypes([
				{
					Id: 1,
					Name: "Phone"
				}, {
					Id: 2,
					Name: "Lunch"
				}, {
					Id: 3,
					Name: "Break"
				}
			]);
		};

		this.ActivityTypes = ko.observableArray();

		this.Apply = function () {
			navigation.GotoPersonScheduleWithoutHistory(personId, self.Date());
		};
		
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
		
		this.SetShiftStartAndEnd = function (data) {
			var layers = data.Projection;
			if (layers) {
				self.ShiftStart(layers.length > 0 ? moment(layers[0].Start) : undefined);
				self.ShiftEnd(layers.length > 0 ? moment(layers[layers.length - 1].Start).add('m', layers[layers.length - 1].Minutes) : undefined);
			}
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
		});
		
		this.ErrorMessage = ko.computed(function () {
			if (!self.StartTimeWithinShift()) {
				return resources.CannotCreateSecondShiftWhenAddingActivity;
			}
			return undefined;
		});

	};
});