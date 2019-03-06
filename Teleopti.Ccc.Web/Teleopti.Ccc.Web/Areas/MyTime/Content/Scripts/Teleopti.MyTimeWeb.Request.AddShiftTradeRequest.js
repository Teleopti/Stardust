Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function($) {
	var vm;
	var vmBulletin;

	function _initShiftTradeRequestView() {
		var elementToBind = $('#Request-add-shift-trade')[0];
		if (elementToBind !== undefined) {
			if ((vm || '') == '') {
				
				var ajax = new Teleopti.MyTimeWeb.Ajax();
				vm = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel(ajax);
				ko.cleanNode(elementToBind);
				ko.applyBindings(vm, elementToBind);
			}
			if (vm.subject() != undefined) {
				vm.subject('');
			}
			if (vm.message() != undefined) {
				vm.message('');
			}
			vm.chooseAgent(null);
			_setWeekStart(vm);
			vm.goToFirstPage();
			vm.loadFilterTimes();
		}

		$(window).bind('resize.addShiftTrade', function() {
			vm && vm.redrawLayers && vm.redrawLayers();
		});
	}

	function _initBulletinBoard() {
		Teleopti.MyTimeWeb.Request.List.HideRequests(true);
		var elementToBind = $('#Request-shift-trade-bulletin-board')[0];
		if (elementToBind !== undefined) {
			if ((vmBulletin || '') == '') {
				var ajax = new Teleopti.MyTimeWeb.Ajax();
				vmBulletin = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel(ajax);
				ko.cleanNode(elementToBind);
				ko.applyBindings(vmBulletin, elementToBind);
			}
			if (vmBulletin.subject() != undefined) {
				vmBulletin.subject('');
			}
			if (vmBulletin.message() != undefined) {
				vmBulletin.message('');
			}
			vmBulletin.chooseAgent(null);
			_setWeekStart(vmBulletin);
			vmBulletin.goToFirstPage();
			vmBulletin.loadFilterTimes();
		}

		$(window).bind('resize.addShiftTradeBullitin', function() {
			vmBulletin.redrawLayers();
		});
	}

	function _setWeekStart(vm) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			vm.weekStart(data.WeekStart);
		});
	}

	function _openAddShiftTradeWindow() {
		Teleopti.MyTimeWeb.Request.RequestDetail.HideNewTextOrAbsenceRequestView();
		_hideShiftTradeBulletinWindow();
		$('#Request-add-shift-trade').show();
	}

	function _openShiftTradeBulletinWindow() {
		Teleopti.MyTimeWeb.Request.RequestDetail.HideNewTextOrAbsenceRequestView();
		_hideShiftTradeWindow();
		$('#Request-shift-trade-bulletin-board').show();
	}

	function setShiftTradeRequestDate(date) {
		vm.isReadyLoaded(false);
		vm.requestedDate(moment(date));
		return vm.requestedDate().format('YYYY-MM-DD');
	}

	function isRunningBehaviorTest() {
		vmBulletin.isRunningBehaviorTest(true);
	}

	function setShiftTradeBulletinBoardDate(date) {
		vmBulletin.isReadyLoaded(false);
		vmBulletin.requestedDate(moment(date));
		return vmBulletin.requestedDate().format('YYYY-MM-DD');
	}

	function _hideShiftTradeWindow() {
		$('#Request-add-shift-trade').hide();
	}

	function _hideShiftTradeBulletinWindow() {
		$('#Request-shift-trade-bulletin-board').hide();
	}

	function _cleanUp() {
		var addShiftTradeElement = $('#Request-add-shift-trade')[0];
		if (addShiftTradeElement) {
			ko.cleanNode(addShiftTradeElement);
		}

		var shiftTradeBulletinElement = $('#Request-shift-trade-bulletin-board')[0];
		if (shiftTradeBulletinElement) {
			ko.cleanNode(shiftTradeBulletinElement);
		}
		vm = null;
		vmBulletin = null;
	}
	return {
		Init: function() {
			vm = '';
			vmBulletin = '';
		},
		SetShiftTradeRequestDate: function(date) {
			return setShiftTradeRequestDate(date);
		},
		IsRunningBehaviorTest: function() {
			isRunningBehaviorTest();
		},
		SetShiftTradeBulletinBoardDate: function(date) {
			return setShiftTradeBulletinBoardDate(date);
		},
		OpenAddShiftTradeWindow: function(date) {
			Teleopti.MyTimeWeb.Request.List.HideRequests(true);
			if (vm.chooseHistorys != undefined) {
				vm.chooseHistorys.removeAll();
			}
			_initShiftTradeRequestView();
			vm.loadPeriod(date);
			_openAddShiftTradeWindow();
		},
		OpenAddShiftTradeBulletinWindow: function(date) {
			_initBulletinBoard();
			vmBulletin.loadPeriod();
			_openShiftTradeBulletinWindow();
		},
		HideShiftTradeWindow: function() {
			_hideShiftTradeWindow();
		},
		HideShiftTradeBulletinBoard: function() {
			_hideShiftTradeBulletinWindow();
		},
		Dispose: function() {
			_cleanUp();
		}
	};
})(jQuery);
