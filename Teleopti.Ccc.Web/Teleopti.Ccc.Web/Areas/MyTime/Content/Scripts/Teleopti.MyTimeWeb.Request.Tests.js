$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Request", {
		setup: function () {
			$('body').append('<div id="Requests-body-inner">"#Requests-body-inner" For testing</div>');
			setUpFunctionsBeforeRun();
		},
		teardown: function () {
			$('#Requests-body-inner').remove();
			restoreFuntionsAfterRun();
		}
	});

	var tempToggleFn,
		tempList,
		tempAddShiftTradeRequest;

	function setUpFunctionsBeforeRun() {
		tempToggleFn = Teleopti.MyTimeWeb.Common.IsToggleEnabled;

		tempList = Teleopti.MyTimeWeb.Request.List;
		Teleopti.MyTimeWeb.Request.List = {
			Init: function () { }
		};

		tempAddShiftTradeRequest = Teleopti.MyTimeWeb.Request.AddShiftTradeRequest;
		Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = {
			Init: function () { }
		};
	}

	function restoreFuntionsAfterRun() {
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = tempToggleFn;
		Teleopti.MyTimeWeb.Request.List = tempList;
		Teleopti.MyTimeWeb.Request.tempAddShiftTradeRequest = tempAddShiftTradeRequest;
	}
});