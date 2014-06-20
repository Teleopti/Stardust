/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
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

    var requestNavigationViewModel = null;
    
    function RequestNavigationViewModel() {
        var self = this;

        self.addTextRequestActive = ko.observable(false);
        self.addAbsenceRequestActive = ko.observable(false);
        self.addShiftTradeRequestActive = ko.observable(false);

        self.addTextRequest = function () {
            self.resetToolbarActiveButtons();
            self.addTextRequestActive(true);
            Teleopti.MyTimeWeb.Request.RequestDetail.AddTextRequestClick();
            Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
        };
        self.addAbsenceRequest = function () {
            self.resetToolbarActiveButtons();
            self.addAbsenceRequestActive(true);
            Teleopti.MyTimeWeb.Request.RequestDetail.AddAbsenceRequestClick();
            Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
        };
        self.addShiftTradeRequest = function (date) {

        	//var rightNow = new Date();
	        //var res = rightNow.toISOString().slice(0, 10).replace(/-/g, "");
        	//portal.NavigateTo("Requests/Index/ShiftTrade/", res);
	        self.resetToolbarActiveButtons();
			self.addShiftTradeRequestActive(true);
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.OpenAddShiftTradeWindow(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
        };

        self.resetToolbarActiveButtons = function() {
            self.addTextRequestActive(false);
            self.addAbsenceRequestActive(false);
            self.addShiftTradeRequestActive(false);
        };
    }

    function _initNavigationViewModel() {
    		requestNavigationViewModel = new RequestNavigationViewModel();
    		ko.applyBindings(requestNavigationViewModel, $('div.navbar')[1]);
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
		},
	    RequestPartialDispose: function() {
	    	Teleopti.MyTimeWeb.Request.List.Dispose();
		    Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Dispose();
	    },
	    ResetToolbarActiveButtons: function () {
	        requestNavigationViewModel.resetToolbarActiveButtons();
	    },
	    ShiftTradeRequest: function (date) {
		    	requestNavigationViewModel.addShiftTradeRequest(date);
	    }
	};

})(jQuery);

