Teleopti.MyTimeWeb.Notifier = (function () {
	var notifyText;
	var baseUrl;
	var header = '';
	var webNotification = function () { return true; }; //default also send as web notification if possible

	function _setOptions(options) {
		notifyText = options.notifyText;
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
		if (options.header) {
			header = options.header;
		}
	}
	function _notify() {
		var time = new Date(new Date().getTeleoptiTime()).toString('hh:mm tt');
		return noty({
			text: notifyText + ' (' + time + ')',
			layout: 'bottom',
			closeWith: ['button'],
			callback: {
				afterClose: _messageClosed
			}
		});
	}
	function _webNotification() {
		if (window.webkitNotifications) {
			if (window.webkitNotifications.checkPermission() == 0) { // 0 is PERMISSION_ALLOWED
				if (webNotification()) {
					var timeout = 5000;
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
	function _messageClosed() {
		$.pinify.clearOverlay();
	}

	function _pinnedNotification() {
		$.pinify.flashTaskbar();
		$.pinify.addOverlay({
			//title: 'gurka', //don't know where this is suppose to be shown?
			icon: baseUrl + 'content/favicon.ico'
		});
	}

	return {
		Notify: function (options) {
			_setOptions(options);
			_notify();
			_webNotification();
			_pinnedNotification();
		}
	};
})(jQuery);