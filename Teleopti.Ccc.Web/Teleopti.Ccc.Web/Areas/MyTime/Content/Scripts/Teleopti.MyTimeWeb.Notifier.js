Teleopti.MyTimeWeb.Notifier = (function () {
	var originalDocumentTitle;
	var baseUrl;
	var header = '';
	var autoCloseTimeout = false;
	var blinkTitleTimer;
	var webNotification = function () { return true; }; //default also send as web notification if possible
	var webNotifications = new Array();

	function _setOptions(options) {
		if(options && options.originalDocumentTitle)
		originalDocumentTitle = options.originalDocumentTitle;
		if (options && options.baseUrl) {
			baseUrl = options.baseUrl;
		} 
		if (options && options.module) {
			header = options.module;
		}
		if (options && options.webNotification) {
			webNotification = options.webNotification;
		}
		if (options && options.header) {
			header = options.header;
		}
		if (options && options.timeout) {
			autoCloseTimeout = options.timeout;
		}
	}
	function _notify(notifyText) {
		return noty({
			text: notifyText,
			layout: 'bottom',
			closeWith: ['button'],
			timeout: autoCloseTimeout,
			callback: {
				afterClose: _messageClosed
			}
		});
	}
	
	function isShowing(notifyText) {
		var res = webNotifications.filter(function (item) {
			return item.text == notifyText;
		});
		return res.length > 0;
	}

	function _webNotification(notifyText) {
		if (window.webkitNotifications) {
			if (window.webkitNotifications.checkPermission() == 0) { // 0 is PERMISSION_ALLOWED
				if (webNotification() && !isShowing(notifyText)) {
					var timeout = 5000;
					var iconUrl = baseUrl + 'content/favicon.ico';
					var decodedText = $('<div />').html(notifyText).text();
					var notification = window.webkitNotifications.createNotification(iconUrl, header, decodedText);
					notification.show();
					webNotifications.push({ notification: notification, text: notifyText });
					setTimeout(function () {
						for (var i = 0; i < webNotifications.length; i++) {
							if (webNotifications[i].notification == notification) {
								notification.cancel();
								webNotifications.splice(i, 1);
							}
						}

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