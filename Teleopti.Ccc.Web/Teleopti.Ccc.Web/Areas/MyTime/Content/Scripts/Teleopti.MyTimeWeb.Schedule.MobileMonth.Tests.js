$(document).ready(function() {
	var hash = '',
		ajax,
		fakeMonthData,
		fakeUserData,
		fetchMonthDataRequestCount = 0;

	module('Teleopti.MyTimeWeb.Schedule.MobileMonth', {
		setup: function() {
			setup();
		},
		teardown: function() {
			hash = '';
		}
	});

	test('should use short month name', function() {
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();

		equal(vm.formattedSelectedDate(), moment().format('MMM YYYY'));
	});

	test('should navigate to corresponding date after tapping on a date cell', function() {
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();
		vm.weekViewModels()[0].dayViewModels()[0].navigateToDayView();

		equal(hash, 'Schedule/MobileDay/2017/10/01');
	});

	test('should navigate to previous month when swiping right', function() {
		var html = '<div class="mobile-month-view"><div class="pagebody"></div></div>';
		$('body').append(html);

		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();

		$('.mobile-month-view .pagebody').swipe('option').swipeRight();

		equal(vm.selectedDate().format('YYYY-MM-DD'), '2017-09-12');
		$('.mobile-month-view .pagebody').remove();
	});

	test('should navigate to next month when swiping left', function(){
		var html = '<div class="mobile-month-view"><div class="pagebody"></div></div>';
		$('body').append(html);

		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();

		$('.mobile-month-view .pagebody').swipe('option').swipeLeft();

		equal(vm.selectedDate().format('YYYY-MM-DD'), '2017-11-12');
		$('.mobile-month-view .pagebody').remove();
	});

	test('should reload data when schedules change within period', function(){
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		fetchMonthDataRequestCount = 0;
		Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener({
			StartDate: '2017-10-01T00:00:00',
			EndDate: '2017-10-02T00:00:00'
		});

		equal(fetchMonthDataRequestCount, 1);
	});

	test('should not reload data when schedules change outside period', function(){
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		fetchMonthDataRequestCount = 0;
		Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener({
			StartDate: '2017-10-08T00:00:00',
			EndDate: '2017-10-09T00:00:00'
		});

		equal(fetchMonthDataRequestCount, 0);
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
		Teleopti.MyTimeWeb.Common.UseJalaaliCalendar = false;
		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow(), ajax);
	}

	function initAjax(){
		fakeMonthData = {
			'ScheduleDays': [{
				'Date': '2017-10-01T00:00:00',
				'FixedDate': '2017-10-01',
				'Absences': null,
				'IsDayOff': true,
				'Shift': {
					'Name': null,
					'ShortName': null,
					'Color': null,
					'TimeSpan': '12:00 PM - 12:00 PM',
					'WorkingHours': '0:00'
				},
				'HasOvertime': false,
				'SeatBookings': []
			}, {
				"Date": "2017-10-02T00:00:00",
				"FixedDate": "2017-10-02",
				"Absences": null,
				"IsDayOff": false,
				"Shift": {
					"Name": "Early",
					"ShortName": "AM",
					"Color": "rgb(128,255,128)",
					"TimeSpan": "8:30 AM - 5:30 PM",
					"WorkingHours": "8:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}, {
				"Date": "2017-10-03T00:00:00",
				"FixedDate": "2017-10-03",
				"Absences": null,
				"IsDayOff": false,
				"Shift": {
					"Name": "Early",
					"ShortName": "AM",
					"Color": "rgb(128,255,128)",
					"TimeSpan": "8:00 AM - 5:00 PM",
					"WorkingHours": "8:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}, {
				"Date": "2017-10-04T00:00:00",
				"FixedDate": "2017-10-04",
				"Absences": null,
				"IsDayOff": false,
				"Shift": {
					"Name": "Early",
					"ShortName": "AM",
					"Color": "rgb(128,255,128)",
					"TimeSpan": "8:00 AM - 5:00 PM",
					"WorkingHours": "8:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}, {
				"Date": "2017-10-05T00:00:00",
				"FixedDate": "2017-10-05",
				"Absences": null,
				"IsDayOff": false,
				"Shift": {
					"Name": "Day",
					"ShortName": "DY",
					"Color": "rgb(255,192,128)",
					"TimeSpan": "10:00 AM - 7:00 PM",
					"WorkingHours": "8:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}, {
				"Date": "2017-10-06T00:00:00",
				"FixedDate": "2017-10-06",
				"Absences": null,
				"IsDayOff": false,
				"Shift": {
					"Name": "Day",
					"ShortName": "DY",
					"Color": "rgb(255,192,128)",
					"TimeSpan": "10:00 AM - 7:00 PM",
					"WorkingHours": "8:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}, {
				"Date": "2017-10-07T00:00:00",
				"FixedDate": "2017-10-07",
				"Absences": null,
				"IsDayOff": true,
				"Shift": {
					"Name": null,
					"ShortName": null,
					"Color": null,
					"TimeSpan": "12:00 PM - 12:00 PM",
					"WorkingHours": "0:00"
				},
				"HasOvertime": false,
				"SeatBookings": []
			}],
			'CurrentDate': '2017-10-12T00:00:00',
			'FixedDate': '2017-10-12',
			'DayHeaders': [{
				'Name': 'Sunday',
				'ShortName': 'Sun'
			}, {
				'Name': 'Monday',
				'ShortName': 'Mon'
			}, {
				'Name': 'Tuesday',
				'ShortName': 'Tue'
			}, {
				'Name': 'Wednesday',
				'ShortName': 'Wed'
			}, {
				'Name': 'Thursday',
				'ShortName': 'Thu'
			}, {
				'Name': 'Friday',
				'ShortName': 'Fri'
			}, {
				'Name': 'Saturday',
				'ShortName': 'Sat'
			}]
		};

		fakeUserData = {
			'BusinessUnitId': '928dd0bc-bf40-412e-b970-9b5e015aadea',
			'DataSourceName': 'Teleopti WFM',
			'Url': 'http://localhost:52858/TeleoptiWFM/Web/',
			'AgentId': '11610fe4-0130-4568-97de-9b5e015b2564'
		};

		ajax = {
			Ajax: function(options) {
				if (options.url === '../api/Schedule/FetchMobileMonthData') {
					fetchMonthDataRequestCount++;
					if (options.data.date) {
						fakeMonthData.FixedDate = moment(options.data.date).format("YYYY-MM-DD");
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
		Teleopti.MyTimeWeb.Common.DateTimeDefaultValues = {
			defaultFulldayStartTime: ''
		};
		Teleopti.MyTimeWeb.Common.Init(null, ajax);
	}
});