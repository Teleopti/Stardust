$(document).ready(function() {
	var hash = '',
		ajax,
		vm,
		fakeMonthData,
		fakeUserData,
		mobileMonthHtml,
		fetchMonthDataRequestCount = 0;

	module('Teleopti.MyTimeWeb.Schedule.MobileMonth', {
		setup: function() {
			setup();
		},
		teardown: function() {
			hash = '';
			$('#page').remove();
		}
	});

	test('should use short month name', function() {
		equal(vm.formattedSelectedDate(), moment().format('MMM YYYY'));
	});

	test('should show short week day names', function() {
		equal(vm.weekDayNames().length, 7);
		equal(vm.weekDayNames()[0].ShortName, 'Sun');
	});

	test('should show shift category name in white when shift background color is dark', function() {
		equal(vm.weekViewModels()[0].dayViewModels()[0].shiftTextColor, 'white');
	});

	test('should show absence text name in white when absence background color is dark', function() {
		equal(vm.weekViewModels()[0].dayViewModels()[0].absenceTextColor, 'white');
	});

	test('should show ellipsis when there are multiple absences', function() {
		equal(vm.weekViewModels()[0].dayViewModels()[0].hasMultipleAbsences, true);
	});

	test('should show overtime text name in white when overtime background color is dark', function() {
		equal(vm.weekViewModels()[0].dayViewModels()[1].overtimeTextColor, 'white');
	});

	test('should show ellipsis when there are multiple overtimes', function() {
		equal(vm.weekViewModels()[0].dayViewModels()[1].hasMultipleOvertimes, true);
	});

	test('should navigate to corresponding date after tapping on a date cell', function() {
		vm.weekViewModels()[0]
			.dayViewModels()[0]
			.navigateToDayView();

		equal(hash, 'Schedule/MobileDay/2017/10/01');
	});

	test('should navigate to previous month when swiping right', function() {
		var expectSelectedDate = moment(vm.selectedDate()).add('months', -1);

		$('.mobile-month-view .pagebody')
			.swipe('option')
			.swipeRight();

		equal(vm.selectedDate().format('YYYY-MM-DD'), expectSelectedDate.format('YYYY-MM-DD'));
		$('.mobile-month-view .pagebody').remove();
	});

	test('should navigate to next month when swiping left', function() {
		var expectSelectedDate = moment(vm.selectedDate()).add('months', 1);

		$('.mobile-month-view .pagebody')
			.swipe('option')
			.swipeLeft();

		equal(vm.selectedDate().format('YYYY-MM-DD'), expectSelectedDate.format('YYYY-MM-DD'));
		$('.mobile-month-view .pagebody').remove();
	});

	test('should reload data when schedules change within period', function() {
		fetchMonthDataRequestCount = 0;
		Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener({
			StartDate: 'D2017-10-01T00:00:00',
			EndDate: 'D2017-10-02T00:00:00'
		});

		equal(fetchMonthDataRequestCount, 1);
	});

	test('should not reload data when schedules change outside period', function() {
		fetchMonthDataRequestCount = 0;
		Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener({
			StartDate: 'D2017-10-08T00:00:00',
			EndDate: 'D2017-10-09T00:00:00'
		});

		equal(fetchMonthDataRequestCount, 0);
	});

	test('should reload data in viewing month', function() {
		vm.selectedDate(moment('2017-10-12'));

		fetchMonthDataRequestCount = 0;
		fakeMonthData.ScheduleDays.forEach(function(d) {
			d.Date = moment(d.Date)
				.add(1, 'month')
				.format('YYYY-MM-DDTHH:mm');
			d.FixedDate = moment(d.FixedDate)
				.add(1, 'month')
				.format('YYYY-MM-DD');
		});
		fakeMonthData.CurrentDate = moment(fakeMonthData.CurrentDate)
			.add(1, 'month')
			.format('YYYY-MM-DDTHH:mm');
		fakeMonthData.FixedDate = moment(fakeMonthData.FixedDate)
			.add(1, 'month')
			.format('YYYY-MM-DDTHH:mm');

		vm.nextMonth();

		setTimeout(function() {
			Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener({
				StartDate: 'D2017-11-12T00:00:00',
				EndDate: 'D2017-11-12T00:00:00'
			});
			equal(fetchMonthDataRequestCount, 1);
		}, 500);

		equal(vm.selectedDate().format('YYYY-MM-DD'), '2017-11-12');
	});

	test('should show message icon when asmEnabled', function() {
		equal(vm.asmEnabled(), true);
	});

	test('should show bank holidays in mobile month', function() {
		vm.selectedDate(moment('2017-10-12'));

		equal(fetchMonthDataRequestCount, 1);
		equal($('.mobile-month-view .mobile-month-box .mobile-month-cell').length, 7);
		equal($('.mobile-month-view .mobile-month-box .bank-holiday-day-shape').length, 1);
		equal(
			$('.mobile-month-view .mobile-month-box .mobile-month-cell:nth-child(1) .bank-holiday-day-shape').data(
				'bs.tooltip'
			).options.title,
			'Bank holiday calendar date'
		);
	});

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

	function setup() {
		fetchMonthDataRequestCount = 0;
		initAjax();
		initContext();
		initHtml();

		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow(), ajax);
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);

		vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();
	}

	function initAjax() {
		//Only 7 days data for testing
		fakeMonthData = {
			ScheduleDays: [
				{
					Date: '2017-10-01T00:00:00',
					FixedDate: '2017-10-01',
					Absences: [
						{
							Color: 'rgb(0, 0, 0)',
							IsFullDayAbsence: false,
							Name: 'Permitted',
							ShortName: 'PT'
						},
						{
							Color: 'rgb(20, 20, 100)',
							IsFullDayAbsence: false,
							Name: 'Illness',
							ShortName: 'IL'
						}
					],
					BankHolidayCalendarInfo: {
						CalendarId: '63b6db25-e400-4b28-9473-a9dd000f7961',
						CalendarName: '2017',
						DateDescription: 'Bank holiday calendar date'
					},
					IsDayOff: true,
					Shift: {
						Name: 'Late',
						ShortName: 'LA',
						Color: 'rgb(0, 0,0)',
						TimeSpan: '12:00 PM - 06:00 PM',
						WorkingHours: '0:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-02T00:00:00',
					FixedDate: '2017-10-02',
					Absences: null,
					Overtimes: [
						{
							Name: 'Meeting',
							ShortName: '',
							Color: 'rgb(0,0,255)'
						},
						{
							Name: 'Phone',
							ShortName: '',
							Color: 'rgb(255,0,0)'
						}
					],
					BankHolidayCalendarInfo: null,
					IsDayOff: false,
					Shift: {
						Name: 'Early',
						ShortName: 'AM',
						Color: 'rgb(0, 0,0)',
						TimeSpan: '8:30 AM - 5:30 PM',
						WorkingHours: '8:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-03T00:00:00',
					FixedDate: '2017-10-03',
					Absences: null,
					Overtimes: null,
					BankHolidayCalendarInfo: null,
					IsDayOff: false,
					Shift: {
						Name: 'Early',
						ShortName: 'AM',
						Color: 'rgb(128,255,128)',
						TimeSpan: '8:00 AM - 5:00 PM',
						WorkingHours: '8:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-04T00:00:00',
					FixedDate: '2017-10-04',
					Absences: null,
					Overtimes: null,
					BankHolidayCalendarInfo: null,
					IsDayOff: false,
					Shift: {
						Name: 'Early',
						ShortName: 'AM',
						Color: 'rgb(128,255,128)',
						TimeSpan: '8:00 AM - 5:00 PM',
						WorkingHours: '8:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-05T00:00:00',
					FixedDate: '2017-10-05',
					Absences: null,
					Overtimes: null,
					BankHolidayCalendarInfo: null,
					IsDayOff: false,
					Shift: {
						Name: 'Day',
						ShortName: 'DY',
						Color: 'rgb(255,192,128)',
						TimeSpan: '10:00 AM - 7:00 PM',
						WorkingHours: '8:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-06T00:00:00',
					FixedDate: '2017-10-06',
					Absences: null,
					Overtimes: null,
					BankHolidayCalendarInfo: null,
					IsDayOff: false,
					Shift: {
						Name: 'Day',
						ShortName: 'DY',
						Color: 'rgb(255,192,128)',
						TimeSpan: '10:00 AM - 7:00 PM',
						WorkingHours: '8:00'
					},
					SeatBookings: null
				},
				{
					Date: '2017-10-07T00:00:00',
					FixedDate: '2017-10-07',
					Absences: null,
					Overtimes: null,
					BankHolidayCalendarInfo: null,
					IsDayOff: true,
					Shift: {
						Name: null,
						ShortName: null,
						Color: null,
						TimeSpan: '12:00 PM - 12:00 PM',
						WorkingHours: '0:00'
					},
					SeatBookings: null
				}
			],
			AsmEnabled: true,
			CurrentDate: '2017-10-12T00:00:00',
			FixedDate: '2017-10-12',
			DayHeaders: [
				{
					Name: 'Sunday',
					ShortName: 'Sun'
				},
				{
					Name: 'Monday',
					ShortName: 'Mon'
				},
				{
					Name: 'Tuesday',
					ShortName: 'Tue'
				},
				{
					Name: 'Wednesday',
					ShortName: 'Wed'
				},
				{
					Name: 'Thursday',
					ShortName: 'Thu'
				},
				{
					Name: 'Friday',
					ShortName: 'Fri'
				},
				{
					Name: 'Saturday',
					ShortName: 'Sat'
				}
			]
		};

		fakeUserData = {
			BusinessUnitId: '928dd0bc-bf40-412e-b970-9b5e015aadea',
			DataSourceName: 'Teleopti WFM',
			Url: 'http://localhost:52858/TeleoptiWFM/Web/',
			AgentId: '11610fe4-0130-4568-97de-9b5e015b2564'
		};

		ajax = {
			Ajax: function(options) {
				if (options.url === '../api/Schedule/FetchMobileMonthData') {
					fetchMonthDataRequestCount++;
					if (options.data.date) {
						fakeMonthData.FixedDate = moment(options.data.date).format('YYYY-MM-DD');
					}
					options.success(fakeMonthData);
				}
				if (options.url === '/UserData/FetchUserData') {
					options.success(fakeUserData);
				}
			}
		};
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
				var data = {
					WeekStart: ''
				};
				whenLoadedCallBack(data);
			}
		};
	}

	function initHtml() {
		mobileMonthHtml = [
			'<div class="mobile-month-view">',
			'	<div class="navbar teleopti-toolbar subnavbar">',
			'		<div class="container mobile-month-view-nav">',
			'			<ul class="nav navbar-nav teleopti-toolbar row submenu">',
			'				<li class="mobile-month-datepicker">',
			'					<a class="text-center formatted-date-text" data-bind="datepicker: selectedDate, datepickerOptions: { autoHide: true, viewMode: 1, minViewMode: 1, calendarPlacement: \'right\' }, text: formattedSelectedDate"></a>',
			'					<a class="mobile-month-calendar-icon" data-bind="datepicker: selectedDate, datepickerOptions: { autoHide: true, viewMode: 1, minViewMode: 1, calendarPlacement: \'center\' }">',
			'						<i class="glyphicon glyphicon-calendar"></i>',
			'					</a>',
			'				</li>',
			'				<li>',
			'					<a href="#Schedule/MobileDay">',
			'						<i class="glyphicon glyphicon-home"></i>',
			'					</a>',
			'				</li>',
			'				<li data-bind="visible: asmEnabled()">',
			'					<a href="#MessageTab">',
			'						<i class="glyphicon glyphicon-envelope"></i>',
			'						<span id="MobileDayView-message" class="badge" data-bind="visible: unreadMessageCount() > 0, text: unreadMessageCount"></span>',
			'					</a>',
			'				</li>',
			'				<li>',
			'					<a href="#Requests/Index">',
			'						<i class="glyphicon glyphicon-comment"></i>',
			'					</a>',
			'				</li>',
			'			</ul>',
			'		</div>',
			'	</div>',
			'	<div class="container pagebody">',
			'		<!-- ko if: isLoading -->',
			'		<img class="data-loading-gif" src=\'@Url.Content("~/Areas/MyTime/Content/Images/ajax-loader-small-f8f8f8.gif")\' alt="...">',
			'		<!-- /ko -->',
			'		<!-- ko if: !isLoading() -->',
			'		<div class="mobile-month-row-fluid mobile-month-row-head clearfix" data-bind="foreach: weekDayNames">',
			'			<div class="mobile-month-cell mobile-month-weekday-header weekday-shortname" data-bind="text: ShortName"></div>',
			'		</div>',
			'		<div class="mobile-month-box clearfix" data-bind="foreach: weekViewModels">',
			'			<div class="mobile-month-row-fluid clearfix" data-bind="foreach: dayViewModels, css: {\'has-overtime-or-absence\': $parent.hasAbsenceOrOvertime, \'has-overtime-and-absence\': $parent.hasAbsenceAndOvertime}">',
			'				<div class="mobile-month-cell" data-bind="attr:{\'date\': date}">',
			'					<div class="mobile-month-day" data-bind="click: navigateToDayView, css: {\'mobile-month-day-outmonth\': isOutsideMonth, \'mobile-month-current-day\': isCurrentDay}">',
			'						<!--ko ifnot: bankHoliday -->',
			'						<span data-bind="text: dayOfMonth, click: function(d,e){e.stopPropagation();}" class="day-header-day"></span>',
			'						<!-- /ko -->',
			'						<!--ko if: bankHoliday -->',
			'						<span data-bind="text: dayOfMonth, click: function(d,e){e.stopPropagation();}, tooltip:{title: bankHoliday.dateDescription, html: true}" class="day-header-day bank-holiday-day-shape"></span>',
			'						<!-- /ko -->',
			'						',
			'						<!-- ko if: !hasShift && !isFullDayAbsence && !isDayOff-->',
			'						<div class="shift"></div>',
			'						<!-- /ko -->',
			'						<!-- ko if: hasShift && !isFullDayAbsence -->',
			'						<div class="shift" data-bind="css: {\'is-day-off\': isDayOff}">',
			'							<div data-bind="style: {backgroundColor: shiftColor, color: shiftTextColor}">',
			'								<div data-bind="css: {\'dayoff\': isDayOff}">',
			'									<strong data-bind="text: shiftShortName"></strong>',
			'								</div>',
			'							</div>',
			'							<div class="shift-timespan" data-bind="ifnot: isDayOff">',
			'								<span data-bind="text: shiftStartTime"></span>',
			'								<p> - </p>',
			'								<span data-bind="text: shiftEndTime"></span>',
			'							</div>',
			'							<div class="absence-and-overtime">',
			'								<!-- ko if: hasAbsence -->',
			'								<div class="absence-info-cell" data-bind="style: {backgroundColor: absenceColor, color: absenceTextColor}">',
			'									<strong data-bind="text: absenceShortName"></strong>',
			'									<span data-bind="visible: hasMultipleAbsences">..</span>',
			'								</div>',
			'								<!-- /ko -->',
			'								<!-- ko if: hasOvertime -->',
			'								<div class="overtime-info-cell" data-bind="style: {backgroundColor:overtimeColor, color: overtimeTextColor}">',
			'									<strong>@Resources.OvertimeShortName</strong>',
			'									<span data-bind="visible: hasMultipleOvertimes">..</span>',
			'								</div>',
			'								<!-- /ko -->',
			'							</div>',
			'						</div>',
			'						<!-- /ko -->',
			'						<!-- ko if: isFullDayAbsence -->',
			'						<div class="shift absence">',
			'							<div class="absence-and-overtime">',
			'								<div class="absence-info-cell" data-bind="style: {backgroundColor: absenceColor, color: absenceTextColor}">',
			'									<strong data-bind="text: absenceShortName"></strong>',
			'								</div>',
			'								<!-- ko if: hasOvertime -->',
			'								<div class="overtime-info-cell" data-bind="style: {backgroundColor: overtimeColor, color: overtimeTextColor}">',
			'									<strong>@Resources.OvertimeShortName</strong>',
			'									<span data-bind="visible: hasMultipleOvertimes">..</span>',
			'								</div>',
			'								<!-- /ko -->',
			'							</div>',
			'						</div>',
			'						<!-- /ko -->',
			'					</div>',
			'				</div>',
			'			</div>',
			'		</div>',
			'		<!-- /ko -->',
			'	</div>',
			'</div>'
		].join('');

		mobileMonthHtml = '<div id="page"> ' + mobileMonthHtml + '</div>';
		$('body').append(mobileMonthHtml);
	}
});
