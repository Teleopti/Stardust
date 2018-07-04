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
		ajax;

	var onMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile();
	var oniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad();
	var headerHeight = 50,
		toolbarHeight = 53,
		agentNameRowHeight = 50,
		offsetTopValue = headerHeight + toolbarHeight + agentNameRowHeight,
		agentScheduleColumnWidth = onMobile ? 50 : 80,
		windowHeight = $(window).height();

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
			$('.new-teamschedule-view .moment-datepicker').attr(
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

	function setupFilterClickFn() {
		$('.new-teamschedule-filter').click(function(e) {
			e.preventDefault();
			e.stopPropagation();

			if (Teleopti.MyTimeWeb.Common.IsHostAMobile()) return;

			$('.new-teamschedule-panel').css({
				left:
					$(this).offset().left -
					$('.new-teamschedule-view-nav .navbar-teleopti').offset().left +
					$(this).width() / 2 -
					$('.new-teamschedule-panel').width() / 2
			});
		});

		$('.teamschedule-filter-component').on('mousedown', function(e) {
			e.preventDefault();
			e.stopPropagation();
		});
		$('body').on('mousedown', function(event) {
			if (
				$(event.target).parents('.new-teamschedule-panel').length == 0 &&
				$(event.target).parents('.new-teamschedule-filter').length == 0
			) {
				vm.isPanelVisible(false);
			}
		});
	}

	function registerSwipeEventOnMobileAndiPad() {
		if (!vm.isHostAMobile && !vm.isHostAniPad) return;

		$('.new-teamschedule-table').swipe({
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
		$('.new-teamschedule-table').scroll(function() {
			adjustArrowPositions();
		});
		if (vm.isHostAMobile || vm.isHostAniPad) {
			$('.teammates-schedules-container').scroll(function() {
				$('.teammates-agent-name-row').css({ left: -$(this).scrollLeft() - 1 });

				adjustArrowPositions();
			});
		}
	}

	function adjustArrowPositions() {
		$('.my-schedule-column').each(function(i, e) {
			toggleShiftArrows($(e), true);
		});

		$('.teammates-schedules-column').each(function(i, e) {
			toggleShiftArrows($(e), false, i);
		});
	}

	function toggleShiftArrows(container, underMySchedule, index) {
		var firstLayer = container.find('.new-teamschedule-layer:first');
		if (!firstLayer || firstLayer.length == 0) return;

		var lastLayer = container.find('.new-teamschedule-layer:last');

		var agentNameContainer;
		if (underMySchedule) {
			agentNameContainer = $('.new-teamschedule-agent-name.my-name');
		} else {
			var childIndex = isNaN(index) ? '' : index + 1;
			agentNameContainer = $(
				'.teammates-agent-name-row .new-teamschedule-agent-name:nth-child(' + childIndex + ')'
			);
		}

		var arrowUp = agentNameContainer.find('.teamschedule-arrow-up');
		var arrowDown = agentNameContainer.find('.teamschedule-arrow-down');

		if (firstLayer.offset().top > (onMobile || oniPad ? windowHeight : windowHeight - 5)) {
			arrowDown.show();
		} else {
			arrowDown.hide();
		}

		if (lastLayer.offset().top + lastLayer.height() < offsetTopValue) {
			arrowUp.show();
		} else {
			arrowUp.hide();
		}

		if (
			!underMySchedule &&
			arrowUp.offset().left <
				$('.my-schedule-column.relative').offset().left + $('.my-schedule-column.relative').width()
		) {
			arrowUp.hide();
		}

		if (
			!underMySchedule &&
			arrowDown.offset().left <
				$('.my-schedule-column.relative').offset().left + $('.my-schedule-column.relative').width()
		) {
			arrowDown.hide();
		}
	}

	function showLoadingGif() {
		$('#loading').show();
	}

	function hideLoadingGif() {
		$('#loading').hide();
	}

	function adjustScroll() {
		var container = $('.teammates-schedules-container');
		container.animate(
			{
				scrollLeft: container.scrollLeft() + agentScheduleColumnWidth * 1.5
			},
			100
		);
	}

	function loadMoreSchedules(callback, paging) {
		if (vm.paging.skip + vm.paging.take < vm.paging.take * vm.totalPageNum()) {
			vm.paging.skip += vm.paging.take;

			showLoadingGif();

			dataService.loadScheduleData(
				vm.selectedDate().format('YYYY/MM/DD'),
				paging || vm.paging,
				vm.filter,
				function(schedules) {
					vm.readMoreTeamScheduleData(schedules, callback);

					adjustScroll();
					fetchDataSuccessCallback();
				}
			);
		}
	}

	function initViewModel() {
		vm = new Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel(filterChangedCallback, null);
		applyBindings();
	}

	function filterChangedCallback(dateStr) {
		showLoadingGif();
		dataService.loadScheduleData(dateStr, vm.paging, vm.filter, function(schedules) {
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

				dataService.loadScheduleData(dateStr, vm.paging, vm.filter, function(schedules) {
					vm.readScheduleData(schedules, dateStr);

					focusMySchedule();
					fetchDataSuccessCallback();
				});
			});
		});
	}

	function focusMySchedule() {
		if (!vm.mySchedule().layers[0]) {
			$('.new-teamschedule-table')
				.stop()
				.animate({ scrollTop: 1 });
		} else {
			$('.new-teamschedule-table')
				.stop()
				.animate(
					{
						scrollTop: vm.mySchedule().layers[0].top() - (timeLineOffset + 10)
					},
					0
				);
		}

		$('.teamschedule-arrow-up').hide();
		$('.teamschedule-arrow-down').hide();
		adjustArrowPositions();
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
			if (
				(onMobile && !Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleView_75989')) ||
				(!onMobile && !Teleopti.MyTimeWeb.Common.IsToggleEnabled('MyTimeWeb_NewTeamScheduleViewDesktop_76313'))
			)
				window.location.replace('MyTime#TeamSchedule/Index');

			ajax = ajaxobj || new Teleopti.MyTimeWeb.Ajax();
			dataService = new Teleopti.MyTimeWeb.Schedule.MobileTeamSchedule.DataService(ajax);
			completelyLoaded = completelyLoadedCallback;
			registerUserInfoLoadedCallback(initialMomentDate);
			setupFilterClickFn();
			registerSwipeEventOnMobileAndiPad();
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
