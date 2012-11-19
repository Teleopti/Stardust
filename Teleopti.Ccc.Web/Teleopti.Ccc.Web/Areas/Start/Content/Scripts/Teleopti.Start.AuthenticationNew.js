/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Areas/Start/Content/Scripts/Teleopti.Start.Common.js" />
/// <reference path="~/Areas/Start/Content/Scripts/Teleopti.Start.LoginViewModel.js" />


if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};

	if (typeof (Teleopti.Start) === 'undefined') {
		Teleopti.Start = {};
	}
}


Teleopti.Start.AuthenticationNew = (function ($) {

	var _options = {};
	var signinViewModel = null;

	function _initViewModels() {
		signinViewModel = new Teleopti.Start.SignInViewModel({
			baseUrl: _options.baseUrl
		});
		signinViewModel.LoadDataSources();
		ko.applyBindings(signinViewModel, $('#Login-container')[0]);
	}

	function _initSubmit() {
		$('#Login-button').click(function () {
Teleopti.Start.Authentication.RequestPermissionForNotification(function () {
				signinViewModel.Logon();
			});
		});
	}

	return {
		Init: function (options) {
			_options  = options;
			_initViewModels();
			_initSubmit();
		},
		RequestPermissionForNotification: function (callback) {
			if (window.webkitNotifications) {
				window.webkitNotifications.requestPermission(callback);
			} else {
				callback();
			}
		}
	};
})(jQuery);