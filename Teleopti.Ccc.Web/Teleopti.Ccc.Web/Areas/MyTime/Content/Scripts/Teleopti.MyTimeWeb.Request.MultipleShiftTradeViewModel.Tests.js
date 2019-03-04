$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel');

	test('should get contract time correctly', function() {
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDate(moment('2017-01-01'));
		viewModel.selectedTeam('TeamId');

		equal(viewModel.mySchedule().contractTime, '8:00');
		equal(viewModel.possibleTradeSchedules()[0].contractTime, '7:05');
	});

	test('should clean filter when choose agent', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.filteredStartTimesText.push('6:00-8:00');
		viewModel.filteredEndTimesText.push('16:00-18:00');
		viewModel.filterStartTimeList.push(
			new Teleopti.MyTimeWeb.Request.FilterStartTimeView('6:00-8:00', 6, 8, true, false)
		);
		viewModel.filterEndTimeList.push(
			new Teleopti.MyTimeWeb.Request.FilterStartTimeView('16:00-18:00', 16, 18, true, false)
		);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			null,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);

		equal(viewModel.filteredStartTimesText().length, 0);
		equal(viewModel.filteredEndTimesText().length, 0);
		for (var i = 0; i < viewModel.filterStartTimeList().length; ++i) {
			equal(viewModel.filterStartTimeList()[i].isChecked(), false);
		}
		for (var j = 0; j < viewModel.filterEndTimeList().length; ++j) {
			equal(viewModel.filterEndTimeList()[j].isChecked(), false);
		}
	});

	test('should set date picker range', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var now = moment('Dec 25, 1995');

		viewModel.openPeriodStartDate(moment(now).add('days', 1));
		viewModel.openPeriodEndDate(moment(now).add('days', 2));

		equal(viewModel.openPeriodStartDate().format('YYYY-MM-DD'), '1995-12-26');
		equal(viewModel.openPeriodEndDate().format('YYYY-MM-DD'), '1995-12-27');
	});

	test('should get date with format', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.requestedDateInternal(moment('Dec 25, 1995'));

		var result = viewModel.requestedDateInternal().format(viewModel.DatePickerFormat());

		equal(result, '1995-12-25');
	});

	test('shift trade date should be invalid when it is not in an open period', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var periodStart = moment('2014-08-01', 'YYYY-MM-DD');
		var periodEnd = moment('2014-08-31', 'YYYY-MM-DD');
		viewModel.openPeriodStartDate(periodStart);
		viewModel.openPeriodEndDate(periodEnd);

		var startRangeResult = viewModel.isRequestedDateValid(moment('2014-07-31', 'YYYY-MM-DD'));
		var endRangeResult = viewModel.isRequestedDateValid(moment('2014-09-01', 'YYYY-MM-DD'));

		equal(startRangeResult, false);
		equal(endRangeResult, false);
	});

	test('shift trade date should be valid when it is in open period', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var periodStart = moment('2014-08-01', 'YYYY-MM-DD');
		var periodEnd = moment('2014-08-31', 'YYYY-MM-DD');
		viewModel.openPeriodStartDate(periodStart);
		viewModel.openPeriodEndDate(periodEnd);

		var result = viewModel.isRequestedDateValid(moment('2014-08-15', 'YYYY-MM-DD'));

		equal(result, true);
	});

	test('should go to next date', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestMyTeam') {
					options.success('');
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var date = moment('12-25-1995', 'MM-DD-YYYY');
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function(date) {
			return true;
		};

		viewModel.nextDate();

		equal(viewModel.requestedDateInternal().format('MM-DD-YYYY'), '12-26-1995');
	});

	test('should go to previous date', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestMyTeam') {
					options.success('');
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var date = moment('12-25-1995', 'MM-DD-YYYY');
		viewModel.requestedDate(date);
		viewModel.isRequestedDateValid = function(date) {
			return true;
		};

		viewModel.previousDate();

		equal(viewModel.requestedDateInternal().format('MM-DD-YYYY'), '12-24-1995');
	});

	test('should clean data when prepare load without Multi shifts trade Enabled', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();

		viewModel.isDetailVisible = function() {
			return false;
		};

		viewModel.prepareLoad();

		equal(viewModel.possibleTradeSchedulesRaw.length, 0);
		equal(viewModel.selectablePages().length, 0);
		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
		equal(viewModel.selectedInternal(), false);
		equal(viewModel.IsLoading(), false);
	});

	test("should get all team ids except 'Team All'", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.availableTeams.push({ id: 'A', text: 'Team A' });
		viewModel.availableTeams.push({ id: 'B', text: 'Team B' });
		viewModel.availableTeams.push({ id: 'allTeams', text: 'Team All' });

		var target = viewModel
			.availableTeams()
			.filter(function(team) {
				return team.id !== viewModel.allTeamsId;
			})
			.map(function(team) {
				return team.id;
			});

		equal(target.length, 2);
		equal(target[0], 'A');
		equal(target[1], 'B');
	});

	test("should get all site ids except 'All Sites'", function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.availableSites.push({ id: 'A', text: 'Site A' });
		viewModel.availableSites.push({ id: 'B', text: 'Site B' });
		viewModel.availableSites.push({ id: 'allSites', text: 'All Sites' });

		var target = viewModel
			.availableSites()
			.filter(function(site) {
				return site.id !== viewModel.allSitesId;
			})
			.map(function(site) {
				return site.id;
			});

		equal(target.length, 2);
		equal(target[0], 'A');
		equal(target[1], 'B');
	});

	test('should load teams under site', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsUnderSiteForShiftTrade') {
					options.success([
						{ id: 'A', text: 'Team A' },
						{ id: 'B', text: 'Team B' },
						{ id: 'C', text: 'Team C' }
					]);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadTeamsUnderSite('site1');

		equal(viewModel.availableTeams().length, 4);
		var teamAll = viewModel.availableTeams()[0];
		equal(teamAll.id, 'allTeams');
		equal(teamAll.text, 'Team All');
	});

	test('should load Team All', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsForShiftTrade') {
					options.success([
						{ id: 'A', text: 'Team A' },
						{ id: 'B', text: 'Team B' },
						{ id: 'C', text: 'Team C' }
					]);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadTeams();

		equal(viewModel.availableTeams().length, 4);
		var teamAll = viewModel.availableTeams()[0];
		equal(teamAll.id, 'allTeams');
		equal(teamAll.text, 'Team All');
	});

	test('should load no teams when no teams returned from server', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsForShiftTrade') {
					options.success([]);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadTeams();
		equal(viewModel.availableTeams().length, 0);
	});

	test('should load teams', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsForShiftTrade') {
					options.success([
						{ id: 'A', text: 'Team A' },
						{ id: 'B', text: 'Team B' },
						{ id: 'C', text: 'Team C' }
					]);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.myTeamId('A');

		viewModel.loadTeams();

		equal(viewModel.selectedTeam(), 'A');
		equal(viewModel.availableTeams().length, 4);
		var teamTwo = viewModel.availableTeams()[2];
		equal(teamTwo.id, 'B');
		equal(teamTwo.text, 'Team B');
	});

	test('should load sites', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/SitesForShiftTrade') {
					options.success([
						{ id: 'A', text: 'Site A' },
						{ id: 'B', text: 'Site B' },
						{ id: 'C', text: 'Site C' }
					]);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.mySiteId('A');

		viewModel.loadSites();

		equal(viewModel.selectedSite(), 'A');
		equal(viewModel.availableSites().length, 4);
		var siteTwo = viewModel.availableSites()[2];
		equal(siteTwo.id, 'B');
		equal(siteTwo.text, 'Site B');
	});

	test('should set default team ID when get nothing', function() {
		var myTeamId = '';
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestMyTeam') {
					options.success(myTeamId);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadMyTeamId();

		equal(viewModel.myTeamId(), undefined);
		equal(viewModel.noAnyTeamToShow(), false);
	});

	test('should set true when get no any team', function() {
		var noTeam = [];
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsForShiftTrade') {
					options.success(noTeam);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.selectedTeamInternal(undefined);
		viewModel.myTeamId(undefined);

		viewModel.loadTeams();

		equal(viewModel.noAnyTeamToShow(), true);
	});

	test('should load my team ID', function() {
		var myTeamId = 'myTeam';
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestMyTeam') {
					options.success(myTeamId);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadMyTeamId();

		equal(viewModel.myTeamId(), myTeamId);
		equal(viewModel.noAnyTeamToShow(), false);
	});

	test('should load my site ID', function() {
		var mySiteId = 'mySite';
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestMySite') {
					options.success(mySiteId);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);

		viewModel.loadMySiteId();

		equal(viewModel.mySiteId(), mySiteId);
		equal(viewModel.noAnySiteToShow(), false);
	});

	test('should hide page view when no data', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();

		viewModel.pageCount(0);
		viewModel.isPageVisible(0 > 0);

		equal(viewModel.isPageVisible(), false);
	});

	test('should show page view when there is data', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();

		viewModel.pageCount(1);
		viewModel.isPageVisible(1 > 0);

		equal(viewModel.isPageVisible(), true);
	});

	test('should update selectable pages when page count changes', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.selectablePages.push(new Teleopti.MyTimeWeb.Request.PageView(1));

		viewModel.pageCount(2);
		viewModel.isPageVisible(2 > 0);

		equal(viewModel.selectablePages().length, 2);
	});

	test('should set page count when set paging infos', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();

		viewModel.pageCount(2);
		viewModel.isPageVisible(2 > 0);

		equal(viewModel.pageCount(), 2);
	});

	test('should set paging info', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.selectedPageIndex(2);
		viewModel.pageCount(2);
		viewModel.isPageVisible(2 > 0);

		equal(viewModel.pageCount(), 2);
		equal(viewModel.selectablePages().length, 2);
	});

	test('should can select page', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.selectedPageIndex(1);
		var page = new Teleopti.MyTimeWeb.Request.PageView(2);

		viewModel.selectPage(page);

		equal(viewModel.selectedPageIndex(), 2);
	});

	test('should set select page', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.selectedPageIndex(1);

		viewModel.setSelectPage(2);

		equal(viewModel.selectedPageIndex(), 2);
	});

	test('should go to next pages', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var pageCount = 8;
		viewModel.pageCount(pageCount);
		viewModel.initSelectablePages(pageCount);

		viewModel.goNextPages();

		equal(viewModel.selectedPageIndex(), 6);
		equal(viewModel.isMore(), false);
		equal(viewModel.isPreviousMore(), true);
		equal(viewModel.selectablePages().length, pageCount - 5);
	});

	test('should go to previous pages', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var pageCount = 8;
		viewModel.pageCount(pageCount);
		viewModel.initSelectablePages(pageCount);
		viewModel.goToLastPage();

		viewModel.goPreviousPages();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isMore(), true);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.selectablePages().length, 5);
	});

	test('should go to last page without previous more', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.pageCount(3);

		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 3);
		equal(viewModel.isPreviousMore(), false); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
	});

	test('should go to last page with previous more', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.pageCount(6);
		viewModel.goToLastPage();

		equal(viewModel.selectedPageIndex(), 6);
		equal(viewModel.isPreviousMore(), true); // only valid when 5 pages or more
		equal(viewModel.isMore(), false);
		equal(viewModel.selectablePages().length, 1);
	});

	test('should go to first page without more', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.pageCount(3);
		viewModel.isPageVisible(3 > 0);

		viewModel.selectedPageIndex(3);

		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), false);
	});

	test('should go to first page with more', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.pageCount(6);
		viewModel.goToFirstPage();

		equal(viewModel.selectedPageIndex(), 1);
		equal(viewModel.isPreviousMore(), false);
		equal(viewModel.isMore(), true);
		equal(viewModel.selectablePages().length, 5);
	});

	test('should recognize time filter with start time', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.filteredStartTimesText.push('6:00 - 8:00');

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test('should recognize time filter with end time', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.filteredEndTimesText.push('6:00 - 8:00');

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test('should recognize time filter with dayoff', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.isDayoffFiltered(true);

		var result = viewModel.isFiltered();

		equal(result, true);
	});

	test('should recognize is not filtered by time filter', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();

		var result = viewModel.isFiltered();

		equal(result, false);
	});

	test('should filter checked start time', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var isChecked = true;
		viewModel.filterStartTimeList.push(
			new Teleopti.MyTimeWeb.Request.FilterStartTimeView('8:00-10:00', 8, 10, isChecked, false)
		);

		viewModel.filterTime();

		equal(viewModel.isDayoffFiltered(), false);
		equal(viewModel.filteredStartTimesText().length, 1);
	});

	test('should filter checked end time', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var isChecked = true;
		viewModel.filterEndTimeList.push(
			new Teleopti.MyTimeWeb.Request.FilterEndTimeView('8:00-10:00', 8, 10, isChecked, false)
		);

		viewModel.filterTime();

		equal(viewModel.filteredEndTimesText().length, 1);
	});

	test('should load filter hours text and dayoff names', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'RequestsShiftTradeScheduleFilter/Get') {
					var hourTexts = [];
					var dayOffNames = ['DO'];
					for (var i = 0; i <= 24; ++i) {
						hourTexts[i] = i + ':00';
					}

					options.success({
						HourTexts: hourTexts,
						DayOffShortNames: dayOffNames
					});
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.loadFilterTimes();

		equal(viewModel.filterStartTimeList().length, 13);
		equal(viewModel.filterStartTimeList()[0].text, '0:00 - 2:00');
		equal(viewModel.filterStartTimeList()[12].text, 'DO');
	});

	test('should not load team when there is no selected team', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.dateChanged(false);

		viewModel.selectedSite('site1');

		equal(viewModel.selectedSiteInternal(), 'site1');
		equal(viewModel.selectedTeamInternal(), null);
	});

	test('should not reload team when date changed', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.dateChanged(true);
		viewModel.selectedTeamInternal('myTeam');

		viewModel.selectedSite('site1');

		equal(viewModel.selectedSiteInternal(), 'site1');
		equal(viewModel.selectedTeamInternal(), 'myTeam');
	});

	test('should load team when there is a selected team', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Team/TeamsUnderSiteForShiftTrade') {
					options.success([
						{ id: 'A', text: 'Team A' },
						{ id: 'B', text: 'Team B' },
						{ id: 'C', text: 'Team C' }
					]);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.selectedTeamInternal('myTeam');
		viewModel.selectedSiteInternal('site1');

		viewModel.selectedSite('site2');

		equal(viewModel.selectedSiteInternal(), 'site2');
		equal(viewModel.selectedTeamInternal(), 'allTeams');
		equal(viewModel.availableTeams().length, 4);
	});

	test('should update time sort order correctly', function() {
		var optionsData = {};
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					optionsData = options;
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.selectedTeamInternal(['Team Id']);
		viewModel.updateTimeSortOrder({ Value: 'start asc' });

		equal(JSON.parse(optionsData.data).TimeSortOrder, 'start asc');
	});

	test('should keep search name after changing team', function() {
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.chooseAgent({ agentName: 'test agent', isVisible: function() {} });
		viewModel.searchNameText('test agent');
		viewModel.selectedTeam('other team');
		equal(viewModel.searchNameText(), 'test agent');
	});

	test('should keep search name after changing site', function() {
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.chooseAgent({ agentName: 'test agent', isVisible: function() {} });
		viewModel.searchNameText('test agent');
		viewModel.selectedSite('other site');
		equal(viewModel.searchNameText(), 'test agent');
	});

	test('should clear search name and reload schedule after cancelling a request', function() {
		var methodCalled = false;
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					methodCalled = true;
					options.success(schedules);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.selectedTeamInternal('myTeam');
		viewModel.chooseAgent({ agentName: 'test', isVisible: function() {} });
		viewModel.searchNameText('test');
		viewModel.cancelRequest();

		equal(viewModel.searchNameText(), '');
		equal(methodCalled, true);
	});

	test('should load multiple days schedules after choose a agent', function() {
		var schedules = {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					MySchedule: null,
					PersonToSchedule: null
				},
				{
					Date: '/Date(1531978200000)/',
					MySchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					},
					PersonToSchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					}
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-10-18'));

		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);

		equal(viewModel.loadedSchedulePairs().length, 2);
		equal(viewModel.loadedSchedulePairs()[0].date.format('YYYY-MM-DD'), '2018-07-18');
	});

	test('should load schedules that within open period start and end date', function() {
		var schedules = {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					MySchedule: null,
					PersonToSchedule: null
				},
				{
					Date: '/Date(1531978200000)/',
					MySchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					},
					PersonToSchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					}
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));

		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);

		equal(viewModel.loadedSchedulePairs().length, 2);
		equal(viewModel.loadedSchedulePairs()[1].date.format('YYYY-MM-DD'), '2018-07-19');
	});

	test('Schedule data should be null if agent is not scheduled', function() {
		var schedules = {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					MySchedule: null,
					PersonToSchedule: null
				},
				{
					Date: '/Date(1531978200000)/',
					MySchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					},
					PersonToSchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: 480,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					}
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};

		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-10-19'));

		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.loadedSchedulePairs().length, 2);
		equal(viewModel.loadedSchedulePairs()[0].mySchedule, null);
		equal(viewModel.loadedSchedulePairs()[1].mySchedule.contractTime, '8:00');
	});

	test('should add plus one day when end time at day after', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm'
		});

		var ret = viewModel.getOvernightFlag(moment('2018-07-19 02:00:00'), moment('2018-07-18'));
		equal(ret, ' +1');
	});

	test('should add minus one day when start time at day before', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm'
		});

		var ret = viewModel.getOvernightFlag(moment('2018-07-19 23:00:00'), moment('2018-07-20'));
		equal(ret, '(-1)');
	});

	test('should get correct overtime and absence text', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		var personSchedule = {
			hasOvertime: true,
			isIntradayAbsence: true,
			absenceCategoryShortName: 'PT'
		};

		var result = viewModel.getOvertimeAndIntradayAbsenceText(personSchedule, 'OT');
		equal(result, '(OT, PT)');
	});

	test('should show period when validate period tolerance', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.myToleranceMessages()[0].periodStart, '2018-06-30');
		equal(viewModel.myToleranceMessages()[0].periodEnd, '2018-07-29');
	});

	test('should reset showCartPanel after send request', function() {
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeRequest') {
					options.success();
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'123',
			false
		);
		viewModel.chooseAgent(agent);

		viewModel.showCartPanel(true);
		viewModel.sendRequest();

		equal(viewModel.showCartPanel(), false);
	});

	test('should show correct tolerance exceed', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.myToleranceMessages()[0].contractTimeGap, '+0:10');
		equal(viewModel.targetToleranceMessages()[0].contractTimeGap, '-0:11');
	});

	test('should calculate tolerance when real gap have exceed tolerance', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 0,
					PositiveToleranceMinutes: 0,
					RealSchedulePositiveGap: 90,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 0,
					PositiveToleranceMinutes: 0,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 89
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.myToleranceMessages()[0].contractTimeGap, '+3:10');
		equal(viewModel.targetToleranceMessages()[0].contractTimeGap, '-3:09');
	});

	test('should calculate tolerance for both side at one time', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 10,
					PositiveToleranceMinutes: 10,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 80
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 10,
					PositiveToleranceMinutes: 10,
					RealSchedulePositiveGap: 70,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.myToleranceMessages()[0].contractTimeGap, '+0:10');
		equal(viewModel.targetToleranceMessages()[0].contractTimeGap, '-0:20');
	});

	test("should not calculate tolerance when there is not diff between two agent' contract time", function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 80,
					PositiveToleranceMinutes: 10,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 10,
					PositiveToleranceMinutes: 70,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 480);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.showToloranceMessage(), false);
	});

	test('should not show tolerance error when there is no gap between schedules', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 30,
					PositiveToleranceMinutes: 30,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 70
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 40,
					PositiveToleranceMinutes: 40,
					RealSchedulePositiveGap: 60,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.showToloranceMessage(), false);
	});

	test('should not show error message when there is no schedules', function() {
		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					IsSelectable: true,
					MySchedule: null,
					PersonToSchedule: null
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.showToloranceMessage(), false);
	});

	test('should enable send button when not break tolerance', function() {
		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					IsSelectable: true,
					MySchedule: null,
					PersonToSchedule: null
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.isSendEnabled(), true);
	});

	test('should not enable send button when has tolerance error message', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY-MM-DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'AM',
			PMDesignator: 'PM'
		});

		var agentId = '123';
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var schedules = createMultiSchedulesForShiftTrade(480, 580);

		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/GetWFCTolerance?personToId=' + agentId) {
					options.success(toleranceInfo);
				}
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(schedules);
				}
			}
		};
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.requestedDateInternal(moment('2018-07-18'));
		viewModel.openPeriodEndDate(moment('2018-07-19'));
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			agentId,
			false
		);
		viewModel.redrawLayers = function() {};

		viewModel.chooseAgent(agent);
		equal(viewModel.isSendEnabled(), false);
	});

	test('should get correct overtime text', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		var personSchedule = {
			hasOvertime: true,
			isIntradayAbsence: false,
			absenceCategoryShortName: 'PT'
		};

		var result = viewModel.getOvertimeAndIntradayAbsenceText(personSchedule, 'OT');
		equal(result, '(OT)');
	});

	test('should get correct absence text', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		var personSchedule = {
			hasOvertime: false,
			isIntradayAbsence: true,
			absenceCategoryShortName: 'PT'
		};

		var result = viewModel.getOvertimeAndIntradayAbsenceText(personSchedule, 'OT');
		equal(result, '(PT)');
	});

	test('should show trade chosen view from cart panel', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		viewModel.showCartPanel(true);

		viewModel.previousPage();
		equal(viewModel.showCartPanel(), false);
	});

	test('should show shift trade list view from trade chosen view', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		viewModel.showCartPanel(false);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.agentChoosed(agent);

		viewModel.previousPage();
		equal(viewModel.showCartPanel(), false);
		equal(viewModel.agentChoosed(), null);
	});

	test('should hide fab after enter shift trade request view', function() {
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (
					options.url === '../ToggleHandler/IsEnabled?toggle=MyTimeWeb_Request_CleanUpRequestHisotry_77776'
				) {
					options.success({ IsEnabled: true });
				}

				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}

				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(createMultiSchedulesForShiftTrade());
				}

				if (options.url === 'Requests/GetWFCTolerance?personToId=id') {
					options.success(toleranceInfo);
					``;
				}
			}
		};

		$('body').append('<div id="Requests-body-inner"></div>');
		Teleopti.MyTimeWeb.Common.Init({ baseUrl: '' }, ajax);

		Teleopti.MyTimeWeb.Request.RequestPartialInit(function() {}, function() {}, ajax);
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.chooseAgent(agent);

		equal(viewModel.agentChoosed() != null, true);
		equal(Teleopti.MyTimeWeb.Request.RequestNavigationViewModel().hideFab(), true);
	});

	test('should show fab button after go back to previous page', function() {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function() {
			return true;
		};
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (
					options.url === '../ToggleHandler/IsEnabled?toggle=MyTimeWeb_Request_CleanUpRequestHisotry_77776'
				) {
					options.success({ IsEnabled: true });
				}

				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}

				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(createMultiSchedulesForShiftTrade());
				}

				if (options.url === 'Requests/GetWFCTolerance?personToId=id') {
					options.success(toleranceInfo);
					``;
				}
			}
		};

		$('body').append('<div id="Requests-body-inner"></div>');
		Teleopti.MyTimeWeb.Common.Init({ baseUrl: '' }, ajax);

		Teleopti.MyTimeWeb.Request.RequestPartialInit(function() {}, function() {}, ajax);
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.chooseAgent(agent);

		equal(viewModel.agentChoosed() != null, true);
		equal(Teleopti.MyTimeWeb.Request.RequestNavigationViewModel().hideFab(), true);

		viewModel.previousPage();
		equal(Teleopti.MyTimeWeb.Request.RequestNavigationViewModel().hideFab(), false);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should show fab button after cancel adding request', function() {
		var tempFn = Teleopti.MyTimeWeb.Common.IsHostAMobile;
		Teleopti.MyTimeWeb.Common.IsHostAMobile = function() {
			return true;
		};
		var schedules = {
			MySchedule: {
				ScheduleLayers: [{}],
				ContractTimeInMinute: 480
			},
			PossibleTradeSchedules: [
				{
					ScheduleLayers: [{}],
					ContractTimeInMinute: 425
				}
			],
			TimeLineHours: [
				{
					HourText: '',
					StartTime: '2017-01-01 00:00:00'
				},
				{
					HourText: '07:00',
					StartTime: '2017-01-01 07:00:00'
				}
			]
		};
		var toleranceInfo = {
			IsNeedToCheck: true,
			MyInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 800,
					PositiveToleranceMinutes: 90,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			],

			PersonToInfos: [
				{
					PeriodStart: moment('2018-06-30'),
					PeriodEnd: moment('2018-07-29'),
					ContractTimeMinutes: 1000,
					NegativeToleranceMinutes: 89,
					PositiveToleranceMinutes: 1189,
					RealSchedulePositiveGap: 0,
					RealScheduleNegativeGap: 0
				}
			]
		};

		var ajax = {
			Ajax: function(options) {
				if (
					options.url === '../ToggleHandler/IsEnabled?toggle=MyTimeWeb_Request_CleanUpRequestHisotry_77776'
				) {
					options.success({ IsEnabled: true });
				}

				if (options.url === 'Requests/ShiftTradeRequestSchedule') {
					options.success(schedules);
				}

				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					options.success(createMultiSchedulesForShiftTrade());
				}

				if (options.url === 'Requests/GetWFCTolerance?personToId=id') {
					options.success(toleranceInfo);
					``;
				}
			}
		};

		$('body').append('<div id="Requests-body-inner"></div>');
		Teleopti.MyTimeWeb.Common.Init({ baseUrl: '' }, ajax);

		Teleopti.MyTimeWeb.Request.RequestPartialInit(function() {}, function() {}, ajax);
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.chooseAgent(agent);

		equal(viewModel.agentChoosed() != null, true);
		equal(Teleopti.MyTimeWeb.Request.RequestNavigationViewModel().hideFab(), true);

		viewModel.cartMenuClick();
		viewModel.cancelRequest();
		equal(Teleopti.MyTimeWeb.Request.RequestNavigationViewModel().hideFab(), false);
		Teleopti.MyTimeWeb.Common.IsHostAMobile = tempFn;
	});

	test('should hide previous contract time violation warning message panel when cancel and choose another agent', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel({});
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			null,
			false
		);
		viewModel.chooseAgent(agent);
		viewModel.showToloranceMessageDetail(true);
		viewModel.cancelRequest();
		var anotherAgent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Jon',
			null,
			false
		);
		viewModel.chooseAgent(anotherAgent);
		equal(viewModel.showToloranceMessageDetail(), false);
	});

	test('should show cart panl when cancel and choose another agent', function() {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			null,
			false
		);
		viewModel.chooseAgent(agent);
		viewModel.cancelRequest();
		var anotherAgent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Jon',
			null,
			false
		);
		viewModel.chooseAgent(anotherAgent);
		equal(viewModel.showCartPanel(), true);
	});

	test('should load 15 days period of schedule ', function() {
		var expectedStartDate, expectedEndDate;
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'Requests/ShiftTradeMultiDaysSchedule') {
					expectedStartDate = JSON.parse(options.data).StartDate;
					expectedEndDate = JSON.parse(options.data).EndDate;
					options.success(createMultiSchedulesForShiftTrade());
				}

				if (options.url === 'Requests/ShiftTradeRequestPeriod') {
					options.success({
						HasWorkflowControlSet: true,
						MiscSetting: { AnonymousTrading: true },
						OpenPeriodRelativeStart: 1,
						OpenPeriodRelativeEnd: 365,
						NowYear: moment().year(),
						NowMonth: moment().month() + 1,
						NowDay: moment().date()
					});
				}
			}
		};

		Teleopti.MyTimeWeb.Common.Init({ baseUrl: '' }, ajax);
		Teleopti.MyTimeWeb.Request.RequestPartialInit(function() {}, function() {}, ajax);
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
		viewModel.loadPeriod(new Date());
		var agent = new Teleopti.MyTimeWeb.Request.PersonScheduleAddShiftTradeViewModel(
			null,
			null,
			null,
			'Ashley',
			'id',
			false
		);
		viewModel.chooseAgent(agent);

		equal(viewModel.shiftPageSize, 15);
		equal(
			expectedStartDate,
			moment()
				.add(1, 'days')
				.format('YYYY-MM-DD')
		);
		equal(
			expectedEndDate,
			moment()
				.add(1, 'days')
				.add(viewModel.shiftPageSize, 'days')
				.format('YYYY-MM-DD')
		);
	});

	function createMultiSchedulesForShiftTrade(myContractTime, personToContractTime) {
		return {
			MultiSchedulesForShiftTrade: [
				{
					Date: '/Date(1531916600000)/',
					IsSelectable: true,
					MySchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: myContractTime,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					},
					PersonToSchedule: {
						ScheduleLayers: [
							{
								Start: '/Date(1531956600000)/',
								End: '/Date(1531964700000)/',
								LengthInMinutes: 135,
								Color: '#80FF80',
								TitleHeader: 'Phone',
								IsAbsenceConfidential: false,
								IsOvertime: false,
								TitleTime: '07:30 - 09:45',
								Meeting: null
							}
						],
						StartTimeUtc: '/Date(1531978200000)/',
						IsDayOff: false,
						IsFullDayAbsence: false,
						Total: 0,
						DayOffName: null,
						ContractTimeInMinute: personToContractTime,
						IsNotScheduled: false,
						ShiftCategory: {
							Id: null,
							ShortName: 'AM',
							Name: 'Early',
							DisplayColor: '#80FF80'
						},
						IntradayAbsenceCategory: {
							ShortName: null,
							Color: null
						}
					}
				}
			]
		};
	}
});
