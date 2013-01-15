/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Content/Scripts/date.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Request = (function ($) {

	return {
		Init: function () {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Requests/Index', Teleopti.MyTimeWeb.Request.RequestPartialInit);
		},
		RequestPartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
			Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();

			var requestDetailViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
			Teleopti.MyTimeWeb.Request.List.Init(requestDetailViewModel, readyForInteractionCallback, completelyLoadedCallback);
			Teleopti.MyTimeWeb.Request.RequestDetail.Init(requestDetailViewModel);
		}
	};

})(jQuery);

