Teleopti.MyTimeWeb.Notifier = (function() {
	var originalDocumentTitle;
	var baseUrl;
	var header = '';
	var autoCloseTimeout = 10000;
	var blinkTitleTimer;
	var webNotification = function() {
		return true;
	}; //default also send as web notification if possible
	var webNotifications = new Array();
	var showingText = null;

	function _setOptions(options) {
		if (options && options.originalDocumentTitle) originalDocumentTitle = options.originalDocumentTitle;
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

	function htmlDecode(notifyText) {
		var div = document.createElement('div');
		div.innerHTML = notifyText;
		return div.childNodes[0].nodeValue;
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
		var res = webNotifications.filter(function(item) {
			return item.text == notifyText;
		});
		return res.length > 0;
	}

	function _webNotification(notifyText) {
		if (window.Notification && Notification.permission !== 'granted') {
			Notification.requestPermission(function(status) {
				if (Notification.permission !== status) {
					Notification.permission = status;
				}
			});
		}
		if (window.Notification && Notification.permission === 'granted') {
			if (webNotification() && !isShowing(notifyText)) {
				var iconUrl = baseUrl + 'WFM/dist/ng2/assets/favicon/favicon.ico?v=2';
				var decodedText = htmlDecode(notifyText);
				var options = {
					body: decodedText,
					icon: iconUrl,
					requireInteraction: true //supported in chrome to keep displaying notification
				};
				var notification = new Notification(header, options);

				webNotifications.push({ notification: notification, text: notifyText });
				setTimeout(closeNotification(notification), autoCloseTimeout);
			}
		}
	}

	function closeNotification(notification) {
		for (var i = 0; i < webNotifications.length; i++) {
			if (webNotifications[i].notification == notification) {
				webNotifications.splice(i, 1);
			}
		}
		return notification.close.bind(notification);
	}

	function _messageClosed() {
		_resetShowingText();
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
		blinkTitleTimer = window.setInterval(function() {
			var decodedTitle = htmlDecode(notifyText);
			top.document.title == decodedTitle
				? (top.document.title = originalDocumentTitle)
				: (top.document.title = decodedTitle);
		}, blinkTimeout);
	}

	function _stopBlinkDocumentTitle() {
		clearInterval(blinkTitleTimer);
		top.document.title = originalDocumentTitle;
	}

	function _resetShowingText() {
		showingText = null;
	}

	return {
		Notify: function(options, notifyText) {
			if (showingText === notifyText) return;
			showingText = notifyText;

			_setOptions(options);
			_notify(notifyText);
			_webNotification(notifyText);
			_pinnedNotification();
			_blinkDocumentTitle(notifyText);
			if (typeof envelopeNotification !== 'undefined') {
				envelopeNotification(htmlDecode(notifyText));
			}
		}
	};
})(jQuery);
