/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js" />

if (typeof (Teleopti) === "undefined") {
    Teleopti = {};
}
if (typeof (Teleopti.MyTimeWeb) === "undefined") {
    Teleopti.MyTimeWeb = {};
}
if (typeof (Teleopti.MyTimeWeb.Schedule) === "undefined") {
    Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileStartDay = (function ($) {
    var vm;
    var completelyLoaded;
    var currentPage = "Teleopti.MyTimeWeb.Schedule";
    var subscribed = false;
    var dataService;

    function cleanBinding() {
        ko.cleanNode($("#page")[0]);
    };

    function subscribeForChanges() {
        Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
            successCallback: Teleopti.MyTimeWeb.Schedule.MobileStartDay.ReloadScheduleListener,
            domainType: "IScheduleChangedInDefaultScenario",
            page: currentPage
        });
        subscribed = true;
    }

    function registerSwipeEvent() {
        $(".dayview-view-body").swipe({
            swipeLeft: function () {
                vm.nextDay();
            },
            swipeRight: function () {
                vm.previousDay();
            },
            allowPageScroll:"none"
        });
    }

    function registerUserInfoLoadedCallback() {
        Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
            $(".moment-datepicker").attr("data-bind",
                "datepicker: selectedDate, datepickerOptions: { calendarPlacement: 'right', autoHide: true, weekStart: " + data.WeekStart + "}");
        });
    }

    function hideAgentScheduleMessenger() {
        $("#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(3)").hide();
        $("#autocollapse.bdd-mytime-top-menu ul.show-outside-toolbar li:nth-child(4)").hide();
    }

    function initViewModel() {
        vm = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
        ko.applyBindings(vm, $("#page")[0]);
    }

    function fetchData() {
        dataService.fetchData(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash,
            vm.selectedProbabilityOptionValue(),
            fetchDataSuccessCallback);
    }

    function fetchDataSuccessCallback(data) {
        vm.readData(data);
        vm.setCurrentDate(moment(data.Date));
        completelyLoaded();
        if (!subscribed) subscribeForChanges();
    }

    return {
        Init: function () {
            if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
                Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack("Schedule/MobileDay",
                    Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit,
                    Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialDispose);
            }
        },
        PartialInit: function (readyForInteractionCallback, completelyLoadedCallback, dataServiceInstance) {
            dataService = dataServiceInstance ||
                new Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService(new Teleopti.MyTimeWeb.Ajax());
            completelyLoaded = completelyLoadedCallback;

            registerUserInfoLoadedCallback();
            registerSwipeEvent();

            hideAgentScheduleMessenger();
            initViewModel();

            fetchData();
            readyForInteractionCallback();
        },
        ReloadScheduleListener: function (notification) {
            var messageDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
            if (vm.isWithinSelected(messageDate, messageDate)) {
                fetchData();
            };
        },
        PartialDispose: function () {
            cleanBinding();
        },
        Vm: function () {
            return vm;
        }
    };
})(jQuery);
