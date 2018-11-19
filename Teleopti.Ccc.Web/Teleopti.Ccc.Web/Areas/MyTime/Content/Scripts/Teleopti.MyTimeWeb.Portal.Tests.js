$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Portal', {
		setup: function() {},
		teardown: function() {}
	});

	test('should navigate to defaultNavigation', function() {
		setup();
		var fakeWindow = getFakeWindow();
		init(fakeWindow);
		equal('', fakeWindow.location.url);
	});

	test('should navigate to mobile day page', function() {
		setup();
		var fakeWindow = getFakeWindow();
		init(fakeWindow);
		equal('#Schedule/MobileDay', fakeWindow.location.url);
	});

	function setup(fakeWindow) {
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
			setHash: function(hash) {
				if (fakeWindow) fakeWindow.location.url = hash;
			},
			init: function() {}
		};
	}

	function init(window) {
		var ajax = {
			Ajax: function(options) {}
		};

		var setting = getDefaultSetting();
		Teleopti.MyTimeWeb.Portal.Init(setting, window, ajax);
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
				userAgent:
					'Mozilla/5.0 (Linux; U; Android 2.3.7; en-us; Nexus One Build/FRF91) AppleWebKit/533.1 (KHTML, like Gecko) Version/4.0 Mobile Safari/533.1'
			}
		};
	}

	function getDefaultSetting() {
		return {
			defaultNavigation: 'Schedule',
			baseUrl: '/',
			startBaseUrl: '/'
		};
	}
});
