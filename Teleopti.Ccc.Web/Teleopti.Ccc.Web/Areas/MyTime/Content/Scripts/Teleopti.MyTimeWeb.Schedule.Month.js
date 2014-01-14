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
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	
	function _fetchMonthData() {
	    ajax.Ajax({
	        url: 'Schedule/FetchMonthData',
	        dataType: "json",
	        type: 'GET',
	        data: {
	            date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
	        },
	        success: function (data) {
	            _bindData(data);
	        }
	    });
	}
    
	function _bindData(data) {
	    var selectedDate = moment(data.FixedDate, 'YYYY-MM-DD');
	    vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel(data, selectedDate);

	    var newWeek;
	    for (var i = 0; i < data.ScheduleDays.length; i++) {
	        if (i % 7 == 0) {
	            if (newWeek)
	                vm.weekViewModels.push(newWeek);
	            newWeek = new Teleopti.MyTimeWeb.Schedule.MonthWeekViewModel();
	        }

	        var newDay = new Teleopti.MyTimeWeb.Schedule.MonthDayViewModel(data.ScheduleDays[i], selectedDate);
	        newWeek.dayViewModels.push(newDay);
	    }
	    vm.weekViewModels.push(newWeek);
	    ko.applyBindings(vm, $('#page')[0]);
	    completelyLoaded();
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
		    completelyLoaded = completelyLoadedCallback;
		    _fetchMonthData();
		    readyForInteractionCallback();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);
