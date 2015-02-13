define([
	'knockout',
	'moment',
	'navigation',
	'shared/timeline',
	'views/personschedule/person',
	'views/personschedule/addactivityform',
	'views/personschedule/addfulldayabsenceform',
	'views/personschedule/absencelistitem',
	'views/personschedule/addintradayabsenceform',
	'views/personschedule/moveactivityform',
	'shared/group-page',
	'shared/timezone-display',
	'shared/timezone-current',
	'helpers',
	'resources',
	'select2',
	'lazy'
], function (
	ko,
	moment,
	navigation,
	timeLineViewModel,
	personViewModel,
	addActivityFormViewModel,
	addFullDayAbsenceFormViewModel,
	absenceListItemViewModel,
	addIntradayAbsenceFormViewModel,
	moveActivityFormViewModel,
	groupPageViewModel,
	timezoneDisplay,
	timezoneCurrent,
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
		}).extend({ throttle: 500 });

		this.BusinessUnitId = ko.observable();
		this.PersonId = ko.observable();
		this.GroupId = ko.observable();
		this.ScheduleDate = ko.observable();
		this.SelectedStartMinutes = ko.observable(NaN);

		var personForId = function (id, personArray) {
			if (!id)
				return undefined;
			var person = lazy(personArray)
				.filter(function(x) {
					return x.Id == id;
				})
				.first();
			if (!person) {
				person = new personViewModel({ Id: id });
				personArray.push(person);
			}
			return person;
		};

		this.SelectedPerson = ko.computed(function () {
			return personForId(self.PersonId(), self.Persons());
		});

		this.Name = ko.computed(function () {
			if(self.SelectedPerson())
				return self.SelectedPerson().Name();
			return "";
		});
		
		this.Absences = ko.observableArray();
		
		var layers = function () {
			return lazy(self.Persons()).filter(function(x) {
					return x.Id == self.PersonId();
				})
				.map(function (x) { return x.Shifts(); })
				.flatten()
				.map(function (x) { return x.Layers(); })
				.flatten();
		};
		this.Layers = layers;

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

		this.Resources = resources;

		this.AddFullDayAbsenceForm = new addFullDayAbsenceFormViewModel();
		this.AddingFullDayAbsence = ko.observable(false);
		this.AddActivityForm = new addActivityFormViewModel();
		this.AddingActivity = ko.observable(false);
		
		this.AddIntradayAbsenceForm = new addIntradayAbsenceFormViewModel();
		this.AddingIntradayAbsence = ko.observable(false);

		this.MoveActivityForm = new moveActivityFormViewModel();
		this.MovingActivity = ko.observable(false);
		this.TimeZoneName = ko.observable(false);

		this.DisplayForm = ko.computed(function() {
			return self.AddingActivity() || self.AddingFullDayAbsence() || self.AddingIntradayAbsence() || self.MovingActivity();
		});

		this.DisplayGroupMates = ko.computed(function () {
			return self.Resources.MyTeam_MakeTeamScheduleConsistent_31897
				? true
				: self.AddingActivity();
		});

		this.SetViewOptions = function (options) {
			self.ScheduleDate(timezoneDisplay.FromDate(moment(options.date, 'YYYYMMDD'), timezoneCurrent.IanaTimeZone()));
			self.BusinessUnitId(options.buid);
			self.PersonId(options.personid || options.id);
			self.GroupId(options.groupid);
			self.SelectedStartMinutes(Number(options.minutes));
            self.initMoveActivityForm();
		};

		this.IsOtherTimeZone = ko.computed(function () {
			return timezoneCurrent.IanaTimeZone() !== self.TimeLine.IanaTimeZoneOther();
		});

		this.TimeZoneNameShort = ko.computed(function () {
			if (self.IsOtherTimeZone() && self.TimeZoneName()) {
				var timeZoneName = self.TimeZoneName();
				var end = timeZoneName.indexOf(')');
				return (end < 0) ? timeZoneName : timeZoneName.substring(1, end);
			}
			return undefined;
		});

		this.UpdateData = function (data) {
			data.Date = self.ScheduleDate().clone();

			self.TimeLine.IanaTimeZoneOther(data.IanaTimeZoneOther);
			self.TimeZoneName(data.TimeZoneName);

			var person = self.SelectedPerson();
			person.AddData(data, self.TimeLine);
			
			self.Absences([]);
			var absences = ko.utils.arrayMap(data.PersonAbsences, function (a) {
				a.PersonId = self.PersonId();
				a.BusinessUnitId = self.BusinessUnitId();
				a.Date = self.ScheduleDate().clone();
				a.GroupId = self.GroupId();
				a.PersonName = self.Name();
				a.IanaTimeZoneOther = data.IanaTimeZoneOther;
				a.TimeZoneName = data.TimeZoneName;
				return new absenceListItemViewModel(a);
			});
			self.Absences.push.apply(self.Absences, absences);

			data.PersonId = self.PersonId();
			data.PersonName = self.Name();
			data.GroupId = self.GroupId();
			data.BusinessUnitId = self.BusinessUnitId();
			self.AddFullDayAbsenceForm.SetData(data);
			self.AddActivityForm.SetData(data);
			self.AddIntradayAbsenceForm.SetData(data);
			self.MoveActivityForm.SetData(data);
			
		};

		this.UpdateSchedules = function (data) {
			var schedules = data.Schedules;

			// if we dont display group mates, then filter out their data
			if (!self.DisplayGroupMates()) {
				schedules = lazy(schedules)
					.filter(function (x) {return x.PersonId == self.PersonId(); })
					.toArray();
			}

			// data might include the same person more than once, with schedule for more than one day
			self.Persons([]);
			var people = [];

			// add schedule data. a person might get more than 1 schedule added
			for (var i = 0; i < schedules.length; i++) {
				var schedule = schedules[i];
				schedule.GroupId = self.GroupId();
				schedule.Offset = self.ScheduleDate().clone();
				schedule.Date = moment.tz(schedule.Date, timezoneCurrent.IanaTimeZone());

				var person = personForId(schedule.PersonId, people);
				person.AddData(schedule, self.TimeLine);
			}
			self.Persons(people);

			self.AddIntradayAbsenceForm.WorkingShift(self.WorkingShift());
			self.AddActivityForm.WorkingShift(self.WorkingShift());

			if (self.MovingActivity()) {
                self.MoveActivityForm.WorkingShift(self.WorkingShift());
                var selectedLayer = self.SelectedLayer();
                self.MoveActivityForm.update(selectedLayer);
			}

			self.TimeLine.BaseDate(data.BaseDate);
		};

		this.SelectedLayer = function () {
			if (typeof self.MoveActivityForm.SelectedStartMinutes() === 'undefined') {
				return null;
			}

			var activeLayer;
			if (isNaN(self.MoveActivityForm.SelectedStartMinutes())) {
				activeLayer = layers().first();
				self.MoveActivityForm.SelectedStartMinutes(activeLayer.StartMinutes());
			} else {
				var selectedLayers = layers().filter(function(layer) {
					var selectedMinutes = self.MoveActivityForm.SelectedStartMinutes();
					var shiftStartMinutes = self.WorkingShift().OriginalShiftStartMinutes;
					if (selectedMinutes < shiftStartMinutes && self.MoveActivityForm.shiftOverMidnight())
						selectedMinutes += 1440;
					if (layer.StartMinutes() === selectedMinutes)
						return layer;
				});
				activeLayer = selectedLayers.first();
			}

			if (activeLayer)
				activeLayer.Selected(true);
			return activeLayer;
		};

		this.SelectLayer = function (layer) {
			if (!self.Resources.MyTeam_MakeTeamScheduleConsistent_31897 || !self.MovingActivity()) return;
			self.MoveActivityForm.IsChangingLayer(true);
			deselectAllLayersExcept();
			self.SelectedStartMinutes(layer.StartMinutes());
			self.initMoveActivityForm();
			self.MoveActivityForm.update(layer);
			
			layer.Selected(!layer.Selected());
			self.MoveActivityForm.IsChangingLayer(false);

			self.initActivityDraggable();
		}

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

	    this.updateStartTime = function(pixels) {
	        var minutes = pixels / self.TimeLine.PixelsPerMinute();
	        var newStartTimeMinutes = self.MoveActivityForm.getMinutesFromStartTime() + Math.round(minutes / 15) * 15;
	        self.MoveActivityForm.StartTime(self.MoveActivityForm.getStartTimeFromMinutes(newStartTimeMinutes));
	    };

		this.lengthMinutesToPixels = function (minutes) {
			var pixels = minutes * self.TimeLine.PixelsPerMinute();
			return Math.round(pixels);
		};

	    this.setTimelineWidth = function(width) {
	        self.TimeLine.WidthPixels(width);
	    };

	    this.initMoveActivityForm = function() {
	    	self.MoveActivityForm.SelectedStartMinutes(self.SelectedStartMinutes());
	    	if (!self.MoveActivityForm.ScheduleDate()) {
	    		self.MoveActivityForm.ScheduleDate(self.ScheduleDate().clone());
		    }
	    };

	    this.backToTeamSchedule = function () {
			navigation.GoToTeamScheduleWithPreselectedParameter(self.BusinessUnitId(), self.GroupId(), self.ScheduleDate(), self.PersonId(), self.SelectedStartMinutes());
	    }

		this.initActivityDraggable = function() {
			$('.time-line-for').attr("data-subscription-done", " ");
			// bind events
			var activeLayer = $(".layer.active");
			if (activeLayer.length !== 0) {

				$(".layer.active").draggable({
					helper: 'clone',
					cursor: "move",
					zIndex: 100,
					stack: ".layer",
					axis: 'x',
					containment: 'parent',
					stop: function (e, ui) {
						var workingShift = self.WorkingShift();
						var minStartPixel = (workingShift.OriginalShiftStartMinutes - self.TimeLine.StartMinutes()) * self.TimeLine.PixelsPerMinute();
						var maxEndPixel = (workingShift.OriginalShiftEndMinutes - self.TimeLine.StartMinutes()) * self.TimeLine.PixelsPerMinute();
						var pixelTolerance = 1; /*make activity can be moved to the beginning or the end of the shift. bug 30603*/
						if (ui.position.left + ui.helper[0].offsetWidth <= (maxEndPixel + pixelTolerance) &&
							ui.position.left >= (minStartPixel - pixelTolerance)) {
							var pixelsChanged = ui.position.left - ui.originalPosition.left;
							self.updateStartTime(pixelsChanged);
							if (self.MoveActivityForm.isMovingToAnotherDay())
								self.MoveActivityForm.reset();
						}
					}
				});
			}
			self.MoveActivityForm.reset();
		}
	};
});
