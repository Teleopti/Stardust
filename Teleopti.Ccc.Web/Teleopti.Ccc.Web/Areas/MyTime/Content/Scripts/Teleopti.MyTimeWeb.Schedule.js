if (typeof Teleopti === 'undefined') {
	Teleopti = {};

	if (typeof Teleopti.MyTimeWeb === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Schedule = (function($) {
	var vm,
		timeIndicatorDateTime,
		ajax = new Teleopti.MyTimeWeb.Ajax(),
		completelyLoaded,
		daylightSavingAdjustment,
		baseUtcOffsetInMinutes,
		currentPage = 'Teleopti.MyTimeWeb.Schedule',
		constants = Teleopti.MyTimeWeb.Common.Constants;

	function _bindData(data) {
		vm.initializeData(data);
		daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;
		baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;

		_initTimeIndicator();
		$('.body-weekview-inner').show();
		if (completelyLoaded && $.isFunction(completelyLoaded)) {
			completelyLoaded();
		}
	}

	function _fetchData(probabilityType, dataHandler) {
		var selectedDate = Teleopti.MyTimeWeb.Portal.ParseHash().dateHash;

		ajax.Ajax({
			url: '../api/Schedule/FetchWeekData',
			dataType: 'json',
			type: 'GET',
			data: {
				date: selectedDate,
				staffingPossiblityType: probabilityType
			},
			success: function(data) {
				callback(data);
			}
		});

		function callback(data) {
			if (vm === null) return; //Might happen if page is unloaded before this callback is executed
			_bindData(data);

			if (vm === null) return; //Might happen if page is unloaded before this callback is executed
			_setTimeIndicator(getCurrentUserDateTime(vm.baseUtcOffsetInMinutes));
			if (dataHandler != undefined) {
				dataHandler(data);
			}
		}
	}

	function _ensureDST(userTime) {
		//whether in DST is judged in UTC time
		if (daylightSavingAdjustment == undefined || daylightSavingAdjustment === null) {
			return;
		}

		var userTimestamp = userTime.valueOf();
		var dstStartTimestamp = moment(daylightSavingAdjustment.StartDateTime + '+00:00').valueOf();
		var dstEndTimestamp = moment(daylightSavingAdjustment.EndDateTime + '+00:00')
			.add(-daylightSavingAdjustment.AdjustmentOffsetInMinutes, 'minutes')
			.valueOf(); // EndDateTime has DST

		if (dstStartTimestamp < dstEndTimestamp) {
			if (dstStartTimestamp < userTimestamp && userTimestamp < dstEndTimestamp) {
				adjustToDST(userTime);
			} else {
				resetToUserTimeWithoutDST(userTime);
			}
		} else {
			// for DST like Brasilia
			if (dstEndTimestamp <= userTimestamp && userTimestamp <= dstStartTimestamp) {
				resetToUserTimeWithoutDST(userTime);
			} else {
				adjustToDST(userTime);
			}
		}
	}

	function adjustToDST(userTime) {
		userTime.zone(-daylightSavingAdjustment.AdjustmentOffsetInMinutes - baseUtcOffsetInMinutes);
	}

	function resetToUserTimeWithoutDST(userTime) {
		userTime.zone(-baseUtcOffsetInMinutes);
	}

	function stripTeleoptiTimeToUTCForScenarioTest() {
		var timeWithTimezone,
			teleoptiTime = Date.prototype.getTeleoptiTime && Date.prototype.getTeleoptiTime();
		if (teleoptiTime) timeWithTimezone = moment(teleoptiTime).format();
		else timeWithTimezone = moment().format();

		return moment(timeWithTimezone.substr(0, 19) + '+00:00'); //btw, timezone info is wrong? why? need confirmation
	}

	function getCurrentUserDateTime(utcOffsetInMinutes) {
		var currentUserDateTime =
			Date.prototype.getTeleoptiTimeChangedByScenario === true
				? stripTeleoptiTimeToUTCForScenarioTest().zone(-utcOffsetInMinutes)
				: moment().zone(-utcOffsetInMinutes); //work in user timezone, just make life easier

		_ensureDST(currentUserDateTime);
		return currentUserDateTime;
	}

	var currentTimeInterval;
	function _initTimeIndicator() {
		if (currentTimeInterval != undefined) {
			clearInterval(currentTimeInterval);
		}
		currentTimeInterval = setInterval(function() {
			var currentUserDateTime = getCurrentUserDateTime(baseUtcOffsetInMinutes);
			if (
				timeIndicatorDateTime === undefined ||
				currentUserDateTime.minutes() !== timeIndicatorDateTime.minutes()
			) {
				timeIndicatorDateTime = currentUserDateTime;
				_setTimeIndicator(timeIndicatorDateTime);
			}
		}, 1000);
	}

	function WeekScheduleViewModel(
		addRequestViewModel,
		navigateToRequestsMethod,
		defaultDateTimes,
		weekStart,
		overtimeLicAvailable
	) {
		var self = this;
		var serviceDateFormat = Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly;

		self.initializeData = initializeWeekViewModel;
		self.navigateToRequestsMethod = navigateToRequestsMethod;
		self.userTexts = Teleopti.MyTimeWeb.Common.GetUserTexts();
		self.textPermission = ko.observable();
		self.requestPermission = ko.observable();
		self.periodSelection = ko.observable();
		self.asmEnabled = ko.observable();
		self.absenceRequestPermission = ko.observable();
		self.absenceReportPermission = ko.observable();
		self.overtimeAvailabilityPermission = ko.observable();
		self.shiftExchangePermission = ko.observable();
		self.personAccountPermission = ko.observable();
		self.staffingProbabilityEnabled = false;
		self.staffingProbabilityForMultipleDaysEnabled = false;
		self.absenceProbabilityEnabled = ko.observable();
		self.overtimeProbabilityEnabled = ko.observable();
		self.isOvertimeRequestAvailable = ko.observable(true);
		self.showProbabilityToggle = ko.observable();
		self.loadingProbabilityData = ko.observable(false);
		self.newTrafficLightIconEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled(
			'MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640'
		);

		self.isCurrentWeek = ko.observable();
		self.timeLines = ko.observableArray();
		self.days = ko.observableArray();
		self.styles = ko.observable();
		self.minDate = ko.observable(moment());
		self.maxDate = ko.observable(moment());

		self.weekStart = weekStart;
		self.displayDate = ko.observable();
		self.currentUserDate = ko.observable();
		self.nextWeekDate = ko.observable(moment());
		self.previousWeekDate = ko.observable(moment());
		self.datePickerFormat = ko.observable(Teleopti.MyTimeWeb.Common.DateFormat);

		self.selectedDate = ko.observable(moment().startOf('day'));

		self.requestViewModel = ko.observable();
		self.requestViewModelTypes = {
			overtimeAvailability: 'overtimeAvailability',
			textRequest: 'textRequest',
			absenceRequest: 'absenceRequest',
			shiftOffer: 'shiftOffer',
			absenceReport: 'absenceReport',
			overtimeRequest: 'overtimeRequest'
		};

		self.initialRequestDay = ko.observable();
		self.selectedDateSubscription = null;

		self.showAddRequestToolbar = ko.computed(function() {
			return (self.requestViewModel() || '') !== '';
		});

		self.increaseRequestCount = function(fixedDate) {
			var arr = $.grep(self.days(), function(item, index) {
				return item.fixedDate === fixedDate;
			});
			if (arr.length > 0) {
				arr[0].requestsCount += 1;
				arr[0].hasRequests(true);
			}
		};

		self.probabilityTypes = constants.probabilityType;
		self.selectedProbabilityType = Teleopti.MyTimeWeb.Portal.ParseHash().probability;
		self.probabilityLabel = function() {
			if (self.selectedProbabilityType == undefined) {
				return self.userTexts.StaffingInfo;
			}

			var selectedProbabilityTypeLabel;
			switch (self.selectedProbabilityType) {
				case constants.probabilityType.none:
					selectedProbabilityTypeLabel = self.userTexts.HideStaffingInfo;
					break;
				case constants.probabilityType.absence:
					selectedProbabilityTypeLabel = self.userTexts.ShowAbsenceProbability;
					break;
				case constants.probabilityType.overtime:
					selectedProbabilityTypeLabel = self.userTexts.ShowOvertimeProbability;
					break;
			}

			var tempEle = document.createElement('p');
			tempEle.innerHTML = selectedProbabilityTypeLabel;
			return tempEle.textContent;
		};
		self.mergeIdenticalProbabilityIntervals = false;
		self.hideProbabilityEarlierThanNow = true;

		self.switchProbabilityType = function(probabilityType) {
			if (self.selectedProbabilityType == probabilityType) return;

			self.selectedProbabilityType = probabilityType;

			Teleopti.MyTimeWeb.Portal.NavigateTo(
				'Schedule/Week' + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability()
			);

			if (self.selectedProbabilityType === constants.probabilityType.none) {
				self.days().forEach(function(d) {
					d.probabilities([]);
				});
				self.loadingProbabilityData(false);
				rebindProbabilityLabel(self);
			}
		};

		self.fetchProbabilityData = function() {
			self.loadingProbabilityData(true);
			ajax.Ajax({
				url: '../api/ScheduleStaffingPossibility',
				dataType: 'json',
				type: 'GET',
				data: {
					date: self.selectedDate().format('YYYY-MM-DD'),
					staffingPossiblityType: self.selectedProbabilityType
				},
				success: function(data) {
					self.updateProbabilityData(data);
					rebindProbabilityLabel(self);
				}
			});
		};

		self.updateProbabilityData = function(data) {
			if (!self.staffingProbabilityEnabled) return;

			var options = {
				probabilityType: self.selectedProbabilityType,
				layoutDirection: constants.layoutDirection.vertical,
				timelines: self.timeLines(),
				mergeSameIntervals: self.mergeIdenticalProbabilityIntervals,
				hideProbabilityEarlierThanNow: self.hideProbabilityEarlierThanNow,
				userTexts: self.userTexts
			};
			if (self.staffingProbabilityForMultipleDaysEnabled) {
				self.days().forEach(function(day) {
					var rawProbabilities = data.filter(function(d) {
						return d.Date == day.fixedDate;
					});
					day.probabilities(
						Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(
							rawProbabilities,
							day,
							options
						)
					);
				});
			} else {
				self.days().forEach(function(day) {
					if (day.fixedDate == getCurrentUserDateTime(self.baseUtcOffsetInMinutes).format('YYYY-MM-DD')) {
						var rawProbabilities = data.filter(function(d) {
							return d.Date == day.fixedDate;
						});
						day.probabilities(
							Teleopti.MyTimeWeb.Schedule.ProbabilityModels.CreateProbabilityModels(
								rawProbabilities,
								day,
								options
							)
						);
					}
				});
			}
			self.loadingProbabilityData(false);
		};

		self.setSelectedDateSubscription = function(date) {
			if (self.selectedDateSubscription) self.selectedDateSubscription.dispose();

			self.selectedDate(date);

			self.selectedDateSubscription = self.selectedDate.subscribe(function(date) {
				Teleopti.MyTimeWeb.Portal.NavigateTo(
					'Schedule/Week' + getUrlPartForDate(date) + getUrlPartForProbability()
				);
			});
		};

		self.previousWeek = function() {
			self.selectedDate(self.previousWeekDate());
		};

		self.nextWeek = function() {
			self.selectedDate(self.nextWeekDate());
		};

		self.today = function() {
			Teleopti.MyTimeWeb.Portal.NavigateTo('Schedule/Week' + getUrlPartForProbability());
		};

		self.week = function() {
			Teleopti.MyTimeWeb.Portal.NavigateTo(
				'Schedule/Week' + getUrlPartForDate(self.selectedDate()) + getUrlPartForProbability()
			);
		};

		self.month = function() {
			Teleopti.MyTimeWeb.Portal.NavigateTo(
				'Schedule/Month' + getUrlPartForDate(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash)
			);
		};

		self.isWithinSelected = function(startDate, endDate) {
			return startDate <= self.maxDate() && endDate >= self.minDate();
		};

		self.isAbsenceReportAvailable = ko.computed(function() {
			if (self.initialRequestDay() === null) return false;

			var momentToday,
				teleoptiTime = Date.prototype.getTeleoptiTime && Date.prototype.getTeleoptiTime();
			if (teleoptiTime) momentToday = moment(teleoptiTime).startOf('day');
			else momentToday = moment().format();

			var momentInitialRequestDay = moment(self.initialRequestDay(), serviceDateFormat);

			var dateDiff = momentInitialRequestDay.diff(momentToday, 'days');

			//Absence report is available only for today and tomorrow.
			var isPermittedDate = dateDiff === 0 || dateDiff === 1;
			return self.absenceReportPermission() && isPermittedDate;
		});

		self.CancelAddingNewRequest = function() {
			self.requestViewModel(undefined);
		};

		var innerRequestModel = createRequestViewModel();

		var addRequestModel = {
			model: innerRequestModel,
			type: innerRequestModel.TypeEnum,
			CancelAddingNewRequest: self.CancelAddingNewRequest
		};

		self.showAddTextRequestForm = function(data) {
			if (self.textPermission() !== true) {
				return;
			}

			addRequestModel.model = createRequestViewModel();
			addRequestModel.type = function() {
				return self.requestViewModelTypes.textRequest;
			};
			self.requestViewModel(addRequestModel);
			self.requestViewModel().model.IsPostingData(false);
			_fillFormData(data);
			self.requestViewModel().model.AddTextRequest(false);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.showAddAbsenceRequestForm = function(data) {
			if (self.absenceRequestPermission() !== true) {
				return;
			}

			addRequestModel.model = createRequestViewModel();
			addRequestModel.type = function() {
				return self.requestViewModelTypes.absenceRequest;
			};

			self.requestViewModel(addRequestModel);
			self.requestViewModel().model.IsPostingData(false);
			self.requestViewModel().model.readPersonalAccountPermission(self.personAccountPermission());
			_fillFormData(data);
			self.requestViewModel().model.AddAbsenceRequest(false);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.showAddAbsenceReportForm = function(data) {
			if (self.absenceReportPermission() !== true) {
				return;
			}

			var addAbsenceReportModel = {
				model: new Teleopti.MyTimeWeb.Schedule.AbsenceReportViewModel(ajax, reloadSchedule),
				type: function() {
					return self.requestViewModelTypes.absenceReport;
				},
				CancelAddingNewRequest: self.CancelAddingNewRequest
			};

			self.requestViewModel(addAbsenceReportModel);
			_fillFormData(data);
		};

		self.showAddShiftExchangeOfferForm = function(data) {
			if (!self.shiftExchangePermission()) {
				return;
			}
			var addShiftOfferModel = {
				model: new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModelFactory(ajax, _displayRequest).Create(
					defaultDateTimes
				),
				type: function() {
					return self.requestViewModelTypes.shiftOffer;
				},
				CancelAddingNewRequest: self.CancelAddingNewRequest
			};
			self.requestViewModel(addShiftOfferModel);
			_fillFormData(data);
		};

		self.showAddOvertimeAvailabilityForm = function(data) {
			if (!self.overtimeAvailabilityPermission()) {
				return;
			}

			var addOvertimeAvailabilityModel = {
				model: new Teleopti.MyTimeWeb.Schedule.OvertimeAvailabilityViewModel(ajax, displayOvertimeAvailability),
				type: function() {
					return self.requestViewModelTypes.overtimeAvailability;
				},
				CancelAddingNewRequest: self.CancelAddingNewRequest
			};

			self.requestViewModel(addOvertimeAvailabilityModel);
			_fillFormData(data);
		};

		self.showAddRequestForm = function(day) {
			self.showAddRequestFormWithData(day.fixedDate, day.overtimeAvailability);
		};

		self.showAddOvertimeRequestForm = function(data) {
			if (!self.isOvertimeRequestAvailable()) {
				return;
			}
			var addOvertimeRequestModel = {
				model: new Teleopti.MyTimeWeb.Request.OvertimeRequestViewModel(
					ajax,
					function(data) {
						self.CancelAddingNewRequest(data);
						_displayRequest(data);
					},
					self,
					self.weekStart
				),
				type: function() {
					return self.requestViewModelTypes.overtimeRequest;
				},
				CancelAddingNewRequest: self.CancelAddingNewRequest
			};
			self.requestViewModel(addOvertimeRequestModel);
			_fillFormData(data);
		};

		self.showAddRequestFormWithData = function(date, data) {
			self.initialRequestDay(date);

			if (
				self.requestViewModel() !== undefined &&
				self.requestViewModel().type() === self.requestViewModelTypes.absenceReport &&
				!self.isAbsenceReportAvailable()
			) {
				self.requestViewModel(null);
			}

			if ((self.requestViewModel() || '') !== '') {
				_fillFormData(data);
				return;
			}

			defaultRequestFunction()(data);
		};

		function displayOvertimeAvailability() {
			self.CancelAddingNewRequest();
			_fetchData();
		}

		function getUrlPartForDate(date) {
			return Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD'));
		}

		function getUrlPartForProbability() {
			return self.selectedProbabilityType != undefined ? '/Probability/' + self.selectedProbabilityType : '';
		}

		function _fillFormData(data) {
			var requestViewModel = self.requestViewModel().model;
			requestViewModel.DateFormat && requestViewModel.DateFormat(self.datePickerFormat());
			var requestDay = moment(self.initialRequestDay(), serviceDateFormat);

			requestViewModel.DateFrom && requestViewModel.DateFrom(requestDay);
			requestViewModel.DateTo && requestViewModel.DateTo(requestDay);
			if (requestViewModel.LoadRequestData) {
				if (data && data.StartTime) {
					requestViewModel.LoadRequestData(data);
				} else {
					var day = ko.utils.arrayFirst(self.days(), function(item) {
						return item.fixedDate === self.initialRequestDay();
					});
					var oaData = day.overtimeAvailability;
					requestViewModel.LoadRequestData(oaData);
				}
			}
		}

		function defaultRequestFunction() {
			if ($('.overtime-availability-add').length > 0 && self.overtimeAvailabilityPermission())
				return self.showAddOvertimeAvailabilityForm;
			if (self.absenceRequestPermission()) return self.showAddAbsenceRequestForm;

			return self.showAddTextRequestForm;
		}

		function createRequestViewModel() {
			var model = addRequestViewModel && addRequestViewModel();
			model.DateFormat(self.datePickerFormat());

			return model;
		}

		function reloadSchedule(probabilityType, callback) {
			self.CancelAddingNewRequest();
			_fetchData(probabilityType, callback);
		}
	}

	function initializeWeekViewModel(data) {
		var self = this;
		self.baseUtcOffsetInMinutes = data.BaseUtcOffsetInMinutes;
		self.daylightSavingAdjustment = data.DaylightSavingTimeAdjustment;
		var currentUserDate = getCurrentUserDateTime(self.baseUtcOffsetInMinutes).format('YYYY-MM-DD');
		self.currentUserDate(currentUserDate);
		self.isCurrentWeek(data.IsCurrentWeek);

		if (data.RequestPermission) {
			self.absenceRequestPermission(!!data.RequestPermission.AbsenceRequestPermission);
			self.absenceReportPermission(!!data.RequestPermission.AbsenceReportPermission);
			self.overtimeAvailabilityPermission(!!data.RequestPermission.OvertimeAvailabilityPermission);
			self.shiftExchangePermission(!!data.RequestPermission.ShiftExchangePermission);
			self.personAccountPermission(!!data.RequestPermission.PersonAccountPermission);
			self.textPermission(!!data.RequestPermission.TextRequestPermission);
			self.requestPermission(
				!!data.RequestPermission.TextRequestPermission || !!data.RequestPermission.AbsenceRequestPermission
			);
			self.isOvertimeRequestAvailable(!!data.RequestPermission.OvertimeRequestPermission);
		}

		self.staffingProbabilityEnabled = data.ViewPossibilityPermission;
		self.staffingProbabilityForMultipleDaysEnabled = self.staffingProbabilityEnabled;

		self.absenceProbabilityEnabled(
			data.AbsenceProbabilityEnabled && data.CheckStaffingByIntraday && self.staffingProbabilityEnabled
		);
		self.overtimeProbabilityEnabled(data.OvertimeProbabilityEnabled);

		if (!self.absenceProbabilityEnabled() && self.selectedProbabilityType === constants.probabilityType.absence) {
			self.selectedProbabilityType = constants.probabilityType.none;
			rebindProbabilityLabel(self);
		}

		if (!self.overtimeProbabilityEnabled() && self.selectedProbabilityType === constants.probabilityType.overtime) {
			self.selectedProbabilityType = constants.probabilityType.none;
			rebindProbabilityLabel(self);
		}

		if (self.staffingProbabilityForMultipleDaysEnabled) {
			var interceptWithinDays =
				moment(data.Days[data.Days.length - 1].FixedDate) >= moment(currentUserDate) &&
				moment(data.Days[0].FixedDate) < moment(currentUserDate).add('day', data.StaffingInfoAvailableDays);
			self.showProbabilityToggle(interceptWithinDays);
		} else {
			self.showProbabilityToggle(self.staffingProbabilityEnabled && self.isCurrentWeek());
		}

		if (!self.overtimeProbabilityEnabled() && !self.absenceProbabilityEnabled()) {
			self.showProbabilityToggle(false);
		}

		self.asmEnabled(data.AsmEnabled);
		self.displayDate(
			Teleopti.MyTimeWeb.Common.FormatDatePeriod(
				moment(data.CurrentWeekStartDate),
				moment(data.CurrentWeekEndDate)
			)
		);

		if (data.PeriodSelection) {
			self.periodSelection(JSON.stringify(data.PeriodSelection));
			self.setSelectedDateSubscription(moment(data.PeriodSelection.Date));
			self.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
			self.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
			var minDateArr = data.PeriodSelection.SelectedDateRange.MinDate.split('-');
			var maxDateArr = data.PeriodSelection.SelectedDateRange.MaxDate.split('-');
			self.minDate(moment(new Date(minDateArr[0], minDateArr[1] - 1, minDateArr[2])).add('days', -1));
			self.maxDate(moment(new Date(maxDateArr[0], maxDateArr[1] - 1, maxDateArr[2])).add('days', 1));
		}
		self.datePickerFormat(Teleopti.MyTimeWeb.Common.DateFormat);

		var styleToSet = {};
		$.each(data.Styles, function(key, value) {
			styleToSet[value.Name] = 'rgb({0})'.format(value.RgbColor);
		});

		self.styles(styleToSet);

		self.staffingInfoAvailableDays = data.StaffingInfoAvailableDays;
		var timelines = ko.utils.arrayMap(data.TimeLine, function(item) {
			// "Week schedule" module will be shown on PC only, so continue apply fixed schedule height
			return new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(item, constants.scheduleHeight, 0, true);
		});
		self.timeLines(timelines);

		var days = [];
		if (Array.isArray(data.Days) && data.Days.length > 0) {
			days = ko.utils.arrayMap(data.Days, function(scheduleDay) {
				return new Teleopti.MyTimeWeb.Schedule.DayViewModel(scheduleDay, self);
			});
		}
		self.days(days);
		if (
			self.showProbabilityToggle() &&
			(self.selectedProbabilityType === constants.probabilityType.absence ||
				self.selectedProbabilityType === constants.probabilityType.overtime)
		) {
			self.fetchProbabilityData();
		}
		
		Teleopti.MyTimeWeb.PollScheduleUpdates.AddListener('WeekScheduleWeb', function(period) {
			var startDate = moment(moment(period.startDate).format('YYYY-MM-DD')).toDate();
			var endDate = moment(moment(period.endDate).format('YYYY-MM-DD')).toDate();
			if (vm.isWithinSelected(startDate, endDate)) {
				_fetchData(vm.selectedProbabilityType);
			}
		});
	}

	function _setTimeIndicator(theDate) {
		if (
			$('.week-schedule-ASM-permission-granted')
				.text()
				.indexOf('yes') === -1 ||
			$('.week-schedule-current-week')
				.text()
				.indexOf('yes') === -1
		) {
			return;
		}

		var timelineHeight = Teleopti.MyTimeWeb.Common.Constants.scheduleHeight;
		var timeindicatorHeight = 2;

		var hours = theDate.hours();
		var minutes = theDate.minutes();
		var clientNowMinutes = hours * 60 + minutes * 1;

		var timelineStartMinutes = getMinutes('.weekview-timeline', true);
		var timelineEndMinutes = getMinutes('.weekview-timeline', false);
		var division = (clientNowMinutes - timelineStartMinutes) / (timelineEndMinutes - timelineStartMinutes);
		var position = Math.round(timelineHeight * division) - Math.round(timeindicatorHeight / 2);
		if (position === -1) {
			position = 0;
		}

		var dayOfWeek = theDate.day();
		var timeIndicator = $("div[data-mytime-dayofweek='" + dayOfWeek + "'] .weekview-day-time-indicator");
		var timeIndicatorTimeLine = $('.weekview-day-time-indicator-small');

		if (timelineStartMinutes <= clientNowMinutes && clientNowMinutes <= timelineEndMinutes) {
			timeIndicator.css('top', position).show();
			timeIndicatorTimeLine.css('top', position).show();
		} else {
			timeIndicator.hide();
			timeIndicatorTimeLine.hide();
		}

		var days = vm.days();
		var currentUserDate = getCurrentUserDateTime(vm.baseUtcOffsetInMinutes).format('YYYY-MM-DD');
		for (var i = 0; i < days.length; i++) {
			var day = days[i];
			if (day.fixedDate === currentUserDate) {
				day.userNowInMinute = clientNowMinutes;
			}
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
		if ($('#page').length > 0) {
			ko.cleanNode($('#page')[0]);
		}
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
		Teleopti.MyTimeWeb.Portal.NavigateTo('Requests/Index');
	}

	function rebindProbabilityLabel(viewmodel) {
		ko.applyBindings(viewmodel, $('.probabilityLabel')[0]);
	}

	function getScheduleHeight() {
		var isMobile = Teleopti.MyTimeWeb.Portal.IsMobile(window);
		if (!isMobile) {
			return constants.scheduleHeight;
		}

		var screenHeight = isNaN(window.innerHeight) ? window.clientHeight : window.innerHeight;
		var totalToolbarHeight = 185;
		var validHeightForSchedule = screenHeight - totalToolbarHeight;
		return validHeightForSchedule > constants.mobileMinScheduleHeight
			? validHeightForSchedule
			: constants.mobileMinScheduleHeight;
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
					'Schedule/Week',
					Teleopti.MyTimeWeb.Schedule.PartialInit,
					Teleopti.MyTimeWeb.Schedule.PartialDispose
				);
			}
		},
		PartialInit: function(readyForInteractionCallback, completelyLoadedCallback, ajaxObj) {
			Teleopti.MyTimeWeb.Test.TestMessage('1partialinit');
			readyForInteractionCallback();
			completelyLoaded = completelyLoadedCallback;
			if (ajaxObj != undefined) {
				ajax = ajaxObj;
			}
			Teleopti.MyTimeWeb.Test.TestMessage('2partialinit');
		},
		SetupViewModel: function(defaultDateTimes, callback) {
			Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
				var addRequestViewModel = function() {
					var model = new Teleopti.MyTimeWeb.Request.RequestViewModel(
						Teleopti.MyTimeWeb.Request.RequestDetail.AddTextOrAbsenceRequest,
						_displayRequest,
						data.WeekStart,
						defaultDateTimes
					);

					return model;
				};

				vm = new WeekScheduleViewModel(
					addRequestViewModel,
					_navigateToRequests,
					defaultDateTimes,
					data.WeekStart
				);

				callback();
				$('.moment-datepicker').attr(
					'data-bind',
					'datepicker: selectedDate, datepickerOptions: { autoHide: true, weekStart: ' + data.WeekStart + ' }'
				);
				ko.applyBindings(vm, $('#page')[0]);
			});
		},
		WeekScheduleViewModel: WeekScheduleViewModel,
		LoadAndBindData: function() {
			_fetchData(vm.selectedProbabilityType, _subscribeForChanges);
		},

		ReloadScheduleListener: function(notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);

			if (vm.isWithinSelected(messageStartDate, messageEndDate)) {
				_fetchData(vm.selectedProbabilityType, _subscribeForChanges);
			}
		},
		PartialDispose: function() {
			_cleanBindings();
			ajax.AbortAll();
			Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
		},
		SetTimeIndicator: function(date) {
			_setTimeIndicator(date);
		},
		GetCurrentUserDateTime: getCurrentUserDateTime,
		GetScheduleHeight: getScheduleHeight,
		Vm: function() {
			return vm;
		}
	};
})(jQuery);

Teleopti.MyTimeWeb.Schedule.Layout = (function($) {
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
		SetSchemaItemsHeights: function() {
			var currentTallest = 0; // Tallest li per row
			var currentLength = 0; // max amount of li's
			var currentHeight = 0; // max height of ul
			var i = 0;
			$('.weekview-day').each(function() {
				if ($('li', this).length > currentLength) {
					currentLength = $('li', this).length;
				}

				_setDayState($(this));
			});
			for (i = 3; i <= currentLength; i++) {
				var currentLiRow = $('.weekview-day li:nth-child(' + i + ')');
				$(currentLiRow).each(function() {
					if ($(this).height() > currentTallest) {
						currentTallest = $(this).height();
					}
				});
				$('>div', $(currentLiRow)).css({ 'min-height': currentTallest - 20 }); // remove padding from height
				currentTallest = 0;
			}

			$('.weekview-day').each(function() {
				if ($(this).height() > currentHeight) {
					currentHeight = $(this).height();
				}
			});

			$('.weekview-day li:last-child').each(function() {
				var ulHeight = $(this)
					.parent()
					.height();
				var incBorders = currentLength * 6;
				$(this).height(currentHeight - ulHeight + incBorders);
			});
		}
	};
})(jQuery);
