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
				var firstOrderBy = first.OrderBy();
				var firstAgentName = first.Name();
				var secondOrderBy = second.OrderBy();
				var secondAgentName = second.Name();
				var nameOrder = firstAgentName == secondAgentName ? 0 : (firstAgentName < secondAgentName ? -1 : 1);
				return firstOrderBy == secondOrderBy ? nameOrder : (firstOrderBy < secondOrderBy ? -1 : 1);
			});
		}).extend({ throttle: 500 });

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

		this.HasGroupPages = ko.computed(function () {
			return self.GroupPages().length > 0;
		});

		this.DisplayDescriptions = ko.observable(true);
		this.ToggleDisplayDescriptions = function () {
			self.DisplayDescriptions(!self.DisplayDescriptions());
		};

		this.setTimelineWidth = function(width) {
			self.TimeLine.WidthPixels(width);
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
			if (options.selectedStartMinutes) {
				self.PreSelectedStartMinute(Number(options.selectedStartMinutes));
			}
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
				var isAnyLayerSelected = false;
				if (!isNaN(self.PreSelectedStartMinute())) {
					ko.utils.arrayForEach(preSelectedPerson.Shifts(), function(shift) {
						ko.utils.arrayForEach(shift.Layers(), function(layer) {
							if (layer.StartMinutes() == self.PreSelectedStartMinute()) {
								self.SelectLayer(layer, shift);
								isAnyLayerSelected = true;
								return;
							}
						});
						if (isAnyLayerSelected) {
							return;
						}
					});
				} else {
					self.SelectPerson(preSelectedPerson);
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

		this.SelectLayer = function (layer, shift) {
			if (self.Resources.MyTeam_MakeTeamScheduleConsistent_31897) {
				var selectedDate = shift.ShiftMenu.Date;
				var personId = shift.ShiftMenu.PersonId;
				var person = personForId(personId, self.Persons());

				deselectAllPersonsExcept(person);
				person.Selected(!layer.Selected());
				person.SelectedStartMinutes(layer.StartMinutes());
				person.Menu.Date(selectedDate);
			}
			else {
				deselectAllPersonsExcept();
			}

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

		this.HasPermissionForMovingActivity = ko.computed(function() {
			return self.permissionMoveActivity();
		});
	};
});
