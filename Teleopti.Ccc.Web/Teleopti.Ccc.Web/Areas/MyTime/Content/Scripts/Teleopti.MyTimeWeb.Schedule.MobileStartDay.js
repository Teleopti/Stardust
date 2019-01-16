Teleopti.MyTimeWeb.Schedule.MobileStartDay = (function($) {
	var vm;
	var completelyLoaded;
	var currentPage = 'Teleopti.MyTimeWeb.Schedule.MobileStartDay';
	var subscribed = false;
	var dataService;
	var ajax;

	function cleanBinding() {
		ko.cleanNode($('#page')[0]);
		Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
	}

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileStartDay.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario',
			page: currentPage
		});

		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileStartDay.ReloadScheduleListener,
			domainType: 'IPushMessageDialogue',
			page: currentPage
		});
		subscribed = true;
	}

	function registPollListener() {
		Teleopti.MyTimeWeb.PollScheduleUpdates.AddListener('DayScheduleMobile', function(period) {
			var startDate = moment(moment(period.startDate).format('YYYY-MM-DD')).toDate();
			var endDate = moment(moment(period.endDate).format('YYYY-MM-DD')).toDate();
			var listeningStartDate = moment(vm.selectedDate())
				.add(-1, 'days')
				.toDate();
			var listeningEndDate = moment(vm.selectedDate())
				.add(1, 'days')
				.toDate();
			if (startDate <= listeningEndDate && endDate >= listeningStartDate) {
				fetchData(vm.selectedDate());
			}
		});
	}

	function registerSwipeEvent() {
		$('.mobile-start-day-body').swipe({
			swipeLeft: function() {
				vm.nextDay();
			},
			swipeRight: function() {
				vm.previousDay();
			},
			preventDefaultEvents: false,
			threshold: 20
		});
	}

	function setUpLogoClick(mywindow) {
		if (Teleopti.MyTimeWeb.Portal.IsMobile(mywindow)) {
			var brand = $('a.navbar-brand');
			if (brand.data('events') && brand.data('events')['click']) brand.unbind('click');

			brand.click(function() {
				if (!vm.selectedDate().isSame(vm.currentUserDate().format('YYYY-MM-DD'), 'day')) vm.today();
			});
		}
	}

	function registerUserInfoLoadedCallback(initialMomentDate) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			$('.moment-datepicker').attr(
				'data-bind',
				"datepicker: selectedDate, datepickerOptions: { calendarPlacement: 'center', autoHide: true, weekStart: " +
					data.WeekStart +
					'}'
			);

			initViewModel(data.WeekStart);

			fetchData(initialMomentDate);
		});
	}

	function initViewModel(weekStart) {
		vm = new Teleopti.MyTimeWeb.Schedule.MobileStartDayViewModel(
			weekStart,
			Teleopti.MyTimeWeb.Schedule.MobileStartDay,
			dataService
		);
		applyBindings();
	}

	function updateUnreadMessageCount() {
		dataService.fetchMessageCount(fetchUnreadMessageCountCallback);
	}

	function fetchUnreadMessageCountCallback(data) {
		vm.unreadMessageCount(data);
	}

	function fetchData(momentDate) {
		vm.isLoading(true);

		var dateStr =
			(momentDate && momentDate.format('YYYY/MM/DD')) ||
			Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ||
			vm.selectedDate().format('YYYY/MM/DD');
		dataService.fetchData(dateStr, vm.selectedProbabilityOptionValue(), fetchDataSuccessCallback);
	}

	function fetchDataSuccessCallback(data) {
		vm.readData(data);
		completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	function applyBindings() {
		ko.applyBindings(vm, $('#page')[0]);
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
					'Schedule/MobileDay',
					Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileStartDay.PartialDispose
				);
			}
		},
		PartialInit: function(
			readyForInteractionCallback,
			completelyLoadedCallback,
			ajaxobj,
			mywindow,
			initialMomentDate
		) {
			if ($('.mobile-start-day').length == 0) return;
			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();
			dataService = new Teleopti.MyTimeWeb.Schedule.MobileStartDay.DataService(ajax);
			completelyLoaded = completelyLoadedCallback;
			registerUserInfoLoadedCallback(initialMomentDate);
			registerSwipeEvent();
			readyForInteractionCallback();
			registPollListener();
			mywindow = mywindow || window;
			setUpLogoClick(mywindow);
		},
		ReloadScheduleListener: function(notification) {
			if (notification.DomainType === 'IScheduleChangedInDefaultScenario') {
				var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(
					notification.StartDate
				);
				var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);
				var selectedDate = vm.selectedDate().toDate();
				if (messageStartDate <= selectedDate && messageEndDate >= selectedDate) {
					fetchData(vm.selectedDate());
					return;
				}
			}

			updateUnreadMessageCount();
		},
		PartialDispose: function() {
			cleanBinding();
		},
		Vm: function() {
			return vm;
		},
		ReloadSchedule: function(date, forceReloadProbabilityData) {
			vm.isLoading(true);
			var requestDate = date || vm.selectedDate();
			dataService.fetchData(requestDate.format('YYYY/MM/DD'), vm.selectedProbabilityOptionValue(), function(
				data
			) {
				vm.readData(data, forceReloadProbabilityData);
			});
		},
		Ajax: function() {
			return ajax;
		}
	};
})(jQuery);
