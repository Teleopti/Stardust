$(document).ready(function() {
	var hash = '';
	var fakeAddRequestViewModel = Teleopti.MyTimeWeb.Schedule.FakeData.fakeAddRequestViewModel;
	var basedDate = momentWithLocale(
		Teleopti.MyTimeWeb.Schedule.GetCurrentUserDateTime(this.BaseUtcOffsetInMinutes)
	).format('YYYY-MM-DD');

	module('Teleopti.MyTimeWeb.Schedule', {
		setup: function() {
			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');
		},
		teardown: function() {
			$('#page').remove();
		}
	});

	test('should not set probability info for month view', function() {
		setupHash();
		var old = Teleopti.MyTimeWeb.Portal.ParseHash;
		Teleopti.MyTimeWeb.Portal.ParseHash = function() {
			return {
				dateHash: '20180209'
			};
		};
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.selectedProbabilityType = 2;

		vm.month();

		equal(hash, '#Schedule/Month/20180209');
		Teleopti.MyTimeWeb.Portal.ParseHash = old;
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should read absence report permission', function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.absenceReportPermission(), true);
	});

	test('should read scheduled days', function() {
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);
		equal(vm.days().length, 3);
		equal(vm.days()[0].headerTitle, fakeScheduleData.Days[0].Header.Title);
		equal(vm.days()[0].summary, fakeScheduleData.Days[0].Summary.Summary);
		equal(vm.days()[0].summaryTitle, fakeScheduleData.Days[0].Summary.Title);
		equal(vm.days()[0].summaryTimeSpan, fakeScheduleData.Days[0].Summary.TimeSpan);
		equal(vm.days()[0].summaryStyleClassName, fakeScheduleData.Days[0].Summary.StyleClassName);
		equal(vm.days()[0].hasShift, true);
		equal(vm.days()[0].noteMessage.indexOf(fakeScheduleData.Days[0].Note.Message) > -1, true);
		equal(vm.days()[0].seatBookings, fakeScheduleData.Days[0].SeatBookings);
	});

	test('should read timelines', function() {
		var fakeScheduleData = getFakeScheduleData();
		//9:30 ~ 17:00 makes 9 timeline points
		fakeScheduleData.TimeLine = [
			{
				Time: '09:15:00',
				TimeLineDisplay: '09:15',
				TimeFixedFormat: null
			}
		];
		for (var i = 10; i <= 17; i++) {
			fakeScheduleData.TimeLine.push({
				Time: i + ':00:00',
				TimeLineDisplay: i + ':00',
				TimeFixedFormat: null
			});
		}
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);

		var timelines = vm.timeLines();
		equal(timelines.length, 9);
		equal(timelines[0].minutes, 9.5 * 60 - 15); //9:30 => 9:15
		equal(timelines[timelines.length - 1].minutes, 16.75 * 60 + 15); //16:45 => 17:00
	});

	test('should refresh and modify url after changing date', function() {
		setupHash();
		var fakeScheduleData = getFakeScheduleData();
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);

		vm.initializeData(fakeScheduleData);
		vm.nextWeek();
		equal(
			hash.indexOf(
				moment(basedDate)
					.add('days', 7)
					.format('YYYY/MM/DD')
			) > 0,
			true
		);
		Teleopti.MyTimeWeb.Portal.ResetParsedHash();
	});

	test('should get correct minutes when there is full day time line', function() {
		var timeline = { Time: '1.00:00:00', TimeLineDisplay: '00:00', PositionPercentage: 1, TimeFixedFormat: null };
		var timelineViewModel = new Teleopti.MyTimeWeb.Schedule.TimelineViewModel(timeline, 100, 0, true);
		equal(timelineViewModel.minutes, 1440);
	});

	test('should read overtime request permission', function() {
		var vm = new Teleopti.MyTimeWeb.Schedule.WeekScheduleViewModel(fakeAddRequestViewModel, null, null, null);
		vm.initializeData(getFakeScheduleData());

		equal(vm.isOvertimeRequestAvailable(), true);
	});

	test('should increase request count after creating an overtime request', function() {
		var tempFn1 = Date.prototype.getTeleoptiTime;
		Date.prototype.getTeleoptiTime = function() {
			return new Date('2018-03-05T02:30:00Z').getTime();
		};

		var tempFn2 = Date.prototype.getTeleoptiTimeInUserTimezone;
		Date.prototype.getTeleoptiTimeInUserTimezone = function() {
			return '2018-03-04';
		};

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		var requestDate = moment().format(Teleopti.MyTimeWeb.Common.Constants.serviceDateTimeFormat.dateOnly);
		var responseData = {};
		responseData.DateFromYear = moment().year();
		responseData.DateFromMonth = moment().month() + 1;
		responseData.DateFromDayOfMonth = moment().date();
		var ajax = {
			Ajax: function(options) {
				if (options.url === 'OvertimeRequests/Save') {
					options.success(responseData);
				}
			}
		};
		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, ajax);

		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		vm.initializeData(getFakeScheduleData());

		var scheduleDayViewModel = $.grep(vm.days(), function(item) {
			return item.fixedDate === requestDate;
		})[0];
		scheduleDayViewModel.requestsCount = 0;

		vm.showAddOvertimeRequestForm();
		var overtimeRequestViewModel = vm.requestViewModel().model;
		overtimeRequestViewModel.Subject('test');
		overtimeRequestViewModel.Subject('overtime request');
		overtimeRequestViewModel.Message('I want to work overtime');
		overtimeRequestViewModel.DateFrom(requestDate);
		overtimeRequestViewModel.StartTime('19:00');
		overtimeRequestViewModel.RequestDuration('03:00');
		overtimeRequestViewModel.MultiplicatorDefinitionSetId('29F7ECE8-D340-408F-BE40-9BB900B8A4CB');
		overtimeRequestViewModel.AddRequest();

		equal(scheduleDayViewModel.requestsCount, 1);

		Date.prototype.getTeleoptiTime = tempFn1;
		Date.prototype.getTeleoptiTimeInUserTimezone = tempFn2;
	});

	test('should show only new traffic light icon when toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON', function() {
		var fakeAjax = {
			Ajax: function(option) {
				if (option.url === '../api/Schedule/FetchWeekData') {
					option.success(getFakeScheduleData());
				}
				if (option.url === 'UserData/FetchUserData') {
					option.success({
						BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
						DataSourceName: 'Teleopti WFM',
						Url: 'http://localhost:52858/TeleoptiWFM/Web/',
						AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
					});
				}
			}
		};

		Teleopti.MyTimeWeb.Common.Init(
			{
				defaultNavigation: '/',
				baseUrl: '/',
				startBaseUrl: '/'
			},
			fakeAjax
		);

		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		$('body').append(setupHtml());

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, fakeAjax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		vm.initializeData(getFakeScheduleData());

		equal(vm.newTrafficLightIconEnabled, true);
		equal(vm.absenceRequestPermission(), true);
		equal($('.traffic-light-progress').length > 0, true);
		equal($('.holiday-agents.weekview-day-icon').length > 0, false);
	});

	test('should show only old traffic light icon when toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is OFF', function() {
		var fakeAjax = {
			Ajax: function(option) {
				if (option.url === '../api/Schedule/FetchWeekData') {
					option.success(getFakeScheduleData());
				}
				if (option.url === 'UserData/FetchUserData') {
					option.success({
						BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
						DataSourceName: 'Teleopti WFM',
						Url: 'http://localhost:52858/TeleoptiWFM/Web/',
						AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
					});
				}
			}
		};
		Teleopti.MyTimeWeb.Common.Init(
			{
				defaultNavigation: '/',
				baseUrl: '/',
				startBaseUrl: '/'
			},
			fakeAjax
		);

		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		$('body').append(setupHtml());

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, fakeAjax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		vm.initializeData(getFakeScheduleData());

		equal($('.traffic-light-progress').length > 0, false);
		equal($('.holiday-agents.weekview-day-icon').length > 0, true);
	});

	test('should show correct traffic light when toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON', function() {
		var fakeAjax = {
			Ajax: function(option) {
				if (option.url === '../api/Schedule/FetchWeekData') {
					option.success(getFakeScheduleData());
				}
				if (option.url === 'UserData/FetchUserData') {
					option.success({
						BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
						DataSourceName: 'Teleopti WFM',
						Url: 'http://localhost:52858/TeleoptiWFM/Web/',
						AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
					});
				}
			}
		};
		Teleopti.MyTimeWeb.Common.Init(
			{
				defaultNavigation: '/',
				baseUrl: '/',
				startBaseUrl: '/'
			},
			fakeAjax
		);

		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		$('body').append(setupHtml());

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, fakeAjax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		var fakeScheduleData = getFakeScheduleData();
		vm.initializeData(fakeScheduleData);

		equal($('.traffic-light-progress.traffic-light-progress-poor').length > 0, true);
		equal($('.traffic-light-progress.traffic-light-progress-fair').length > 0, true);
		equal($('.traffic-light-progress.traffic-light-progress-good').length > 0, true);
	});

	test('should not show new "traffic light" icon when there is no data and toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON', function() {
		var fakeAjax = {
			Ajax: function(option) {
				if (option.url === '../api/Schedule/FetchWeekData') {
					option.success(getFakeScheduleData());
				}
				if (option.url === 'UserData/FetchUserData') {
					option.success({
						BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
						DataSourceName: 'Teleopti WFM',
						Url: 'http://localhost:52858/TeleoptiWFM/Web/',
						AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
					});
				}
			}
		};
		Teleopti.MyTimeWeb.Common.Init(
			{
				defaultNavigation: '/',
				baseUrl: '/',
				startBaseUrl: '/'
			},
			fakeAjax
		);

		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		$('body').append(setupHtml());

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, fakeAjax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].ProbabilityClass = '';
		fakeScheduleData.Days[0].ProbabilityText = '';
		fakeScheduleData.Days[1].ProbabilityClass = '';
		fakeScheduleData.Days[1].ProbabilityText = '';
		fakeScheduleData.Days[2].ProbabilityClass = '';
		fakeScheduleData.Days[2].ProbabilityText = '';

		vm.initializeData(fakeScheduleData);

		equal($('.traffic-light-progress').length, 0);
	});

	test('should not show new "traffic light" icon for past time on week view', function() {
		var fakeAjax = {
			Ajax: function(option) {
				if (option.url === '../api/Schedule/FetchWeekData') {
					option.success(getFakeScheduleData());
				}
				if (option.url === 'UserData/FetchUserData') {
					option.success({
						BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
						DataSourceName: 'Teleopti WFM',
						Url: 'http://localhost:52858/TeleoptiWFM/Web/',
						AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
					});
				}
			}
		};
		Teleopti.MyTimeWeb.Common.Init(
			{
				defaultNavigation: '/',
				baseUrl: '/',
				startBaseUrl: '/'
			},
			fakeAjax
		);

		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		$('body').append(setupHtml());

		Teleopti.MyTimeWeb.UserInfo.WhenLoaded = function(callback) {
			callback({ WeekStart: 1 });
		};

		Teleopti.MyTimeWeb.Schedule.PartialInit(function() {}, function() {}, fakeAjax);
		Teleopti.MyTimeWeb.Schedule.SetupViewModel(
			Teleopti.MyTimeWeb.Common.DateTimeDefaultValues,
			Teleopti.MyTimeWeb.Schedule.LoadAndBindData
		);

		var vm = Teleopti.MyTimeWeb.Schedule.Vm();
		var fakeScheduleData = getFakeScheduleData();
		fakeScheduleData.Days[0].FixedDate = moment(fakeScheduleData.Days[0].FixedDate)
			.add('day', -3)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[1].FixedDate = moment(fakeScheduleData.Days[1].FixedDate)
			.add('day', -3)
			.format('YYYY-MM-DD');
		fakeScheduleData.Days[2].FixedDate = moment(fakeScheduleData.Days[2].FixedDate)
			.add('day', -3)
			.format('YYYY-MM-DD');
		vm.initializeData(fakeScheduleData);

		equal($('.traffic-light-progress').length, 0);
	});

	function getFakeScheduleData() {
		return Teleopti.MyTimeWeb.Schedule.FakeData.getFakeScheduleData();
	}

	function setupHash() {
		this.hasher = {
			initialized: {
				add: function() {}
			},
			changed: {
				add: function() {}
			},
			init: function() {},
			setHash: function(data) {
				hash = '#' + data;
			}
		};
	}

	function setupHtml() {
		var html = [
			'<div class="navbar teleopti-toolbar subnavbar">',
			'	<div class="container">',
			'		<ul class="nav navbar-nav teleopti-toolbar row submenu">',
			'			<li class="col-xs-12 col-sm-4">',
			'				<span class="input-group " style="max-width: 329px;">',
			'					<span class="input-group-btn">',
			'						<button class="btn btn-default" data-bind="click: previousWeek, css: \'done\'" type="button" id="btnPreviousWeek" aria-label="@Resources.PreviousWeek">',
			'							<i class="glyphicon glyphicon-chevron-left"></i>',
			'						</button>',
			'					</span>',
			'					<input class="form-control text-center date-input-style" style="width: 100%; font-size: 13px" data-bind="value: displayDate"',
			'					 type="text" readonly="readonly" />',
			'					<span class="input-group-btn">',
			'						<button class="btn btn-default moment-datepicker" type="button" aria-label="@Resources.SelectWeek">',
			'							<i class="glyphicon glyphicon-calendar"></i>',
			'						</button>',
			'						<button class="btn btn-default" data-bind="click: nextWeek, css: \'done\'" type="button" id="btnNextWeek" aria-label="@Resources.NextWeek">',
			'							<i class="glyphicon glyphicon-chevron-right"></i>',
			'						</button>',
			'					</span>',
			'				</span>',
			'			</li>',
			'			<li class="col-xs-12 col-sm-4"><a href="#" id="week-schedule-today" data-bind="click: today">@Resources.Today</a></li>',
			'			<li class="col-xs-12 col-sm-4 active"><a href="#" id="week-schedule-week" data-bind="click: week">@Resources.Week</a></li>',
			'			<li class="col-xs-12 col-sm-4"><a href="#" id="week-schedule-month" data-bind="click: month, visible: !Teleopti.MyTimeWeb.Common.UseJalaaliCalendar">@Resources.Month</a></li>',
			'			<li class="col-xs-12 col-sm-4 weekview-probability-toggle" data-bind="visible: showProbabilityToggle">',
			'				<div class="input-group pull-right" id="probabilityDropdownMenu">',
			'					<a class="btn btn-default dropdown-toggle" id="dropdown-probability-type" data-toggle="dropdown" aria-haspopup="true" href="#"',
			'					 aria-expanded="true">',
			'						<i class="glyphicon" data-bind="css: {\'glyphicon-eye-open\': selectedProbabilityType > probabilityTypes.none, \'glyphicon-eye-close\': selectedProbabilityType === probabilityTypes.none}"></i>',
			'						<span data-bind="text: probabilityLabel()" class="probabilityLabel"></span>',
			'						<span class="caret"></span>',
			'						<img class="probability-loading-gif" src=\'@Url.Content("~/Areas/MyTime/Content/Images/ajax-loader.gif")\' alt="..."',
			'						 data-bind="visible: loadingProbabilityData" />',
			'					</a>',
			'					<ul class="dropdown-menu staffing-info-dropdown-form">',
			'						<li>',
			'							<a href="#" data-bind="click: function (data, event) { switchProbabilityType(probabilityTypes.none); }">',
			'								@Resources.HideStaffingInfo',
			'							</a>',
			'						</li>',
			'						<li data-bind="visible: absenceProbabilityEnabled()">',
			'							<a href="#" data-bind="click: function (data, event) { switchProbabilityType(probabilityTypes.absence); }">',
			'								@Resources.ShowAbsenceProbability',
			'							</a>',
			'						</li>',
			'						<li data-bind="visible: overtimeProbabilityEnabled()">',
			'							<a href="#" data-bind="click: function (data, event) { switchProbabilityType(probabilityTypes.overtime); }">',
			'								@Resources.ShowOvertimeProbability',
			'							</a>',
			'						</li>',
			'					</ul>',
			'				</div>',
			'			</li>',
			'		</ul>',
			'	</div>',
			'</div>',
			'<div class="container pagebody">',
			'	<div class="schedule-request-form">',
			'		<ul class="nav nav-pills navbar-weekview" data-bind="visible: showAddRequestToolbar">',
			'			@if (Model.RequestLicense.IsOvertimeAvailabilityEnabled)',
			'			{',
			'			<li data-bind="visible: overtimeAvailabilityPermission, css: { active: requestViewModel() != undefined && requestViewModel().type() == requestViewModelTypes.overtimeAvailability }">',
			'				<a class="overtime-availability-add" href="#" data-bind="click: showAddOvertimeAvailabilityForm"><i class="glyphicon glyphicon-plus" aria-label="@Resources.Add"></i>&nbsp;@Resources.OvertimeAvailability</a>',
			'			</li>',
			'			}',
			'			<li data-bind="visible: textPermission, css: {active: requestViewModel()!=undefined && requestViewModel().type() == requestViewModelTypes.textRequest}">',
			'				<a id="addTextRequest" class="text-request-add" href="#" data-bind="click: showAddTextRequestForm"><i class="glyphicon glyphicon-plus" aria-label="@Resources.Add"></i>&nbsp;@Resources.TextRequest</a>',
			'			</li>',
			'			<li data-bind="visible: absenceRequestPermission, css: { active: requestViewModel()!=undefined && requestViewModel().type() == requestViewModelTypes.absenceRequest }">',
			'				<a id="addAbsenceRequest" class="absence-request-add" href="#" data-bind="click: showAddAbsenceRequestForm"><i',
			'					 class="glyphicon glyphicon-plus" aria-label="@Resources.Add"></i>&nbsp;@Resources.AbsenceRequest</a>',
			'			</li>',
			'			<li data-bind="visible: shiftExchangePermission, css: { active: requestViewModel()!=undefined && requestViewModel().type() == requestViewModelTypes.shiftOffer }">',
			'				<a class="shift-exchange-offer-add" href="#" data-bind="click: showAddShiftExchangeOfferForm"><i class="glyphicon glyphicon-pushpin"></i>&nbsp;@Resources.AnnounceShift</a>',
			'			</li>',
			'			<li data-bind="visible: isAbsenceReportAvailable, css: { active: requestViewModel()!=undefined && requestViewModel().type() == requestViewModelTypes.absenceReport  }">',
			'				<a id="addAbsenceReport" class="addAbsenceReport" href="#" data-bind="click: showAddAbsenceReportForm"><i class="glyphicon glyphicon-plus" aria-label="@Resources.Add"></i>&nbsp;@Resources.AbsenceReport</a>',
			'			</li>',
			'			@if (Model.RequestLicense.IsOvertimeRequestEnabled)',
			'			{',
			'			<li data-bind="visible: isOvertimeRequestAvailable(), css: { active: requestViewModel()!=undefined && requestViewModel().type() == requestViewModelTypes.overtimeRequest }">',
			'				<a id="addOvertimeRequest" href="#" data-bind="click: showAddOvertimeRequestForm"><i class="glyphicon glyphicon-plus" aria-label="@Resources.Add"></i>&nbsp;@Resources.OvertimeRequest</a>',
			'			</li>',
			'			}',
			'		</ul>',
			'		<!-- ko with: requestViewModel -->',
			'		<div data-bind="with: model">',
			'			<div id="Request-add-section" class="well well-sm clearfix">',
			'				<div data-bind="template: Template">',
			'				</div>',
			'			</div>',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'	<div class="schedule-table-container">',
			'		<div class="week-schedule-ASM-permission-granted" data-bind="if : asmEnabled">yes</div>',
			'		<div class="week-schedule-current-week" data-bind="if : isCurrentWeek">yes</div>',
			'		<div class="body-weekview-inner clearfix">',
			'			<div class="weekview-timeline floatleft clearfix" aria-hidden="true">',
			'				<div class="weekview-day-time-indicator-small absolute">',
			'				</div>',
			'				<!-- ko foreach: timeLines -->',
			'				<div class="weekview-timeline-label absolute" data-bind="style: {top: topPosition}, text: timeText, visible: isHour">',
			'				</div>',
			'				<!-- /ko -->',
			'			</div>',
			'			<!-- ko foreach: days -->',
			"			<div class=\"weekview-day\" data-bind=\"attr: {'data-mytime-date': fixedDate, 'data-mytime-state': state, 'data-mytime-dayofweek': dayOfWeek}\">",
			'				<div class="weekview-day-header" data-bind="text: headerTitle"></div>',
			'				<div class="weekview-day-date clearfix">',
			'					<label class="weekview-day-month pull-left badge day-header-day" data-bind="text: headerDayDescription"></label>',
			'					<label class="pull-right badge day-header-day" data-bind="text: headerDayNumber"></label>',
			'				</div>',
			"				<div data-bind=\"css: { 'show-request': requestPermission}, attr: { 'class': classForDaySummary },",
			'		            style: { backgroundColor: colorForDaySummary, color: textColor }, click: $parent.showAddRequestForm"',
			'				 class="weekview-day-summary">',
			'					<strong class="fullwidth displayblock" data-bind="text: summaryTitle"></strong>',
			'					<span class="fullwidth displayblock" data-bind="text: summaryTimeSpan"></span>',
			'					<span data-bind="text: summary"></span>',
			'					<!-- ko if: hasOvertime && hasShift -->',
			'					<strong class="fullwidth displayblock">@Resources.Overtime</strong>',
			'					<!-- /ko -->',
			'					<!-- ko if: hasOvertime && !hasShift-->',
			'					<strong class="fullwidth displayblock overtime">@Resources.Overtime</strong>',
			'					<!-- /ko -->',
			'				</div>',
			'				<div class="weekview-day-symbol" data-bind="click: $parent.showAddRequestForm, css: { \'weekview-day-show-request\': requestPermission }">',
			'					<!-- ko if: hasNote -->',
			'					<div class="weekview-day-icon cursorpointer" data-bind="tooltip: { title: noteMessage, html: true }">',
			'						<i class="glyphicon glyphicon-exclamation-sign"></i>',
			'					</div>',
			'					<!-- /ko -->',
			'					<div class="seatbooking-symbol weekview-day-icon cursorpointer" data-bind="visible: seatBookingIconVisible, tooltip: { title: seatBookingMessage, html: true }">',
			'						<i class="mdi mdi-chair-school"></i>',
			'					</div>',
			'					<div class="overtime-availability-symbol floatright weekview-day-icon cursorpointer" data-bind="visible: overtimeAvailability.HasOvertimeAvailability, tooltip: { title: textOvertimeAvailabilityText, html: true }">',
			'						<i class="glyphicon glyphicon-time floatright"></i>',
			'					</div>',
			'					<div class="weekview-day-symbol-request weekview-day-icon cursorpointer" data-bind="visible: hasRequests, tooltip: { title: requestsText, html: true }, click: navigateToRequests">',
			'						<i class="glyphicon glyphicon-comment"></i>',
			'					</div>',
			'					<!--ko ifnot: $parent.newTrafficLightIconEnabled -->',
			'					<div class="holiday-agents weekview-day-icon cursorpointer" data-bind="visible: absenceRequestPermission, tooltip: { title: holidayChanceText, html: true }">',
			'						<div class="small-circle" data-bind="style: {background: absenceChanceColor}"></div>',
			'					</div>',
			'					<!-- /ko -->',
			'					<!--ko if: $parent.newTrafficLightIconEnabled && absenceRequestPermission && trafficLightClass.length > 0 && notPastTime -->',
			'					<div class="progress traffic-light-progress" data-bind="tooltip: { title: holidayChanceText, html: true}, css: trafficLightClass">',
			'						<div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0" aria-valuemax="100">',
			'							<span class="sr-only">60% Complete</span>',
			'						</div>',
			'					</div>',
			'					<!-- /ko -->',
			'				</div>',
			'				<div>',
			'					<div class="weekview-day-schedule relative" data-bind="style: {width: probabilities().length > 0 ? \'116px\' : \'127px\'}">',
			'						<div class="weekview-day-time-indicator relative" data-bind="style: {width: probabilities().length > 0 ? \'114px\' : \'129px\'}">',
			'						</div>',
			'						<!-- ko foreach: layers -->',
			'						<!-- ko if: isOvertimeAvailability -->',
			'						<div class="weekview-day-schedule-layer absolute overtime-availability-bar" data-bind="tooltip: { title: tooltipText, html: true }, style: styleJson, click: $parent.showOvertimeAvailability">',
			'							<i class="glyphicon glyphicon-time"></i>',
			'						</div>',
			'						<!-- /ko -->',
			'						<!-- ko ifnot: isOvertimeAvailability -->',
			'						<div class="weekview-day-schedule-layer absolute" data-bind="tooltip: { title: tooltipText, html: true }, style: styleJson, css:{\'overtime-background-image-light\': isOvertime && overTimeLighterBackgroundStyle, \'overtime-background-image-dark\': isOvertime && overTimeDarkerBackgroundStyle}">',
			'							<div data-bind="visible: showTitle">',
			'								<strong class="weekview-day-schedule-layer-activity truncate" data-bind="text: title"></strong>',
			'								<span class="weekview-day-schedule-layer-time fullwidth displayblock" data-bind="visible: showDetail, text: timeSpan"></span>',
			'								<!-- ko if: hasMeeting -->',
			'								<div class="meeting mt5">',
			'									<i class="meeting-icon icon-user mr10">',
			'										<i class="glyphicon glyphicon-user ml10"></i>',
			'									</i>',
			'								</div>',
			'								<!-- /ko -->',
			'							</div>',
			'						</div>',
			'						<!-- /ko -->',
			'						<!-- /ko -->',
			'					</div>',
			'					<!-- ko if: showStaffingProbabilityBar -->',
			'					<div class="probability-vertical-bar ">',
			'						<!--ko foreach: probabilities() -->',
			'						<div class="probability-cell" data-bind="css:cssClass(), tooltip: {title: tooltips, html:true}, style: styleJson"></div>',
			'						<!-- /ko -->',
			'					</div>',
			'					<!-- /ko -->',
			'					<!-- /ko -->',
			'				</div>',
			'			</div>',
			'			<!-- /ko -->',
			'		</div>',
			'	</div>',
			'</div>'
		].join('');

		html = '<div id="page"> ' + html + '</div>';
		return html;
	}

	function momentWithLocale(date) {
		return moment(date).locale('en-gb');
	}
});
