/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel.js" />

if (typeof(Teleopti) === "undefined") {
	Teleopti = {};
}
if (typeof(Teleopti.MyTimeWeb) === "undefined") {
	Teleopti.MyTimeWeb = {};
}
if (typeof(Teleopti.MyTimeWeb.Schedule) === "undefined") {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileMonth = (function($) {
	var vm,
		completelyLoaded,
		subscribed = false,
		dataService,
		ajax;

	function cleanBinding() {
		ko.cleanNode($("#page")[0]);
	};

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener,
			domainType: "IScheduleChangedInDefaultScenario",
			page: "Teleopti.MyTimeWeb.Schedule"
		});
		subscribed = true;
	}

	function registerSwipeEvent() {
		$(".mobile-month-view .pagebody").swipe({
			swipeLeft: function () {
				vm.nextMonth();
			},
			swipeRight: function () {
				vm.previousMonth();
			},
			preventDefaultEvents: false,
			threshold: 20
		});
	}

	function initViewModel() {
		vm = new Teleopti.MyTimeWeb.Schedule.MobileMonthViewModel(Teleopti.MyTimeWeb.Schedule.MobileMonth);
		applyBindings();
	}

	function fetchData() {
		dataService.fetchData(Teleopti.MyTimeWeb.Portal.ParseHash().dateHash,
			fetchDataSuccessCallback);
	}

	function fetchDataSuccessCallback(data) {
		vm.readData(data);
		completelyLoaded && completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	function applyBindings() {
		ko.applyBindings(vm, $("#page")[0]);
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack("Schedule/MobileMonth",
					Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialDispose);
			}
		},
		PartialInit: function(readyForInteractionCallback, completelyLoadedCallback, ajaxobj) {
			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();
			dataService = new Teleopti.MyTimeWeb.Schedule.MobileMonth.DataService(ajax);
			completelyLoaded = completelyLoadedCallback;
			initViewModel();
			fetchData();

			registerSwipeEvent();
			readyForInteractionCallback && readyForInteractionCallback();
		},
		ReloadScheduleListener: function(notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);
			var selectedDate = vm.selectedDate().toDate();

			var weekViewModels = vm.weekViewModels();
			var periodStartDateMoment = moment(weekViewModels[0].dayViewModels()[0].date);
			var periodEndDateMoment = moment(weekViewModels[weekViewModels.length -1].dayViewModels()[6].date);
			if (periodStartDateMoment <= moment(messageStartDate) && moment(messageEndDate) <= periodEndDateMoment) {
				fetchData();
			}
		},
		PartialDispose: function() {
			cleanBinding();
		},
		Vm: function() {
			return vm;
		},
		ReloadSchedule: function(date) {
			vm.isLoading(true);
			var requestDate = date || vm.selectedDate();
			dataService.fetchData(requestDate.format("YYYY/MM/DD"),
				function (data) {
					vm.readData(data);
				});
		},
		Ajax: function() {
			return ajax;
		}
	};
})(jQuery);