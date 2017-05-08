/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Request.RequestViewModel.Tests.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.ViewModels.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.MessageBroker.js" />

if (typeof (Teleopti) === "undefined") {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === "undefined") {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Schedule = (function ($) {
	var vm,
		timeIndicatorDateTime,
		timeLineOffset = 119,
		ajax = new Teleopti.MyTimeWeb.Ajax(),
		completelyLoaded,
		daylightSavingAdjustment,
		baseUtcOffsetInMinutes,
		currentPage = "Teleopti.MyTimeWeb.Schedule",
		constants = Teleopti.MyTimeWeb.Common.Constants;

	function _bindData(data) {
		vm.initializeData(data);
		daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;
		baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;
		_initTimeIndicator();
		$(".body-weekview-inner").show();
		if (completelyLoaded && $.isFunction(completelyLoaded)) {
			completelyLoaded();
		}
	}

	function _fetchData(dataHandler) {
		var selectedDate = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
		ajax.Ajax({
			url: "../api/Schedule/FetchWeekData",
			dataType: "json",
			type: "GET",
			data: {
				date: selectedDate,
			},
			success: function (data) {
				vm.setCurrentDate(moment(data.PeriodSelection.Date));
				_bindData(data);
				_setTimeIndicator(getCurrentUserDateTime(vm.baseUtcOffsetInMinutes));
				if (dataHandler != undefined) {
					dataHandler(data);
				}
			}
		});
	}

	function _ensureDST(userTime) {
		//whether in DST is judged in UTC time
		if (daylightSavingAdjustment == undefined || daylightSavingAdjustment === null) {
			return;
		}

		var userTimestamp = userTime.valueOf();
		var dstStartTimestamp = moment(daylightSavingAdjustment.StartDateTime + "+00:00").valueOf();
		var dstEndTimestamp = moment(daylightSavingAdjustment.EndDateTime + "+00:00").add(-daylightSavingAdjustment.AdjustmentOffsetInMinutes, "minutes").valueOf();// EndDateTime has DST

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
	};

	function adjustToDST(userTime) {
		userTime.zone(-daylightSavingAdjustment.AdjustmentOffsetInMinutes - baseUtcOffsetInMinutes);
	}

	function resetToUserTimeWithoutDST(userTime) {
		userTime.zone(-baseUtcOffsetInMinutes);
	}

	function stripTeleoptiTimeToUTCForScenarioTest() {
		var timeWithTimezone, teleoptiTime = Date.prototype.getTeleoptiTime && Date.prototype.getTeleoptiTime();
		if(teleoptiTime)
		 timeWithTimezone = moment(teleoptiTime).format();
		else
			timeWithTimezone = moment().format();

		return moment(timeWithTimezone.substr(0, 19) + "+00:00");//btw, timezone info is wrong? why? need confirmation
	}

	function getCurrentUserDateTime(utcOffsetInMinutes) {
		var currentUserDateTime = Date.prototype.getTeleoptiTimeChangedByScenario === true
				? stripTeleoptiTimeToUTCForScenarioTest().zone(-utcOffsetInMinutes)
				: moment().zone(-utcOffsetInMinutes);//work in user timezone, just make life easier

		_ensureDST(currentUserDateTime);
		return currentUserDateTime;
	}

	var currentTimeInterval;
	function _initTimeIndicator() {
		if (currentTimeInterval != undefined) {
			clearInterval(currentTimeInterval);
		}
		currentTimeInterval = setInterval(function () {
			var currentUserDateTime = getCurrentUserDateTime(baseUtcOffsetInMinutes);
			if (timeIndicatorDateTime === undefined || currentUserDateTime.minutes() !== timeIndicatorDateTime.minutes()) {
				timeIndicatorDateTime = currentUserDateTime;
				_setTimeIndicator(timeIndicatorDateTime);
			}
		}, 1000);
	};

	var TimelineViewModel = function (rawTimeline, scheduleHeight, offset) {
		var self = this;
		self.positionPercentage = ko.observable(rawTimeline.PositionPercentage);
		var hourMinuteSecond = rawTimeline.Time.split(":");
		self.minutes = hourMinuteSecond[0] * 60 + parseInt(hourMinuteSecond[1]);
		var timeFromMinutes = moment().startOf("day").add("minutes", self.minutes);

		self.timeText = rawTimeline.TimeLineDisplay;

		self.topPosition = ko.computed(function () {
			return Math.round(scheduleHeight * self.positionPercentage()) + offset + "px";
		});
		self.evenHour = ko.computed(function () {
			return timeFromMinutes.minute() === 0;
		});
	};

    var WeekScheduleViewModel = function (addRequestViewModel, navigateToRequestsMethod, defaultDateTimes, weekStart) {
		var self = this;
		self.initializeData = initializeWeekViewModel;
		self.navigateToRequestsMethod = navigateToRequestsMethod;
        self.userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
		self.textPermission = ko.observable();
		self.requestPermission = ko.observable();
		self.periodSelection = ko.observable();
		self.asmPermission = ko.observable();
		self.absenceRequestPermission = ko.observable();
		self.absenceReportPermission = ko.observable();
		self.overtimeAvailabilityPermission = ko.observable();
		self.shiftExchangePermission = ko.observable();
		self.personAccountPermission = ko.observable();
		self.staffingProbabilityEnabled = false;
		self.staffingProbabilityForMultipleDaysEnabled = false;
		self.absenceProbabilityEnabled = ko.observable();
		self.showProbabilityToggle = ko.observable();
		self.loadingProbabilityData = ko.observable(false); 

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

		self.selectedDate = ko.observable(moment().startOf("day"));

		self.requestViewModel = ko.observable();

		self.initialRequestDay = ko.observable();
		self.selectedDateSubscription = null;

		self.showAddRequestToolbar = ko.computed(function () {
			return (self.requestViewModel() || "") !== "";
		});

		self.increaseRequestCount = function (fixedDate) {
			var arr = $.grep(self.days(), function (item, index) {
				return item.fixedDate() === fixedDate;
			});
			if (arr.length > 0) {
				arr[0].textRequestCount(arr[0].textRequestCount() + 1);
			}
		};

		var validProbabilitiesTypes = [
			self.userTexts.HideStaffingInfo,
			self.userTexts.ShowAbsenceProbability,
			self.userTexts.ShowOvertimeProbability
		];

        self.selectedProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability; 
		self.probabilityTypes = constants.probabilityType;
		self.probabilityLabel = function () {
			var selectedProbabilityType = validProbabilitiesTypes[self.selectedProbabilityType];
			if (!selectedProbabilityType) {
                return self.userTexts.StaffingInfo;
			}
			return selectedProbabilityType;
		};
		self.mergeIdenticalProbabilityIntervals = false;
		self.hideProbabilityEarlierThanNow = true;

		self.switchProbabilityType = function (probabilityType) {
			self.selectedProbabilityType = probabilityType;
			if(self.selectedProbabilityType === constants.probabilityType.none){
				self.days().forEach(function(d){
					d.probabilities([]);
				});
                self.loadingProbabilityData(false);
                rebindProbabilityLabel(self);
				return;
			}

			self.fetchProbabilityData();
		};

		self.fetchProbabilityData = function() {
			self.loadingProbabilityData(true);
			ajax.Ajax({
				url: "../api/ScheduleStaffingPossibility",
				dataType: "json",
				type: "GET",
				data: {
					date: self.selectedDate().format('YYYY-MM-DD'),
					staffingPossiblityType: self.selectedProbabilityType
				},
				success: function (data) {
                    self.updateProbabilityData(data);
				    rebindProbabilityLabel(self);
				}
			});
		};

		self.updateProbabilityData = function(data){
			if(!self.staffingProbabilityEnabled) return;

			var options = {
				probabilityType: self.selectedProbabilityType,
				layoutDirection: constants.layoutDirection.vertical,
				timelines: self.timeLines(),
				mergeSameIntervals: self.mergeIdenticalProbabilityIntervals,
				hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
				userTexts: self.userTexts
			};
			if(self.staffingProbabilityForMultipleDaysEnabled) {
				self.days().forEach(function(day){
					var rawProbabilities = data.filter(function(d){return d.Date == day.fixedDate();});
					day.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(rawProbabilities, day, options));
				});
			}else{
				self.days().forEach(function(day){
					if(day.fixedDate() == getCurrentUserDateTime(self.baseUtcOffsetInMinutes).format("YYYY-MM-DD")){
						var rawProbabilities = data.filter(function(d){return d.Date == day.fixedDate();});
						day.probabilities(Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(rawProbabilities, day, options));
					}
				});
			}
			self.loadingProbabilityData(false);
		};

		self.setCurrentDate = function (selectedDate) {
			self.selectedDate(selectedDate);
		};

		self.previousWeek = function () {
			self.selectedDate(self.previousWeekDate());
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability());
		};

		self.nextWeek = function () {
			self.selectedDate(self.nextWeekDate());
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability());
		};

		self.today = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + getUrlPartForProbability());
		};

		self.week = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Week" + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability());
		};

		self.month = function () {
			Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability());
		};

        self.mobile = function () {
            if (showDayScheduleForStartPage()) {
	            Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileDay");
            } else {
		        Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/MobileWeek" + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability());
	        }
		};

		function getUrlPartForDate(date) {
			return Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format("YYYY-MM-DD"));
		}

		function getUrlPartForProbability() {
			return (self.selectedProbabilityType !== constants.probabilityType.none && self.selectedProbabilityType)
				? "/Probability/" + self.selectedProbabilityType
				: "";
		}

		self.isWithinSelected = function (startDate, endDate) {
			return (startDate <= self.maxDate() && endDate >= self.minDate());
		};

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

			var momentToday, teleoptiTime = Date.prototype.getTeleoptiTime && Date.prototype.getTeleoptiTime();
			if(teleoptiTime)
		 		momentToday = moment(teleoptiTime).startOf('day');
			else
				momentToday = moment().format();

			var momentInitialRequestDay = moment(self.initialRequestDay(), Teleopti.MyTimeWeb.Common.ServiceDateFormat);

			var dateDiff = momentInitialRequestDay.diff(momentToday, "days");

			//Absence report is available only for today and tomorrow.
			var isPermittedDate = (dateDiff === 0 || dateDiff === 1);
			return self.absenceReportPermission() && isPermittedDate;
		});

		self.showAddTextRequestForm = function (data) {
			if (self.textPermission() !== true) {
				return;
			}
			self.requestViewModel(addRequestModel);
			self.requestViewModel().model.IsPostingData(false);
			_fillFormData(data);
			self.requestViewModel().model.AddTextRequest(false);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.showAddAbsenceRequestForm = function (data) {
			if (self.absenceRequestPermission() !== true) {
				return;
			}
			self.requestViewModel(addRequestModel);
			self.requestViewModel().model.IsPostingData(false);
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

		var addOvertimeModel = {
			model: new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, displayOvertimeAvailability),
			type: function () { return "overtime"; },
			CancelAddingNewRequest: self.CancelAddingNewRequest
		};

		var innerRequestModel = createRequestViewModel();

		var addRequestModel = {
			model: innerRequestModel,
			type: innerRequestModel.TypeEnum,
			CancelAddingNewRequest: self.CancelAddingNewRequest
		};

		var addAbsenceReportModel = {
			model: new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(ajax, reloadSchedule),
			type: function () { return "absenceReport"; },
			CancelAddingNewRequest: self.CancelAddingNewRequest
		};

		self.showAddShiftExchangeOfferForm = function (data) {
			if (!self.shiftExchangePermission()) {
				return;
			}
			var addShiftOfferModel = {
				model: new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _displayRequest).Create(defaultDateTimes),
				type: function () { return "shiftOffer"; },
				CancelAddingNewRequest: self.CancelAddingNewRequest
			};
			self.requestViewModel(addShiftOfferModel);
			_fillFormData(data);
		};

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
		};

		self.showAddRequestFormWithData = function (date, data) {
			self.initialRequestDay(date);

			if ((self.requestViewModel() !== undefined) && (self.requestViewModel().type() === "absenceReport") && !self.isAbsenceReportAvailable()) {
				self.requestViewModel(null);
			}

			if ((self.requestViewModel() || "") !== "") {
				_fillFormData(data);
				return;
			}

			defaultRequestFunction()(data);
		};

		function createRequestViewModel() {
			var model = addRequestViewModel && addRequestViewModel();
			model.DateFormat(self.datePickerFormat());

			return model;
		}

		function reloadSchedule() {
			self.CancelAddingNewRequest();
			_fetchData();
        }

	};

	function initializeWeekViewModel(data) {
		var self = this;
		var currentUserDate = getCurrentUserDateTime(self.baseUtcOffsetInMinutes).format("YYYY-MM-DD");
		self.isCurrentWeek(data.IsCurrentWeek);

		if(data.RequestPermission){
			self.absenceRequestPermission(data.RequestPermission.AbsenceRequestPermission);
			self.absenceReportPermission(data.RequestPermission.AbsenceReportPermission);
			self.overtimeAvailabilityPermission(data.RequestPermission.OvertimeAvailabilityPermission);
			self.shiftExchangePermission(data.RequestPermission.ShiftExchangePermission);
			self.personAccountPermission(data.RequestPermission.PersonAccountPermission);
			self.textPermission(data.RequestPermission.TextRequestPermission);
			self.requestPermission(data.RequestPermission.TextRequestPermission || data.RequestPermission.AbsenceRequestPermission);
		}

		self.staffingProbabilityEnabled = data.ViewPossibilityPermission && Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_ViewIntradayStaffingProbability_41608");
		self.staffingProbabilityForMultipleDaysEnabled = self.staffingProbabilityEnabled && Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_ViewStaffingProbabilityForMultipleDays_43880");

        self.absenceProbabilityEnabled(data.CheckStaffingByIntraday && self.staffingProbabilityEnabled);

		if (!self.absenceProbabilityEnabled() && self.selectedProbabilityType === constants.probabilityType.absence) {
            self.selectedProbabilityType = constants.probabilityType.none;
            rebindProbabilityLabel(self);
		}

		if(self.staffingProbabilityForMultipleDaysEnabled){
			var interceptWith14Days = (moment(data.Days[data.Days.length - 1].FixedDate) >= moment(currentUserDate)) && (moment(data.Days[0].FixedDate) < moment(currentUserDate).add('day', constants.maximumDaysDisplayingProbability));
			self.showProbabilityToggle(interceptWith14Days);
		}else{
			self.showProbabilityToggle(self.staffingProbabilityEnabled && self.isCurrentWeek());
		}

		self.asmPermission(data.AsmPermission);
		self.displayDate(Teleopti.MyTimeWeb.Common.FormatDatePeriod(
			moment(data.CurrentWeekStartDate),
			moment(data.CurrentWeekEndDate)));

		if(data.PeriodSelection){
			self.periodSelection(JSON.stringify(data.PeriodSelection));
			self.setCurrentDate(moment(data.PeriodSelection.Date));
			self.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
			self.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
			var minDateArr = data.PeriodSelection.SelectedDateRange.MinDate.split("-");
			var maxDateArr = data.PeriodSelection.SelectedDateRange.MaxDate.split("-");
			self.minDate(moment(new Date(minDateArr[0], minDateArr[1] - 1, minDateArr[2])).add("days", -1));
			self.maxDate(moment(new Date(maxDateArr[0], maxDateArr[1] - 1, maxDateArr[2])).add("days", 1));
		}
		self.datePickerFormat(Teleopti.MyTimeWeb.Common.DateFormat);

		var styleToSet = {};
		$.each(data.Styles, function (key, value) {
			styleToSet[value.Name] = "rgb({0})".format(value.RgbColor);
		});

		self.styles(styleToSet);

		var timelines = ko.utils.arrayMap(data.TimeLine, function (item) {
			return new TimelineViewModel(item, constants.scheduleHeight, timeLineOffset);
		});
		self.timeLines(timelines);

		var days = [];
		if(Array.isArray(data.Days) && data.Days.length > 0) {
			days = ko.utils.arrayMap(data.Days, function (scheduleDay) {
				return new Teleopti.MyTimeWeb.Schedule.DayViewModel(scheduleDay, self);
			});
		}
		self.days(days);
		if (self.showProbabilityToggle() && (self.selectedProbabilityType == constants.probabilityType.absence || self.selectedProbabilityType == constants.probabilityType.overtime)) {

			self.fetchProbabilityData();
		}
	}

	function _setTimeIndicator(theDate) {
		if ($(".week-schedule-ASM-permission-granted").text().indexOf("yes") === -1 ||
			$(".week-schedule-current-week").text().indexOf("yes") === -1) {
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
		var timeIndicator = $("div[data-mytime-dayofweek='" + dayOfWeek + "'] .weekview-day-time-indicator");
		var timeIndicatorTimeLine = $(".weekview-day-time-indicator-small");

		if (timelineStartMinutes <= clientNowMinutes && clientNowMinutes <= timelineEndMinutes) {
			timeIndicator.css("top", position).show();
			timeIndicatorTimeLine.css("top", position + offset).show();
		}
		else {
			timeIndicator.hide();
			timeIndicatorTimeLine.hide();
		}

		var days = vm.days();
		var currentUserDate = getCurrentUserDateTime(vm.baseUtcOffsetInMinutes).format("YYYY-MM-DD");
		for (var i = 0; i < days.length; i++) {
			var day = days[i];
			if (day.fixedDate() === currentUserDate) {
				day.userNowInMinute(clientNowMinutes);
			}
		}
	}

	function getMinutes(elementSelector, first) {
		var parent = $(elementSelector);
		var children = parent.children(".weekview-timeline-label");
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
			domainType: "IScheduleChangedInDefaultScenario",
			page: currentPage
		});
	}

	function _cleanBindings() {
		ko.cleanNode($("#page")[0]);
		if (vm !== null) {
			vm.days([]);
			vm.timeLines([]);
			vm = null;
		}
	}

	function _displayRequest(data) {
		var date = moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth));
		var formattedDate = date.format("YYYY-MM-DD");
		vm.increaseRequestCount(formattedDate);
		vm.CancelAddingNewRequest();
	}

	function _navigateToRequests() {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index");
	}

	function rebindProbabilityLabel(viewmodel) {
		ko.applyBindings(viewmodel, $(".probabilityLabel")[0]);
    }

	function showDayScheduleForStartPage() {
		return Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_43446");
	}

	function getMobileScheduleHeight() {
		var screenHeight = isNaN(window.innerHeight) ? window.clientHeight : window.innerHeight;
		var totalToolbarHeight = 185;
		var validHeightForSchedule = screenHeight - totalToolbarHeight;
		return validHeightForSchedule > constants.mobileMinScheduleHeight
			? validHeightForSchedule
			: constants.mobileMinScheduleHeight;
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack("Schedule/Week", Teleopti.MyTimeWeb.Schedule.PartialInit, Teleopti.MyTimeWeb.Schedule.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback, ajaxObj) {
			Teleopti.MyTimeWeb.Test.TestMessage("1partialinit");
			readyForInteractionCallback();
			completelyLoaded = completelyLoadedCallback;
			if (ajaxObj != undefined) {
				ajax = ajaxObj;
			}
			Teleopti.MyTimeWeb.Test.TestMessage("2partialinit");
		},
		SetupViewModel: function (defaultDateTimes, callback) {
			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
				var addRequestViewModel = function () {
					var model = new Teleopti.MyTimeWeb.Request.RequestViewModel(Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest, data.WeekStart, defaultDateTimes);
					model.AddRequestCallback = _displayRequest;

					return model;
				};

				vm = new WeekScheduleViewModel(addRequestViewModel, _navigateToRequests, defaultDateTimes, data.WeekStart);

				callback();
				$(".moment-datepicker").attr("data-bind", "datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: " + data.WeekStart + " }");
				ko.applyBindings(vm, $("#page")[0]);
			});
		},
		TimelineViewModel: TimelineViewModel,
		WeekScheduleViewModel: WeekScheduleViewModel,
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
		},
		GetCurrentUserDateTime: getCurrentUserDateTime,
		GetMobileScheduleHeight: getMobileScheduleHeight
	};
})(jQuery);

