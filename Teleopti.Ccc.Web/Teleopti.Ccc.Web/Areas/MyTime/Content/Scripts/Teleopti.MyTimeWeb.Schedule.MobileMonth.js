if (typeof Teleopti === 'undefined') {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === 'undefined') {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileMonth = (function($) {
	var vm,
		completelyLoaded,
		subscribed = false,
		dataService,
		ajax,
		currentPage = 'Teleopti.MyTimeWeb.Schedule.MobileMonth';

	function cleanBinding() {
		ko.cleanNode($('#page')[0]);
		Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
	}

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileMonth.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario',
			page: currentPage
		});
		subscribed = true;
	}

	function registPollListener() {
		Teleopti.MyTimeWeb.PollScheduleUpdates.SetListener('MonthScheduleMobile', function(period) {
			var startDate = moment(moment(period.startDate).format('YYYY-MM-DD')).toDate();
			var endDate = moment(moment(period.endDate).format('YYYY-MM-DD')).toDate();
			if (vm.isWithinSelected(startDate, endDate)) {
				fetchData(vm.selectedDate().format('YYYY/MM/DD'));
			}
		});
	}

	function registerSwipeEvent() {
		$('.mobile-month-view .pagebody').swipe({
			swipeLeft: function() {
				vm.nextMonth();
			},
			swipeRight: function() {
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

	function fetchData(date) {
		dataService.fetchData(date || Teleopti.MyTimeWeb.Portal.ParseHash().dateHash, fetchDataSuccessCallback);
	}

	function fetchDataSuccessCallback(data) {
		vm.readData(data);
		completelyLoaded && completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	function applyBindings() {
		ko.applyBindings(vm, $('#page')[0]);
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
					'Schedule/MobileMonth',
					Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileMonth.PartialDispose
				);
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
			registPollListener();
		},
		ReloadScheduleListener: function(notification) {
			if (
				vm.isWithinSelected(
					Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate),
					Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate)
				)
			) {
				fetchData(vm.selectedDate().format('YYYY/MM/DD'));
			}
		},
		PartialDispose: function() {
			cleanBinding();
		},
		Vm: function() {
			return vm;
		},
		ReloadSchedule: function(date) {
			var requestDate = date || vm.selectedDate();
			dataService.fetchData(requestDate.format('YYYY/MM/DD'), function(data) {
				vm.readData(data);
			});
		},
		Ajax: function() {
			return ajax;
		}
	};
})(jQuery);
