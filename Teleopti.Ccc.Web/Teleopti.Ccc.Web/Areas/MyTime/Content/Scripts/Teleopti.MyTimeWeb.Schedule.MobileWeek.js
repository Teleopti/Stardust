/// <reference path="~/Content/jquery/jquery-1.10.2.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />

if (typeof (Teleopti) === 'undefined') {
    Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === 'undefined') {
    Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Schedule) === 'undefined') {
    Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileWeek = (function ($) {
    var ajax = new Teleopti.MyTimeWeb.Ajax();
    var vm;
    var completelyLoaded;
    var _fetchMobileWeekData = function () {
       
    };
    function _cleanBindings() {
        ko.cleanNode($('#page')[0]);
        if (vm != null) {
            vm.weekViewModels([]);
            vm = null;
        }
    }
    
	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/MobileWeek', Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialInit, Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
		    completelyLoaded = completelyLoadedCallback;
		    vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(Teleopti.MyTimeWeb.Portal.NavigateTo);
		    ko.applyBindings(vm, $('#page')[0]);
		    _fetchMobileWeekData();
		    readyForInteractionCallback();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);
