Teleopti.MyTimeWeb.Notifier = (function () {
	var notifyText;
	var timeout = 5000;
	var baseUrl;
	var header = '';
	var webNotification = function () { return true; }; //default also send as web not if possible

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
		if (options.module) {
			header = options.module;
		}
		if (options.webNotification) {
			webNotification = options.webNotification;
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
				if (webNotification()) {
					var iconUrl = baseUrl + 'content/favicon.ico';
					var notification = window.webkitNotifications.createNotification(iconUrl, header, notifyText);
					notification.show();
					setTimeout(function () {
						notification.cancel();
					}, timeout);
				}
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