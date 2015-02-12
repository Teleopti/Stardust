define([
	'knockout',
	'navigation',
	'lazy',
	'shared/timeline',
	'shared/group-page',
	'views/teamschedule/person',
	'resources',
	'moment',
	'momentTimezoneData',
	'shared/timezone-current'
], function (
	ko,
	navigation,
	lazy,
	timeLineViewModel,
	groupPageViewModel,
	personViewModel,
	resources,
	moment,
	momentTimezoneData,
	timezoneCurrent
    ) {

	return function () {

		var self = this;
		
		this.permissionAddFullDayAbsence = ko.observable();
		this.permissionAddIntradayAbsence = ko.observable();
		this.permissionRemoveAbsence = ko.observable();
		this.permissionAddActivity = ko.observable();
		this.permissionMoveActivity = ko.observable();

		this.Loading = ko.observable(false);
		this.PreSelectedPersonId = ko.observable(false);
		this.PreSelectedStartMinute = ko.observable(NaN);
		this.BusinessUnitId = ko.observable();

		this.Persons = ko.observableArray();
		this.SortedPersons = ko.computed(function() {
			return self.Persons().sort(function (first, second) {
				first = first.OrderBy();
				second = second.OrderBy();
				return first == second ? 0 : (first < second ? -1 : 1);
			});
		});

		var layers = function() {
			return lazy(self.Persons())
				.map(function(x) { return x.Shifts(); })
				.flatten()
				.map(function(x) { return x.Layers(); })
				.flatten();
		};

		this.TimeLine = new timeLineViewModel(ko.computed(function() { return layers().toArray(); }));
		
		this.Resources = resources;

		this.GroupId = ko.observable();
		this.Date = ko.observable(moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day'));

		this.DateFormatted = ko.computed(function () {
		    return self.Date().format(resources.DateFormatForMoment);
		});

		this.GroupPages = ko.observableArray();

		this.DisplayDescriptions = ko.observable(false);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		this.SetGroupPages = function (data) {
			self.GroupPages([]);
			var newItems = ko.utils.arrayMap(data.GroupPages, function (d) {
				return new groupPageViewModel(d);
			});
			self.GroupPages.push.apply(self.GroupPages, newItems);
		};

		var personForId = function(id, personArray) {
			if (!id)
				return undefined;
			var personvm = lazy(personArray)
				.filter(function(x) { return x.Id == id; })
				.first();
			if (!personvm) {
				personvm = new personViewModel({ Id: id });
				personArray.push(personvm);
			}
			return personvm;
		};

		this.SetViewOptions = function (options) {
			self.BusinessUnitId(options.buid);
			self.PreSelectedPersonId(options.personid);
			self.PreSelectedStartMinute(options.selectedStartMinutes);
			self.Date(function() {
				var date = options.date;
				if (date == undefined) {
					return moment.tz(timezoneCurrent.IanaTimeZone()).startOf('day');
				} else {
					return moment.tz(moment(date, 'YYYYMMDD').format('YYYY-MM-DD'), timezoneCurrent.IanaTimeZone());
				}
			}());
		};

		this.UpdateSchedules = function (data) {
			// data might include the same person more than once, with data for more than one day
			self.Persons([]);
			var people = [];

			// add schedule data. a person might get more than 1 schedule added
			var schedules = data.Schedules;
			for (var i = 0; i < schedules.length; i++) {
				var schedule = schedules[i];

				schedule.BusinessUnitId = self.BusinessUnitId();
				schedule.GroupId = self.GroupId();
				schedule.Offset = self.Date();
				schedule.Date = moment.tz(schedule.Date, timezoneCurrent.IanaTimeZone());
				var personvm = personForId(schedule.PersonId, people);
				personvm.AddData(schedule, self.TimeLine);
			}

			self.Persons(people);
			if (self.PreSelectedPersonId()) {
				var preSelectedPerson = personForId(self.PreSelectedPersonId(), people);
				self.SelectPerson(preSelectedPerson);
				var isAnyLayerSelected = false;
				if (!isNaN(self.PreSelectedStartMinute())) {
					ko.utils.arrayForEach(preSelectedPerson.Shifts(), function(shift) {
						ko.utils.arrayForEach(shift.Layers(), function(layer) {
							if (layer.StartMinutes() == self.PreSelectedStartMinute()) {
								layer.Selected(true);
								isAnyLayerSelected = true;
								return;
							}
						});
						if (isAnyLayerSelected) {
							return;
						}
						
					});

				}
				if (isAnyLayerSelected) {
					preSelectedPerson.Selected(false);
				}
				
			}

			this.TimeLine.BaseDate(data.BaseDate);
		};

		this.NextDay = function () {
			self.Date(self.Date().clone().add('d', 1));
		};

		this.PreviousDay = function () {
			self.Date(self.Date().clone().add('d', -1));
		};

		this.SelectPerson = function (person) {
			deselectAllPersonsExcept(person);
			deselectAllLayersExcept();

			person.Selected(!person.Selected());
		};

		this.SelectLayer = function (layer) {
			deselectAllPersonsExcept();
			deselectAllLayersExcept(layer);

			layer.Selected(!layer.Selected());
		};

		var deselectAllPersonsExcept = function(person) {
			var selectedPersons = lazy(self.Persons())
				.filter(function(x) {
					if (person && x === person)
						return false;
					return x.Selected();
				});
			selectedPersons.each(function(x) {
				x.Selected(false);
			});
		};

		var deselectAllLayersExcept = function (layer) {
			var selectedLayers = layers()
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

		this.StaffingMetricsVisible = ko.observable(false);

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
		
		this.moveActivityVisible = ko.observable(false);
	};
});
