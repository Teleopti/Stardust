$(document).ready(function() {
	var hash = '',
		ajax,
		requestSuccessData,
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
		Teleopti.MyTimeWeb.Common.UseJalaaliCalendar = false;
		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow());
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();

		equal(vm.formattedSelectedDate(), moment().format('MMM YYYY'));
	});

	test('should navigate to corresponding date after tapping on a date cell', function() {
		Teleopti.MyTimeWeb.Common.UseJalaaliCalendar = false;
		Teleopti.MyTimeWeb.Portal.Init(getDefaultSetting(), getFakeWindow());
		Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit(null, null, ajax);
		var vm = Teleopti.MyTimeWeb.Schedule.MobileMonth.Vm();
		vm.weekViewModels()[0].dayViewModels()[0].navigateToDayView();

		equal(hash, 'Schedule/MobileDay/2017/10/01');
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
		initContext();

		var fakeMonthData = {
			"ScheduleDays": [{
				"Date": "2017-10-01T00:00:00",
				"FixedDate": "2017-10-01",
				"Absence": null,
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
			"CurrentDate": "2017-10-12T00:00:00",
			"FixedDate": "2017-10-12",
			"DayHeaders": [{
				"Name": "Sunday",
				"ShortName": "Sun"
			}, {
				"Name": "Monday",
				"ShortName": "Mon"
			}, {
				"Name": "Tuesday",
				"ShortName": "Tue"
			}, {
				"Name": "Wednesday",
				"ShortName": "Wed"
			}, {
				"Name": "Thursday",
				"ShortName": "Thu"
			}, {
				"Name": "Friday",
				"ShortName": "Fri"
			}, {
				"Name": "Saturday",
				"ShortName": "Sat"
			}]
		};

		ajax = {
			Ajax: function (options) {
				if (options.url === "../api/Schedule/FetchMonthData") {
					fetchMonthDataRequestCount++;
					options.success(fakeMonthData);
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