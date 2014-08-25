/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="jquery.visible.js" />
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>

if (typeof (Teleopti) === 'undefined') {
	Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Request) === 'undefined') {
	Teleopti.MyTimeWeb.Request = {};
}

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var vm;

	function _init() {
		var elementToBind = $('#Request-add-shift-trade')[0];
		if (elementToBind !== undefined) {
			if ((vm || '') == '') {
				var ajax = new Teleopti.MyTimeWeb.Ajax();
				vm = new Teleopti.MyTimeWeb.Request.ShiftTradeViewModel(ajax);
				vm.featureCheck();
				ko.applyBindings(vm, elementToBind);
			}
			if (vm.subject() != undefined) {
				vm.subject("");
			}
			if (vm.message() != undefined) {
				vm.message("");
			}
			vm.chooseAgent(null);
			_setWeekStart(vm);
			vm.goToFirstPage();
			if(vm.isFilterByTimeEnabled) vm.loadFilterTimes();
		}

		$(window).resize(function() {
			vm.redrawLayers();
		});
	}

	function _setWeekStart(vm) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			vm.weekStart(data.WeekStart);
		});
	}

	function _openAddShiftTradeWindow() {
	    Teleopti.MyTimeWeb.Request.RequestDetail.HideNewTextOrAbsenceRequestView();
	    $('#Request-add-shift-trade').show();
	}
	
	function setShiftTradeRequestDate(date) {
	    vm.isReadyLoaded(false);
		vm.requestedDate(moment(date));
	    return vm.requestedDate().format('YYYY-MM-DD');
	}

	function _hideShiftTradeWindow() {
		$('#Request-add-shift-trade').hide();
	}

	function _cleanUp() {
		var addShiftTradeElement = $('#Request-add-shift-trade')[0];
		if (addShiftTradeElement) {
			ko.cleanNode(addShiftTradeElement);
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
		OpenAddShiftTradeWindow: function (date) {
			if (vm.chooseHistorys != undefined) {
				vm.chooseHistorys.removeAll();
			}
			_init();
		    vm.loadPeriod(date);
		    _openAddShiftTradeWindow();
		},
		HideShiftTradeWindow: function () {
			_hideShiftTradeWindow();
		},
		Dispose: function () {
			_cleanUp();
		}
	};
})(jQuery);