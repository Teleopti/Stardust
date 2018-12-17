(function () {
	'use strict';
	var rootScope,
		$controller,
		weekViewCreator,
		viewStateKeeper,
		serviceDateFormatHelper,
		weekViewScheduleService,
		groupPageService,
		getScheduleCalledTimes = 0,
		mockSignalRBackendServer = {},

		fakeToggle;

	describe("#teamschedule week controller tests#", function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				initProvideService($provide);
			});
		});
		beforeEach(function () { moment.locale('sv'); });
		afterEach(function () { moment.locale('en'); });

		beforeEach(inject(function (
			_$rootScope_,
			_$controller_,
			_PersonScheduleWeekViewCreator_,
			_ViewStateKeeper_,
			_serviceDateFormatHelper_,
			CurrentUserInfo) {
			rootScope = _$rootScope_.$new();
			weekViewCreator = _PersonScheduleWeekViewCreator_;
			viewStateKeeper = _ViewStateKeeper_;
			serviceDateFormatHelper = _serviceDateFormatHelper_;
			$controller = _$controller_;

			CurrentUserInfo.SetCurrentUserInfo({
				FirstDayOfWeek: 1,
				DateFormatLocale: 'sv-SE',
				DefaultTimeZone: 'Europe/Berlin',
				DayNames: ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday']
			});
		}));

		it("should init all settings", function () {
			var state = {
				selectedDate: '2018-07-04',
				keyword: 'search',
				teamNameMap: '1 team selected',
				selectedSortOption: 'Name DESC',
				selectedTeamIds: [],
				selectedGroupPage: { pageId: '', groupIds: [] },
				staffingEnabled: true,
				timezone: 'Europe/Berlin'
			};
			viewStateKeeper.save(state);

			var controller = setUpController();

			expect(controller.searchOptions.keyword).toEqual('search');
			expect(controller.scheduleDate).toEqual('2018-07-02');
			expect(controller.teamNameMap).toEqual('1 team selected');
			expect(controller.sortOption).toEqual('Name DESC');

			expect(controller.selectedGroups.groupPageId).toEqual('');
			expect(controller.selectedGroups.groupIds).toEqual([]);
			expect(controller.staffingEnabled).toEqual(true);
			expect(controller.timezone).toEqual("Europe/Berlin");
		});

		it("should clear group weeks and set person count when selected person larger than 500", function () {
			weekViewScheduleService.has({
				"PersonWeekSchedules": [], "Total": 800, "Keyword": ""
			});
			var controller = setUpController();

			controller.scheduleDate = new Date("2015-10-26");
			controller.searchOptions = {
				focusingSearch: false
			}
			controller.onScheduleDateChanged();

			expect(controller.groupWeeks.length).toEqual(0);
			expect(controller.total).toEqual(800);
		});

		it('should get correct first day of week and week days when start of week is changed', function () {
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-06-25T00:00:00" }
							},
							{
								"Date": { "Date": "2018-06-26T00:00:00" }
							},
							{
								"Date": { "Date": "2018-06-27T00:00:00" }
							},
							{
								"Date": { "Date": "2018-06-28T00:00:00" }
							},
							{
								"Date": { "Date": "2018-06-29T00:00:00" }
							},
							{
								"Date": { "Date": "2018-06-30T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-01T00:00:00" }
							}], "ContractTimeMinutes": 2400.0
				}], "Total": 1, "Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-07-01';
			controller.onScheduleDateChanged();

			expect(serviceDateFormatHelper.getDateOnly(controller.weekDays[0].date)).toEqual('2018-06-25');
			expect(serviceDateFormatHelper.getDateOnly(controller.weekDays[6].date)).toEqual('2018-07-01');
		});

		it('should get group pages with correct period', function () {
			viewStateKeeper.save({ selectedDate: '2018-07-01' });

			setUpController();
			expect(groupPageService.period.StartDate).toEqual('2018-06-25');
			expect(groupPageService.period.EndDate).toEqual('2018-07-01');
		});

		it("should not update schedule when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();

			getScheduleCalledTimes = 0;
			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'b0e35119-4661-4a1b-8772-9b5e015b2564',
					StartDate: 'D2018-12-17T00:00:00',
					EndDate: 'D2018-12-17T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);

			expect(getScheduleCalledTimes).toBe(0);
		});

		it("should update schedule when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is off", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = false;

			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();

			getScheduleCalledTimes = 0;

			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'b0e35119-4661-4a1b-8772-9b5e015b2564',
					StartDate: 'D2018-12-17T00:00:00',
					EndDate: 'D2018-12-17T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);
			expect(getScheduleCalledTimes).toBe(1);
		});

		it("should show refresh button and disable on init when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;

			var controller = setUpController();
			expect(controller.isRefreshButtonVisible).toBeTruthy();
			expect(controller.havingScheduleChanged).toBeFalsy();
		});

		it("should enable refresh button after schedule was changed when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();

			getScheduleCalledTimes = 0;

			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'b0e35119-4661-4a1b-8772-9b5e015b2564',
					StartDate: 'D2018-12-17T00:00:00',
					EndDate: 'D2018-12-17T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);

			expect(controller.havingScheduleChanged).toBeTruthy();
		});

		it("should not enable refresh button when the date of schedule changes is not in view range and WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();
			
			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
					StartDate: 'D2018-12-27T00:00:00',
					EndDate: 'D2018-12-27T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);

			expect(controller.havingScheduleChanged).toBeFalsy();
		});

		it("should not enable refresh button when the person of schedule changes is not in view range and WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();

			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx',
					StartDate: 'D2018-12-17T00:00:00',
					EndDate: 'D2018-12-17T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);

			expect(controller.havingScheduleChanged).toBeFalsy();
		});

		it("should load schedule after clicking refresh button when WfmTeamSchedule_DisableAutoRefreshSchedule_79826 is on", function () {
			fakeToggle.WfmTeamSchedule_DisableAutoRefreshSchedule_79826 = true;
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-12-17T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-18T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-19T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-20T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-21T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-22T00:00:00" }
							},
							{
								"Date": { "Date": "2018-12-23T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();
			controller.scheduleDate = '2018-12-17';
			controller.onScheduleDateChanged();

			getScheduleCalledTimes = 0;
			mockSignalRBackendServer.notifyClients([
				{
					DomainReferenceId: 'b0e35119-4661-4a1b-8772-9b5e015b2564',
					StartDate: 'D2018-12-17T00:00:00',
					EndDate: 'D2018-12-17T00:00:00',
					TrackId: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxx'
				}
			]);
			controller.onRefreshButtonClicked();

			expect(getScheduleCalledTimes).toBe(1);
			expect(controller.havingScheduleChanged).toBeFalsy();
		});

	});
	describe('#teamschedule week controller tests in ar-OM #', function () {
		beforeEach(function () {
			module("wfm.teamSchedule");
			module(function ($provide) {
				initProvideService($provide);
			});
		});

		beforeEach(function () { moment.locale('ar-OM'); });
		afterEach(function () { moment.locale('en'); });

		beforeEach(inject(function (
			_$rootScope_,
			_$controller_,
			_PersonScheduleWeekViewCreator_,
			_ViewStateKeeper_,
			_serviceDateFormatHelper_,
			CurrentUserInfo) {
			rootScope = _$rootScope_.$new();
			weekViewCreator = _PersonScheduleWeekViewCreator_;
			viewStateKeeper = _ViewStateKeeper_;
			serviceDateFormatHelper = _serviceDateFormatHelper_;
			$controller = _$controller_;

			CurrentUserInfo.SetCurrentUserInfo({
				FirstDayOfWeek: 0,
				DateFormatLocale: 'en-US',
				DefaultTimeZone: 'Europe/Berlin'
			});
		}));

		it('should get correct start of week and week days when start of week is changed', function () {
			weekViewScheduleService.has({
				"PersonWeekSchedules": [{
					"PersonId": "b0e35119-4661-4a1b-8772-9b5e015b2564",
					"Name": "Pierre Baldi",
					"DaySchedules":
						[
							{
								"Date": { "Date": "2018-07-01T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-02T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-03T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-04T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-05T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-06T00:00:00" }
							},
							{
								"Date": { "Date": "2018-07-07T00:00:00" }
							}],
					"ContractTimeMinutes": 2400.0
				}],
				"Total": 1,
				"Keyword": ""
			});

			var controller = setUpController();

			controller.scheduleDate = "2018-07-01";
			controller.onScheduleDateChanged();

			expect(serviceDateFormatHelper.getDateOnly(controller.weekDays[0].date)).toEqual('2018-07-01');
			expect(serviceDateFormatHelper.getDateOnly(controller.weekDays[6].date)).toEqual('2018-07-07');
		});

		it('should get group pages with correct period', function () {
			viewStateKeeper.save({ selectedDate: '2018-07-01' });

			var controller = setUpController();
			expect(groupPageService.period.StartDate).toEqual('2018-07-01');
			expect(groupPageService.period.EndDate).toEqual('2018-07-07');
		});

	});

	function initProvideService($provide) {
		weekViewScheduleService = new WeekViewScheduleService();
		groupPageService = new GroupPageService();
		fakeToggle = {};

		$provide.service('weekViewScheduleSvc', function () {
			return weekViewScheduleService;
		});
		$provide.service('groupPageService', function () {
			return groupPageService;
		});
		$provide.service('Toggle', function () {
			return fakeToggle;
		});
		$provide.service('signalRSVC', setupMockSignalRService);
	}

	function setUpController() {
		return $controller("TeamScheduleWeeklyController", {
			$scope: rootScope,
			WeekViewCreator: weekViewCreator
		});
	};

	function WeekViewScheduleService() {
		var scheduleData = {
			"PersonWeekSchedules": [], "Total": 0, "Keyword": ""
		};
		this.has = function (currentScheduleData) {
			scheduleData = currentScheduleData;
		}
		this.getSchedules = function () {
			var response = { data: scheduleData };
			getScheduleCalledTimes = getScheduleCalledTimes + 1;
			return {
				then: function (callback) {
					callback(response.data);
				}
			}
		}
	}

	function GroupPageService() {
		this.period = {};
		this.fetchAvailableGroupPages = function (startDate, endDate) {
			this.period = { StartDate: startDate, EndDate: endDate };
			return {
				then: function (cb) {
				}
			}
		}
	}

	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}
})();