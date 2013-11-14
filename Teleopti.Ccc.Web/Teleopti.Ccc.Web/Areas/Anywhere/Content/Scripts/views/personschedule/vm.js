define([
	'knockout',
	'navigation',
	'views/personschedule/shift',
	'shared/timeline',
	'views/personschedule/addactivityform',
	'views/personschedule/addfulldayabsenceform',
	'views/personschedule/absencelistitem',
	'views/personschedule/addabsenceform',
	'shared/group-page',
	'helpers',
	'resources!r',
	'select2'
], function (
	ko,
	navigation,
	shiftViewModel,
	timeLineViewModel,
	addActivityFormViewModel,
	addFullDayAbsenceFormViewModel,
	absenceListItemViewModel,
	addAbsenceFormViewModel,
	groupPageViewModel,
	helpers,
	resources,
	select2
	) {

	return function () {

		var self = this;

		this.Loading = ko.observable(false);

		this.Id = ko.observable("");
		this.Date = ko.observable();

		this.Name = ko.observable("");
		this.Site = ko.observable("");
		this.Team = ko.observable("");

		this.IsDayOff = ko.observable(false);
		this.DayOffName = ko.observable("");
		
		this.SelectedGroup = ko.observable();

		this.DayOffs = ko.observableArray();
		this.Shifts = ko.observableArray();
		this.GroupPages = ko.observableArray();
		this.Absences = ko.observableArray();
		this.PersonsInGroup = ko.observableArray();

		this.IsShift = ko.computed(function () {
			return self.Shifts().length > 0;
		});

		this.WorkTimeMinutes = ko.observable(0);
		this.ContractTimeMinutes = ko.observable(0);

		this.ContractTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.ContractTimeMinutes());
			return time.format("H:mm");
		});

		this.WorkTime = ko.computed(function () {
			var time = moment().startOf('day').add('minutes', self.WorkTimeMinutes());
			return time.format("H:mm");
		});
		
		this.TimeLine = new timeLineViewModel(this.PersonsInGroup);

		this.Resources = resources;

		this.AddFullDayAbsenceForm = new addFullDayAbsenceFormViewModel();
		this.AddingFullDayAbsence = ko.observable(false);
		this.AddActivityForm = new addActivityFormViewModel();
		this.AddingActivity = ko.observable(false);
		
		this.AddAbsenceForm = new addAbsenceFormViewModel();
		this.AddingAbsence = ko.observable(false);

		this.DisplayDescriptions = ko.observable(false);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		this.SetPersonsInGroup = function(persons) {
			self.PersonsInGroup([]);
			self.PersonsInGroup.push.apply(self.PersonsInGroup, persons);
		};
		
		this.SetGroupPages = function (data) {
			self.GroupPages([]);

			var groupPages = data.GroupPages;
			
			var newItems = ko.utils.arrayMap(groupPages, function (d) {
				return new groupPageViewModel(d);
			});
			self.GroupPages.push.apply(self.GroupPages, newItems);
			
			self.SelectedGroup(data.SelectedGroupId);
		};

		this.SetData = function (data, groupid) {
			data.Date = self.Date();
			data.PersonId = self.Id();

			self.Name(data.Name);
			self.Site(data.Site);
			self.Team(data.Team);
			self.IsDayOff(data.IsDayOff);
			self.DayOffName(data.DayOffName);

			if (data.Layers.length > 0) {
				var newShift = new shiftViewModel(self.TimeLine, groupid, self.Id(), data.Date);
				newShift.AddLayers(data);
				self.Shifts.push(newShift);
			}

			if (data.DayOff) {
				data.DayOff.Date = data.Date;
				var newDayOff = new dayOff(self.TimeLine, data.DayOff);
				self.DayOffs.push(newDayOff);
			}
			
			self.ContractTimeMinutes(self.ContractTimeMinutes() + data.ContractTimeMinutes);
			self.WorkTimeMinutes(self.WorkTimeMinutes() + data.WorkTimeMinutes);

			self.Absences([]);
			var absences = ko.utils.arrayMap(data.PersonAbsences, function (a) {
				a.PersonId = self.Id();
				a.Date = self.Date();
				return new absenceListItemViewModel(a);
			});
			self.Absences.push.apply(self.Absences, absences);

			self.AddFullDayAbsenceForm.SetData(data);
			self.AddActivityForm.SetData(data);
			self.AddAbsenceForm.SetData(data);
		};
		
		this.ClearData = function () {
			self.Shifts([]);
			self.DayOffs([]);
			self.WorkTimeMinutes(0);
			self.ContractTimeMinutes(0);
		};
	};
});
