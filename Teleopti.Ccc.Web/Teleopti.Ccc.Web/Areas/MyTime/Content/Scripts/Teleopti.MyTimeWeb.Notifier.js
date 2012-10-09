Teleopti.MyTimeWeb.Notifier = (function () {
	var notifyText;
	var timeout = 5000;
	var baseUrl;

	function _setOptions(options) {
		notifyText = options.notifyText;
		if (options.timeout) {
			timeout = options.timeout;
		}
		if (options.baseUrl) {
			baseUrl = options.baseUrl;
		} else {
			console.warning('Missing base url for icon in notification!');
		}
	}
	function _notify() {
		return noty({
			text: notifyText,
			layout: 'bottom',
			timeout: timeout,
			animation: {
				open: { height: 'toggle' },
				close: { height: 'toggle' },
				easing: 'swing',
				speed: 500 // opening & closing animation speed
			}
		});
	}
	function _webNotification() {
		if (window.webkitNotifications) {
			if (window.webkitNotifications.checkPermission() == 0) { // 0 is PERMISSION_ALLOWED
				var iconUrl = baseUrl + 'content/favicon.ico';
				var notification = window.webkitNotifications.createNotification(iconUrl, notifyText, '');
				notification.show();
				setTimeout(function () {
					notification.cancel();
				}, timeout);
			} else {
				window.webkitNotifications.requestPermission();
			}
		}
	}

	return {
		Notify: function (options) {
			_setOptions(options);
			_notify();
			_webNotification();
		}
	};
})(jQuery);