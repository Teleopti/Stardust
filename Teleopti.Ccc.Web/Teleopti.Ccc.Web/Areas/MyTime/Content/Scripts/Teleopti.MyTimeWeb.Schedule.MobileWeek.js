﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
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
    var currentPage = 'Teleopti.MyTimeWeb.Schedule';
    var _subscribed = false;

    var _fetchData = function() {
        ajax.Ajax({
            url: '../api/Schedule/FetchData',
            dataType: "json",
            type: 'GET',
            data: {
                date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash
            },
            success: function (data) {
            	vm.readData(data);
            	vm.setCurrentDate(moment(data.PeriodSelection.Date));
            	vm.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
            	vm.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
            	completelyLoaded();
	            if (!_subscribed) _subscribeForChanges();
            }
        });
    };
	_cleanBinding = function() {
		ko.cleanNode($('#page')[0]);
		if (vm != null) {
			vm.dayViewModels([]);
			vm = null;
		}
	}

	function _subscribeForChanges() {		
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileWeek.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario',
			page: currentPage
		});
		_subscribed = true;
	}

    return {
        Init: function () {
            if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
                Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack('Schedule/MobileWeek', Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialInit, Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialDispose);
            }
        },
        PartialInit: function (readyForInteractionCallback, completelyLoadedCallback) {
        	if ($('.weekview-mobile').length > 0) {
                $('#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(3)').hide();
                $('#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(4)').hide();
                
		        completelyLoaded = completelyLoadedCallback;
		        vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(null, ajax, _fetchData);
	        	ko.applyBindings(vm, $('#page')[0]);
	        	_fetchData();
	        	readyForInteractionCallback();
	        }
        },
        ReloadScheduleListener: function (notification) {

        	var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
        	var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);

        	if (vm.isWithinSelected(messageStartDate, messageEndDate)) {
        		_fetchData();
        	};
        },

        PartialDispose: function () {
	        _cleanBinding();
        }
    };

})(jQuery);
