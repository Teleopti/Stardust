/// <reference path="~/Scripts/jquery-1.5.1.js" />
/// <reference path="~/Scripts/jquery-ui-1.8.11.js" />
/// <reference path="~/Scripts/jquery-1.5.1-vsdoc.js" />
/// <reference path="~/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="~/Scripts/date.js" />
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

			var requestDetailViewModel = new Teleopti.MyTimeWeb.Request.RequestViewModel();
			Teleopti.MyTimeWeb.Request.List.Init(requestDetailViewModel, readyForInteractionCallback, completelyLoadedCallback);
			Teleopti.MyTimeWeb.Request.RequestDetail.Init(requestDetailViewModel);
		}
	};

})(jQuery);

