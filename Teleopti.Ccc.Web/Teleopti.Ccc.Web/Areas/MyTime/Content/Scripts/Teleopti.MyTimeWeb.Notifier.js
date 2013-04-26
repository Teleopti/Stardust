Teleopti.MyTimeWeb.Notifier = (function () {
	var originalDocumentTitle;
	var baseUrl;
	var header = '';
	var blinkTitleTimer;
	var webNotification = function () { return true; }; //default also send as web notification if possible

	function _setOptions(options) {
		originalDocumentTitle = options.originalDocumentTitle;
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
	function _notify(notifyText) {
		return noty({
			text: notifyText,
			layout: 'bottom',
			closeWith: ['button'],
			callback: {
				afterClose: _messageClosed
			}
		});
	}
	function _webNotification(notifyText) {
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
		_stopBlinkDocumentTitle();
	}

	function _pinnedNotification() {
		$.pinify.flashTaskbar();
	}

	function _blinkDocumentTitle(notifyText) {
		var blinkTimeout = 750;
		if (blinkTitleTimer) {
			clearInterval(blinkTitleTimer);
		}
		blinkTitleTimer = window.setInterval(function () {
			var decodedTitle = $('<div/>').html(notifyText).text();
			top.document.title == decodedTitle ?
										top.document.title = originalDocumentTitle :
										top.document.title = decodedTitle;
		}, blinkTimeout);
	}

	function _stopBlinkDocumentTitle() {
		clearInterval(blinkTitleTimer);
		top.document.title = originalDocumentTitle;
	}

	return {
		Notify: function (options, notifyText) {
			_setOptions(options);
			_notify(notifyText);
			_webNotification(notifyText);
			_pinnedNotification();
			_blinkDocumentTitle(notifyText);
		}
	};
})(jQuery);