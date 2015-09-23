/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestViewModel.js"/>
/// <reference path="Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel.js"/>
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Schedule = (function ($) {

	var timeIndicatorDateTime;
	var scheduleHeight = 668;
	var timeLineOffset = 110;
	var pixelToDisplayAll = 38;
	var pixelToDisplayTitle = 16;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	var daylightSavingAdjustment;
	var baseUtcOffsetInMinutes;
	
	function _bindData(data) {
		vm.Initialize(data);
		daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;
		baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;
		_initTimeIndicator();
		$('.body-weekview-inner').show();
		completelyLoaded();
	}

	function _fetchData() {
		ajax.Ajax({
			url: 'Schedule/FetchData',
			dataType: "json",
			type: 'GET',
			data: {
				date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
			},
			success: function (data) {
				_bindData(data);
			}
		});
	}

	function _shouldApplyDaylightSavingAdjustment(time) {
		if (daylightSavingAdjustment != null) {
			var daylightSavingStart = moment(daylightSavingAdjustment.StartDateTime);
			var daylightSavingEnd = moment(daylightSavingAdjustment.EndDateTime);
			if (time <= daylightSavingEnd && time >= daylightSavingStart) {
				return true;
			}
		}
		return false;
	};

	function _startUpTimeIndicator() {
		setInterval(function () {
			var currentDateTime = moment().utc().add(baseUtcOffsetInMinutes, 'minutes');
			if (_shouldApplyDaylightSavingAdjustment(currentDateTime)) {
				currentDateTime.add(daylightSavingAdjustment.AdjustmentOffsetInMinutes, 'minutes');
			}
			if (timeIndicatorDateTime == undefined || currentDateTime.minutes() != timeIndicatorDateTime.minutes()) {
				timeIndicatorDateTime = currentDateTime;
				_setTimeIndicator(timeIndicatorDateTime);
			}
		}, 1000);
	};

	function _initTimeIndicator() {
		_startUpTimeIndicator();
	};
	function getDateFormat() {
		return Teleopti.MyTimeWeb.Common.UseJalaaliCalendar ? "DD/MM/YYYY" : Teleopti.MyTimeWeb.Common.DateFormat;
	};

	var WeekScheduleViewModel = function (userTexts, addRequestViewModel, navigateToRequestsMethod, defaultDateTimes, weekStart, undefined) {
		var self = this;
		self.navigateToRequestsMethod = navigateToRequestsMethod;
		self.userTexts = userTexts;
		self.textPermission = ko.observable();
		self.requestPermission = ko.observable();
		self.periodSelection = ko.observable();
		self.asmPermission = ko.observable();
		self.absenceRequestPermission = ko.observable();
		self.absenceReportPermission = ko.observable();
		self.overtimeAvailabilityPermission = ko.observable();
		self.shiftExchangePermission = ko.observable();
		self.personAccountPermission = ko.observable();
		self.showSeatBookingPermission = ko.observable();
		self.isCurrentWeek = ko.observable();
		self.timeLines = ko.observableArray();
		self.days = ko.observableArray();
		self.styles = ko.observable();
		self.minDate = ko.observable(moment());
		self.maxDate = ko.observable(moment());

		self.weekStart = weekStart;
		self.displayDate = ko.observable();
		self.nextWeekDate = ko.observable(moment());
		self.previousWeekDate = ko.observable(moment());
		self.datePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

		self.selectedDate = ko.observable(moment().startOf('day'));
		
		self.requestViewModel = ko.observable();

		self.initialRequestDay = ko.observable();
		self.selectedDateSubscription = null;

		self.showAddRequestToolbar = ko.computed(function () {
			return (self.requestViewModel() || '') != '';
		});

		self.increaseRequestCount = function(fixedDate) {
			var arr = $.grep(self.days(), function(item, index) {
				return item.fixedDate() == fixedDate;
			});
			if (arr.length > 0) {
				arr[0].textRequestCount(arr[0].textRequestCount() + 1);
			}
		};

		self.setCurrentDate = function (date) {
			if (self.selectedDateSubscription)
				self.selectedDateSubscription.dispose();
			self.selectedDate(date);
			self.selectedDateSubscription = self.selectedDate.subscribe(function(d) {
				Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
			});
		};

		self.nextWeek = function() {
			self.selectedDate(self.nextWeekDate());
		};

		self.previousWeek = function () {
			self.selectedDate(self.previousWeekDate());
		};
		
		self.today = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week");
		};

		self.week = function(date) {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
		};
		
		self.month = function () {
			var d = self.selectedDate();
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
		};
		
		self.isWithinSelected = function (startDate, endDate) {
			return (startDate <= self.maxDate() && endDate >= self.minDate());
		};
		
		self.mobile = function () {
			var date = self.selectedDate();
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
		}

		function _fillFormData(data) {
			var requestViewModel = self.requestViewModel().model;
			requestViewModel.DateFormat(self.datePickerFormat());
			var requestDay = moment(self.initialRequestDay(), getDateFormat());
			requestViewModel.DateFrom(requestDay);
			requestViewModel.DateTo(requestDay);
			if (requestViewModel.LoadRequestData) {
				if (data && data.StartTime) {
					requestViewModel.LoadRequestData(data);
				} else {
					var day = ko.utils.arrayFirst(self.days(), function (item) {
						return item.date() == self.initialRequestDay();
					});
					var oaData = day.overtimeAvailability();
					requestViewModel.LoadRequestData(oaData);
				}
			}
		}

		self.isAbsenceReportAvailable = ko.computed(function () {
			if (self.initialRequestDay() == null)
				return false;

			var momentToday = moment(new Date(new Date().getTeleoptiTime())).startOf('day');
			var momentInitialRequestDay = moment(self.initialRequestDay(), getDateFormat());
			var dateDiff = momentInitialRequestDay.diff(momentToday, 'days');

			//Absence report is available only for today and tomorrow.
			var isPermittedDate = (dateDiff == 0 || dateDiff == 1);
			return self.absenceReportPermission() && isPermittedDate;
		});

		self.showAddTextRequestForm = function(data) {
			if (self.textPermission() !== true) {
				return;
			}
			self.requestViewModel(addRequestModel);
			_fillFormData(data);
			self.requestViewModel().model.AddTextRequest(false);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};
		
		self.showAddAbsenceRequestForm = function (data) {
			if (self.absenceRequestPermission() !== true) {
				return;
			}
			self.requestViewModel(addRequestModel);
			self.requestViewModel().model.readPersonalAccountPermission(self.personAccountPermission());
			_fillFormData(data);
			self.requestViewModel().model.AddAbsenceRequest(false);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.showAddAbsenceReportForm = function(data) {
			if (self.absenceReportPermission() !== true) {
				return;
			}
			self.requestViewModel(addAbsenceReportModel);
			_fillFormData(data);
		};

		self.CancelAddingNewRequest = function () {
			self.requestViewModel(undefined);
		};

		function displayOvertimeAvailability() {
			self.CancelAddingNewRequest();
			_fetchData();
		}

		var addOvertimeModel = { model: new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, displayOvertimeAvailability), type: function () { return 'overtime'; }, CancelAddingNewRequest: self.CancelAddingNewRequest };
		var innerRequestModel = createRequestViewModel();
		var addRequestModel = { model: innerRequestModel, type: innerRequestModel.TypeEnum, CancelAddingNewRequest: self.CancelAddingNewRequest };
		var addAbsenceReportModel = { model: new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(ajax, reloadSchedule), type: function () { return 'absenceReport'; }, CancelAddingNewRequest: self.CancelAddingNewRequest };

		self.showAddShiftExchangeOfferForm = function(data) {
			if (!self.shiftExchangePermission()) {
				return;
			}
			var addShiftOfferModel = { model: new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _displayRequest).Create(defaultDateTimes), type: function () { return 'shiftOffer'; }, CancelAddingNewRequest: self.CancelAddingNewRequest };
			self.requestViewModel(addShiftOfferModel);
			_fillFormData(data);
		}

		self.showAddOvertimeAvailabilityForm = function(data) {
			if (!self.overtimeAvailabilityPermission()) {
				return;
			}
			self.requestViewModel(addOvertimeModel);
			_fillFormData(data);
		};

		self.showAddRequestForm = function (day) {
			self.showAddRequestFormWithData(day.date(), day.overtimeAvailability());
		};

		var defaultRequestFunction = function () {
			if (self.overtimeAvailabilityPermission())
				return self.showAddOvertimeAvailabilityForm;
			if (self.absenceRequestPermission())
				return self.showAddAbsenceRequestForm;

			return self.showAddTextRequestForm;
		}

		self.showAddRequestFormWithData = function(date, data) {
			self.initialRequestDay(date);

			if ((self.requestViewModel() != undefined) && (self.requestViewModel().type() == 'absenceReport') && !self.isAbsenceReportAvailable()) {
				self.requestViewModel(null);
			}

			if ((self.requestViewModel() || '') != '') {
				_fillFormData(data);
				return;
			}

			defaultRequestFunction()(data);
		};

		function createRequestViewModel() {
			var datePickerFormat = self.datePickerFormat();
			var model = addRequestViewModel();

			model.DateFormat(datePickerFormat);

			return model;
		}

		function reloadSchedule() {
			self.CancelAddingNewRequest();
			_fetchData();
		}
	};

	ko.utils.extend(WeekScheduleViewModel.prototype, {
		Initialize: function (data) {
			var self = this;
			self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
			self.absenceReportPermission(data.RequestPermission.AbsenceReportPermission);
			self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.shiftExchangePermission(data.RequestPermission.ShiftExchangePermission);
			self.personAccountPermission(data.RequestPermission.PersonAccountPermission);
			self.textPermission(data.RequestPermission.TextRequestPermission);
			self.requestPermission(data.RequestPermission.TextRequestPermission || data.RequestPermission.AbsenceRequestPermission);
			self.showSeatBookingPermission(data.ShowSeatBookingPermission);
			self.periodSelection(JSON.stringify(data.PeriodSelection));
			self.asmPermission(data.AsmPermission);
			self.isCurrentWeek(data.IsCurrentWeek);
			self.displayDate(Teleopti.MyTimeWeb.Common.FormatDatePeriod(
				moment(data.CurrentWeekStartDate),
				moment(data.CurrentWeekEndDate)));

			self.setCurrentDate(moment(data.PeriodSelection.Date));
			self.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
			self.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
			self.datePickerFormat(Teleopti.MyTimeWeb.Common.DateFormat);

			var styleToSet = {};
			$.each(data.Styles, function (key, value) {
				styleToSet[value.Name] = 'rgb({0})'.format(value.RgbColor);
			});
			self.styles(styleToSet);
			var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
				return new TimelineViewModel(item, data.TimeLineCulture);
			});
			self.timeLines(timelines);
			var days = ko.utils.arrayMap(data.Days, function (item) {
				return new DayViewModel(item, self);
			});
			self.days(days);
			var minDateArr = data.PeriodSelection.SelectedDateRange.MinDate.split('-');
			var maxDateArr = data.PeriodSelection.SelectedDateRange.MaxDate.split('-');

			self.minDate(moment(new Date(minDateArr[0], minDateArr[1] - 1, minDateArr[2])).add('days', -1));
			self.maxDate(moment(new Date(maxDateArr[0], maxDateArr[1] - 1, maxDateArr[2])).add('days', 1));
		}
	});

	var DayViewModel = function (day, parent) {
		var self = this;

		self.fixedDate = ko.observable(day.FixedDate);
		
		self.date = ko.observable(day.Date);
		self.state = ko.observable(day.State);

		self.headerTitle = ko.observable(day.Header.Title);
		
		var dayDescription = "";
		var dayNumberDisplay = "";
		var dayDate = moment(day.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);

		if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {

			self.dayOfWeek = ko.observable(dayDate.weekday());
			dayNumberDisplay = dayDate.jDate();
			
			if (dayNumberDisplay == 1 || self.dayOfWeek() == parent.weekStart) {
				dayDescription = dayDate.format("jMMMM");
			}
		} else {
			self.dayOfWeek = ko.observable(day.DayOfWeekNumber);
			dayNumberDisplay = dayDate.date();
			if (dayNumberDisplay == 1 || self.dayOfWeek() == parent.weekStart) {
				dayDescription = dayDate.format("MMMM");
			}
		}
		
		self.headerDayDescription = ko.observable(dayDescription);
	
		self.headerDayNumber = ko.observable(dayNumberDisplay);
		
		self.textRequestPermission = ko.observable(parent.textPermission());
		self.requestPermission = ko.observable(parent.requestPermission());

		self.summaryStyleClassName = ko.observable(day.Summary.StyleClassName);
		self.summaryTitle = ko.observable(day.Summary.Title);
		self.summaryTimeSpan = ko.observable(day.Summary.TimeSpan);
		self.summary = ko.observable(day.Summary.Summary);
		self.noteMessage = ko.computed(function () {
			//need to html encode due to not bound to "text" in ko
			return $('<div/>').text(day.Note.Message).html();
		});
		
		self.textRequestCount = ko.observable(day.TextRequestCount);
		self.overtimeAvailability = ko.observable(day.OvertimeAvailabililty);
		self.probabilityClass = ko.observable(day.ProbabilityClass);
		self.probabilityText = ko.observable(day.ProbabilityText);

		self.holidayChanceText = ko.computed(function () {
			return parent.userTexts.chanceOfGettingAbsencerequestGranted + self.probabilityText();
		});
		
		self.holidayChanceColor = ko.computed(function () {
			return self.probabilityClass();
		});
		
		self.hasTextRequest = ko.computed(function () {
			return self.textRequestCount() > 0;
		});

		self.hasNote = ko.observable(day.HasNote);
		self.seatBookings = ko.observableArray(day.SeatBookings);

		self.seatBookingIconVisible = ko.computed(function () {
			return parent.showSeatBookingPermission() && self.seatBookings().length > 0;
		});

		self.seatBookingMessage = ko.computed(function () {
			var message = '<div class="seatbooking-tooltip">' +
								'<span class="tooltip-header">{0}</span><table>'.format(parent.userTexts.seatBookingsTitle);
			var bookingText = '<tr><td>{0} - {1}</td><td>{2}</td></tr>';
			var messageEnd = '</table></div>';

			if (self.seatBookings() != null ) {

				self.seatBookings().forEach(function (seat) {

					message += bookingText.format(
						Teleopti.MyTimeWeb.Common.FormatTime(seat.StartDateTime),
						Teleopti.MyTimeWeb.Common.FormatTime(seat.EndDateTime),
						seat.LocationPath + '/' + seat.SeatName);
				});

			}
			message += messageEnd;

			return message;

		});
		self.textRequestText = ko.computed(function () {
			return parent.userTexts.xRequests.format(self.textRequestCount());
		});
		
		self.textOvertimeAvailabilityText = ko.computed(function () {
			return self.overtimeAvailability().StartTime + " - " + self.overtimeAvailability().EndTime;
		});

		self.classForDaySummary = ko.computed(function () {
			var showRequestClass = self.requestPermission() ? 'weekview-day-summary weekview-day-show-request ' : 'weekview-day-summary ';
			if (self.summaryStyleClassName() != null && self.summaryStyleClassName() != 'undefined') {
				return showRequestClass + self.summaryStyleClassName(); //last one needs to be becuase of "stripes" and similar
			}
			return showRequestClass; //last one needs to be becuase of "stripes" and similar
		});

		self.colorForDaySummary = ko.computed(function () {
			return parent.styles()[self.summaryStyleClassName()];
		});

		self.textColor = ko.computed(function () {

			var backgroundColor = parent.styles()[self.summaryStyleClassName()];
			if (backgroundColor != null && backgroundColor != 'undefined') {
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});

		self.layers = ko.utils.arrayMap(day.Periods, function (item) {
			return new LayerViewModel(item, parent);
		});
		
		self.availability = ko.observable(day.Availability);
		
		self.absenceRequestPermission = ko.computed(function () {
			return (parent.absenceRequestPermission() && self.availability());
		});

		self.navigateToRequests = function() {
			parent.navigateToRequestsMethod();
		};

		self.showOvertimeAvailability = function (data) {
			var date = self.fixedDate();
			var momentDate = moment(date, "YYYY-MM-DD");
			if (data.startPositionPercentage() == 0 && data.overtimeAvailabilityYesterday) {
				momentDate.add('days', -1);
			}
			date = momentDate.format("YYYY-MM-DD");
			var day = ko.utils.arrayFirst(parent.days(), function (item) {
				return item.fixedDate() == date;
			});
			if (day) {
				parent.showAddRequestForm(day);
			} else {
				parent.showAddRequestFormWithData(momentDate.format(parent.datePickerFormat()), data.overtimeAvailabilityYesterday);
			}
		};
	};
	var LayerViewModel = function (layer, parent) {
		var self = this;

		self.title = ko.observable(layer.Title);
		self.hasMeeting = ko.computed(function () {
			return layer.Meeting != null;
		});
		self.meetingTitle = ko.computed(function () {
			if (self.hasMeeting()) {
				return layer.Meeting.Title;
			}
			return null;
		});
		self.meetingLocation = ko.computed(function () {
			if (self.hasMeeting()) {
				return layer.Meeting.Location;
			}
			return null;
		});
		self.meetingDescription = ko.computed(function () {
			if (self.hasMeeting()) {
				if (layer.Meeting.Description.length > 300) {
					return layer.Meeting.Description.substring(0, 300) + '...';
				}
				return layer.Meeting.Description;
			}
			return null;
		});
		self.timeSpan = ko.computed(function () {
			var originalTimespan = layer.TimeSpan;
			// Remove extra space for extreme long timespan (For example: "10:00 PM - 12:00 AM +1")
			var realTimespan = originalTimespan.length >= 22 ? originalTimespan.replace(" - ", "-").replace(" +1", "+1") : originalTimespan;
			return realTimespan;
		});
		self.color = ko.observable('rgb(' + layer.Color + ')');
		self.textColor = ko.computed(function () {
			if (layer.Color != null && layer.Color != 'undefined') {
				var backgroundColor = 'rgb(' + layer.Color + ')';
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});
		self.tooltipText = ko.computed(function () {
			//not nice! rewrite tooltips in the future!
			var text = '';
			if (self.hasMeeting()) {
				text = ('<div>{0}</div><div style="text-align: left">' +
					'<div class="tooltip-wordwrap" style="overflow: hidden"><i>{1}</i> {2}</div>' +
					'<div class="tooltip-wordwrap" style="overflow: hidden"><i>{3}</i> {4}</div>' +
					'<div class="tooltip-wordwrap" style="white-space: normal"><i>{5}</i> {6}</div>' +
					'</div>')
					.format(self.timeSpan(),
							parent.userTexts.subjectColon,
							$('<div/>').text(self.meetingTitle()).html(),
							parent.userTexts.locationColon,
							$('<div/>').text(self.meetingLocation()).html(),
							parent.userTexts.descriptionColon,
							$('<div/>').text(self.meetingDescription()).html());
			} else {
				text = self.timeSpan();
			}

			return '<div>{0}</div>{1}'.format(self.title(), text);
		});
		self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
		self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
		self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
		self.isOvertimeAvailability = ko.observable(layer.IsOvertimeAvailability);
		self.top = ko.computed(function () {
			return Math.round(scheduleHeight * self.startPositionPercentage());
		});
		self.height = ko.computed(function () {
			var bottom = Math.round(scheduleHeight * self.endPositionPercentage()) + 1;
			var top = self.top();
			return bottom > top ? bottom - top : 0;
		});
		self.width = ko.observable(layer.IsOvertimeAvailability ? '20' : '127');
		self.topPx = ko.computed(function () {
			return self.top() + 'px';
		});
		self.widthPx = ko.computed(function () {
			return self.width() + 'px';
		});
		self.heightPx = ko.computed(function () {
			return self.height() + 'px';
		});
		self.heightDouble = ko.computed(function () {
			return scheduleHeight * (self.endPositionPercentage() - self.startPositionPercentage());
		});
		self.showTitle = ko.computed(function () {
			return self.heightDouble() > pixelToDisplayTitle;
		});
		self.showDetail = ko.computed(function () {
			return self.heightDouble() > pixelToDisplayAll;
		});
	};

	var TimelineViewModel = function (timeline, timelineCulture) {
		var self = this;
		self.positionPercentage = ko.observable(timeline.PositionPercentage);

		self.minutes = ko.observable(timeline.Time.TotalMinutes);
		var timeFromMinutes = moment().startOf('day').add('minutes', self.minutes());

		self.timeText = timeline.TimeLineDisplay;

		self.topPosition = ko.computed(function () {
			return Math.round(scheduleHeight * self.positionPercentage()) + timeLineOffset + 'px';
		});
		self.evenHour = ko.computed(function () {
			return timeFromMinutes.format('mm') == 0;
		});
	};

	function _setTimeIndicator(theDate) {
		if ($('.week-schedule-ASM-permission-granted').text().indexOf('yes') == -1 ||
			$('.week-schedule-current-week').text().indexOf('yes') == -1) {
			return;
		}

		var timelineHeight = 668;
		var offset = 117;
		var timeindicatorHeight = 2;

		var hours = theDate.hours();
		var minutes = theDate.minutes();
		var clientNowMinutes = (hours * 60) + (minutes * 1);

		var timelineStartMinutes = getMinutes(".weekview-timeline", true);
		var timelineEndMinutes = getMinutes(".weekview-timeline", false);

		var division = (clientNowMinutes - timelineStartMinutes) / (timelineEndMinutes - timelineStartMinutes);
		var position = Math.round(timelineHeight * division) - Math.round(timeindicatorHeight / 2);
		if (position == -1) {
			position = 0;
		}

		var dayOfWeek = theDate.day();
		var timeIndicator = $('div[data-mytime-dayofweek="' + dayOfWeek + '"] .weekview-day-time-indicator');
		var timeIndicatorTimeLine = $('.weekview-day-time-indicator-small');

		if (timelineStartMinutes <= clientNowMinutes && clientNowMinutes <= timelineEndMinutes) {
			timeIndicator.css("top", position).show();
			timeIndicatorTimeLine.css("top", position + offset).show();
		}
		else {
			timeIndicator.hide();
			timeIndicatorTimeLine.hide();
		}
	}

	function getMinutes(elementSelector, first) {
		var parent = $(elementSelector);
		var children = parent.children('.weekview-timeline-label');
		var element = first ? children.first()[0] : children.last()[0];

		if (element) {
			var timeLineViewModel = ko.dataFor(element);
			if (timeLineViewModel) {
				return timeLineViewModel.minutes();
			}
		}
		return null;
	}

	function _subscribeForChanges() {
		
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario'
		});

	}
	
	function _cleanBindings() {
		ko.cleanNode($('#page')[0]);
		if (vm != null) {
			vm.days([]);
			vm.timeLines([]);
			vm = null;
		}
	}
	
	function _displayRequest(data) {
		var date = moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth));
		var formattedDate = date.format('YYYY-MM-DD');
		vm.increaseRequestCount(formattedDate);
		vm.CancelAddingNewRequest();
	}
	
	function _navigateToRequests() {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index");
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/Week', Teleopti.MyTimeWeb.Schedule.PartialInit, Teleopti.MyTimeWeb.Schedule.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			readyForInteractionCallback();
			completelyLoaded = completelyLoadedCallback;
		},
		SetupViewModel: function (userTexts, defaultDateTimes) {

			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
				
				var addRequestViewModel = function() {
					var model = new Teleopti.MyTimeWeb.Request.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest, data.WeekStart, defaultDateTimes);
					model.AddRequestCallback = _displayRequest;

					return model;
				};

				vm = new WeekScheduleViewModel(userTexts, addRequestViewModel, _navigateToRequests, defaultDateTimes, data.WeekStart);

				$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
				ko.applyBindings(vm, $('#page')[0]);

			});
		},
		LoadAndBindData: function () {
			ajax.Ajax({
				url: 'Schedule/FetchData',
				dataType: "json",
				type: 'GET',
				data: {
					date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
				},
				success: function (data) {
					_bindData(data);
					_subscribeForChanges();
				}
			});
		},

		ReloadScheduleListener: function (notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);

			if (vm.isWithinSelected(messageStartDate, messageEndDate)) {
				_fetchData();
			};
		},
		PartialDispose: function () {
			_cleanBindings();
		},
		SetTimeIndicator: function (date) {
			_setTimeIndicator(date);
		}
	};

})(jQuery);


