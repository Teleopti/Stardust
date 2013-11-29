define([
	'knockout',
	'navigation',
	'shared/timeline',
	'views/personschedule/addactivityform',
	'views/personschedule/addfulldayabsenceform',
	'views/personschedule/absencelistitem',
	'views/personschedule/addintradayabsenceform',
	'shared/group-page',
	'helpers',
	'resources!r',
	'select2',
	'lazy'
], function (
	ko,
	navigation,
	timeLineViewModel,
	addActivityFormViewModel,
	addFullDayAbsenceFormViewModel,
	absenceListItemViewModel,
	addIntradayAbsenceFormViewModel,
	groupPageViewModel,
	helpers,
	resources,
	select2,
	lazy
	) {

	return function () {

		var self = this;
		
		this.Loading = ko.observable(false);
		
		this.Persons = ko.observableArray();

		this.Id = ko.observable();
		this.GroupId = ko.observable();
		this.Date = ko.observable();

		this.SelectedPerson = ko.computed(function () {
			return lazy(self.Persons())
				.select(function (x) { return x.Id == self.Id(); })
				.first();
		});

		this.Name = ko.computed(function () {
			if(self.SelectedPerson())
				return self.SelectedPerson().Name;
			return "";
		});

		this.Site = ko.computed(function () {
			if (self.SelectedPerson())
				return self.SelectedPerson().Site;
			return "";
		});
		
		this.Team = ko.computed(function () {
			if (self.SelectedPerson())
				return self.SelectedPerson().Team;
			return "";
		});
		
		this.Absences = ko.observableArray();
		
		this.TimeLine = new timeLineViewModel(this.Persons);
		
		this.Shift = ko.computed(function () {
			if (self.SelectedPerson()) {
				return self.SelectedPerson().Shifts()[0];
			}
			return undefined;
		});
		this.IsShift = ko.computed(function () {
			if(self.Shift())
				return self.Shift().Layers().length > 0;
			return false;
		});
		
		this.DayOff = ko.computed(function () {
			if (self.SelectedPerson()) {
				return self.SelectedPerson().DayOffs()[0];
			}
			return undefined;
		});
		
		this.IsDayOff = ko.computed(function () {
			if (self.DayOff())
				return true;
			return false;
		});
		
		this.FormStartPixel = ko.computed(function () {
			if (self.Shift()) {
				return self.Shift().Layers().length > 0 ? self.Shift().ShiftStartPixels() : 0;
			}
			return 0;
		});

		this.Resources = resources;

		this.AddFullDayAbsenceForm = new addFullDayAbsenceFormViewModel();
		this.AddingFullDayAbsence = ko.observable(false);
		this.AddActivityForm = new addActivityFormViewModel();
		this.AddingActivity = ko.observable(false);
		
		this.AddIntradayAbsenceForm = new addIntradayAbsenceFormViewModel();
		this.AddingIntradayAbsence = ko.observable(false);

		this.DisplayShiftMenu = ko.computed(function() {
			return self.AddingActivity() || self.AddingFullDayAbsence() || self.AddingIntradayAbsence();
		});

		this.DisplayDescriptions = ko.observable(false);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		this.AddPersons = function (persons) {
			self.Persons.push.apply(self.Persons, persons);
		};

		this.SetData = function (data, groupId) {
			self.Absences([]);
			var absences = ko.utils.arrayMap(data.PersonAbsences, function (a) {
				a.PersonId = self.Id();
				a.Date = self.Date();
				return new absenceListItemViewModel(a);
			});
			self.Absences.push.apply(self.Absences, absences);

			data.PersonId = self.Id();

			self.AddFullDayAbsenceForm.SetData(data);
			self.AddActivityForm.SetData(data);
			self.AddIntradayAbsenceForm.SetData(data, groupId);
		};

		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceFormWithoutHistory(self.Id(), self.Date());
		};
	};
});
