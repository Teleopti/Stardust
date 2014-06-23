define([
	'knockout',
	'navigation',
	'shared/timeline',
	'views/personschedule/person',
	'views/personschedule/addactivityform',
	'views/personschedule/addfulldayabsenceform',
	'views/personschedule/absencelistitem',
	'views/personschedule/addintradayabsenceform',
	'views/personschedule/moveactivityform',
	'shared/group-page',
	'helpers',
	'resources',
	'select2',
	'lazy'
], function (
	ko,
	navigation,
	timeLineViewModel,
	personViewModel,
	addActivityFormViewModel,
	addFullDayAbsenceFormViewModel,
	absenceListItemViewModel,
	addIntradayAbsenceFormViewModel,
	moveActivityFormViewModel,
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
		this.SortedPersons = ko.computed(function () {
			return self.Persons().sort(function(first, second) {
				first = first.OrderBy();
				second = second.OrderBy();
				return first == second ? 0 : (first < second ? -1 : 1);
			});
		});

		this.PersonId = ko.observable();
		this.GroupId = ko.observable();
		this.ScheduleDate = ko.observable();
		this.SelectedStartMinutes = ko.observable();

		var personForId = function(id, personArray) {
			if (!id)
				return undefined;
			var person = lazy(self.Persons())
				.filter(function (x) { return x.Id == id; })
				.first();
			if (!person) {
				person = new personViewModel({ Id: id });
				if (!personArray)
					self.Persons.push(person);
				else
					personArray.push(person);
			}
			return person;
		};

		this.SelectedPerson = ko.computed(function () {
			return personForId(self.PersonId());
		});

		this.Name = ko.computed(function () {
			if(self.SelectedPerson())
				return self.SelectedPerson().Name();
			return "";
		});
		
		this.Absences = ko.observableArray();
		
		var layers = function () {
			return lazy(self.Persons())
				.map(function (x) { return x.Shifts(); })
				.flatten()
				.map(function (x) { return x.Layers(); })
				.flatten();
		};

		this.TimeLine = new timeLineViewModel(ko.computed(function () { return layers().toArray(); }));

		this.WorkingShift = ko.computed(function () {
			var person = self.SelectedPerson();
			if (person)
				return lazy(person.Shifts()).filter(function(x) {
					return x.Layers()[0].StartMinutes() > 0;
				}).toArray()[0];
			return undefined;
		});

		this.Shifts = ko.computed(function() {
			var person = self.SelectedPerson();
			if (person)
				return person.Shifts();
			return undefined;
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
			if (self.WorkingShift()) {
				return self.WorkingShift().Layers().length > 0 ? self.WorkingShift().ShiftStartPixels() : 0;
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

		this.MoveActivityForm = new moveActivityFormViewModel();
		this.MovingActivity = ko.observable(false);

		this.DisplayForm = ko.computed(function() {
			return self.AddingActivity() || self.AddingFullDayAbsence() || self.AddingIntradayAbsence() || self.MovingActivity();
		});

		this.DisplayGroupMates = ko.computed(function () {
			return self.AddingActivity();
		});

		this.SetViewOptions = function (options) {
			self.ScheduleDate(moment(options.date, 'YYYYMMDD'));
			self.PersonId(options.personid || options.id);
			self.GroupId(options.groupid);
			if (options.minutes)
				self.SelectedStartMinutes(Number(options.minutes));

		};

		this.UpdateData = function (data) {
			data.Date = self.ScheduleDate();
			
			var person = self.SelectedPerson();
			person.AddData(data, self.TimeLine);

			self.Absences([]);
			var absences = ko.utils.arrayMap(data.PersonAbsences, function (a) {
				a.PersonId = self.PersonId();
				a.Date = self.ScheduleDate();
				return new absenceListItemViewModel(a);
			});
			self.Absences.push.apply(self.Absences, absences);

			data.PersonId = self.PersonId();
			data.Date = self.ScheduleDate();
			data.GroupId = self.GroupId();
			self.AddFullDayAbsenceForm.SetData(data);
			self.AddActivityForm.SetData(data);
			self.AddIntradayAbsenceForm.SetData(data);
			self.MoveActivityForm.SetData(data);
			var selectedLayer = self.SelectedLayer();
			self.MoveActivityForm.update(selectedLayer);
		};

		this.UpdateSchedules = function (data) {
			// if we dont display group mates, then filter out their data
			if (!self.DisplayGroupMates()) {
				data = lazy(data)
					.filter(function (x) {return x.PersonId == self.PersonId(); })
					.toArray();
			}
			// data might include the same person more than once, with data for more than one day
			
			self.Persons([]);

			// add schedule data. a person might get more than 1 schedule added
			var personArray = [];
			for (var i = 0; i < data.length; i++) {
				var schedule = data[i];
				schedule.GroupId = self.GroupId();
				schedule.Offset = self.ScheduleDate();
				schedule.Date = moment(schedule.Date, resources.FixedDateFormatForMoment);
				var person = personForId(schedule.PersonId, personArray);

				person.AddData(schedule, self.TimeLine);
			}

			self.Persons.push.apply(self.Persons, personArray);

			self.AddIntradayAbsenceForm.WorkingShift(self.WorkingShift());
			self.AddActivityForm.WorkingShift(self.WorkingShift());
			var selectedLayer = self.SelectedLayer();
			self.MoveActivityForm.update(selectedLayer);
			
		};

		this.SelectedLayer = function() {
			if (!self.SelectedStartMinutes()) return null;
			var selectedLayers = layers().filter(function(layer) {
				if (layer.StartMinutes() === self.SelectedStartMinutes())
					return layer;
			});
			var activeLayer = selectedLayers.first();
			activeLayer.Selected(true);
			return activeLayer;
		};
		
		this.updateStartTime = function (pixels) {
			var minutes = pixels / this.TimeLine.PixelsPerMinute();
			var newStartTime = (this.MoveActivityForm.OldStartMinutes() + Math.round(minutes/15)*15);
			this.MoveActivityForm.StartTime(newStartTime);
		}

		this.lengthMinutesToPixels = function (minutes) {
			var pixels = minutes * self.TimeLine.PixelsPerMinute();
			return Math.round(pixels);
		};
	};
});