Teleopti.MyTimeWeb.Schedule.Layout = (function ($) {

	function _setDayState(curDay) {
		switch ($(curDay).data('mytime-state')) {
			case 1:
				break;
			case 2:
				$(curDay).addClass('today');
				break;
			case 3:
				$(curDay).addClass('editable');
				break;
			case 4:
				$(curDay).addClass('non-editable');
				break;
		}
	}

	return {
		SetSchemaItemsHeights: function () {
			var currentTallest = 0; // Tallest li per row
			var currentLength = 0;  // max amount of li's
			var currentHeight = 0;  // max height of ul
			var i = 0;
			$('.weekview-day').each(function () {
				if ($('li', this).length > currentLength) {
					currentLength = $('li', this).length;
				}

				_setDayState($(this));
			});
			for (i = 3; i <= currentLength; i++) {
				var currentLiRow = $('.weekview-day li:nth-child(' + i + ')');
				$(currentLiRow).each(function () {
					if ($(this).height() > currentTallest) {
						currentTallest = $(this).height();
					}

				});
				$('>div', $(currentLiRow)).css({ 'min-height': currentTallest - 20 }); // remove padding from height
				currentTallest = 0;
			}

			$('.weekview-day').each(function () {
				if ($(this).height() > currentHeight) {
					currentHeight = $(this).height();
				}
			});

			$('.weekview-day li:last-child').each(function () {
				var ulHeight = $(this).parent().height();
				var incBorders = (currentLength * 6);
				$(this).height((currentHeight - ulHeight) + incBorders);
			});
		}
	};

})(jQuery);
