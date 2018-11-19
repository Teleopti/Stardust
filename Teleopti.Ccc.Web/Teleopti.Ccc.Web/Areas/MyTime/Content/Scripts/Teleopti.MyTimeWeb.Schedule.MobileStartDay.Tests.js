$(document).ready(function() {
	var hash = '',
		startDayData,
		ajax,
		ajaxOption,
		probabilityAjaxCallCount = 0,
		templates = [],
		fetchDayDataRequestCount,
		requestSuccessData,
		constants = Teleopti.MyTimeWeb.Common.Constants,
		propabilities = [],
		templateConfig = {
			default: 'add-new-request-detail-template',
			absenceReporting: 'add-absence-report-detail-template',
			postShiftForTrade: 'shift-exchange-offer-template',
			overtimeAvailability: 'add-overtime-availability-template'
		},
		fakeWindow = {
			navigator: {
				appVersion:
					'5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1',
				userAgent:
					'Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1'
			},
			location: {
				hash: '#Schedule/MobileDay',
				host: 'localhost:52858',
				hostname: 'localhost',
				href: 'http://localhost:52858/TeleoptiWFM/Web/MyTime#Schedule/MobileDay',
				origin: 'http://localhost:52858',
				pathname: '/TeleoptiWFM/Web/MyTime',
				port: '52858',
				protocol: 'http:'
			}
		};

	module('Teleopti.MyTimeWeb.Schedule.MobileStartDay', {
		setup: function() {
			setup();
		},
		teardown: function() {
			$('body').removeClass('mobile-start-day-body');
			$('body').removeClass('mobile-start-day');

			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
			Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

			templates.forEach(function(template) {
				if (template) {
					template.remove();
				}
			});
		}
	});

	test('should navigate to default start day view when clicking logo', function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_43446');

		var html = [
			'<div class="navbar-header pull-left fake-navbar-brand">',
			'	<button type="button" class="navbar-toggle navbar-toggle-button" data-toggle="offcanvas" data-target=".navbar-offcanvas">',
			'		<span class="icon-bar"></span>',
			'		<span class="icon-bar"></span>',
			'		<span class="icon-bar"></span>',
			'	</button>',
			'	<a class="navbar-brand" href="#Schedule/MobileDay">Teleopti</a>',
			'</div>'
		].join('');

		$('body').append(html);
		Teleopti.MyTimeWeb.Portal.Init(
			{ defaultNavigation: 'Schedule/Week', baseUrl: '/', startBaseUrl: '/' },
			fakeWindow,
			ajax
		);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			fakeWindow
		);

		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		var currentDate = vm.selectedDate();
		vm.nextDay();

		equal(vm.selectedDate().format('YYYY-MM-DD'), currentDate.add(1, 'days').format('YYYY-MM-DD'));
		equal($('a.navbar-brand').attr('href'), '#Schedule/MobileDay');
		$('a.navbar-brand').click();

		equal(vm.selectedDate().format('YYYY-MM-DD'), vm.currentUserDate().format('YYYY-MM-DD'));

		$('.fake-navbar-brand').remove();
	});

	test('should navigate to default start date when clicking schedule menu item', function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_43446');

		var html = [
			'<div class="navbar-offcanvas offcanvas">',
			'	<ul class="nav navbar-nav">',
			'		<li class="active">',
			'			<a href="#ScheduleTab" onclick="hideNavMenu();" data-mytime-action="Schedule/Week">',
			'				Schedule',
			'				<span class="badge hide no-unread-msg"></span>',
			'			</a>',
			'		</li>',
			'		<li>',
			'			<a href="#TeamScheduleTab" onclick="hideNavMenu();" data-mytime-action="TeamSchedule/NewIndex">',
			'				Team Schedule',
			'				<span class="badge hide no-unread-msg"></span>',
			'			</a>',
			'		</li>',
			'		<li>',
			'			<a href="#AvailabilityTab" onclick="hideNavMenu();" data-mytime-action="Availability/Index">',
			'				Availability',
			'				<span class="badge hide no-unread-msg"></span>',
			'			</a>',
			'		</li>',
			'		<li>',
			'			<a href="#PreferenceTab" onclick="hideNavMenu();" data-mytime-action="Preference/Index">',
			'				Preferences',
			'				<span class="badge hide no-unread-msg"></span>',
			'			</a>',
			'		</li>',
			'		<li>',
			'			<a href="#RequestsTab" onclick="hideNavMenu();" data-mytime-action="Requests/Index">',
			'				Requests',
			'				<span class="badge hide no-unread-msg"></span>',
			'			</a>',
			'		</li>',
			'		<li>',
			'			<a href="#MessageTab" onclick="hideNavMenu();" data-mytime-action="Message/Index" class="asm-new-message-indicator ">',
			'				Messages',
			'				<span class="badge badge-important">146</span>',
			'			</a>',
			'		</li>',
			'	</ul>',
			'</div>'
		].join('');

		$('body').append(html);
		Teleopti.MyTimeWeb.Portal.Init(
			{ defaultNavigation: 'Schedule/Week', baseUrl: '/', startBaseUrl: '/' },
			fakeWindow,
			ajax
		);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			fakeWindow
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.nextDay();

		equal($('ul.navbar-nav li a[href="#Schedule/MobileDay"]').attr('data-mytime-action'), 'Schedule/MobileDay');

		$('.teleopti-mytime-top-menu .navbar-offcanvas').remove();
	});

	test('should navigate to next date when swiping left', function() {
		$('body').addClass('mobile-start-day-body');
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		var currentDate = vm.selectedDate();
		$('.mobile-start-day-body')
			.swipe('option')
			.swipeLeft();
		$('.mobile-start-day-body').swipe('disable');

		equal(
			vm.selectedDate().format('MMM Do YY'),
			moment(currentDate)
				.add(1, 'days')
				.format('MMM Do YY')
		);
	});

	test('should navigate to previous date when swiping right', function() {
		$('body').addClass('mobile-start-day-body');
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		var currentDate = vm.selectedDate();
		$('.mobile-start-day-body')
			.swipe('option')
			.swipeRight();
		$('.mobile-start-day-body').swipe('disable');
		equal(
			vm.selectedDate().format('MMM Do YY'),
			moment(currentDate)
				.add(-1, 'days')
				.format('MMM Do YY')
		);
	});

	test("should go back to current date after clicking 'home' icon", function() {
		var currentUserDate = moment().zone(-startDayData.BaseUtcOffsetInMinutes);
		startDayData.Date = currentUserDate.format(constants.serviceDateTimeFormat.dateOnly);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.nextDay();
		equal(
			vm.selectedDate().format(constants.serviceDateTimeFormat.dateOnly),
			currentUserDate.add('days', 1).format(constants.serviceDateTimeFormat.dateOnly)
		);
		vm.today();
		equal(
			vm.selectedDate().format(constants.serviceDateTimeFormat.dateOnly),
			currentUserDate.subtract('days', 1).format(constants.serviceDateTimeFormat.dateOnly)
		);
	});

	test("should navigate to month view after clicking 'month' icon", function() {
		var currentUserDate = moment().zone(-startDayData.BaseUtcOffsetInMinutes);
		startDayData.Date = currentUserDate.format(constants.serviceDateTimeFormat.dateOnly);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.navigateToMonthView();
		equal(hash, 'Schedule/MobileMonth');
	});

	test('should set timelines', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		equal(vm.timeLines().length, 12);
	});

	test('should set top position for timeline', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		equal(vm.timeLines()[1].topPosition(), '13px');
	});

	test('should set display time for timeline', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		equal(vm.timeLines()[1].timeText, '07:00');
	});

	test('should set hour flag correctly for timeline', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		equal(vm.timeLines()[0].isHour(), false);
		equal(vm.timeLines()[1].isHour(), true);
	});

	test('should set timeline height', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		equal(vm.scheduleHeight(), '668px'); // Not mobile, applied constants.scheduleHeight
	});

	test('should set unreadMessage', function() {
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		equal(vm.unreadMessageCount(), 2);
	});

	test('should not show unreadMessage number if it is zero', function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_43446');

		startDayData.UnReadMessageCount = 0;
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		var html =
			'<div id="testUnreadMessageHtml">' +
			'<ul class="nav navbar-nav teleopti-toolbar row submenu">' +
			'<li class="mobile-message" data-bind="click: navigateToMessages">' +
			'<a>' +
			'<i class="glyphicon glyphicon-envelope"></i>' +
			'<span id="MobileDayView-message" class="badge" data-bind="visible: unreadMessageCount() > 0, text: unreadMessageCount">' +
			'</span>' +
			'</a>' +
			'</li>' +
			'</ul>' +
			'</div>';
		$('body').append(html);

		ko.applyBindings(vm, $('#testUnreadMessageHtml')[0]);

		equal(vm.unreadMessageCount(), 0);
		equal($('#MobileDayView-message')[0].style['display'], 'none');

		$('#testUnreadMessageHtml').remove();
	});

	test('should navigate to messages', function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_43446');

		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow());

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.navigateToMessages();

		equal(hash, 'MessageTab');
	});

	test('should show request link for current day when there are requests', function() {
		startDayData.Schedule.RequestsCount = 1;

		setupRequestCountTemplate();

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		equal(vm.requestsCount(), 1);

		equal(1, $('#page:visible').length);
	});

	test('should navigate to requests', function() {
		startDayData.Schedule.RequestsCount = 1;
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.navigateToRequests();
		equal(hash, 'Requests/Index');
	});

	test('should hide request link for current day when there are no requests', function() {
		setupRequestCountTemplate();

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		equal(vm.requestsCount(), 0);

		equal(0, $('#page:visible').length);
	});

	test('should show add text request form', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.showAddTextRequestForm();
		var requestViewModel = vm.requestViewModel().model;
		equal(requestViewModel.DateTo(), vm.requestDay);
		equal(requestViewModel.DateFrom(), vm.requestDay);

		equal(templateConfig.default, vm.requestViewModel().model.Template());
	});

	test('should add text request', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Request.RequestDetail.Init(ajax);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			null,
			moment('2017-04-28')
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showAddTextRequestForm();

		var requestViewModel = vm.requestViewModel().model;

		requestViewModel.DateFrom('28/04/2017');
		requestViewModel.TimeFrom('08:00');
		requestViewModel.DateTo('28/04/2017');
		requestViewModel.TimeTo('09:10');
		requestViewModel.Subject('subject');
		requestViewModel.Message('msg');
		requestViewModel.IsFullDay(false);

		requestViewModel.AddRequest();

		equal(vm.requestViewModel(), undefined);

		equal(requestViewModel.Template(), templateConfig.default);

		equal(vm.requestsCount(), 1);
	});

	test('should show add absence request form', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.showAddAbsenceRequestForm();
		var requestViewModel = vm.requestViewModel().model;

		equal(vm.requestViewModel().model.Template(), templateConfig.default);
		equal(requestViewModel.DateTo(), vm.requestDay);
		equal(requestViewModel.DateFrom(), vm.requestDay);

		equal(true, vm.requestViewModel().model.ShowAbsencesCombo());
	});

	test('should display personal account in absence request form', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);

		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.showAddAbsenceRequestForm();

		var requestViewModel = vm.requestViewModel().model;
		requestViewModel.SetAjax(ajax);

		requestViewModel.AbsenceId('2');

		equal(requestViewModel.Template(), templateConfig.default);

		equal(true, requestViewModel.ShowAbsenceAccount());
	});

	test('should add absence request', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Request.RequestDetail.Init(ajax);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			null,
			moment('2017-04-28')
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showAddAbsenceRequestForm();

		var requestViewModel = vm.requestViewModel().model;
		requestViewModel.SetAjax(ajax);

		requestViewModel.DateFrom(moment('28/04/2017'));
		requestViewModel.TimeFrom('08:00');
		requestViewModel.DateTo(moment('28/04/2017'));
		requestViewModel.TimeTo('09:10');
		requestViewModel.Subject('subject');
		requestViewModel.Message('msg');
		requestViewModel.IsFullDay(false);
		requestViewModel.AbsenceId('2');

		requestViewModel.AddRequest();

		equal(vm.requestViewModel(), undefined);

		equal(requestViewModel.Template(), templateConfig.default);

		equal(vm.requestsCount(), 1);
	});

	test('should show absence reporting form', function() {
		setupAddRequestTemplate(templateConfig.absenceReporting);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showAbsenceReportingForm();

		var requestViewModel = vm.requestViewModel().model;
		equal(requestViewModel.DateTo(), vm.requestDay);

		equal(vm.requestViewModel().model.Template, templateConfig.absenceReporting);
	});

	test('should add absence reporting', function() {
		setupAddRequestTemplate(templateConfig.absenceReporting);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showAbsenceReportingForm();

		var requestViewModel = vm.requestViewModel().model;
		requestViewModel.AbsenceId('1');
		requestViewModel.DateFrom(moment('11/05/2017'));
		requestViewModel.SaveAbsenceReport();

		equal(requestViewModel.Template, templateConfig.absenceReporting);

		equal(fetchDayDataRequestCount, 1);

		equal(vm.requestViewModel(), undefined);
	});

	test('should show overtime availability form with default value when no overtime availability', function() {
		setupAddRequestTemplate(templateConfig.overtimeAvailability);

		Teleopti.MyTimeWeb.Common.DateFormat = 'DD/MM/YYYY';

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			null,
			moment('2017-04-28')
		);

		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.showOvertimeAvailabilityForm(vm);

		equal(
			vm
				.requestViewModel()
				.model.DateFrom()
				.format(vm.datePickerFormat()),
			startDayData.Schedule.Date
		);

		equal(vm.requestViewModel().model.StartTime(), startDayData.Schedule.OvertimeAvailabililty.DefaultStartTime);

		equal(vm.requestViewModel().model.EndTime(), startDayData.Schedule.OvertimeAvailabililty.DefaultEndTime);

		equal(
			vm.requestViewModel().model.EndTimeNextDay(),
			startDayData.Schedule.OvertimeAvailabililty.DefaultEndTimeNextDay
		);

		equal(vm.requestViewModel().model.Template, templateConfig.overtimeAvailability);
	});

	test('should show overtime availability form with specific value when overtime availability is available', function() {
		setupAddRequestTemplate(templateConfig.overtimeAvailability);
		Teleopti.MyTimeWeb.Common.DateFormat = 'DD/MM/YYYY';

		startDayData.Schedule.OvertimeAvailabililty.HasOvertimeAvailability = true;

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			null,
			moment('2017-04-28')
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		vm.showOvertimeAvailabilityForm(vm);

		equal(
			vm
				.requestViewModel()
				.model.DateFrom()
				.format(vm.datePickerFormat()),
			startDayData.Schedule.Date
		);

		equal(vm.requestViewModel().model.StartTime(), startDayData.Schedule.OvertimeAvailabililty.StartTime);

		equal(vm.requestViewModel().model.EndTime(), startDayData.Schedule.OvertimeAvailabililty.EndTime);

		equal(vm.requestViewModel().model.EndTimeNextDay(), startDayData.Schedule.OvertimeAvailabililty.EndTimeNextDay);

		equal(vm.requestViewModel().model.Template, templateConfig.overtimeAvailability);
	});

	test('should add overtime availability', function() {
		setupAddRequestTemplate(templateConfig.overtimeAvailability);
		startDayData.Schedule.OvertimeAvailabililty.HasOvertimeAvailability = true;

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showOvertimeAvailabilityForm();

		var requestViewModel = vm.requestViewModel().model;
		requestViewModel.DateFrom(moment('11/05/2017'));
		requestViewModel.StartTime('08:00');
		requestViewModel.EndTime('15:00');
		requestViewModel.EndTimeNextDay(false);
		requestViewModel.SetOvertimeAvailability(vm);

		equal(requestViewModel.Template, templateConfig.overtimeAvailability);

		equal(fetchDayDataRequestCount, 2);

		equal(vm.requestViewModel(), undefined);
	});

	test('should show post shift for trade form', function() {
		setupAddRequestTemplate(templateConfig.postShiftForTrade);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showPostShiftForTradeForm();

		equal(vm.requestViewModel().model.Template, templateConfig.postShiftForTrade);
	});

	test('should post shift for trade', function() {
		setupAddRequestTemplate(templateConfig.postShiftForTrade);
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax,
			null,
			moment('2017-04-28')
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showPostShiftForTradeForm();

		var requestViewModel = vm.requestViewModel().model;
		requestViewModel.DateTo('2017-04-28');
		requestViewModel.OfferValidTo('2017-04-27');
		requestViewModel.startTimeInternal();
		requestViewModel.endTimeInternal();
		requestViewModel.EndTimeNextDay();
		requestViewModel.WishShiftTypeOption('Working Day');
		startDayData.Date = '2017-04-28';
		requestViewModel.SaveShiftExchangeOffer();

		equal(vm.requestsCount(), 1);
		equal(requestViewModel.Template, templateConfig.postShiftForTrade);
	});

	test('should not add request count if request day is not equal to current day', function() {
		setupAddRequestTemplate();

		Teleopti.MyTimeWeb.Request.RequestDetail.Init(ajax);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.showAddTextRequestForm();

		var requestViewModel = vm.requestViewModel().model;

		requestViewModel.DateFrom('11/05/2017');
		requestViewModel.TimeFrom('08:00');
		requestViewModel.DateTo('11/05/2017');
		requestViewModel.TimeTo('09:10');
		requestViewModel.Subject('subject');
		requestViewModel.Message('msg');
		requestViewModel.IsFullDay(false);

		requestSuccessData = { DateFromYear: 2017, DateFromMonth: 5, DateFromDayOfMonth: 11 };

		requestViewModel.AddRequest();

		equal(vm.requestViewModel(), undefined);

		equal(requestViewModel.Template(), templateConfig.default);

		equal(vm.requestsCount(), 0);
	});

	test('should navigate to team schedule', function() {
		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow());
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.redirectToShiftTradeRequest();
		equal(hash, 'Requests/Index/ShiftTrade/' + vm.requestDay.format('YYYYMMDD'));
	});

	test("should get today by agent's timezone", function() {
		startDayData.BaseUtcOffsetInMinutes = -600;
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.today();
		equal(
			vm.currentUserDate().format('YYYY-MM-DD'),
			moment()
				.zone(-startDayData.BaseUtcOffsetInMinutes)
				.format('YYYY-MM-DD')
		);
	});

	test("should show poor 'traffic light' on mobile", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		startDayData.Schedule.ProbabilityClass = 'poor';
		startDayData.Schedule.ProbabilityText = 'Poor';

		vm.nextDay();

		equal(vm.trafficLightColor(), 'red');
	});

	test("should update 'traffic light' base on absence proability data on mobile", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';

		vm.nextDay();

		equal(vm.trafficLightColor(), 'green');
	});

	test("should set tooltip text for 'traffic light' base on absence proability data on mobile", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();

		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';

		vm.nextDay();

		equal(vm.trafficLightColor(), 'green');
		equal(vm.trafficLightTooltip(), '@Resources.ChanceOfGettingAbsenceRequestGranted: Good');
	});

	test("should show new 'traffic light' icon when toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';
		vm.nextDay();

		equal(vm.trafficLightColor(), '');
		equal(vm.trafficLightIconClass(), 'traffic-light-progress-good');
	});

	test("should show old 'traffic light' icon when toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is OFF", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';
		vm.nextDay();

		equal(vm.trafficLightColor(), 'green');
		equal(vm.trafficLightIconClass(), '');
	});

	test("should not show new 'traffic light' icon when there is no data and toggle MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		startDayData.Schedule.ProbabilityClass = '';
		startDayData.Schedule.ProbabilityText = '';
		vm.nextDay();

		equal(vm.trafficLightIconClass(), '');
		equal(vm.showNewTrafficLightIconOnMobile(), false);
	});

	test("should not show old 'traffic light' icon for passed days and MyTimeWeb_TrafficLightOnMobileDayView_77447 is ON", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.DisableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';
		vm.previousDay();

		equal(vm.trafficLightColor(), '');
		equal(vm.showOldTrafficLightIconOnMobile(), false);
	});

	test("should not show new 'traffic light' icon for passed days and MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640 is ON", function() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		startDayData.Schedule.ProbabilityClass = 'good';
		startDayData.Schedule.ProbabilityText = 'Good';
		vm.previousDay();

		equal(vm.trafficLightIconClass(), '');
		equal(vm.showNewTrafficLightIconOnMobile(), false);
	});

	test("should show probability toggle by agent's timezone", function() {
		startDayData.BaseUtcOffsetInMinutes = -600;
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.today();
		equal(vm.showProbabilityOptionsToggleIcon(), true);
	});

	test('should show probability toggle when the day is within staffing info availableDays', function() {
		startDayData.BaseUtcOffsetInMinutes = -600;
		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.today();
		equal(vm.showProbabilityOptionsToggleIcon(), true);
	});

	test('should show overtime probability by site open hour period', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		startDayData.Date = moment()
			.zone(-startDayData.BaseUtcOffsetInMinutes)
			.format(constants.serviceDateTimeFormat.dateOnly);
		startDayData.Schedule.OpenHourPeriod = { EndTime: '13:00:00', StartTime: '08:00:00' };
		propabilities = createPropabilities(['12:00:00', '15:00:00'], startDayData.Date);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.onProbabilityOptionSelectCallback(constants.probabilityType.overtime);
		var probabilities = vm.probabilities();
		var lastTooltips = probabilities[probabilities.length - 1].tooltips();
		ok(lastTooltips.indexOf('12:00 - 12:15') > -1, 'expect contains 12:00 - 12:15 but it is ' + lastTooltips);
	});

	test('should load one week probability data each time', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		startDayData.Date = moment()
			.zone(-startDayData.BaseUtcOffsetInMinutes)
			.format(constants.serviceDateTimeFormat.dateOnly);
		startDayData.Schedule.OpenHourPeriod = { EndTime: '13:00:00', StartTime: '08:00:00' };
		propabilities = createPropabilities(['12:00:00', '15:00:00'], startDayData.Date);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.onProbabilityOptionSelectCallback(constants.probabilityType.overtime);

		equal(ajaxOption.data.returnOneWeekData, true);
	});

	test('should cache one week probability data once it is loaded', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		startDayData.Date = moment()
			.zone(-startDayData.BaseUtcOffsetInMinutes)
			.format(constants.serviceDateTimeFormat.dateOnly);
		startDayData.Schedule.OpenHourPeriod = { EndTime: '13:00:00', StartTime: '08:00:00' };
		propabilities = createPropabilities(['12:00:00', '12:15:00'], startDayData.Date);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(1, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(2, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(3, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(4, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(5, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.onProbabilityOptionSelectCallback(constants.probabilityType.overtime);

		equal(ajaxOption.data.returnOneWeekData, true);
		equal(probabilityAjaxCallCount, 1);

		vm.reloadProbabilityData();

		equal(probabilityAjaxCallCount, 1);
		equal(vm.probabilities().length, 1);
	});

	test('should reload one week probability data when probability option is changed', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		startDayData.CheckStaffingByIntraday = true;
		startDayData.Date = moment()
			.zone(-startDayData.BaseUtcOffsetInMinutes)
			.format(constants.serviceDateTimeFormat.dateOnly);
		startDayData.Schedule.OpenHourPeriod = { EndTime: '13:00:00', StartTime: '08:00:00' };
		propabilities = createPropabilities(['12:00:00', '12:15:00'], startDayData.Date);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(1, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(2, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(3, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(4, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);
		propabilities = propabilities.concat(
			createPropabilities(
				['12:00:00', '12:15:00'],
				moment(startDayData.Date)
					.add(5, 'day')
					.format(constants.serviceDateTimeFormat.dateOnly)
			)
		);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.onProbabilityOptionSelectCallback(constants.probabilityType.overtime);

		equal(ajaxOption.data.returnOneWeekData, true);
		equal(probabilityAjaxCallCount, 1);

		propabilities = [];

		vm.onProbabilityOptionSelectCallback(constants.probabilityType.absence);

		equal(probabilityAjaxCallCount, 2);
		equal(vm.probabilities().length, 0);
	});

	test('should clear probabilities when probability option is none', function() {
		Teleopti.MyTimeWeb.Common.TimeFormat = 'HH:mm';
		startDayData.CheckStaffingByIntraday = true;
		startDayData.Date = moment()
			.zone(-startDayData.BaseUtcOffsetInMinutes)
			.format(constants.serviceDateTimeFormat.dateOnly);

		startDayData.Schedule.Periods.forEach(function(period) {
			period.StartTime = startDayData.Date + 'T' + moment(period.StartTime).format('HH:mm:ss');
			period.EndTime = startDayData.Date + 'T' + moment(period.EndTime).format('HH:mm:ss');
		});

		propabilities = createPropabilities(['07:00:00', '08:00:00'], startDayData.Date);

		Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit(
			fakeReadyForInteractionCallback,
			fakeCompletelyLoadedCallback,
			ajax
		);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileStartDay.Vm();
		vm.onProbabilityOptionSelectCallback(constants.probabilityType.absence);

		startDayData.CheckStaffingByIntraday = false;
		vm.nextDay();
		var probabilities = vm.probabilities();
		equal(probabilities.length, 0);
	});

	function createPropabilities(periods, date) {
		var values = [];
		periods.forEach(function(period) {
			values.push({
				Date: date,
				StartTime: moment(date + ' ' + period).format('YYYY-MM-DDTHH:mm:ss'),
				EndTime: moment(date + ' ' + period)
					.add(15, 'minute')
					.format('YYYY-MM-DDTHH:mm:ss'),
				Possibility: 1
			});
		});
		return values;
	}

	function setupRequestCountTemplate() {
		var template = $(
			"<span id='page' class='glyphicon glyphicon-comment' data-bind='visible: requestsCount() > 0'></span>"
		);
		$('body').append(template);
		templates.push(template);
	}

	function setupAddRequestTemplate(id) {
		if (!id) {
			id = templateConfig.default;
		}
		var addRequestTemplate = $(
			"<script type='text/html' id='" +
				id +
				"'><div></div></script ><span id='page'><!-- ko with: requestViewModel -->" +
				"<div data-bind='with: model'><div><div data-bind='template: Template'></div></div></div>" +
				'<!-- /ko --></span>'
		);
		$('body').append(addRequestTemplate);
		templates.push(addRequestTemplate);
	}

	function fakeCompletelyLoadedCallback() {}

	function fakeReadyForInteractionCallback() {}

	function setup() {
		fetchDayDataRequestCount = 0;
		probabilityAjaxCallCount = 0;

		setupDayData();
		setupRequestSuccessData();
		setupAjax();
		setupElement();
		setupToggles();
		initContext();
	}

	function setupElement() {
		$('body').addClass('mobile-start-day');
	}

	function setupToggles() {
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_DayScheduleForStartPage_Command_44209');
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_TrafficLightOnMobileDayView_77447');
		Teleopti.MyTimeWeb.Common.EnableToggle('MyTimeWeb_NewTrafficLightIconHelpingColorBlindness_78640');
	}

	function setupAjax() {
		var requestUrls = [
			'Requests/TextRequest',
			'Requests/AbsenceRequest',
			'Schedule/ReportAbsence',
			'Schedule/OvertimeAvailability',
			'ShiftExchange/NewOffer',
			'Requests/ShiftTradeRequestPeriod',
			'ShiftExchange/GetAllWishShiftOptions'
		];

		ajax = {
			Ajax: function(options) {
				ajaxOption = options;
				if (options.url === '../api/Schedule/FetchDayData') {
					fetchDayDataCallback(options);
				} else if (options.url === 'Requests/FetchAbsenceAccount') {
					fetchAbsenceAccountCallback(options);
				} else if (requestUrls.indexOf(options.url) > -1) {
					requestSuccessCallback(options);
				} else if (options.url === '../api/ScheduleStaffingPossibility') {
					probabilityAjaxCallCount++;
					scheduleStaffingPossibilityCallback(options);
				}
			}
		};

		function fetchDayDataCallback(options) {
			fetchDayDataRequestCount++;
			if (options.data.date) {
				startDayData.Date = moment(options.data.date).format('YYYY-MM-DD');
			}
			options.success(startDayData);
		}

		function fetchAbsenceAccountCallback(options) {
			var absenceAccountData = {
				TrackerType: 'Days',
				PeriodStart: '08:00',
				PeriodEnd: '09:00',
				Remaining: '1',
				Used: '2'
			};
			options.success(absenceAccountData);
		}

		function requestSuccessCallback(options) {
			options.success(requestSuccessData);
		}

		function scheduleStaffingPossibilityCallback(options) {
			options.success(propabilities);
		}
	}

	function setupDayData() {
		startDayData = {
			UnReadMessageCount: 2,
			Date: '2017-04-28',
			DisplayDate: '28/04/2017',
			IsToday: false,
			StaffingInfoAvailableDays: 14,
			Schedule: {
				RequestsCount: 0,
				Date: '28/04/2017',
				FixedDate: '2017-04-28',
				State: 0,
				Header: {
					Title: 'Friday',
					Date: '28/04/2017',
					DayDescription: '',
					DayNumber: '28'
				},
				Note: {
					Message: ''
				},
				OvertimeAvailabililty: {
					HasOvertimeAvailability: false,
					StartTime: null,
					EndTime: null,
					EndTimeNextDay: false,
					DefaultStartTime: '16:00',
					DefaultEndTime: '17:00',
					DefaultEndTimeNextDay: false
				},
				HasOvertime: false,
				IsFullDayAbsence: false,
				IsDayOff: false,
				Summary: {
					Title: 'Early',
					TimeSpan: '07:00 - 16:00',
					StartTime: '0001-01-01T00:00:00',
					EndTime: '0001-01-01T00:00:00',
					Summary: '8:00',
					StyleClassName: 'color_80FF80',
					Meeting: null,
					StartPositionPercentage: 0.0,
					EndPositionPercentage: 0.0,
					Color: 'rgb(128,255,128)',
					IsOvertime: false
				},
				Periods: [
					{
						Title: 'Invoice',
						TimeSpan: '07:00 - 08:00',
						StartTime: '2017-04-28T07:00:00',
						EndTime: '2017-04-28T08:00:00',
						Summary: '1:00',
						StyleClassName: 'color_FF8080',
						Meeting: null,
						StartPositionPercentage: 0.0263157894736842105263157895,
						EndPositionPercentage: 0.1315789473684210526315789474,
						Color: '255,128,128',
						IsOvertime: false
					},
					{
						Title: 'Phone',
						TimeSpan: '08:00 - 09:00',
						StartTime: '2017-04-28T08:00:00',
						EndTime: '2017-04-28T09:00:00',
						Summary: '1:00',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.1315789473684210526315789474,
						EndPositionPercentage: 0.2368421052631578947368421053,
						Color: '128,255,128',
						IsOvertime: false
					},
					{
						Title: 'Short break',
						TimeSpan: '09:00 - 09:15',
						StartTime: '2017-04-28T09:00:00',
						EndTime: '2017-04-28T09:15:00',
						Summary: '0:15',
						StyleClassName: 'color_FF0000',
						Meeting: null,
						StartPositionPercentage: 0.2368421052631578947368421053,
						EndPositionPercentage: 0.2631578947368421052631578947,
						Color: '255,0,0',
						IsOvertime: false
					},
					{
						Title: 'Phone',
						TimeSpan: '09:15 - 11:00',
						StartTime: '2017-04-28T09:15:00',
						EndTime: '2017-04-28T11:00:00',
						Summary: '1:45',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.2631578947368421052631578947,
						EndPositionPercentage: 0.4473684210526315789473684211,
						Color: '128,255,128',
						IsOvertime: false
					},
					{
						Title: 'Lunch',
						TimeSpan: '11:00 - 12:00',
						StartTime: '2017-04-28T11:00:00',
						EndTime: '2017-04-28T12:00:00',
						Summary: '1:00',
						StyleClassName: 'color_FFFF00',
						Meeting: null,
						StartPositionPercentage: 0.4473684210526315789473684211,
						EndPositionPercentage: 0.5526315789473684210526315789,
						Color: '255,255,0',
						IsOvertime: false
					},
					{
						Title: 'Phone',
						TimeSpan: '12:00 - 14:00',
						StartTime: '2017-04-28T12:00:00',
						EndTime: '2017-04-28T14:00:00',
						Summary: '2:00',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.5526315789473684210526315789,
						EndPositionPercentage: 0.7631578947368421052631578947,
						Color: '128,255,128',
						IsOvertime: false
					},
					{
						Title: 'Short break',
						TimeSpan: '14:00 - 14:15',
						StartTime: '2017-04-28T14:00:00',
						EndTime: '2017-04-28T14:15:00',
						Summary: '0:15',
						StyleClassName: 'color_FF0000',
						Meeting: null,
						StartPositionPercentage: 0.7631578947368421052631578947,
						EndPositionPercentage: 0.7894736842105263157894736842,
						Color: '255,0,0',
						IsOvertime: false
					},
					{
						Title: 'Phone',
						TimeSpan: '14:15 - 16:00',
						StartTime: '2017-04-28T14:15:00',
						EndTime: '2017-04-28T16:00:00',
						Summary: '1:45',
						StyleClassName: 'color_80FF80',
						Meeting: null,
						StartPositionPercentage: 0.7894736842105263157894736842,
						EndPositionPercentage: 0.9736842105263157894736842105,
						Color: '128,255,128',
						IsOvertime: false
					}
				],
				DayOfWeekNumber: 5,
				Availability: false,
				HasNote: false,
				ProbabilityClass: 'poor',
				ProbabilityText: 'Poor',
				SeatBookings: []
			},
			RequestPermission: {
				TextRequestPermission: true,
				AbsenceRequestPermission: true,
				ShiftTradeRequestPermission: false,
				OvertimeAvailabilityPermission: true,
				AbsenceReportPermission: true,
				ShiftExchangePermission: true,
				ShiftTradeBulletinBoardPermission: true,
				PersonAccountPermission: true
			},
			TimeLine: [
				{
					Time: '06:45:00',
					TimeLineDisplay: '06:45',
					PositionPercentage: 0.0,
					TimeFixedFormat: null
				},
				{
					Time: '07:00:00',
					TimeLineDisplay: '07:00',
					PositionPercentage: 0.0263157894736842105263157895,
					TimeFixedFormat: null
				},
				{
					Time: '08:00:00',
					TimeLineDisplay: '08:00',
					PositionPercentage: 0.1315789473684210526315789474,
					TimeFixedFormat: null
				},
				{
					Time: '09:00:00',
					TimeLineDisplay: '09:00',
					PositionPercentage: 0.2368421052631578947368421053,
					TimeFixedFormat: null
				},
				{
					Time: '10:00:00',
					TimeLineDisplay: '10:00',
					PositionPercentage: 0.3421052631578947368421052632,
					TimeFixedFormat: null
				},
				{
					Time: '11:00:00',
					TimeLineDisplay: '11:00',
					PositionPercentage: 0.4473684210526315789473684211,
					TimeFixedFormat: null
				},
				{
					Time: '12:00:00',
					TimeLineDisplay: '12:00',
					PositionPercentage: 0.5526315789473684210526315789,
					TimeFixedFormat: null
				},
				{
					Time: '13:00:00',
					TimeLineDisplay: '13:00',
					PositionPercentage: 0.6578947368421052631578947368,
					TimeFixedFormat: null
				},
				{
					Time: '14:00:00',
					TimeLineDisplay: '14:00',
					PositionPercentage: 0.7631578947368421052631578947,
					TimeFixedFormat: null
				},
				{
					Time: '15:00:00',
					TimeLineDisplay: '15:00',
					PositionPercentage: 0.8684210526315789473684210526,
					TimeFixedFormat: null
				},
				{
					Time: '16:00:00',
					TimeLineDisplay: '16:00',
					PositionPercentage: 0.9736842105263157894736842105,
					TimeFixedFormat: null
				},
				{
					Time: '16:15:00',
					TimeLineDisplay: '16:15',
					PositionPercentage: 1.0,
					TimeFixedFormat: null
				}
			],
			AsmPermission: true,
			ViewPossibilityPermission: true,
			DatePickerFormat: 'dd/MM/yyyy',
			DaylightSavingTimeAdjustment: {
				StartDateTime: '2017-03-26T01:00:00',
				EndDateTime: '2017-10-29T02:00:00',
				AdjustmentOffsetInMinutes: 60.0
			},
			BaseUtcOffsetInMinutes: 60.0,
			AbsenceProbabilityEnabled: true,
			OvertimeProbabilityEnabled: true,
			CheckStaffingByIntraday: false,
			Possibilities: [],
			SiteOpenHourPeriod: null,
			ShiftTradeRequestSetting: {
				HasWorkflowControlSet: true,
				NowDay: moment().format('D'),
				NowMonth: moment().format('M'),
				NowYear: moment().format('YYYY'),
				OpenPeriodRelativeEnd: 99,
				OpenPeriodRelativeStart: 1
			}
		};
	}

	function setupRequestSuccessData() {
		requestSuccessData = { DateFromYear: 2017, DateFromMonth: 4, DateFromDayOfMonth: 28 };
	}

	function initContext() {
		this.crossroads = {
			addRoute: function() {}
		};
		this.hasher = {
			initialized: {
				add: function() {}
			},
			changed: {
				add: function() {}
			},
			init: function() {},
			setHash: function(data) {
				hash = data;
			}
		};
		Teleopti.MyTimeWeb.UserInfo = {
			WhenLoaded: function(whenLoadedCallBack) {
				var data = { WeekStart: '' };
				whenLoadedCallBack(data);
			}
		};
		Teleopti.MyTimeWeb.Common.DateTimeDefaultValues = { defaultFulldayStartTime: '' };
		Teleopti.MyTimeWeb.Common.Init(getDefaultSetting(), ajax);
	}

	function getDefaultSetting() {
		return {
			defaultNavigation: '/',
			baseUrl: '/',
			startBaseUrl: '/'
		};
	}

	function getFakeWindow() {
		return {
			location: {
				hash: '#',
				url: '',
				replace: function(newUrl) {
					this.url = newUrl;
				}
			},
			navigator: {
				userAgent: 'Android'
			}
		};
	}
});
