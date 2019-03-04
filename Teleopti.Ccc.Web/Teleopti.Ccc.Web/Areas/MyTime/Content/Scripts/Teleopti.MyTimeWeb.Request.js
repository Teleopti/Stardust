Teleopti.MyTimeWeb.Request = (function($) {
	var readyForInteraction = function() {};
	var completelyLoaded = function() {};

	var requestNavigationViewModel = null;

	function RequestNavigationViewModel() {
		var self = this;

		self.showFabButton =
			Teleopti.MyTimeWeb.Common.IsHostAMobile();

		self.hideFab = ko.observable(false);

		self.requestListActive = ko.observable(true);
		self.addTextRequestActive = ko.observable(false);
		self.addAbsenceRequestActive = ko.observable(false);
		self.addShiftTradeRequestActive = ko.observable(false);
		self.addShiftTradeBulletinBoardActive = ko.observable(false);
		self.addPostShiftForTradeActive = ko.observable(false);
		self.addOvertimeRequestActive = ko.observable(false);
		self.menuIsVisible = ko.observable(false);

		self.enableMenu = function(blah, e) {
			self.menuIsVisible(true);
		};

		self.disableMenu = function() {
			self.menuIsVisible(false);
		};

		self.cancelOrSendCallback = function() {
			self.resetToolbarActiveButtons();
			self.requestListActive(true);
		};

		self.clickRequestList = function() {
			self.resetToolbarActiveButtons();
			self.requestListActive(true);
			self.disableMenu();
			Teleopti.MyTimeWeb.Request.RequestDetail.CancelAddingNewRequest();
			$('#Request-add-shift-trade').hide();
			$('#Request-shift-trade-bulletin-board').hide();
		};

		self.addTextRequest = function() {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addTextRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddTextRequestClick();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addAbsenceRequest = function() {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addAbsenceRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddAbsenceRequestClick();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addShiftTradeRequest = function(date, e) {
			if (e) {
				e.stopPropagation();
			}
			self.closeDatePickers();
			self.resetToolbarActiveButtons();
			self.addShiftTradeRequestActive(true);
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.OpenAddShiftTradeWindow(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
			self.disableMenu();
		};

		self.addShiftTradeBulletinBoardRequest = function(date, e) {
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

		self.addPostShiftForTradeRequest = function(date) {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addPostShiftForTradeActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddPostShiftForTradeClick(date);
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.addOvertimeRequest = function() {
			self.hideFab(true);
			self.resetToolbarActiveButtons();
			self.addOvertimeRequestActive(true);
			Teleopti.MyTimeWeb.Request.RequestDetail.AddOvertimeRequest();
			Teleopti.MyTimeWeb.Common.Layout.ActivatePlaceHolder();
		};

		self.resetToolbarActiveButtons = function() {
			self.requestListActive(false);
			self.addTextRequestActive(false);
			self.addAbsenceRequestActive(false);
			self.addShiftTradeRequestActive(false);
			self.addShiftTradeBulletinBoardActive(false);
			self.addPostShiftForTradeActive(false);
			self.addOvertimeRequestActive(false);
		};

		self.closeDatePickers = function() {
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

	return {
		Init: function() {
			Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
				'Requests/Index',
				Teleopti.MyTimeWeb.Request.RequestPartialInit,
				Teleopti.MyTimeWeb.Request.RequestPartialDispose
			);
		},
		RequestPartialInit: function(readyForInteractionCallback, completelyLoadedCallback, ajax) {
			readyForInteraction = readyForInteractionCallback;
			completelyLoaded = completelyLoadedCallback;

			if (!$('#Requests-body-inner').length) {
				readyForInteraction && readyForInteraction();
				completelyLoaded && completelyLoaded();
				return;
			}

			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Init();
			Teleopti.MyTimeWeb.Request.List.Init(readyForInteraction, completelyLoaded, ajax);

			_initNavigationViewModel();
			Teleopti.MyTimeWeb.Request.RequestDetail.Init();
		},
		RequestPartialDispose: function() {
			Teleopti.MyTimeWeb.Request.List.Dispose();
			Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.Dispose();
		},
		ResetToolbarActiveButtons: function () {
			if (requestNavigationViewModel == null) return;
			requestNavigationViewModel.resetToolbarActiveButtons();
		},
		ActiveRequestList: function () {
			if (requestNavigationViewModel == null) return;
			requestNavigationViewModel.requestListActive(true);
		},
		ShiftTradeRequest: function (date) {
			if (requestNavigationViewModel == null) return;
			requestNavigationViewModel.addShiftTradeRequest(date);
		},
		ShiftTradeBulletinBoardRequest: function (date) {
			if (requestNavigationViewModel == null) return;
			requestNavigationViewModel.addShiftTradeBulletinBoardRequest(date);
		},
		PostShiftForTradeRequest: function (date) {
			if (requestNavigationViewModel == null) return;
			requestNavigationViewModel.addPostShiftForTradeRequest(date);
		},
		HideFab: function(show) {
			if (requestNavigationViewModel !== null) {
				requestNavigationViewModel.hideFab(show);
				requestNavigationViewModel.menuIsVisible(show);
			}
		},
		MenuIsOpen: function() {
			if (requestNavigationViewModel !== null) {
				return requestNavigationViewModel.menuIsVisible();
			}
		},
		RequestNavigationViewModel: function() {
			return requestNavigationViewModel;
		}
	};
})(jQuery);
