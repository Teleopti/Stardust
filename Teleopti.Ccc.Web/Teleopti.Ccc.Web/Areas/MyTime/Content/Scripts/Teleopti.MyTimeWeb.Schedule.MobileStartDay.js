/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
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
	var ajax;

	function cleanBinding() {
		ko.cleanNode($("#page")[0]);
	};

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileStartDay.ReloadScheduleListener,
			domainType: "IScheduleChangedInDefaultScenario",
			page: currentPage
		});

		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileStartDay.ReloadScheduleListener,
			domainType: "IPushMessageDialogue",
			page: currentPage
		});
		subscribed = true;
	}

	function registPollListener() {
		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_PollToCheckScheduleChanges_46595")) {
			Teleopti.MyTimeWeb.PollScheduleUpdates.SetListener("DayScheduleMobile",
				function (period) {
					var startDate = moment(moment(period.startDate).format('YYYY-MM-DD')).toDate();
					var endDate = moment(moment(period.endDate).format('YYYY-MM-DD')).toDate();
					if (vm.isWithinSelected(startDate, endDate)) {
						fetchData(vm.selectedDate().format('YYYY/MM/DD'));
					}
				});
		}
	}

	function registerSwipeEvent() {
		$(".mobile-start-day-body").swipe({
			swipeLeft: function () {
				vm.nextDay();
			},
			swipeRight: function () {
				vm.previousDay();
			},
			preventDefaultEvents: false,
			threshold: 20
		});
	}

	function registerUserInfoLoadedCallback(initialMomentDate) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function (data) {
			$(".moment-datepicker").attr("data-bind",
				"datepicker: selectedDate, datepickerOptions: { calendarPlacement: 'center', autoHide: true, weekStart: " + data.WeekStart + "}");

			initViewModel(data.WeekStart);

			fetchData(initialMomentDate);
		});
	}

	function initViewModel(weekStart) {
		vm = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel(weekStart, Teleopti.MyTimeWeb.Schedule.MobileStartDay, dataService);
		applyBindings();
	}

	function updateUnreadMessageCount() {
		dataService.fetchMessageCount(fetchUnreadMessageCountCallback);
	}

	function fetchUnreadMessageCountCallback(data) {
		vm.unreadMessageCount(data);
	}

	function fetchData(momentDate) {
		dataService.fetchData((momentDate || vm.selectedDate()).format('YYYY/MM/DD'), vm.selectedProbabilityOptionValue(),fetchDataSuccessCallback);
	}

	function fetchDataSuccessCallback(data) {
		vm.readData(data);
		completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	function applyBindings() {
		ko.applyBindings(vm, $("#page")[0]);
	}

	function setUpLogoClickFn(mywindow) {
		if (Teleopti.MyTimeWeb.Portal.IsMobile(mywindow) && Teleopti.MyTimeWeb.Common.IsToggleEnabled("MyTimeWeb_DayScheduleForStartPage_43446")) {
			var brand = $('a.navbar-brand');
			if (brand.data('events') && brand.data('events')['click'])
				brand.unbind('click');

			brand.attr({
				href: '#Schedule/MobileDay'
			})
				.click(function () {
					if (!vm.selectedDate().isSame(vm.currentUserDate().format('YYYY-MM-DD'), 'day'))
						vm.today();
				});

			$('a[href="#ScheduleTab"]').attr({
				href: '#Schedule/MobileDay',
			});
		}
	}

	return {
		Init: function () {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack("Schedule/MobileDay",
					Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialDispose);
			}
		},
		PartialInit: function (readyForInteractionCallback, completelyLoadedCallback, ajaxobj, mywindow, initialMomentDate) {
			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();
			dataService = new Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService(ajax);
			completelyLoaded = completelyLoadedCallback;
			registerUserInfoLoadedCallback(initialMomentDate);
			registerSwipeEvent();
			mywindow = mywindow || window;
			setUpLogoClickFn(mywindow);
			readyForInteractionCallback();
			registPollListener();
		},
		ReloadScheduleListener: function (notification) {
			if (notification.DomainType === "IScheduleChangedInDefaultScenario") {
				var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
				var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);
				var selectedDate = vm.selectedDate().toDate();
				if (messageStartDate <= selectedDate && messageEndDate >= selectedDate) {
					fetchData();
					return;
				}
			}

			updateUnreadMessageCount();
		},
		PartialDispose: function () {
			cleanBinding();
		},
		Vm: function () {
			return vm;
		},
		ReloadSchedule: function (date) {
			vm.isLoading(true);
			var requestDate = date || vm.selectedDate();
			dataService.fetchData(requestDate.format("YYYY/MM/DD"), vm.selectedProbabilityOptionValue(), vm.readData);
		},
		Ajax: function () { return ajax; }
	};
})(jQuery);