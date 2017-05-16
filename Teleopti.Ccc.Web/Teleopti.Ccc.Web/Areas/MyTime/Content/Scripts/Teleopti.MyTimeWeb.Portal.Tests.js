$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Portal");

	test("should navigate to defaultNavigation", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			return false;
		};
		(function () {
			setup();
			var fakeWindow = getFakeWindow();
			init(fakeWindow);
			equal("#Schedule/MobileWeek", fakeWindow.location.url);
		})();
	});

	test("should navigate to mobile day page when toggle 43446 is on", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		(function () {
			setup();
			var fakeWindow = getFakeWindow();
			init(fakeWindow);
			equal("#Schedule/MobileDay", fakeWindow.location.url);
		})();
	});

	test("should navigate to mobile week page when toggle 43446 is on and access from ipad", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		(function () {
			setup();
			var fakeWindow = getFakeWindow();
			fakeWindow.navigator.userAgent = "iPad";
			init(fakeWindow);
			equal("#Schedule/MobileWeek", fakeWindow.location.url);
		})();
	});

	test("should navigate to mobile week page when toggle 43446 is on and access from tablet", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		(function () {
			setup();
			var fakeWindow = getFakeWindow();
			fakeWindow.navigator.userAgent = "Mozilla/5.0 (Linux; U; Android 3.0; en-us; Xoom Build/HRI39) AppleWebKit/534.13 (KHTML, like Gecko) Version/4.0 Safari/534.13";
			init(fakeWindow);
			equal("#Schedule/MobileWeek", fakeWindow.location.url);
		})();
	});


	function setup() {
		this.crossroads = {
			addRoute: function () { }
		};
		this.hasher = {
			initialized: {
				add: function () { }
			},
			changed: {
				add: function () { }
			},
			init: function () { }
		};
	}

	function init(window) {
		var ajax = {
			Ajax: function (options) {
			}
		};

		var setting = getDefaultSetting();
		Teleopti.MyTimeWeb.Portal.Init(setting, window, ajax);
	}

	function getFakeWindow() {
		return {
			location: {
				hash: "#",
				url: "",
				replace: function (newUrl) {
					this.url = newUrl;
				}
			},
			navigator: {
				userAgent: "Mozilla/5.0 (Linux; U; Android 2.3.7; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1"
			}
		};
	}

	function getDefaultSetting() {
		return {
			defaultNavigation: 'Schedule/MobileWeek',
			baseUrl: '/',
			startBaseUrl: '/'
		};
	}
})