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

Teleopti.MyTimeWeb.Schedule.Month = (function ($) {
	//var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	
	function _bindData(data) {
	    var startDate = new moment('2013-12-30');
	    var selectedDate = startDate.clone().add('days',3);
	    vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel();
	    for (var i = 0; i < 5; i++) {
	        var newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
	        for (var j = 0; j < 7; j++) {
	            var date = startDate.clone();
	            var newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(date,selectedDate);
	            startDate.add('days', 1);
	            newWeek.dayViewModels.push(newDay);
	        }
	        vm.weekViewModels.push(newWeek);
	    }
	    ko.applyBindings(vm, $('#page')[0]);
	}
    
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
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/Month', Teleopti.MyTimeWeb.Schedule.Month.PartialInit, Teleopti.MyTimeWeb.Schedule.Month.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
		    _bindData();
		    readyForInteractionCallback();
		    completelyLoadedCallback();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);
