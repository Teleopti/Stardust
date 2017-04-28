$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Portal");

	test("should navigate to defaultNavigation", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			return false;
		};
		(function () {
			setup();
			var fakeWindow = getFakeWindow();
			var setting = getDefaultSetting();
			Teleopti.MyTimeWeb.Portal.Init(setting, fakeWindow);
			equal("#Schedule/MobileWeek", fakeWindow.location.url);
		})();
	});

	test("should navigate to mobile day page when toggle 43446 is on", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		(function() {
			setup();
			var fakeWindow = getFakeWindow();
			var setting = getDefaultSetting();
			Teleopti.MyTimeWeb.Portal.Init(setting, fakeWindow);
			equal("#Schedule/MobileDay", fakeWindow.location.url);
		})();
    });

	test("should navigate to mobile day page when toggle 43446 is on and access from ipad", function () {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) {
			if (x === "MyTimeWeb_DayScheduleForStartPage_43446") return true;
			return false;
		};

		(function () {
			setup();
            var fakeWindow = getFakeWindow();
			fakeWindow.navigator.userAgent = "iPad";
			var setting = getDefaultSetting();
			Teleopti.MyTimeWeb.Portal.Init(setting, fakeWindow);
			equal("#Schedule/MobileDay", fakeWindow.location.url);
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
                userAgent: "Android"
			}
		};
	}

	function getDefaultSetting() {
		return {
			defaultNavigation: '/',
			baseUrl: '/',
			startBaseUrl: '/'
		};
	}
})