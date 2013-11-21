define([
	'knockout',
	'navigation',
	'shared/timeline',
	'views/personschedule/addactivityform',
	'views/personschedule/addfulldayabsenceform',
	'views/personschedule/absencelistitem',
	'views/personschedule/addabsenceform',
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
	addAbsenceFormViewModel,
	groupPageViewModel,
	helpers,
	resources,
	select2,
	lazy
	) {

	return function () {

		var self = this;
		
		this.Loading = ko.observable(false);
		
		this.PersonsInGroup = ko.observableArray();

		this.Id = ko.observable("");
		this.Date = ko.observable();

		this.SelectedPerson = ko.computed(function () {
			if (self.PersonsInGroup().length > 0) {
				var selectedPerson = lazy(self.PersonsInGroup())
					.select(function(x) {
						if (x.Id == self.Id()) {
							return x.Name;
						}
					});
				return selectedPerson.first();
			}
			return "";
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
		
		this.ContractTime = ko.computed(function () {
			if (self.SelectedPerson())
				return self.SelectedPerson().ContractTime();
			return 0;
		});

		this.Absences = ko.observableArray();
		
		this.TimeLine = new timeLineViewModel(this.PersonsInGroup); 
		
		this.Shift = ko.computed(function () {
			if (self.SelectedPerson()) {
				return self.SelectedPerson().Shifts()[0];
			}
			return "";
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
			return "";
		});
		this.IsDayOff = ko.computed(function () {
			return self.DayOff();
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
		
		this.AddAbsenceForm = new addAbsenceFormViewModel();
		this.AddingAbsence = ko.observable(false);

		this.DisplayShiftMenu = ko.computed(function() {
			return self.AddingActivity() || self.AddingFullDayAbsence() || self.AddingAbsence();
		});

		this.DisplayDescriptions = ko.observable(false);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		this.AddPersonsToGroup = function (persons) {
			self.PersonsInGroup.push.apply(self.PersonsInGroup, persons);
		};

		this.SetData = function (data, groupid) {
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
			self.AddAbsenceForm.SetData(data, groupid);
		};

		this.AddFullDayAbsence = function () {
			navigation.GotoPersonScheduleAddFullDayAbsenceFormWithoutHistory(self.Id(), self.Date());
		};
	};
});
