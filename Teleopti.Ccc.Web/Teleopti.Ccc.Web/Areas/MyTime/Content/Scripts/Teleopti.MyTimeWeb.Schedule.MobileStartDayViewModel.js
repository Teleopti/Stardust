Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel = function(weekStart, parent, dataService) {
	var self = this;

	var constants = Teleopti.MyTimeWeb.Common.Constants,
		oneWeekRawProbabilities = [];

	self.displayDate = ko.observable();
	self.selectedDate = ko.observable(moment().startOf('day'));
	self.currentUserDate = ko.observable();
	self.isToday = ko.observable(false);
	self.selectedDateSubscription = null;
	self.baseUtcOffsetInMinutes = 0;

	self.summaryColor = ko.observable();
	self.textColor = ko.observable();
	self.summaryName = ko.observable();
	self.summaryTime = ko.observable();

	self.showTrafficLight = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_TrafficLightOnMobileDayView_77447');
	self.trafficLightColor = ko.observable('');
	self.trafficLightIconClass = ko.observable('');
	self.showOldTrafficLightIconOnMobile = ko.observable(false);
	self.showNewTrafficLightIconOnMobile = ko.observable(false);
	self.newTrafficLightIconEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled(
		'MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640'
	);

	self.trafficLightTooltip = ko.observable('');
	self.dayOfWeek = ko.observable();
	self.isFullDayAbsence = false;
	self.isDayOff = false;
	self.hasOvertime = ko.observable(false);
	self.timeLines = ko.observableArray();
	self.periods = [];
	self.layers = ko.observableArray();
	self.scheduleHeight = ko.observable();
	self.unreadMessageCount = ko.observable();
	self.asmEnabled = ko.observable(false);
	self.requestsCount = ko.observable(0);
	self.requestViewModel = ko.observable();

	self.textRequestPermission = ko.observable(false);
	self.absenceRequestPermission = ko.observable(false);
	self.absenceReportPermission = ko.observable(false);
	self.overtimeAvailabilityPermission = ko.observable(false);
	self.shiftTradeRequestPermission = ko.observable(false);
	self.personAccountPermission = ko.observable(false);
	self.requestPermission = ko.observable(false);
	self.showAbsenceReportingCommandItem = ko.observable(false);
	self.showPostShiftTradeMenu = ko.observable(false);
	self.showAddOvertimeRequestMenu = ko.observable(false);

	self.overtimeAvailabililty = null;

	self.menuIsVisible = ko.observable(false);
	self.menuIconIsVisible = ko.observable(true);
	self.focusingRequestForm = ko.observable(false);
	self.isCommandEnable = ko.observable(false);

	self.datePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);
	self.requestDay = null;

	var initializeProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
	self.selectedProbabilityOptionValue = ko.observable(
		initializeProbabilityType != undefined ? initializeProbabilityType : 0
	);
	self.showingAbsenceProbability = ko.observable(initializeProbabilityType === constants.probabilityType.absence);
	self.showingOvertimeProbability = ko.observable(initializeProbabilityType === constants.probabilityType.overtime);
	self.absenceProbabilityEnabled = ko.observable(false);
	self.overtimeProbabilityEnabled = ko.observable(false);
	self.showProbabilityOptionsToggleIcon = ko.observable(false);
	self.mergeIdenticalProbabilityIntervals = true;
	self.hideProbabilityEarlierThanNow = false;
	self.staffingProbabilityOnMobileEnabled = ko.observable(false);
	self.loadingProbabilityData = ko.observable(false);
	self.probabilities = ko.observableArray();
	self.userNowInMinute = 0;
	self.userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
	self.openHourPeriod = null;
	self.isLoading = ko.observable(false);
	self.mobileMonthUrl = '#Schedule/MobileMonth/' + Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;
	self.navigateToMonthView = function() {
		Teleopti.MyTimeWeb.Portal.NavigateTo('Schedule/MobileMonth');
	};

	self.navigateToMessages = function() {
		Teleopti.MyTimeWeb.Portal.NavigateTo('MessageTab');
	};

	self.navigateToRequests = function() {
		Teleopti.MyTimeWeb.Portal.NavigateTo('Requests/Index');
	};

	self.readData = function(data, forceReloadProbabilityData) {
		self.requestDay = moment(data.Date);

		disposeSelectedDateSubscription();
		self.selectedDate(moment(data.Date));

		self.overtimeAvailabililty = data.Schedule.OvertimeAvailabililty;

		self.displayDate(moment(data.Date).format(Teleopti.MyTimeWeb.Common.DateFormat));
		self.summaryColor(data.Schedule.Summary.Color);
		self.textColor(Teleopti.MyTimeWeb.Common.GetTextColorBasedOnBackgroundColor(self.summaryColor()));
		self.summaryName(data.Schedule.Summary.Title);
		self.summaryTime(data.Schedule.Summary.TimeSpan);
		self.isDayOff = data.Schedule.IsDayOff;
		self.isFullDayAbsence = data.Schedule.IsFullDayAbsence;
		self.periods = data.Schedule.Periods;
		self.unreadMessageCount(data.UnReadMessageCount);
		self.asmEnabled(data.AsmEnabled);
		self.openHourPeriod = data.Schedule.OpenHourPeriod;

		self.hasOvertime(data.Schedule.HasOvertime);
		self.requestsCount(data.Schedule.RequestsCount);
		self.baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;
		self.daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;

		if (Teleopti.MyTimeWeb.Common.UseJalaaliCalendar) {
			var dayDate = moment(
				data.Schedule.FixedDate,
				Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly
			);
			self.dayOfWeek(dayDate.format('dddd'));
		} else {
			self.dayOfWeek(data.Schedule.Header.Title);
		}

		var scheduleHeight = Teleopti.MyTimeWeb.Schedule.GetScheduleHeight();
		self.scheduleHeight(scheduleHeight + 'px');

		var timelines = ko.utils.arrayMap(data.TimeLine, function(item) {
			// 5 is half of timeline label height (10px)
			return new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(item, scheduleHeight, -5, true);
		});
		self.timeLines(timelines);

		var rawPeriods = data.Schedule.Periods || [];
		var layers = rawPeriods.map(function(item, index) {
			var layer = new Teleopti.MyTimeWeb.Schedule.LayerViewModel(
				item,
				self,
				true,
				0,
				true,
				null,
				undefined,
				false
			);
			layer.isLastLayer = index == rawPeriods.length - 1;
			return layer;
		});
		self.layers(layers);

		if (data.RequestPermission) {
			self.overtimeAvailabilityPermission(!!data.RequestPermission.OvertimeAvailabilityPermission);
			self.absenceReportPermission(!!data.RequestPermission.AbsenceReportPermission);
			self.textRequestPermission(!!data.RequestPermission.TextRequestPermission);
			self.absenceRequestPermission(!!data.RequestPermission.AbsenceRequestPermission);
			self.shiftTradeRequestPermission(!!data.RequestPermission.ShiftTradeRequestPermission);
			self.personAccountPermission(!!data.RequestPermission.PersonAccountPermission);
			self.showAddOvertimeRequestMenu(!!data.RequestPermission.OvertimeRequestPermission);
		}

		var hasAtLeastOnePermission =
			self.overtimeAvailabilityPermission() ||
			self.absenceReportPermission() ||
			self.textRequestPermission() ||
			self.absenceRequestPermission() ||
			self.shiftTradeRequestPermission() ||
			self.showAddOvertimeRequestMenu();

		self.isCommandEnable(hasAtLeastOnePermission);

		setSelectedDateSubscription(data.Date);

		var dateDiff = self
			.selectedDate()
			.startOf('day')
			.diff(moment().startOf('day'), 'days');
		var isTodayOrTomorrow = dateDiff === 0 || dateDiff === 1;

		self.showAbsenceReportingCommandItem(self.absenceReportPermission() && isTodayOrTomorrow);
		setStaffingProbabilityToggleStates(data);

		if (
			!self.showProbabilityOptionsToggleIcon() ||
			self.selectedProbabilityOptionValue() === constants.probabilityType.none
		) {
			self.probabilities([]);
		}

		if (
			self.showProbabilityOptionsToggleIcon() &&
			(self.selectedProbabilityOptionValue() === constants.probabilityType.absence ||
				self.selectedProbabilityOptionValue() === constants.probabilityType.overtime)
		)
			self.reloadProbabilityData(forceReloadProbabilityData);

		if (self.newTrafficLightIconEnabled) {
			self.trafficLightIconClass(getTrafficLightIconClass(data.Schedule.ProbabilityClass));
		} else {
			self.trafficLightColor(getTrafficLightColor(data.Schedule.ProbabilityClass));
		}

		self.trafficLightTooltip(buildTrafficLightTooltip(data.Schedule.ProbabilityText));
		self.showOldTrafficLightIconOnMobile(
			self.showTrafficLight &&
				!self.newTrafficLightIconEnabled &&
				self.trafficLightColor().length > 0 &&
				self.selectedDate() >= getCurrentUserDate()
		);
		self.showNewTrafficLightIconOnMobile(
			self.showTrafficLight &&
				self.newTrafficLightIconEnabled &&
				self.absenceRequestPermission() &&
				self.trafficLightIconClass().length > 0 &&
				self.selectedDate() >= getCurrentUserDate()
		);

		setPostShiftTradeMenuVisibility(data);
		self.currentUserDate(getCurrentUserDate());
		self.daylightSavingTimeAdjustment = data.DaylightSavingTimeAdjustment;
		self.isLoading(false);
	};

	self.isWithinSelected = function(startDate, endDate) {
		var selectedDate = self.selectedDate().toDate();
		return startDate <= selectedDate && endDate >= selectedDate;
	};

	function setPostShiftTradeMenuVisibility(data) {
		if (!data.RequestPermission) {
			return;
		}
		if (!data.RequestPermission.ShiftExchangePermission) {
			self.showPostShiftTradeMenu(false);
			return;
		}
		var shiftTradeRequestSetting = data.ShiftTradeRequestSetting;
		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel();
		shiftExchangeOfferViewModel.DateTo(self.requestDay);
		shiftExchangeOfferViewModel.OpenPeriodRelativeStart(shiftTradeRequestSetting.OpenPeriodRelativeStart);
		shiftExchangeOfferViewModel.OpenPeriodRelativeEnd(shiftTradeRequestSetting.OpenPeriodRelativeEnd);
		self.showPostShiftTradeMenu(shiftExchangeOfferViewModel.IsSelectedDateInShiftTradePeriod());
	}

	function setSelectedDateSubscription(date) {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();

		self.selectedDate(moment(date));

		self.selectedDateSubscription = self.selectedDate.subscribe(function(date) {
			parent.ReloadSchedule(date);
		});
	}

	function disposeSelectedDateSubscription() {
		if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();
	}

	function getTrafficLightColor(probability) {
		if (self.selectedDate() < getCurrentUserDate()) return '';
		switch (probability) {
			case 'poor': {
				return 'red';
			}
			case 'fair': {
				return 'yellow';
			}
			case 'good': {
				return 'green';
			}
			default:
				return '';
		}
	}

	function getTrafficLightIconClass(probability) {
		if (self.selectedDate() < getCurrentUserDate()) return '';
		switch (probability) {
			case 'poor': {
				return 'traffic-light-progress-poor';
			}
			case 'fair': {
				return 'traffic-light-progress-fair';
			}
			case 'good': {
				return 'traffic-light-progress-good';
			}
			default:
				return '';
		}
	}

	function buildTrafficLightTooltip(text) {
		var userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
		return userTexts.ChanceOfGettingAbsenceRequestGranted + text;
	}

	function fillRequestFormData(requestViewModel) {
		if (requestViewModel.DateFrom) {
			requestViewModel.DateFrom(self.requestDay);
		}
		if (requestViewModel.DateTo) {
			requestViewModel.DateTo(self.requestDay);
		}
	}

	function getCurrentUserDate() {
		var day = Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(self.baseUtcOffsetInMinutes).format(
			constants.serviceDateTimeFormat.dateOnly
		);
		return moment(day);
	}

	self.today = function() {
		self.currentUserDate = ko.observable(getCurrentUserDate());
		parent.ReloadSchedule(self.currentUserDate());
	};

	self.nextDay = function() {
		var nextDate = moment(self.selectedDate()).add(1, 'days');
		parent.ReloadSchedule(nextDate);
	};

	self.previousDay = function() {
		var previousDate = moment(self.selectedDate()).add(-1, 'days');
		parent.ReloadSchedule(previousDate);
	};

	function setStaffingProbabilityToggleStates(data) {
		self.staffingProbabilityOnMobileEnabled(!!data.ViewPossibilityPermission);

		self.absenceProbabilityEnabled(
			!!data.CheckStaffingByIntraday &&
				!!data.AbsenceProbabilityEnabled &&
				self.staffingProbabilityOnMobileEnabled()
		);
		self.overtimeProbabilityEnabled(!!data.OvertimeProbabilityEnabled && self.staffingProbabilityOnMobileEnabled());

		if (
			!self.absenceProbabilityEnabled() &&
			self.selectedProbabilityOptionValue() === constants.probabilityType.absence
		) {
			resetProbabilityOption();
		}

		if (
			!self.overtimeProbabilityEnabled() &&
			self.selectedProbabilityOptionValue() === constants.probabilityType.overtime
		) {
			resetProbabilityOption();
		}

		var withinProbabilityDisplayPeriod =
			self.selectedDate() >= getCurrentUserDate() &&
			self.selectedDate() <
				getCurrentUserDate()
					.add('day', data.StaffingInfoAvailableDays)
					.startOf('day');
		self.showProbabilityOptionsToggleIcon(
			(self.absenceProbabilityEnabled() || self.overtimeProbabilityEnabled()) && withinProbabilityDisplayPeriod
		);
	}

	var probabilityOptionModel = {
		model: new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(self.selectedProbabilityOptionValue(), self),
		type: function() {
			return 'probabilityOptions';
		},
		OnProbabilityOptionSelectCallback: function(selectedOptionValue) {
			self.onProbabilityOptionSelectCallback(selectedOptionValue);
		}
	};

	self.toggleProbabilityOptionsPanel = function() {
		probabilityOptionModel.model = new Teleopti.MyTimeWeb.Schedule.ProbabilityOptionViewModel(
			self.selectedProbabilityOptionValue(),
			self
		);

		if (self.requestViewModel() && self.requestViewModel().type() === probabilityOptionModel.type()) {
			self.requestViewModel(undefined);
			self.focusingRequestForm(false);
		} else {
			self.requestViewModel(probabilityOptionModel);
			self.focusingRequestForm(true);
		}
	};

	self.onProbabilityOptionSelectCallback = function(selectedOptionValue) {
		self.focusingRequestForm(false);

		resetRequestViewModel();
		if (selectedOptionValue === self.selectedProbabilityOptionValue()) {
			return;
		}
		self.selectedProbabilityOptionValue(selectedOptionValue);
		self.showingAbsenceProbability(self.selectedProbabilityOptionValue() === constants.probabilityType.absence);
		self.showingOvertimeProbability(self.selectedProbabilityOptionValue() === constants.probabilityType.overtime);

		if (self.selectedProbabilityOptionValue() === constants.probabilityType.none) {
			self.probabilities([]);
		}

		parent.ReloadSchedule(null, true);
	};

	self.reloadProbabilityData = function(forceReloadProbabilityData) {
		self.loadingProbabilityData(true);

		var isWithinLoadedProbabilityPeriod =
			oneWeekRawProbabilities.length > 0 &&
			moment(oneWeekRawProbabilities[0].Date) <= self.selectedDate() &&
			self.selectedDate() <= moment(oneWeekRawProbabilities[oneWeekRawProbabilities.length - 1].Date);

		self.fixedDate = moment(self.selectedDate());

		if (!isWithinLoadedProbabilityPeriod || forceReloadProbabilityData) {
			self.probabilities([]);
			dataService.fetchProbabilityData(
				self.selectedDate().format('YYYY-MM-DD'),
				self.selectedProbabilityOptionValue(),
				self.updateProbabilityData
			);
		} else {
			var probabilities = oneWeekRawProbabilities.filter(function(r) {
				return r.Date === self.selectedDate().format('YYYY-MM-DD');
			});

			self.probabilities(
				Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(
					probabilities,
					self,
					buildProbabilityOptions()
				)
			);
			self.loadingProbabilityData(false);
		}
	};

	self.updateProbabilityData = function(rawProbabilities) {
		if (!self.absenceProbabilityEnabled() && !self.overtimeProbabilityEnabled()) return;
		oneWeekRawProbabilities = rawProbabilities;

		self.fixedDate = moment(self.selectedDate());

		self.probabilities(
			Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(
				oneWeekRawProbabilities.filter(function(r) {
					return r.Date === self.selectedDate().format('YYYY-MM-DD');
				}),
				self,
				buildProbabilityOptions()
			)
		);

		self.loadingProbabilityData(false);
	};

	self.showMenu = function() {
		self.menuIsVisible(true);
		self.menuIconIsVisible(false);
	};

	self.hideMenuAndRequestForm = function() {
		self.menuIsVisible(false);
		self.focusingRequestForm(false);
		self.menuIconIsVisible(true);
		resetRequestViewModel();
	};

	function buildProbabilityOptions() {
		return {
			probabilityType: self.selectedProbabilityOptionValue(),
			layoutDirection: constants.layoutDirection.vertical,
			timelines: self.timeLines(),
			mergeSameIntervals: self.mergeIdenticalProbabilityIntervals,
			hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
			userTexts: self.userTexts,
			daylightSavingTimeAdjustment: self.daylightSavingTimeAdjustment
		};
	}

	function resetRequestViewModel() {
		self.requestViewModel(undefined);
		self.menuIconIsVisible(true);
		self.focusingRequestForm(false);
	}

	self.CancelAddingNewRequest = resetRequestViewModel;

	function setupRequestViewModel(requestViewModel) {
		self.requestViewModel({
			model: requestViewModel,
			CancelAddingNewRequest: self.CancelAddingNewRequest
		});
		if (requestViewModel.DateFormat) {
			requestViewModel.DateFormat(self.datePickerFormat());
		}
		self.menuIsVisible(false);
		self.menuIconIsVisible(false);
		self.focusingRequestForm(true);
	}

	function addRequestCallBack(data) {
		if (data) {
			var count = self.requestsCount();
			var date = moment(new Date(data.DateFromYear, data.DateFromMonth - 1, data.DateFromDayOfMonth));
			var formattedDate = date.format(constants.serviceDateTimeFormat.dateOnly);
			if (self.requestDay.format(constants.serviceDateTimeFormat.dateOnly) === formattedDate) {
				count++;
			}
			self.requestsCount(count);
		}
		resetRequestViewModel();
	}

	self.showOvertimeAvailabilityForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(parent.Ajax(), function(
			data
		) {
			parent.ReloadSchedule();
			resetRequestViewModel();
		});
		setupRequestViewModel(requestViewModel);

		fillRequestFormData(requestViewModel);
		requestViewModel.LoadRequestData(self.overtimeAvailabililty);
	};

	self.showAbsenceReportingForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(parent.Ajax(), function() {
			resetRequestViewModel();
		});
		setupRequestViewModel(requestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.showAddTextRequestForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel(
			Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			addRequestCallBack,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues
		);

		requestViewModel.AddTextRequest(false);
		setupRequestViewModel(requestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.showAddAbsenceRequestForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel(
			Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
			addRequestCallBack,
			weekStart,
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues
		);

		requestViewModel.readPersonalAccountPermission(self.personAccountPermission());
		requestViewModel.AddAbsenceRequest(false);
		setupRequestViewModel(requestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.redirectToShiftTradeRequest = function() {
		Teleopti.MyTimeWeb.Portal.NavigateTo('Requests/Index/ShiftTrade/' + self.requestDay.format('YYYYMMDD'));
	};

	self.showPostShiftForTradeForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(
			parent.Ajax(),
			addRequestCallBack
		).Create(Teleopti.MyTimeWeb.Common.DateTimeDefaultValues);

		setupRequestViewModel(requestViewModel);
		fillRequestFormData(requestViewModel);
	};

	self.showAddOvertimeRequestForm = function() {
		var requestViewModel = new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(
			parent.Ajax(),
			addRequestCallBack,
			self,
			weekStart
		);

		setupRequestViewModel(requestViewModel);
		fillRequestFormData(requestViewModel);
	};

	function resetProbabilityOption() {
		self.selectedProbabilityOptionValue(constants.probabilityType.none);
		self.showingAbsenceProbability(false);
		self.showingOvertimeProbability(false);
	}
};
