﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
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

		self.showFabButton = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_MobileResponsive_43826') && Teleopti.MyTimeWeb.Common.IsHostAMobile();

		self.hideFab = ko.observable(false);

		self.addTextRequestActive = ko.observable(false);
		self.addAbsenceRequestActive = ko.observable(false);
		self.addShiftTradeRequestActive = ko.observable(false);
		self.addShiftTradeBulletinBoardActive = ko.observable(false);
		self.addPostShiftForTradeActive = ko.observable(false);
		self.addOvertimeRequestActive = ko.observable(false);
		self.menuIsVisible = ko.observable(false);
		self.addOvertimeRequestEnabled = Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_OvertimeRequest_44558');
		self.overtimeRequestsLicenseAvailable = ko.observable(false);

		checkOvertimeRequestsLicenseAvailability(self);

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
			self.closeDatePickers();
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
			self.closeDatePickers();
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

		self.addOvertimeRequest = function () {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addOvertimeRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddOvertimeRequest();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.resetToolbarActiveButtons = function () {
			self.addTextRequestActive(false);
			self.addAbsenceRequestActive(false);
			self.addShiftTradeRequestActive(false);
			self.addShiftTradeBulletinBoardActive(false);
			self.addPostShiftForTradeActive(false);
			self.addOvertimeRequestActive(false);
		};

		self.closeDatePickers = function () {
			$('.datepicker').hide();
		};
	}

	function _initNavigationViewModel() {
		requestNavigationViewModel = new RequestNavigationViewModel();
		var elementToBind = $('div.navbar-container')[0];
		if (elementToBind) {
			ko.cleanNode(elementToBind);
		}
		ko.applyBindings(requestNavigationViewModel, elementToBind);
	}

	function checkOvertimeRequestsLicenseAvailability(self) {
		var ajax = new Teleopti.MyTimeWeb.Ajax();
		ajax.Ajax({
			url: 'OvertimeRequests/GetLicenseAvailability',
			dataType: "json",
			type: 'GET',
			success: function (response) {
				self.overtimeRequestsLicenseAvailable(response);
			},
			error: function (error) {
				self.overtimeRequestsLicenseAvailable(false);
				throw error;
			}
		});
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
				readyForInteraction && readyForInteraction();
				completelyLoaded && completelyLoaded();
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
			if (requestNavigationViewModel !== null) {
				requestNavigationViewModel.hideFab(show);
				requestNavigationViewModel.menuIsVisible(show);
			}
		},
		MenuIsOpen: function () {
			if (requestNavigationViewModel !== null) {
				return requestNavigationViewModel.menuIsVisible();
			}
		},
		RequestNavigationViewModel: function() {
			return requestNavigationViewModel;
		}
	};

})(jQuery);
