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
		currentPage = 'Teleopti.MyTimeWeb.Schedule.NewTeamSchedule',
		subscribed = false,
		dataService,
		timeLineOffset = 50,
		ajax,
		onMobile = Teleopti.MyTimeWeb.Common.IsHostAMobile(),
		oniPad = Teleopti.MyTimeWeb.Common.IsHostAniPad(),
		agentScheduleColumnWidth = onMobile ? 50 : 80,
		minScrollBlockWidth = 60;

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
			loadDefaultTeam(function() {
				filterChangedCallback(getDateStr(initialMomentDate));
			});
		});
	}

	function setupFilterClickFn() {
		$('.new-teamschedule-team-filter').click(function(e) {
			if (!vm.isTeamsAndGroupsLoaded()) {
				loadGroupAndTeams(function() {
					vm.isTeamsAndGroupsLoaded(true);

					vm.isPanelVisible(!vm.isPanelVisible());
				});
			} else {
				vm.isPanelVisible(!vm.isPanelVisible());
			}
			e.preventDefault();
			e.stopPropagation();
		});

		$('.teamschedule-filter-component').on('mousedown', function(e) {
			e.preventDefault();
			e.stopPropagation();
		});

		$('body').on('mousedown', function(event) {
			if (
				$(event.target)[0].className != 'new-teamschedule-panel' &&
				$(event.target).parents('.new-teamschedule-panel').length == 0 &&
				$(event.target)[0].className != 'new-teamschedule-filter' &&
				$(event.target).parents('.new-teamschedule-team-filter').length == 0
			) {
				vm.isPanelVisible(false);
			}
		});
	}

	function registerSwipeEventOnMobileAndiPad() {
		if (!onMobile && !oniPad) return;

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
		if (onMobile || oniPad) {
			$('.teammates-schedules-container').scroll(function() {
				$('.teammates-agent-name-row').css({ left: -$(this).scrollLeft() - 1 });
			});
		}

		$('body').on('keydown', function(e) {
			if (oniPad || onMobile) return;
			if ((e.keyCode !== 37 && e.keyCode !== 39) || scrollIntervalsInPixelsRepresentingAPageOfAgents <= 0) return;

			var scrollIntervalOfPixelsFRepresentingOneAgent =
				scrollIntervalsInPixelsRepresentingAPageOfAgents / vm.paging.take;

			var orginalLeft = parseInt(
				$('.teamschedule-scroll-block')
					.css('left')
					.slice(0, -2)
			);

			var newLeft = 0;
			if (e.keyCode == 37) {
				newLeft = orginalLeft - scrollIntervalOfPixelsFRepresentingOneAgent;
				if (newLeft < scrollIntervalOfPixelsFRepresentingOneAgent) newLeft = 0;
			}

			if (e.keyCode == 39) {
				newLeft = orginalLeft + scrollIntervalOfPixelsFRepresentingOneAgent;
				var containerWidth = $('.teamschedule-scroll-block-container').width();
				var scrollBlockWidth = $('.teamschedule-scroll-block').width();
				if (containerWidth - scrollBlockWidth - newLeft < scrollIntervalOfPixelsFRepresentingOneAgent) {
					newLeft = containerWidth - scrollBlockWidth;
				}

				loadSchedulesBasedOnPageDiffAndUpdateCurrentPageNum(newLeft);
			}

			setScrollPositionForNameAndSchedule(-newLeft * pixelsRatioOfHorizontalScrolling - (oniPad ? -1 : 1));
			$('.teamschedule-scroll-block').css({
				left: newLeft
			});
		});
	}

	function setDraggableScrollBlockOnDesktop() {
		if (!vm) throw 'Call this function after initializing vm';

		if (onMobile || oniPad) return;

		setTimeout(function() {
			calculateTheScrollingRatio();

			$('.teamschedule-scroll-block').draggable({
				axis: 'x',
				containment: 'parent',
				scrollSpeed: 1,
				start: function(event, ui) {},
				drag: function(event, ui) {
					var leftDistance = ui.position.left;

					if (scrollIntervalsInPixelsRepresentingAPageOfAgents <= 0) return;
					setScrollPositionForNameAndSchedule(
						-leftDistance * pixelsRatioOfHorizontalScrolling - (oniPad ? -1 : 1)
					);

					loadSchedulesBasedOnPageDiffAndUpdateCurrentPageNum(leftDistance);
				},
				stop: function(event, ui) {
					var interval = setInterval(function() {
						if (!vm.isLoadingMoreAgentSchedules) {
							loadSchedulesBasedOnPageDiffAndUpdateCurrentPageNum(ui.position.left);
							clearInterval(interval);
						}
					});
				}
			});

			$(window).on('resize', calculateTheScrollingRatio);
		}, 0);
	}

	function loadSchedulesBasedOnPageDiffAndUpdateCurrentPageNum(left) {
		var currentScrollPage = Math.ceil(left / scrollIntervalsInPixelsRepresentingAPageOfAgents) + 1;
		var pageDiff = currentScrollPage - vm.currentPageNum();

		if (!vm.isLoadingMoreAgentSchedules && pageDiff > 0) {
			loadMoreSchedules(
				{
					skip: vm.paging.take * vm.currentPageNum(),
					take: vm.paging.take * pageDiff
				},
				function() {
					vm.currentPageNum(currentScrollPage);
					vm.isLoadingMoreAgentSchedules = false;
				}
			);
		}
	}

	function calculateTheScrollingRatio() {
		if (vm.totalPageNum() == 0) return;

		var agentListTotalWidth = vm.totalAgentCount() * agentScheduleColumnWidth;
		var scrollContainerWidth = $('.teamschedule-scroll-block-container').width();
		var scrollBlockWidth = 0;

		vm.isScrollbarVisible(agentListTotalWidth > scrollContainerWidth);

		$('.teammates-schedules-container.relative').width(agentListTotalWidth);
		setScrollPositionForNameAndSchedule(0);

		if (vm.totalPageNum() > 1 || agentListTotalWidth >= scrollContainerWidth) {
			scrollBlockWidth = scrollContainerWidth * (scrollContainerWidth / agentListTotalWidth);

			if (scrollBlockWidth < minScrollBlockWidth) {
				scrollBlockWidth = minScrollBlockWidth;
			}

			var scrollRangeWidthInPixels = scrollContainerWidth - scrollBlockWidth;
			var toBeScrolledAgentsWidth = agentListTotalWidth + agentScheduleColumnWidth * 2 - scrollContainerWidth;

			scrollIntervalsInPixelsRepresentingAPageOfAgents = scrollContainerWidth / vm.totalPageNum();
			pixelsRatioOfHorizontalScrolling = toBeScrolledAgentsWidth / scrollRangeWidthInPixels;
		} else {
			scrollIntervalsInPixelsRepresentingAPageOfAgents = pixelsRatioOfHorizontalScrolling = 0;
			scrollBlockWidth = scrollContainerWidth;
		}

		$('.teamschedule-scroll-block')
			.width(scrollBlockWidth)
			.css({ color: 'white', left: 0 });
	}

	function setScrollPositionForNameAndSchedule(leftDistance) {
		$('.teammates-agent-name-row').css({ left: leftDistance });
		$('.teammates-schedules-container.relative').css({ left: leftDistance });
	}

	function showLoadingGif() {
		$('#loading').show();
	}

	function hideLoadingGif() {
		$('#loading').hide();
	}

	function adjustScrollOnMobileAndiPad() {
		var container = $('.teammates-schedules-container');
		container.animate(
			{
				scrollLeft: container.scrollLeft() + agentScheduleColumnWidth * 1.5
			},
			100
		);
	}

	function loadMoreSchedules(paging, callback) {
		if (vm.paging.skip + vm.paging.take < vm.paging.take * vm.totalPageNum()) {
			vm.paging.skip += vm.paging.take;

			showLoadingGif();
			vm.isLoadingMoreAgentSchedules = true;

			dataService.loadScheduleData(
				vm.selectedDate().format('YYYY/MM/DD'),
				paging || vm.paging,
				vm.filter,
				function(schedules) {
					vm.readMoreTeamScheduleData(schedules, callback);

					if (onMobile || oniPad) adjustScrollOnMobileAndiPad();

					fetchDataSuccessCallback();
				}
			);
		}
	}

	function initViewModel() {
		vm = new Teleopti.MyTimeWeb.Schedule.NewTeamScheduleViewModel(
			filterChangedCallback,
			loadGroupAndTeams,
			setDraggableScrollBlockOnDesktop
		);
		applyBindings();
	}

	function applyBindings() {
		ko.applyBindings(vm, $('#page')[0]);
	}

	function loadGroupAndTeams(callback) {
		dataService.loadGroupAndTeams(function(teams) {
			vm.readTeamsData(teams);

			$('#teams-and-groups-selector')
				.select2('data', { id: vm.selectedTeam(), text: vm.selectedTeamName() })
				.trigger('change');

			callback && callback();
		});
	}

	function loadDefaultTeam(callback) {
		dataService.loadDefaultTeam(function(defaultTeam) {
			vm.readDefaultTeamData(defaultTeam);
			callback && callback();
		});
	}

	function filterChangedCallback(dateStr) {
		showLoadingGif();

		vm.isAgentScheduleLoaded(false);
		dataService.loadScheduleData(dateStr, vm.paging, vm.filter, function(schedules) {
			vm.readScheduleData(schedules, dateStr);

			focusMySchedule();
			fetchDataSuccessCallback();
		});
	}

	function focusMySchedule() {
		if (!vm.mySchedule() || !vm.mySchedule().layers[0]) {
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
	}

	function fetchDataSuccessCallback() {
		hideLoadingGif();
		completelyLoaded();
		if (!subscribed) subscribeForChanges();
	}

	function getDateStr(momentDate) {
		var dateStr =
			(momentDate && momentDate.format('YYYY/MM/DD')) ||
			Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ||
			vm.selectedDate().format('YYYY/MM/DD');

		return dateStr;
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
		PartialInit: function(readyForInteractionCallback, completelyLoadedCallback, ajaxobj, initialMomentDate) {
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
					filterChangedCallback(getDateStr(vm.selectedDate()));
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
