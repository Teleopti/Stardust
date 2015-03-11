Teleopti.MyTimeWeb.TeamScheduleViewModel = function () {
	var self = this;

	self.isPossibleSchedulesForAllEnabled = ko.observable(false);
	self.isTradeForMultiDaysEnabled = ko.observable(false);
	self.isFilterByTimeEnabled = ko.observable(false);
	self.isTeamScheduleSorttingFeatureEnabled = ko.observable(false);

	self.isLoading = ko.observable(true);

	self.hasError = ko.observable(false);
	self.errorMessage = ko.observable();

	self.featureCheck = function () {
		var tradeForMultiDaysEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_ShiftTradeRequestForMoreDays_20918");
		self.isTradeForMultiDaysEnabled(tradeForMultiDaysEnabled);

		var possibleSchedulesForAllEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_SeePossibleShiftTradesFromAllTeams_28770");
		self.isPossibleSchedulesForAllEnabled(possibleSchedulesForAllEnabled);

		var filterByTimeEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled("Request_FilterPossibleShiftTradeByTime_24560");
		self.isFilterByTimeEnabled(filterByTimeEnabled);

		self.isTeamScheduleSorttingFeatureEnabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_SortSchedule_32092"));
	};

	self.initializeShiftTrade = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index/ShiftTrade/", self.requestedDate().format("YYYYMMDD"));
	};

	self.initCurrentDate = function (urlDate) {
		self.loadCurrentDate(
			function (data) {
				if (urlDate) {
					self.requestedDate(moment(urlDate));
				} else {
					self.requestedDate(moment(new Date(data.NowYear, data.NowMonth - 1, data.NowDay)));
				}
				if (data.DateTimeFormat)
					self.setDatePickerFormat(data.DateTimeFormat.toUpperCase());
					
			},
			null
		);
	};

	var loadSchedule = function () {
		self.loadSchedule(
			self.requestedDateWithFormat(),
			self.requestedFilter(),
			(self.requestedPaging != null) ? self.requestedPaging() : null,
			function () {
				self.isLoading(true);
			},
			function (data) {
				if (data.AgentSchedules.length > 0) {
					self.createTimeLine(data.TimeLine);
				} else {
					self.CleanTimeHourLine();
				}			
				self.createToDrawSchedules(data.AgentSchedules);
				self.setPagingInfo(data.PageCount);
				self.redrawLayers();
			},
			null,
			function () {				
				self.isLoading(false);			
				if (self.refocusToNameSearch.callable != null) {					
					self.refocusToNameSearch.callable();
					self.refocusToNameSearch.callable = null;
				}
				self.suppressChangeInSearchBox = false;
			}
		);
	};

	self.setDayMixinChangeHandler(function (newDate) {
		self.hasError(false);
		self.errorMessage(null);
		self.loadDefaultTeam(
			newDate,
			function (myTeam) {
				self.loadTeams(
					newDate,
					function (allTeams) {
						self.suspendFilterMixinChangeHandler();
						self.suspendPagingMixinChangeHandler();
						self.hasError(false);
						self.errorMessage();
						self.setTeamPicker(allTeams.teams,myTeam, allTeams.allTeam);
						self.activateFilterMixinChangeHandler();
						self.activatePagingMixinChangeHandler();
						loadSchedule();
					}
				); 
			},
			function (error) {				
				self.hasError(true);
				self.errorMessage(error.Message);
			}
		);		
	});

	self.setFilterMixinChangeHandler(loadSchedule);
	self.setPagingMixinChangeHandler(loadSchedule);

	self.loadFilterTimes(function (data) {
		self.suspendFilterMixinChangeHandler();
		if (data != null) self.setTimeFilters(data);
		self.activateFilterMixinChangeHandler();
	});


	self.isLocked(self.isLoading());
	self.isLoading.subscribe(function(newValue) { self.isLocked(newValue);  });
};


var ajax = new Teleopti.MyTimeWeb.Ajax();

var endpoints = {
	loadCurrentDate: "TeamSchedule/TeamScheduleCurrentDate",
	loadFilterTimes: "RequestsShiftTradeScheduleFilter/Get",
	loadMyTeam: "Requests/ShiftTradeRequestMyTeam",
	loadDefaultTeam: "TeamSchedule/DefaultTeam",
	loadTeams: "Team/TeamsAndOrGroupings",
	loadSchedule: "TeamSchedule/TeamSchedule"
};

Teleopti.MyTimeWeb.DayScheduleMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.TeamScheduleDataProviderMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype, ajax, endpoints);
Teleopti.MyTimeWeb.TeamScheduleFilterMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.PagingMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.TeamScheduleDrawerMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);