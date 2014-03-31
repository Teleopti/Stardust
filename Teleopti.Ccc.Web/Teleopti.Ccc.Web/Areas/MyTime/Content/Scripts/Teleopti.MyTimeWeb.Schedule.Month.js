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
	            vm.readData(data);

	            vm.selectedDate.subscribe(function () {
	                var date = vm.selectedDate();
	                date.startOf('month');
	                Teleopti.MyTimeWeb.Portal.NavigateTo("Schedule/Month" + Teleopti.MyTimeWeb.Common.FixedDateToPartsUrl(date.format('YYYY-MM-DD')));
	            });
	            
	            completelyLoaded();
	        }
	    });
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
		    vm = new Teleopti.MyTimeWeb.Schedule.MonthViewModel(Teleopti.MyTimeWeb.Portal.NavigateTo);
		    ko.applyBindings(vm, $('#page')[0]);
		    _fetchMonthData();
		    readyForInteractionCallback();
		},
		PartialDispose: function () {
		    _cleanBindings();
		}
	};

})(jQuery);
