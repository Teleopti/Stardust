/// <reference path="~/Content/jquery/jquery-1.12.4.js" />
/// <reference path="~/Content/jqueryui/jquery-ui-1.10.2.custom.js" />
/// <reference path="~/Content/Scripts/knockout-2.2.1.js"/>
/// <reference path="Teleopti.MyTimeWeb.Common.js"/>
/// <reference path="Teleopti.MyTimeWeb.Portal.js"/>
/// <reference path="Teleopti.MyTimeWeb.Ajax.js"/>
/// <reference path="~/Content/moment/moment.js" />
/// <reference path="~/Areas/MyTime/Content/Scripts/Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel.js" />

if (typeof Teleopti === 'undefined') {
	Teleopti = {};
}
if (typeof Teleopti.MyTimeWeb === 'undefined') {
	Teleopti.MyTimeWeb = {};
}
if (typeof Teleopti.MyTimeWeb.Schedule === 'undefined') {
	Teleopti.MyTimeWeb.Schedule = {};
}

Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule = (function($) {
	var vm,
		completelyLoaded,
		currentPage = 'Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule',
		subscribed = false,
		dataService,
		timeLineOffset = 50,
		agentScheduleColumnWidth = 50,
		ajax;

	function cleanBinding() {
		ko.cleanNode($('#page')[0]);
		Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
	}

	function subscribeForChanges() {
		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker(
			{
				successCallback: Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.ReloadScheduleListener,
				domainType: 'IScheduleChangedInDefaultScenario',
				page: currentPage
			},
			ajax
		);

		Teleopti.MyTimeWeb.Common.SubscribeToMessageBroker(
			{
				successCallback: Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.ReloadScheduleListener,
				domainType: 'IPushMessageDialogue',
				page: currentPage
			},
			ajax
		);
		subscribed = true;
	}

	function registerUserInfoLoadedCallback(initialMomentDate) {
		Teleopti.MyTimeWeb.UserInfo.WhenLoaded(function(data) {
			$('.mobile-teamschedule-view .moment-datepicker').attr(
				'data-bind',
				"datepicker: selectedDate, datepickerOptions: { calendarPlacement: 'center', autoHide: true, weekStart: " +
					data.WeekStart +
					'}, ' +
					'text: displayDate'
			);
			initViewModel();
			fetchData(initialMomentDate);
		});
	}

	function registerSwipeEvent() {
		$('.mobile-teamschedule-view-body').swipe({
			swipeLeft: function() {
				var ele = $('.teammates-schedules-container');
				if (
					(vm.paging.skip + vm.paging.take) * agentScheduleColumnWidth - (ele.scrollLeft() + ele.width()) <=
						agentScheduleColumnWidth - 10 &&
					vm.currentPageNum() < vm.totalPageNum()
				) {
					showLoadingGif();
					loadMoreSchedules();
				}
			},
			preventDefaultEvents: false,
			threshold: 20
		});
	}

	function registerScrollEvent() {
		$('.teammates-schedules-container').scroll(function() {
			setAgentNameCellPosition();
		});
	}

	function setAgentNameCellPosition() {
		$('.agent-name').each(function(i, e) {
			$(e).css({
				left: $(e)
					.parent()
					.offset().left
			});
		});
	}

	function showLoadingGif() {
		$('#loading').show();
	}

	function hideLoadingGif() {
		$('#loading').hide();
	}

	function adjustScroll() {
		var container = $('.teammates-schedules-container');
		container.animate({
			scrollLeft: container.scrollLeft() + agentScheduleColumnWidth
		});
	}

	function loadMoreSchedules() {
		if (vm.paging.skip + vm.paging.take < vm.paging.take * vm.totalPageNum()) {
			vm.paging.skip += vm.paging.take;

			dataService.loadScheduleData(
				vm.selectedDate().format('YYYY/MM/DD'),
				vm.selectedTeamIds,
				vm.paging,
				vm.filter,
				function(schedules) {
					vm.readMoreTeamScheduleData(schedules);

					adjustScroll();
					fetchDataSuccessCallback();
				}
			);
		}
	}

	function initViewModel() {
		vm = new Teleopti.MyTimeWeb.Schedule.MobileTeamScheduleViewModel(filterChangedCallback);
		applyBindings();
	}

	function filterChangedCallback(dateStr) {
		dataService.loadScheduleData(dateStr, vm.selectedTeamIds, vm.paging, vm.filter, function(schedules) {
			vm.readScheduleData(schedules, moment(dateStr));

			focusMySchedule();
			fetchDataSuccessCallback();
		});
	}

	function applyBindings() {
		ko.applyBindings(vm, $('#page')[0]);
	}

	function fetchData(momentDate) {
		showLoadingGif();
		dataService.loadGroupAndTeams(function(teams) {
			vm.readTeamsData(teams);

			dataService.loadDefaultTeam(function(defaultTeam) {
				vm.readDefaultTeamData(defaultTeam);

				var dateStr =
					(momentDate && momentDate.format('YYYY/MM/DD')) ||
					Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ||
					vm.selectedDate().format('YYYY/MM/DD');

				dataService.loadScheduleData(dateStr, vm.selectedTeamIds, vm.paging, vm.filter, function(schedules) {
					vm.readScheduleData(schedules, dateStr);

					focusMySchedule();
					fetchDataSuccessCallback();
				});
			});
		});
	}

	function focusMySchedule() {
		if (!vm.mySchedule().layers[0]) {
			$('.mobile-teamschedule-view-body')
				.stop()
				.animate({ scrollTop: 1 });
		} else {
			$('.mobile-teamschedule-view-body')
				.stop()
				.animate(
					{
						scrollTop: vm.mySchedule().layers[0].top() - (timeLineOffset + 10)
					},
					0
				);
		}
		setAgentNameCellPosition();
	}

	function fetchDataSuccessCallback() {
		hideLoadingGif();
		completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	return {
		Init: function() {
			if ($.isFunction(Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack)) {
				Teleopti.MyTimeWeb.Portal.RegisterPartialCallBack(
					'TeamSchedule/NewIndex',
					Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.PartialInit,
					Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.PartialDispose
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
			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();
			dataService = new Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.DataService(ajax);
			completelyLoaded = completelyLoadedCallback;
			registerUserInfoLoadedCallback(initialMomentDate);
			registerSwipeEvent();
			registerScrollEvent();
			readyForInteractionCallback();
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
		},
		PartialDispose: function() {
			cleanBinding();
		},
		Vm: function() {
			return vm;
		}
	};
})(jQuery);
