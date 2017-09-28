$(document).ready(function() {
	var hash = '';
	var ajax;
	var requestSuccessData;

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