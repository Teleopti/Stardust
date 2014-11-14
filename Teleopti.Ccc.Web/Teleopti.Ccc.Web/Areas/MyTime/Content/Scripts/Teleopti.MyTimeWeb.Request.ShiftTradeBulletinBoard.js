/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Request) === 'undefined') {
	Teleopti.MyTimeWeb.Request = {};
}

Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoard = (function ($) {
	var vm;

	function _init() {
		var elementToBind = $('#Request-shift-trade-bulletin-board')[0];
		if (elementToBind !== undefined) {
			if ((vm || '') == '') {
				var ajax = new Teleopti.MyTimeWeb.Ajax();
				vm = new Teleopti.MyTimeWeb.Request.ShiftTradeBulletinBoardViewModel(ajax);
				ko.applyBindings(vm, elementToBind);
			}
			//if (vm.subject() != undefined) {
			//	vm.subject("");
			//}
			//if (vm.message() != undefined) {
			//	vm.message("");
			//}
			//vm.chooseAgent(null);
			//_setWeekStart(vm);
			//vm.goToFirstPage();
			//if(vm.isFilterByTimeEnabled) vm.loadFilterTimes();
		}

		$(window).resize(function() {
			//vm.redrawLayers();
		});
	}

	function _setWeekStart(vm) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			//vm.weekStart(data.WeekStart);
		});
	}

	function _openShiftTradeBulletinBoard() {
		Teleopti.MyTimeWeb.Request.RequestDetail.HideNewTextOrAbsenceRequestView();
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.HideShiftTradeWindow();
	    $('#Request-shift-trade-bulletin-board').show();
	}
	
	function setShiftTradeRequestDate(date) {
	    //vm.isReadyLoaded(false);
		//vm.requestedDate(moment(date));
	    //return vm.requestedDate().format('YYYY-MM-DD');
	}

	function _hideShiftTradeBulletinBoard() {
		$('#Request-shift-trade-bulletin-board').hide();
	}

	function _cleanUp() {
		var addShiftTradeBulletinBoardElement = $('#Request-shift-trade-bulletin-board')[0];
		if (addShiftTradeBulletinBoardElement) {
			ko.cleanNode(addShiftTradeBulletinBoardElement);
		}
		vm = null;
	}
	return {
		Init: function () {
			vm = '';
		},
		SetShiftTradeRequestDate: function (date) {
			return setShiftTradeRequestDate(date);
		},
		OpenAddShiftTradeBulletinBoard: function (date) {
			_init();
		    //vm.loadPeriod(date);
		    _openShiftTradeBulletinBoard();
		},
		HideShiftTradeBulletinBoard: function () {
			_hideShiftTradeBulletinBoard();
		},
		Dispose: function () {
			_cleanUp();
		}
	};
})(jQuery);