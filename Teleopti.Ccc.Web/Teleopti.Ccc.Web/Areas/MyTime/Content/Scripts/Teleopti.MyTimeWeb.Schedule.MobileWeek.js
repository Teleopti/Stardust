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
    
    var _fetchData = function()
    {
        ajax.Ajax({
            url: 'Schedule/FetchData',
            dataType: "json",
            type: 'GET',
            data: {
                date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
            },
            success: function (data) {
            	vm.readData(data);
	            completelyLoaded();
            }
        });
    };

    return {
        Init: function () {
            if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
                Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/MobileWeek', Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialInit, Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialDispose);
            }
        },
        PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
        	if ($('.weekview-mobile').length > 0) {
		        completelyLoaded = completelyLoadedCallback;
	        	vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel();
	        	ko.applyBindings(vm, $('#page')[0]);
	        	_fetchData();
	        	readyForInteractionCallback();
	        }
        },
        PartialDispose: function () {
        }
    };

})(jQuery);
