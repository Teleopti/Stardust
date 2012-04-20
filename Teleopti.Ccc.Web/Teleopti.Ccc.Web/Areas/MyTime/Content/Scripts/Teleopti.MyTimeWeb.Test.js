/// <reference path="~/Content/Scripts/jquery-1.6.4.js" />
/// <reference path="~/Content/Scripts/jquery-ui-1.8.16.min.js" />
/// <reference path="~/Content/Scripts/jquery-1.6.4-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/jquery.qtip.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Ajax.js" />


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
			.append(message+ '<br/>')
			;
	}

	function _expireMyCookie() {
		_pageLog("Waiting for requests to complete before expiring my cookie");
		Teleopti.MyTimeWeb.Ajax.WhenAllRequestsCompleted(function () {
			_pageLog("Expireing my cookie!");
			$.ajax({
				url: _settings.startBaseUrl + 'Test/ExpireMyCookie',
				global: false,
				cache: false,
				async: false,
				success: function () {
					_pageLog("Cookie is expired!");
				},
				error: function (r) {
					if (r.status == 401 || r.status == 403) {
						_pageLog("Cookie is expired!");
					}
				}
			});
		});
	}

	return {
		Init: function (settings) {
			_settings = settings;
		},
		ExpireMyCookie: function () {
			_expireMyCookie();
		}
	};

})(jQuery);
