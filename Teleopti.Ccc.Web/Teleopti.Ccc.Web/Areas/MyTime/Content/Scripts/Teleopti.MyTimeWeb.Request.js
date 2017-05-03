/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
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

		self.toggle = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_MobileResponsive_43826')
		self.hideFab = ko.observable(false);

		self.addTextRequestActive = ko.observable(false);
		self.addAbsenceRequestActive = ko.observable(false);
		self.addShiftTradeRequestActive = ko.observable(false);
		self.addShiftTradeBulletinBoardActive = ko.observable(false);
		self.addPostShiftForTradeActive = ko.observable(false);
		self.menuIsVisible = ko.observable(false);

		self.enableMenu = function (blah, e) {
			self.menuIsVisible(true);
		};

		self.disableMenu = function () {
			self.menuIsVisible(false);
		};

		self.addTextRequest = function () {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addTextRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddTextRequestClick();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addAbsenceRequest = function () {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addAbsenceRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddAbsenceRequestClick();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addShiftTradeRequest = function (date, e) {
			if (e) {
				e.stopPropagation();
			}
			self.resetToolbarActiveButtons();
			self.addShiftTradeRequestActive(true);
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.OpenAddShiftTradeWindow(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
			self.disableMenu()
		};

		self.addShiftTradeBulletinBoardRequest = function (date, e) {
			if (e) {
				e.stopPropagation();
			}
			self.disableMenu();
			self.resetToolbarActiveButtons();
			self.addShiftTradeBulletinBoardActive(true);
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.OpenAddShiftTradeBulletinWindow(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addPostShiftForTradeRequest = function (date) {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addPostShiftForTradeActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddPostShiftForTradeClick(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.resetToolbarActiveButtons = function () {
			self.addTextRequestActive(false);
			self.addAbsenceRequestActive(false);
			self.addShiftTradeRequestActive(false);
			self.addShiftTradeBulletinBoardActive(false);
			self.addPostShiftForTradeActive(false);
		};
	}

	function _initNavigationViewModel() {
		requestNavigationViewModel = new RequestNavigationViewModel();
		ko.applyBindings(requestNavigationViewModel, $('div.navbar-container')[0]);
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
				readyForInteraction();
				completelyLoaded();
				return;
			}

			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Init();
			Teleopti.MyTimeWeb.Request.List.Init(readyForInteraction, completelyLoaded);

			_initNavigationViewModel();
			Teleopti.MyTimeWeb.Request.RequestDetail.Init();
		},
		RequestPartialDispose: function () {
			Teleopti.MyTimeWeb.Request.List.Dispose();
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Dispose();
		},
		ResetToolbarActiveButtons: function () {
			requestNavigationViewModel.resetToolbarActiveButtons();
		},
		ShiftTradeRequest: function (date) {
			requestNavigationViewModel.addShiftTradeRequest(date);
		},
		ShiftTradeBulletinBoardRequest: function (date) {
			requestNavigationViewModel.addShiftTradeBulletinBoardRequest(date);
		},
		PostShiftForTradeRequest: function (date) {
			requestNavigationViewModel.addPostShiftForTradeRequest(date);
		},
		HideFab: function (show) {
			if (requestNavigationViewModel != null) {
				requestNavigationViewModel.hideFab(show);
				requestNavigationViewModel.menuIsVisible(show);
			}
		},
		MenuIsOpen: function () {
			if (requestNavigationViewModel != null) {
				return requestNavigationViewModel.menuIsVisible();
			}
		}
	};

})(jQuery);
