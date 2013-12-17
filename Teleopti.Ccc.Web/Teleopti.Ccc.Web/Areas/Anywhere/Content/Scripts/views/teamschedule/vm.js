define([
	'knockout',
	'navigation',
	'lazy',
	'shared/timeline',
	'shared/group-page',
	'views/teamschedule/person',
	'resources!r',
	'moment',
	'select2',
	'shared/current-state'
], function (
	ko,
	navigation,
	lazy,
	timeLineViewModel,
	groupPageViewModel,
	personViewModel,
	resources,
	moment,
	select2,
	currentState
    ) {

	return function () {

		var self = this;

		this.Loading = ko.observable(false);

		this.Persons = ko.observableArray();

		this.TimeLine = new timeLineViewModel(this.Persons);

		this.Resources = resources;

		this.GroupId = ko.observable();
		this.Date = ko.observable(moment());

		this.GroupPages = ko.observableArray();

		this.DisplayDescriptions = ko.observable(false);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		//this.SetPersons = function (persons) {
		//	self.Persons([]);
		//	self.Persons.push.apply(self.Persons, persons);
		//};

		this.SetGroupPages = function (data) {
			self.GroupPages([]);

			var groupPages = data.GroupPages;
			//self.GroupId(data.SelectedGroupId);

			var newItems = ko.utils.arrayMap(groupPages, function (d) {
				return new groupPageViewModel(d);
			});
			self.GroupPages.push.apply(self.GroupPages, newItems);
		};

		var personForId = function (id) {
			if (!id)
				return undefined;
			var person = lazy(self.Persons())
				.select(function (x) { return x.Id == id; })
				.first();
			if (!person) {
				person = new personViewModel({ Id: id });
				self.Persons.push(person);
			}
			return person;
		};

		this.UpdateSchedules = function (data, timeLine) {
			// data might include the same person more than once, with data for more than one day
			// clear all existing persons schedules
			var persons = self.Persons();
			for (var i = 0; i < persons.length; i++) {
				persons[i].ClearData();
			}
			// create any missing persons
			for (var i = 0; i < data.length; i++) {
				var schedule = data[i];
				personForId(schedule.PersonId);
			}

			// add schedule data. a person might get more than 1 schedule added
			for (var i = 0; i < data.length; i++) {
				var schedule = data[i];
				schedule.GroupId = self.GroupId();
				schedule.Date = self.Date();
				var person = personForId(schedule.PersonId);
				person.AddData(schedule, timeLine);

				// refact
				//if (person == self.SelectedPerson())
				//	self.AddIntradayAbsenceForm.SetShiftStartAndEnd(schedule);
			}

			self.Persons().sort(function (first, second) {
				first = first.OrderBy();
				second = second.OrderBy();
				return first == second ? 0 : (first < second ? -1 : 1);
			});
		};

		this.NextDay = function () {
			self.Date(self.Date().add('d', 1));
		};

		this.PreviousDay = function () {
			self.Date(self.Date().add('d', -1));
		};

		this.SelectPerson = function (person) {
			deselectAllPersons(person.Id);
			deselectAllLayers();

			person.Selected(!person.Selected());
			currentState.SetSelectedPersonId(person.Id);
		};

		this.SelectLayer = function (layer, personId) {
			deselectAllPersons();
			deselectAllLayers(layer);

			layer.Selected(!layer.Selected());
			currentState.SetSelectedPersonId(personId);
			if (layer.Selected()) {
				currentState.SetSelectedLayer(layer);
			}
		};

		var deselectAllPersons = function (person) {
			var selectedPersons = lazy(self.Persons())
		    .filter(function (x) {
			if (person && x === person)
				return false;
			return x.Selected();
		    });
			selectedPersons.each(function (x) {
				x.Selected(false);
			});
		};

		var deselectAllLayers = function (layer) {
			var selectedLayers = lazy(self.Persons())
				   .map(function (x) { return x.Shifts(); })
				   .flatten()
				   .map(function (x) { return x.Layers(); })
				   .flatten()
				   .filter(function (x) {
					if (layer && x === layer) {
						return false;
					}
					return x.Selected();
				   });
			selectedLayers.each(function (x) {
				x.Selected(false);
			});
		};

















		this.Skills = ko.observableArray();
		this.SetSkills = function (skills) {
			self.Skills([]);
			if (skills.length > 0) {
				self.Skills.push.apply(self.Skills, skills);
			}
		};
		this.SelectSkillById = function (id) {
			var skills = self.Skills();
			var foundItem = ko.utils.arrayFirst(skills, function (item) {
				return item.Id == id;
			});
			self.SelectedSkill(foundItem);
		};
		this.DisplayStaffingMetrics = ko.computed(function () {
			return self.Skills().length > 0;
		});
		this.SelectedSkill = ko.observable();
		this.SelectSkill = function (skill) {
			self.SelectedSkill(skill);
		};
		this.SelectedSkillName = ko.computed(function () {
			var skill = self.SelectedSkill();
			if (!skill)
				return undefined;
			else
				return skill.Name;
		});


		this.LoadingStaffingMetrics = ko.observable(false);

		this.ForecastedHours = ko.observable();
		this.ForecastedHoursDisplay = ko.computed(function () {
			var forecastedHours = self.ForecastedHours();
			if (forecastedHours != undefined) {
				return self.Resources.Forecasted + ': ' + forecastedHours.toFixed(2) + ' ' + self.Resources.HourShort;
			}
			return self.Resources.Forecasted + ': ' + '__' + ' ' + self.Resources.HourShort;
		});
		this.ScheduledHours = ko.observable();
		this.ScheduledHoursDisplay = ko.computed(function () {
			var scheduledHours = self.ScheduledHours();
			if (scheduledHours != undefined) {
				return self.Resources.Scheduled + ': ' + scheduledHours.toFixed(2) + ' ' + self.Resources.HourShort;
			}
			return self.Resources.Scheduled + ': ' + '__' + ' ' + self.Resources.HourShort;
		});
		this.DiffHours = ko.observable();
		this.DiffPercentage = ko.observable();
		this.DifferenceDisplay = ko.computed(function () {
			var diffHours = self.DiffHours();
			var diffPercentage = self.DiffPercentage();
			if (diffHours != undefined && diffPercentage != undefined)
				return self.Resources.Difference + ': ' + diffHours.toFixed(2) + ' ' + self.Resources.HourShort + ', ' + (diffPercentage * 100).toFixed(2) + ' %';

			return self.Resources.Difference + ': ' + '__' + ' ' + self.Resources.HourShort + ', ' + '__' + ' %';
		});
		this.ESL = ko.observable();
		this.ESLDisplay = ko.computed(function () {
			var esl = self.ESL();
			if (esl != undefined)
				return self.Resources.ESL + ': ' + (esl * 100).toFixed(2) + ' %';
			return '';
		});
		this.SetDailyMetrics = function (data) {
			self.ForecastedHours(data.ForecastedHours);
			self.ESL(data.ESL);
			self.ScheduledHours(data.ScheduledHours);
			self.DiffHours(data.AbsoluteDifferenceHours);
			self.DiffPercentage(data.RelativeDifference);
		};
	};
});
