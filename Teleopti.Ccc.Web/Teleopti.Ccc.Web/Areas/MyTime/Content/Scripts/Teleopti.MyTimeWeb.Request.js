/// <reference path="~/Content/Scripts/jquery-1.9.1.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.9.1-vsdoc.js" />
/// <reference path="~/Content/Scripts/MicrosoftMvcAjax.debug.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.RequestDetail.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
	if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
		Teleopti.MyTimeWeb = {};
	}
}

Teleopti.MyTimeWeb.Request = (function ($) {
    var readyForInteraction = function () { };
    var completelyLoaded = function () { };
    
    function _initNavigationViewModel() {
        //Teleopti.MyTimeWeb.Request.RequestDetail.AddTextRequestClick()

        ko.applyBindings({}, $('div.navbar')[1]);
	}

    function _activatePlaceHolderText() {
        $('textarea, :text, :password').placeholder();
    }

	return {
		Init: function () {
		    Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Requests/Index',
		        Teleopti.MyTimeWeb.Request.RequestPartialInit,
		        Teleopti.MyTimeWeb.Request.RequestPartialDispose);
		},
		RequestPartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
		    readyForInteraction = readyForInteractionCallback;
		    completelyLoaded = completelyLoadedCallback;
		    
		    if (!$('#Requests-body-inner').length) {
		        readyForInteraction();
		        completelyLoaded();
		        return;
		    }

			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Init();
			Teleopti.MyTimeWeb.Request.List.Init(readyForInteraction, completelyLoaded);

			_initNavigationViewModel();
			Teleopti.MyTimeWeb.Request.RequestDetail.Init();
		    _activatePlaceHolderText();
		},
	    RequestPartialDispose: function() {
	        Teleopti.MyTimeWeb.Request.List.Dispose();
	    }
	};

})(jQuery);

