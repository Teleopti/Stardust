/// <reference path="~/Content/Scripts/jquery-1.8.3.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.9.1.custom.js" />
/// <reference path="~/Content/Scripts/jquery-1.8.3-vsdoc.js" />
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

	function _initRequestToolbar() {
		_activateAddRequestMenu();
		_initAddtextAndAbsenceRequestMenuItems();
		_initAddShiftTradeRequestMenuItem();
	}

	function _initAddtextAndAbsenceRequestMenuItems() {
		$('#Requests-addTextRequest-menuItem').add('#Requests-addAbsenceRequest-menuItem')
			.click(function () {
				Teleopti.MyTimeWeb.Request.RequestDetail.AddTextAndAbsenceRequestClick();
			});
	}

	function _initAddShiftTradeRequestMenuItem() {
		$('#Requests-addShiftTradeRequest-menuItem')
			.click(function () {
				Teleopti.MyTimeWeb.Request.AddShiftTradeRequest.OpenAddShiftTradeWindow();
			});
	}

	function _hideAddRequestMenu() {
		$("#Requests-addRequest-dropdown dd ul").hide();
	}

	function _activateAddRequestMenu() {
		$("#Requests-addRequest-dropdown dt span").live("click", function () {
			$("#Requests-addRequest-dropdown dd ul").toggle();
		});

		$("#Requests-addRequest-dropdown dd ul").live("click", function () {
			_hideAddRequestMenu();
		});


		$(document).bind('click', function (e) {
			var $clicked = $(e.target);
			if (!$clicked.parents('#Requests-addRequest-dropdown').length > 0)
				_hideAddRequestMenu();
		});

		$("#Requests-addRequest-dropdown a").hover(function () {
			$(this).addClass('ui-state-hover');
		}, function () {
			$(this).removeClass('ui-state-hover');
		});
	}

	return {
		Init: function () {
			_initRequestToolbar();
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

