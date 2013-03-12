/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
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
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Init();
			Teleopti.MyTimeWeb.Common.Layout.ActivateStdButtons();
			Teleopti.MyTimeWeb.Request.List.Init(readyForInteractionCallback, completelyLoadedCallback);
			Teleopti.MyTimeWeb.Request.RequestDetail.Init();
		}
	};

})(jQuery);

