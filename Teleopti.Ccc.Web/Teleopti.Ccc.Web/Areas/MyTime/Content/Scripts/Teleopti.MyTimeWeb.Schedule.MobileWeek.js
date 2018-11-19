if (typeof Teleopti === 'undefined') {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === 'undefined') {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileWeek = (function($) {
	var ajax = new Teleopti.MyTimeWeb.Ajax();
	var vm;
	var completelyLoaded;
	var currentPage = 'Teleopti.MyTimeWeb.Schedule.MobileWeek';
	var subscribed = false;

	var fetchData = function() {
		ajax.Ajax({
			url: '../api/Schedule/FetchWeekData',
			dataType: 'json',
			type: 'GET',
			data: {
				date: Teleopti.MyTimeWeb.Portal.ParseHash().dateHash,
				staffingPossiblityType: vm.selectedProbabilityOptionValue()
			},
			success: function(data) {
				vm.setCurrentDate(moment(data.PeriodSelection.Date));
				vm.nextWeekDate(moment(data.PeriodSelection.PeriodNavigation.NextPeriod));
				vm.previousWeekDate(moment(data.PeriodSelection.PeriodNavigation.PrevPeriod));
				vm.readData(data);
				completelyLoaded();
				if (!subscribed) subscribeForChanges();
			}
		});
	};

	var cleanBinding = function() {
		ko.cleanNode($('#page')[0]);
		if (vm != null) {
			vm.dayViewModels([]);
			vm = null;
		}
		Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
	};

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker({
			successCallback: Teleopti.MyTimeWeb.Schedule.MobileWeek.ReloadScheduleListener,
			domainType: 'IScheduleChangedInDefaultScenario',
			page: currentPage
		});
		subscribed = true;
	}

	function registPollListener() {
		if (Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_PollToCheckScheduleChanges_46595')) {
			Teleopti.MyTimeWeb.PollScheduleUpdates.SetListener('WeekScheduleMobile', function(period) {
				var startDate = moment(moment(period.startDate).format('YYYY-MM-DD')).toDate();
				var endDate = moment(moment(period.endDate).format('YYYY-MM-DD')).toDate();
				if (vm.isWithinSelected(startDate, endDate)) {
					fetchData();
				}
			});
		}
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
					'Schedule/MobileWeek',
					Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileWeek.PartialDispose
				);
			}
		},
		PartialInit: function(readyForInteractionCallback, completelyLoadedCallback) {
			if ($('.weekview-mobile').length > 0) {
				completelyLoaded = completelyLoadedCallback;
				vm = new Teleopti.MyTimeWeb.Schedule.MobileWeekViewModel(ajax, fetchData);
				ko.applyBindings(vm, $('#page')[0]);
				fetchData();
				readyForInteractionCallback();
				registPollListener();
			}
		},
		ReloadScheduleListener: function(notification) {
			var messageStartDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.StartDate);
			var messageEndDate = Teleopti.MyTimeWeb.MessageBroker.ConvertMbDateTimeToJsDate(notification.EndDate);

			if (vm.isWithinSelected(messageStartDate, messageEndDate)) {
				fetchData();
			}
		},

		PartialDispose: function() {
			cleanBinding();
		}
	};
})(jQuery);
