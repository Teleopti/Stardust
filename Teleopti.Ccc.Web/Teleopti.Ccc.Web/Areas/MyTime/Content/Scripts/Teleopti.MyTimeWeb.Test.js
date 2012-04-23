/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
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

	function _pageLog(message) {
		$('#page')
			.append(message + '<br/>')
			;
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

	function _informWhenPreferenceFeedbackIsLoaded(message) {
		if (Teleopti.MyTimeWeb.Preference.FeedbackIsLoaded())
			_pageLog(message);
		else
			setTimeout(function () { _informWhenPreferenceFeedbackIsLoaded(message); }, 100);
	}

	return {
		Init: function (settings) {
			_settings = settings;
		},
		PageLog: function (message) {
			_pageLog(message);
		},
		ExpireMyCookie: function (message) {
			_expireMyCookie(message);
		},
		InformWhenPreferenceFeedbackIsLoaded: function (message) {
			_informWhenPreferenceFeedbackIsLoaded(message);
		}
	};

})(jQuery);
