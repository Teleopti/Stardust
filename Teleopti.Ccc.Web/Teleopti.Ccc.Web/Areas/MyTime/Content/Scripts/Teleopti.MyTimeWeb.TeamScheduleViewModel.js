Teleopti.MyTimeWeb.TeamScheduleViewModel = function () {
	var self = this;

	self.isTeamScheduleSortingFeatureEnabled = ko.observable(false);
	self.isTeamScheduleNoReadModelEnable = ko.observable(false);

	self.isLoading = ko.observable(true);
	self.cultureLoaded = ko.observable(false); // To delay rendering of date-picker

	self.hasError = ko.observable(false);
	self.errorMessage = ko.observable();

	self.featureCheck = function () {
		self.isTeamScheduleSortingFeatureEnabled(Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_SortSchedule_32092"));
		self.isTeamScheduleNoReadModelEnable(Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_TeamScheduleNoReadModel_36210"));

	};

	self.initializeShiftTrade = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index/ShiftTrade/", self.requestedDate().format("YYYYMMDD"));
	};

	self.initializeShiftTradeBulletinBoard = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index/ShiftTradeBulletinBoard/", self.requestedDate().format("YYYYMMDD"));
	};

	self.initializePostShiftForTrade = function () {
		Teleopti.MyTimeWeb.Portal.NavigateTo("Requests/Index/PostShiftForTrade/", self.requestedDate().format("YYYYMMDD"));
	};

	self.initCurrentDate = function (urlDate) {
		self.loadCurrentDate(
			function (data) {
				if (urlDate) {
					self.requestedDate(moment(urlDate));
				} else {
					self.requestedDate(moment(new Date(data.NowYear, data.NowMonth - 1, data.NowDay)));
				}
			},
			null
		);
	};

	self.loadCulture(
		function (data) {
			self.weekStart(data.WeekStart);
			self.cultureLoaded(true);
		},
		null
	);

	var loadSchedule = function () {
		self.loadSchedule(
			Teleopti.MyTimeWeb.Common.FormatServiceDate(self.requestedDate()),
			self.requestedFilter(),
			(self.requestedPaging != null) ? self.requestedPaging() : null,
			function () {
				self.isLoading(true);
			},
			function (data) {
				if (data.AgentSchedules.length > 0 || data.MySchedule != null) {
					self.createTimeLine(data.TimeLine);
				} else {
					self.CleanTimeHourLine();
				}

				self.createMySchedule(data.MySchedule);
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
						self.setTeamPicker(allTeams.teams, myTeam, allTeams.allTeam);
						self.activateFilterMixinChangeHandler();
						self.activatePagingMixinChangeHandler();
						self.selectedPageIndex(1);
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

	self.setFilterMixinChangeHandler(function (callback) {
		loadSchedule();
		if (callback) callback();
	});
	self.setPagingMixinChangeHandler(loadSchedule);

	self.loadFilterTimes(function (data) {
		self.suspendFilterMixinChangeHandler();
		if (data != null) self.setTimeFilters(data);
		self.activateFilterMixinChangeHandler();
	});

	self.isLocked(self.isLoading());
	self.isLoading.subscribe(function (newValue) { self.isLocked(newValue); });
};


Teleopti.MyTimeWeb.TeamScheduleViewModelFactory = {
	createViewModel: function (endpoints, ajax) {
		var vm = {};
		Teleopti.MyTimeWeb.DayScheduleMixin.call(vm);
		Teleopti.MyTimeWeb.TeamScheduleDataProviderMixin.call(vm, ajax, endpoints);
		Teleopti.MyTimeWeb.TeamScheduleFilterMixin.call(vm);
		Teleopti.MyTimeWeb.PagingMixin.call(vm);
		Teleopti.MyTimeWeb.TeamScheduleDrawerMixin.call(vm);
		Teleopti.MyTimeWeb.TeamScheduleViewModel.call(vm);
		vm.initCurrentDate(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash);
		vm.featureCheck();

		return vm;
	}
};
