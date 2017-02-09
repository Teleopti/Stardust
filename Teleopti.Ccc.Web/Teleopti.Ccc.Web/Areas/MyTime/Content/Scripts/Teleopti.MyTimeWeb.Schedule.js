﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
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
	var scheduleHeight = 668; // Same value as height of class 'weekview-day-schedule'
	var timeLineOffset = 110;
	var pixelToDisplayAll = 38;
	var pixelToDisplayTitle = 16;
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	var daylightSavingAdjustment;
	var baseUtcOffsetInMinutes;
	var currentPage = 'Teleopti.MyTimeWeb.Schedule';

	function _bindData(data) {
		vm.Initialize(data);
		daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;
		baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;
		_initTimeIndicator();
		$('.body-weekview-inner').show();
		completelyLoaded();
	}

	function _fetchData(dataHandler) {
		var selectedDate = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
		ajax.Ajax({
			url: "../api/Schedule/FetchData",
			dataType: "json",
			type: "GET",
			data: {
				date: selectedDate,
				staffingPossiblity: vm.possibilityType
			},
			success: function (data) {
				_bindData(data);
				if (dataHandler != undefined) {
					dataHandler(data);
				}
			}
		});
	}

	function _ensureDST(userTime) {
		//whether in DST is judged in UTC time
		if (daylightSavingAdjustment !== null) {
			var userTimestamp = userTime.valueOf();
			var dstStartTimestamp = moment(daylightSavingAdjustment.StartDateTime + '+00:00').valueOf();
			var dstEndTimestamp = moment(daylightSavingAdjustment.EndDateTime + '+00:00').add(-daylightSavingAdjustment.AdjustmentOffsetInMinutes, 'minutes').valueOf();// EndDateTime has DST

			if (dstStartTimestamp < dstEndTimestamp) {
				if (dstStartTimestamp < userTimestamp && userTimestamp < dstEndTimestamp) {
					adjustToDST(userTime);
				} else {
					resetToUserTimeWithoutDST(userTime);
				}
			} else { // for DST like Brasilia
				if (dstEndTimestamp <= userTimestamp && userTimestamp <= dstStartTimestamp) {
					resetToUserTimeWithoutDST(userTime);
				}
				else {
					adjustToDST(userTime);
				}
			}
		}
	};

	function adjustToDST(userTime) {
		userTime.zone(-daylightSavingAdjustment.AdjustmentOffsetInMinutes - baseUtcOffsetInMinutes);
	}

	function resetToUserTimeWithoutDST(userTime) {
		userTime.zone(-baseUtcOffsetInMinutes);
	}

	function stripTeleoptiTimeToUTCForScenarioTest() {
		var timeWithTimezone = moment(Date.prototype.getTeleoptiTime()).format();
		var timeWithoutTimezone = timeWithTimezone.substr(0, 19);// btw, timezone info is wrong
		return moment(timeWithoutTimezone + "+00:00");
	}

	function _startUpTimeIndicator() {
		setInterval(function () {
			var currentUserDateTime = Date.prototype.getTeleoptiTimeChangedByScenario === true
				? stripTeleoptiTimeToUTCForScenarioTest().zone(-baseUtcOffsetInMinutes)
				: moment().zone(-baseUtcOffsetInMinutes);//work in user timezone, just make life easier

			_ensureDST(currentUserDateTime);

			if (timeIndicatorDateTime === undefined || currentUserDateTime.minutes() !== timeIndicatorDateTime.minutes()) {
				timeIndicatorDateTime = currentUserDateTime;
				_setTimeIndicator(timeIndicatorDateTime);
			}
		}, 1000);
	};

	function _initTimeIndicator() {
		_startUpTimeIndicator();
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
		self.viewPossibilityPermission = ko.observable();
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
			return (self.requestViewModel() || '') !== '';
		});

		self.increaseRequestCount = function (fixedDate) {
			var arr = $.grep(self.days(), function (item, index) {
				return item.fixedDate() === fixedDate;
			});
			if (arr.length > 0) {
				arr[0].textRequestCount(arr[0].textRequestCount() + 1);
			}
		};

		self.setCurrentDate = function (date) {
			if (self.selectedDateSubscription)
				self.selectedDateSubscription.dispose();
			self.selectedDate(date);
			self.selectedDateSubscription = self.selectedDate.subscribe(function (d) {
				Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(d.format('YYYY-MM-DD')));
			});
		};

		self.nextWeek = function () {
			self.selectedDate(self.nextWeekDate());
		};

		var validPossibilitiesTypes = [
			userTexts.hideStaffingPossibility,
			userTexts.showAbsencePossibility,
			userTexts.showOvertimePossibility
		];

		self.possibilityType = ko.observable(0);
		self.possibilityLabel = function () { return validPossibilitiesTypes[self.possibilityType()] };

		self.switchPossibilityType = function (possibilityType) {
			self.possibilityType(possibilityType);
			_fetchData();
		}

		self.previousWeek = function () {
			self.selectedDate(self.previousWeekDate());
		};

		self.today = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week");
		};

		self.week = function (date) {
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
			var requestDay = moment(self.initialRequestDay(), Teleopti.MyTimeWeb.Common.ServiceDateFormat);

			requestViewModel.DateFrom(requestDay);
			requestViewModel.DateTo(requestDay);
			if (requestViewModel.LoadRequestData) {
				if (data && data.StartTime) {
					requestViewModel.LoadRequestData(data);
				} else {
					var day = ko.utils.arrayFirst(self.days(), function (item) {
						return item.fixedDate() === self.initialRequestDay();
					});
					var oaData = day.overtimeAvailability();
					requestViewModel.LoadRequestData(oaData);
				}
			}
		}

		self.isAbsenceReportAvailable = ko.computed(function () {
			if (self.initialRequestDay() === null)
				return false;

			var momentToday = moment(new Date(new Date().getTeleoptiTime())).startOf('day');
			var momentInitialRequestDay = moment(self.initialRequestDay(), Teleopti.MyTimeWeb.Common.ServiceDateFormat);

			var dateDiff = momentInitialRequestDay.diff(momentToday, 'days');

			//Absence report is available only for today and tomorrow.
			var isPermittedDate = (dateDiff === 0 || dateDiff === 1);
			return self.absenceReportPermission() && isPermittedDate;
		});

		self.showAddTextRequestForm = function (data) {
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

		self.showAddAbsenceReportForm = function (data) {
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

		self.showAddShiftExchangeOfferForm = function (data) {
			if (!self.shiftExchangePermission()) {
				return;
			}
			var addShiftOfferModel = { model: new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _displayRequest).Create(defaultDateTimes), type: function () { return 'shiftOffer'; }, CancelAddingNewRequest: self.CancelAddingNewRequest };
			self.requestViewModel(addShiftOfferModel);
			_fillFormData(data);
		}

		self.showAddOvertimeAvailabilityForm = function (data) {
			if (!self.overtimeAvailabilityPermission()) {
				return;
			}
			self.requestViewModel(addOvertimeModel);
			_fillFormData(data);
		};

		self.showAddRequestForm = function (day) {
			self.showAddRequestFormWithData(day.fixedDate(), day.overtimeAvailability());
		};

		var defaultRequestFunction = function () {
			if (self.overtimeAvailabilityPermission())
				return self.showAddOvertimeAvailabilityForm;
			if (self.absenceRequestPermission())
				return self.showAddAbsenceRequestForm;

			return self.showAddTextRequestForm;
		}

		self.showAddRequestFormWithData = function (date, data) {
			self.initialRequestDay(date);

			if ((self.requestViewModel() !== undefined) && (self.requestViewModel().type() === 'absenceReport') && !self.isAbsenceReportAvailable()) {
				self.requestViewModel(null);
			}

			if ((self.requestViewModel() || '') !== '') {
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
			self.viewPossibilityPermission(data.ViewPossibilityPermission);
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
				var isToday = moment(item.FixedDate).isSame(moment(), "day");
				return new DayViewModel(item, isToday ? data.Possibilities : undefined, self);
			});
			self.days(days);
			var minDateArr = data.PeriodSelection.SelectedDateRange.MinDate.split('-');
			var maxDateArr = data.PeriodSelection.SelectedDateRange.MaxDate.split('-');

			self.minDate(moment(new Date(minDateArr[0], minDateArr[1] - 1, minDateArr[2])).add('days', -1));
			self.maxDate(moment(new Date(maxDateArr[0], maxDateArr[1] - 1, maxDateArr[2])).add('days', 1));
		}
	});

	var DayViewModel = function (day, possibility, parent) {
		var self = this;

		self.fixedDate = ko.observable(day.FixedDate);

		self.date = ko.observable(day.Date);
		self.state = ko.observable(day.State);

		var dayDescription = "";
		var dayNumberDisplay = "";
		var dayDate = moment(day.FixedDate, Teleopti.MyTimeWeb.Common.ServiceDateFormat);

		if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
			self.headerTitle = ko.observable(dayDate.format("jdddd"));
			self.dayOfWeek = ko.observable(dayDate.weekday());
			dayNumberDisplay = dayDate.jDate();

			if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
				dayDescription = dayDate.format("jMMMM");
			}
		} else {
			self.headerTitle = ko.observable(day.Header.Title);
			self.dayOfWeek = ko.observable(day.DayOfWeekNumber);
			dayNumberDisplay = dayDate.date();
			if (dayNumberDisplay === 1 || self.dayOfWeek() === parent.weekStart) {
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
		self.hasOvertime = day.HasOvertime && !day.IsFullDayAbsence;
		self.hasShift = day.Summary.Color !== null ? true : false;
		self.noteMessage = ko.computed(function () {
			//need to html encode due to not bound to "text" in ko
			return $('<div/>').text(day.Note.Message).html();
		});

		self.textRequestCount = ko.observable(day.TextRequestCount);
		self.overtimeAvailability = ko.observable(day.OvertimeAvailabililty);
		self.probabilityClass = ko.observable(day.ProbabilityClass);
		self.probabilityText = ko.observable(day.ProbabilityText);

		self.holidayChanceText = ko.computed(function () {
			var probabilityText = self.probabilityText();
			if (probabilityText)
				return parent.userTexts.chanceOfGettingAbsencerequestGranted + probabilityText;
			return probabilityText;
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
			return self.seatBookings().length > 0;
		});

		var getValueOrEmptyString = function (object) {
			return object || '';
		}

		var formatSeatBooking = function (seatBooking) {
			var bookingText = '<tr><td>{0} - {1}</td><td>{2}</td></tr>';

			var fullSeatName = seatBooking.LocationPath !== '' ? seatBooking.LocationPath + '/' : '';
			fullSeatName += getValueOrEmptyString(seatBooking.LocationPrefix) + seatBooking.SeatName + getValueOrEmptyString(seatBooking.LocationSuffix);

			return bookingText.format(
					Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.StartDateTime),
					Teleopti.MyTimeWeb.Common.FormatTime(seatBooking.EndDateTime),
					fullSeatName
					);
		};

		self.seatBookingMessage = ko.computed(function () {
			var message = '<div class="seatbooking-tooltip">' +
								'<span class="tooltip-header">{0}</span><table>'.format(parent.userTexts.seatBookingsTitle);

			var messageEnd = '</table></div>';

			if (self.seatBookings() !== null) {
				self.seatBookings().forEach(function (seatBooking) {
					message += formatSeatBooking(seatBooking);
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
			if (self.summaryStyleClassName() !== null && self.summaryStyleClassName() !== undefined) {
				return showRequestClass + self.summaryStyleClassName(); //last one needs to be becuase of "stripes" and similar
			}
			return showRequestClass; //last one needs to be becuase of "stripes" and similar
		});

		self.colorForDaySummary = ko.computed(function () {
			return parent.styles()[self.summaryStyleClassName()];
		});

		self.textColor = ko.computed(function () {
			var backgroundColor = parent.styles()[self.summaryStyleClassName()];
			if (backgroundColor !== null && backgroundColor !== undefined) {
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});
		self.availability = ko.observable(day.Availability);

		self.absenceRequestPermission = ko.computed(function () {
			return (parent.absenceRequestPermission() && self.availability());
		});

		self.navigateToRequests = function () {
			parent.navigateToRequestsMethod();
		};

		self.showOvertimeAvailability = function (data) {
			var date = self.fixedDate();
			var momentDate = moment(date, Teleopti.MyTimeWeb.Common.ServiceDateFormat);
			if (data.startPositionPercentage() === 0 && data.overtimeAvailabilityYesterday) {
				momentDate.add('days', -1);
			}
			date = Teleopti.MyTimeWeb.Common.FormatServiceDate(momentDate);
			var day = ko.utils.arrayFirst(parent.days(), function (item) {
				return item.fixedDate() === date;
			});
			if (day) {
				parent.showAddRequestForm(day);
			} else {
				parent.showAddRequestFormWithData(date, data.overtimeAvailabilityYesterday);
			}
		};

		var getContinousPeriods = function (periods) {
			if (!periods || periods.length === 0) return [];

			var continousPeriods = [];
			var previousEndTime = "";
			var continousPeriodStart = "";
			for (var l = 0; l < periods.length; l++) {
				var periodTimeSpan = day.Periods[l].TimeSpan;
				var periodStartTime = !periodTimeSpan || periodTimeSpan.indexOf("+1") > 0 ? "00:00" : periodTimeSpan.substring(0, 5);
				var periodEndTime = !periodTimeSpan ? "00:00" : periodTimeSpan.substring(8, 13);

				if (l === 0) {
					continousPeriodStart = periodStartTime;
				}

				if (l === periods.length - 1 || (previousEndTime !== "" && periodStartTime !== previousEndTime)) {
					continousPeriods.push({
						"startTime": continousPeriodStart,
						"endTime": l === periods.length - 1 ? periodEndTime : previousEndTime
					});
					continousPeriodStart = periodStartTime;
				}

				previousEndTime = periodEndTime;
			}

			return continousPeriods;
		};

		var createPossibilityModel = function (rawPossibility) {
			if (rawPossibility == undefined || rawPossibility.length === 0) return [];
			// TODO: If today is full day absence or dayoff, Then hide absence possibility

			var possibilityType = parent.possibilityType();
			if (possibilityType === 1 && day.IsFullDayAbsence) {
				return [];
			}

			var shiftStartTime = "00:00";
			var shiftEndTime = "24:00";
			var shiftStartPosition = 1;
			var shiftEndPosition = 0;
			for (var i = 0; i < day.Periods.length; i++) {
				var period = day.Periods[i];
				var timeSpan = period.TimeSpan;

				if (i === 0 && timeSpan.indexOf("+1") === -1) {
					shiftStartTime = timeSpan.substring(0, 5);
				}

				if (i === day.Periods.length - 1) {
					shiftEndTime = timeSpan.substring(8, 13);
				}

				if (shiftStartPosition > period.StartPositionPercentage) {
					shiftStartPosition = period.StartPositionPercentage;
				}
				if (shiftEndPosition < period.EndPositionPercentage) {
					shiftEndPosition = period.EndPositionPercentage;
				}
			}

			var possibilityNames = ["fair", "good"];
			var possibilityLabels = [parent.userTexts.fair, parent.userTexts.good];
			var possibilities = [];
			possibilities.push({
				styleJson: {
					"top": 0,
					"height": Math.round(scheduleHeight * shiftStartPosition) + "px"
				},
				cssClass: "possibility-none",
				tooltips: ""
			});

			var totalHeight = shiftEndPosition - shiftStartPosition;

			var continousPeriods = [];
			var tooltipsTitle = "";
			if (possibilityType === 1) {
				tooltipsTitle = parent.userTexts.possibilityForAbsence;
				continousPeriods = getContinousPeriods(day.Periods);
			} else if (possibilityType === 2) {
				tooltipsTitle = parent.userTexts.possibilityForOvertime;
			}

			for (var j = 0; j < rawPossibility.length; j++) {
				var intervalPossibility = rawPossibility[j];
				var startMoment = moment(intervalPossibility.StartTime);
				var endMoment = moment(intervalPossibility.EndTime);

				var intervalStartTime = startMoment.format("HH:mm");
				var intervalEndTime = endMoment.isSame(startMoment, "day") ? endMoment.format("HH:mm") : "23:59";

				var visible = shiftStartTime <= intervalStartTime && intervalEndTime <= shiftEndTime;
				if (!visible) continue;

				var inScheduleTimeRange = false;
				if (possibilityType === 1) {
					// Show absence possibility within schedule time range only
					for (var m = 0; m < continousPeriods.length; m++) {
						var continousPeriod = continousPeriods[m];
						if (continousPeriod.startTime <= intervalStartTime && intervalEndTime <= continousPeriod.endTime) {
							inScheduleTimeRange = true;
							break;
						}
					}
				} else {
					inScheduleTimeRange = true;
				}

				var index = intervalPossibility.Possibility;
				var tooltips = inScheduleTimeRange
					? "<div style='text-align: center'>" +
					"  <div>" + tooltipsTitle + "</div>" +
					"  <div class='tooltip-wordwrap' style='font-weight: bold'>" + possibilityLabels[index] + "</div>" +
					"  <div class='tooltip-wordwrap' style='overflow: hidden'>" + intervalStartTime + " - " + intervalEndTime + "</div>" +
					"</div>"
					: "";

				possibilities.push({
					cssClass: "possibility-" + (inScheduleTimeRange ? possibilityNames[index] : "none"),
					tooltips: tooltips
				});
			}

			var heightPerInterval = totalHeight / (possibilities.length - 1);
			for (var k = 1; k < possibilities.length; k++) {
				var topPositionPercentage = shiftStartPosition + heightPerInterval * (k - 1);
				possibilities[k].styleJson = {
					"top": scheduleHeight * topPositionPercentage + "px",
					"height": scheduleHeight * heightPerInterval + "px"
				};
			}

			return possibilities;
		}

		self.possibility = createPossibilityModel(possibility);

		self.layers = ko.utils.arrayMap(day.Periods, function (item) {
			return new LayerViewModel(item, parent.userTexts, self);
		});
	};

	var LayerViewModel = function (layer, userTexts, parent) {
		var self = this;

		self.title = ko.observable(layer.Title);
		self.hasMeeting = ko.computed(function () {
			return layer.Meeting !== null;
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
							userTexts.subjectColon,
							$('<div/>').text(self.meetingTitle()).html(),
							userTexts.locationColon,
							$('<div/>').text(self.meetingLocation()).html(),
							userTexts.descriptionColon,
							$('<div/>').text(self.meetingDescription()).html());
			} else {
				text = self.timeSpan();
			}

			return '<div>{0}</div>{1}'.format(self.title(), text);
		});

		self.backgroundColor = ko.observable('rgb(' + layer.Color + ')');
		self.textColor = ko.computed(function () {
			if (layer.Color !== null && layer.Color !== undefined) {
				var backgroundColor = 'rgb(' + layer.Color + ')';
				return Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(backgroundColor);
			}
			return 'black';
		});

		self.startPositionPercentage = ko.observable(layer.StartPositionPercentage);
		self.endPositionPercentage = ko.observable(layer.EndPositionPercentage);
		self.overtimeAvailabilityYesterday = layer.OvertimeAvailabilityYesterday;
		self.isOvertimeAvailability = ko.observable(layer.IsOvertimeAvailability);
		self.isOvertime = layer.IsOvertime;
		self.top = ko.computed(function () {
			return Math.round(scheduleHeight * self.startPositionPercentage());
		});
		self.height = ko.computed(function () {
			var bottom = Math.round(scheduleHeight * self.endPositionPercentage()) + 1;
			var top = self.top();
			return bottom > top ? bottom - top : 0;
		});
		self.topPx = ko.computed(function () {
			return self.top() + 'px';
		});
		self.widthPx = ko.computed(function () {
			var width;
			if (layer.IsOvertimeAvailability) {
				width = 20;
			} else if (parent.possibility && parent.possibility.length > 0) {
				width = 115;
			} else {
				width = 127;
			}
			return width + "px";
		});
		self.heightPx = ko.computed(function () {
			return self.height() + 'px';
		});
		self.overTimeLighterBackgroundStyle = ko.computed(function () {
			var rgbTohex = function (rgb) {
				if (rgb.charAt(0) === '#')
					return rgb;
				var ds = rgb.split(/\D+/);
				var decimal = Number(ds[1]) * 65536 + Number(ds[2]) * 256 + Number(ds[3]);
				var digits = 6;
				var hexString = decimal.toString(16);
				while (hexString.length < digits)
					hexString += "0";

				return "#" + hexString;
			}

			var getLumi = function (cstring) {
				var matched = /#([\w\d]{2})([\w\d]{2})([\w\d]{2})/.exec(cstring);
				if (!matched) return null;
				return (299 * parseInt(matched[1], 16) + 587 * parseInt(matched[2], 16) + 114 * parseInt(matched[3], 16)) / 1000;
			}

			var lightColor = "#00ffff";
			var darkColor = "#795548";
			var backgroundColor = rgbTohex(self.backgroundColor());
			var useLighterStyle = Math.abs(getLumi(backgroundColor) - getLumi(lightColor)) > Math.abs(getLumi(backgroundColor) - getLumi(darkColor));

			return useLighterStyle;
		});

		self.overTimeDarkerBackgroundStyle = ko.computed(function () { return !self.overTimeLighterBackgroundStyle(); });

		self.styleJson = ko.computed(function () {
			return {
				'top': self.topPx,
				'width': self.widthPx,
				'height': self.heightPx,
				'color': self.textColor,
				'background-size': self.isOvertime ? '11px 11px' : 'initial',
				'background-color': self.backgroundColor
			};
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
		var hourMinuteSecond = timeline.Time.split(":");
		self.minutes = hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1]);
		var timeFromMinutes = moment().startOf('day').add('minutes', self.minutes);

		self.timeText = timeline.TimeLineDisplay;

		self.topPosition = ko.computed(function () {
			return Math.round(scheduleHeight * self.positionPercentage()) + timeLineOffset + 'px';
		});
		self.evenHour = ko.computed(function () {
			return timeFromMinutes.minute() === 0;
		});
	};

	function _setTimeIndicator(theDate) {
		if ($('.week-schedule-ASM-permission-granted').text().indexOf('yes') === -1 ||
			$('.week-schedule-current-week').text().indexOf('yes') === -1) {
			return;
		}

		var timelineHeight = 668;
		var offset = 123;
		var timeindicatorHeight = 2;

		var hours = theDate.hours();
		var minutes = theDate.minutes();
		var clientNowMinutes = (hours * 60) + (minutes * 1);

		var timelineStartMinutes = getMinutes(".weekview-timeline", true);
		var timelineEndMinutes = getMinutes(".weekview-timeline", false);

		var division = (clientNowMinutes - timelineStartMinutes) / (timelineEndMinutes - timelineStartMinutes);
		var position = Math.round(timelineHeight * division) - Math.round(timeindicatorHeight / 2);
		if (position === -1) {
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
				return timeLineViewModel.minutes;
			}
		}
		return null;
	}

	function _subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario',
			page: currentPage
		});
	}

	function _cleanBindings() {
		ko.cleanNode($('#page')[0]);
		if (vm !== null) {
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
		SetupViewModel: function (userTexts, defaultDateTimes, callback) {
			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
				var addRequestViewModel = function () {
					var model = new Teleopti.MyTimeWeb.Request.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest, data.WeekStart, defaultDateTimes);
					model.AddRequestCallback = _displayRequest;

					return model;
				};

				vm = new WeekScheduleViewModel(userTexts, addRequestViewModel, _navigateToRequests, defaultDateTimes, data.WeekStart);
				vm.staffingPossibilityEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_ViewIntradayStaffingPossibility_41608');

				callback();
				$('.moment-datepicker').attr('data-bind', 'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }');
				ko.applyBindings(vm, $('#page')[0]);
			});
		},
		LoadAndBindData: function () {
			_fetchData(_subscribeForChanges);
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
			ajax.AbortAll();
			Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
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
			var currentLength = 0; // max amount of li's
			var currentHeight = 0; // max height of ul
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