Teleopti.MyTimeWeb.Schedule.Layout = (function ($) {
	function _setDayState(curDay) {
		switch ($(curDay).data("mytime-state")) {
			case 1:
				break;
			case 2:
				$(curDay).addClass("today");
				break;
			case 3:
				$(curDay).addClass("editable");
				break;
			case 4:
				$(curDay).addClass("non-editable");
				break;
		}
	}

	return {
		SetSchemaItemsHeights: function () {
			var currentTallest = 0; // Tallest li per row
			var currentLength = 0; // max amount of li's
			var currentHeight = 0; // max height of ul
			var i = 0;
			$(".weekview-day").each(function () {
				if ($("li", this).length > currentLength) {
					currentLength = $("li", this).length;
				}

				_setDayState($(this));
			});
			for (i = 3; i <= currentLength; i++) {
				var currentLiRow = $(".weekview-day li:nth-child(" + i + ")");
				$(currentLiRow).each(function () {
					if ($(this).height() > currentTallest) {
						currentTallest = $(this).height();
					}
				});
				$(">div", $(currentLiRow)).css({ "min-height": currentTallest - 20 }); // remove padding from height
				currentTallest = 0;
			}

			$(".weekview-day").each(function () {
				if ($(this).height() > currentHeight) {
					currentHeight = $(this).height();
				}
			});

			$(".weekview-day li:last-child").each(function () {
				var ulHeight = $(this).parent().height();
				var incBorders = (currentLength * 6);
				$(this).height((currentHeight - ulHeight) + incBorders);
			});
		}
	};
})(jQuery);