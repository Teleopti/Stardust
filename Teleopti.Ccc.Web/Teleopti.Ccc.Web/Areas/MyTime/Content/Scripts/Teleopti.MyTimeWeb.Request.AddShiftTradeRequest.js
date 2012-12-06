/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.js"/>
/// <reference path="Teleopti.MyTimeWeb.Request.List.js"/>
/// <reference path="~/Content/Scripts/jquery-1.8.2-vsdoc.js" />
/// <reference path="~/Content/Scripts/knockout-2.1.0.js" />

Teleopti.MyTimeWeb.Request.AddShiftTradeRequest = (function ($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;

	function shiftTradeViewModel() {
		var self = this;

		self.hasWorkflowControlSet = ko.observable(false);

		self.loadViewModel = function () {
			ajax.Ajax({
				url: "Requests/ShiftTradeRequest",
				dataType: "json",
				type: 'GET',
				//beforeSend: _loading,
				success: function (data, textStatus, jqXHR) {
					self.hasWorkflowControlSet(!data.HasWorkflowControlSet);
					console.log(!self.hasWorkflowControlSet());
				},
				error: function () {
					console.log('Something went wrong here...');
				}
			});
		};
	}

	function _init() {
		_showContent();
	}

	function _showContent() {
		$('#Request-add-shift-trade-link')
			.click(function () {
				$('#Request-add-shift-trade')
					.show();

				vm = new shiftTradeViewModel();
				var elementToBind = $('#Request-add-shift-trade').get(0);
				ko.applyBindings(vm, elementToBind);
				//ko.applyBindings(vm, document.getElementById('Request-add-shift-trade'));
				vm.loadViewModel();
			})
			;
	}

	return {
		Init: function () {
			_init();
		}
	};

})(jQuery);