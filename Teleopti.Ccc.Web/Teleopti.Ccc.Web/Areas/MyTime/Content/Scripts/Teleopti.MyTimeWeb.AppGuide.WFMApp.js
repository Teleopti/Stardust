Teleopti.MyTimeWeb.AppGuide = Teleopti.MyTimeWeb.AppGuide || {};
Teleopti.MyTimeWeb.AppGuide.WFMApp = (function ($) {
	var vm;

	function _bindData() {
		ko.applyBindings(vm, $('#page')[0]);
	};

	function _cleanBindings() {
		ko.cleanNode($('#page')[0]);
		if (vm != null)
			vm = null;
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('AppGuide/WFMApp', Teleopti.MyTimeWeb.AppGuide.WFMApp.PartialInit, Teleopti.MyTimeWeb.AppGuide.WFMApp.PartialDispose);
			}
		},
		PartialInit: function (readyForInteraction, completelyLoaded) {
			var customUrl = $("#customUrlForMyTime").text();
			vm = new Teleopti.MyTimeWeb.AppGuide.WFMAppViewModel();
			readyForInteraction();
			completelyLoaded();
			vm.init(customUrl);
			_bindData();
		},
		PartialDispose: function () {
			_cleanBindings();
		}
	};
})(jQuery);