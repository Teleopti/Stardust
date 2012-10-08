Teleopti.MyTimeWeb.Notifier = (function () {
	var notifyText;
	var timeout = 5000;

	function _setOptions(options) {
		notifyText = options.text;
		if (options.timeout) {
			timeout = options.timeout;
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
				var notification = window.webkitNotifications.createNotification('', notifyText, '');
				notification.show();
				setTimeout(function() {
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