﻿/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel.js" />
/// <reference path="Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService.js"/>

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
		$(".mobile-start-day-body").swipe({
			swipeLeft: function () {
				vm.nextDay();
			},
			swipeRight: function () {
				vm.previousDay();
			},
			preventDefaultEvents:false
		});
	}

	function registerUserInfoLoadedCallback() {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
			$(".moment-datepicker").attr("data-bind",
				"datepicker: selectedDate, datepickerOptions: { calendarPlacement: 'right', autoHide: true, weekStart: " + data.WeekStart + "}");
		});
	}

	function initViewModel() {
		vm = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel();
		applyBindings();
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

	function applyBindings() {
		ko.applyBindings(vm, $("#page")[0]);
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
			Teleopti.MyTimeWeb.Common.HideAgentScheduleMessenger();

			initViewModel();

			fetchData();

			readyForInteractionCallback();
		},
		ReloadScheduleListener: function (notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);
			var selectedDate = vm.selectedDate().toDate();

			if (messageStartDate <= selectedDate && messageEndDate >= selectedDate) {
				fetchData();
			}
		},
		PartialDispose: function () {
			cleanBinding();
		},
		Vm: function () {
			return vm;
		}
	};
})(jQuery);
