Teleopti.MyTimeWeb.TeamScheduleViewModel = function() {
	var self = this;

	self.isPossibleSchedulesForAllEnabled = ko.observable(true);
	self.isTradeForMultiDaysEnabled = ko.observable(true);
	self.isFilterByTimeEnabled = ko.observable(true);
	self.isTeamScheduleSorttingFeatureEnabled = ko.observable(true);
	self.isLoading = ko.observable(false);

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
				if (self.refocusToNameSearch != null) {
					self.refocusToNameSearch();
					self.refocusToNameSearch = null;
				}
			}
		);
	};

	self.setDayMixinChangeHandler(function (newDate) {		
		self.loadMyTeam(
			newDate,
			function (myTeam) {			
				self.loadTeams(
					newDate,
					function (allTeams) {
						self.suspendFilterMixinChangeHandler();
						self.suspendPagingMixinChangeHandler();
						self.defaultTeam(myTeam);
						self.setTeamPicker(allTeams);
						if (self.selectedTeam() == null) self.selectedTeam(myTeam);
						self.activateFilterMixinChangeHandler();
						self.activatePagingMixinChangeHandler();
						loadSchedule();
					}
				);
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

	self.isLoading.subscribe(function(newValue) { self.isLocked(newValue);  });
};


var ajax = new Teleopti.MyTimeWeb.Ajax();
Teleopti.MyTimeWeb.DayScheduleMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.TeamScheduleDataProviderMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype, ajax);
Teleopti.MyTimeWeb.TeamScheduleFilterMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.PagingMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);
Teleopti.MyTimeWeb.TeamScheduleDrawerMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);



//Teleopti.MyTimeWeb.PagingMixin.call(Teleopti.MyTimeWeb.TeamScheduleViewModel.prototype);





