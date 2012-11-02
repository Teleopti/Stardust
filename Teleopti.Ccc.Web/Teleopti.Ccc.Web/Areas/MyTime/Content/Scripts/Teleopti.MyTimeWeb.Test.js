/// <reference path="~/Content/Scripts/jquery-1.8.2.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Preference.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Test = (function ($) {
	var _settings = {};
	var _messages = [];

	function _pageLog(message) {
		_messages.push(message);
	}

	function _flushPageLog() {
		var page = $('#page');
		for (var i = 0; i < _messages.length; i++) {
			var message = _messages[i];
			page.append(message + "</ br>");
		}
		_messages = [];
	}

	function _expireMyCookie(message) {
		$.ajax({
			url: _settings.startBaseUrl + 'Test/ExpireMyCookie',
			global: false,
			cache: false,
			async: false,
			success: function () {
				_pageLog(message);
			},
			error: function (r) {
				if (r.status == 401 || r.status == 403) {
					_pageLog(message);
				}
			}
		});
	}

	return {
		Init: function (settings) {
			_settings = settings;
		},
		PageLog: function (message) {
			_pageLog(message);
		},
		FlushPageLog: function () {
			_flushPageLog();
		},
		ExpireMyCookie: function (message) {
			_expireMyCookie(message);
		}
	};

})(jQuery);
