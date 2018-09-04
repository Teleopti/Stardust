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
		MIN_SCROLL_BLOCK_WIDTH = 60,
		scrollIntervalsInPixelsRepresentingAPageOfAgents = 0,
		startTimeStartInMinute = 0,
		startTimeEndInMinute = 0,
		endTimeStartInMinute = 0,
		endTimeEndInMinute = 0;

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

	function cleanBinding() {
		ko.cleanNode($('#page')[0]);
		Teleopti.MyTimeWeb.MessageBroker.RemoveListeners(currentPage);
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
				filterChangedCallback(initialMomentDate || moment());
			});
		});
	}

	function setupFilterClickBindingFns() {
		$('.new-teamschedule-time-filter').click(function(e) {
			vm.isPanelVisible(!vm.isPanelVisible());
			if (vm.isPanelVisible()) setDraggableTimeSlider();
		});
		$('.new-teamschedule-team-filter').click(function(e) {
			if (!vm.isTeamsAndGroupsLoaded()) {
				loadGroupAndTeams(function() {
					vm.isTeamsAndGroupsLoaded(true);
					vm.isPanelVisible(!vm.isPanelVisible());
					setDraggableTimeSlider();
				});
			} else {
				vm.isPanelVisible(!vm.isPanelVisible());

				setDraggableTimeSlider();
			}

			e.preventDefault();
			e.stopPropagation();
		});

		$('.teamschedule-filter-component').on('mousedown click', function(e) {
			e.preventDefault();
			e.stopPropagation();
		});

		$('body')
			.on('mousedown', function(event) {
				var excludedClassList = [
					'new-teamschedule-time-filter',
					'new-teamschedule-team-filter',
					'new-teamschedule-panel'
				];

				if (
					excludedClassList.indexOf($(event.target)[0].className) == -1 &&
					excludedClassList.every(function(c) {
						return $(event.target).parents('.' + c).length == 0;
					})
				) {
					vm.isPanelVisible(false);
				}
			})
			.on('touchmove', function(event) {
				var tooltip = $('.tooltip');
				if (tooltip.length > 0) {
					tooltip.hide();
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

			var scrollIntervalOfPixelsRepresentingOneAgent =
				scrollIntervalsInPixelsRepresentingAPageOfAgents / vm.paging.take;

			var orginalLeft = parseInt(
				$('.teamschedule-scroll-block')
					.css('left')
					.slice(0, -2)
			);

			var newLeft = 0;
			if (e.keyCode == 37) {
				newLeft = orginalLeft - scrollIntervalOfPixelsRepresentingOneAgent;
				if (newLeft < scrollIntervalOfPixelsRepresentingOneAgent) newLeft = 0;
			}

			if (e.keyCode == 39) {
				newLeft = orginalLeft + scrollIntervalOfPixelsRepresentingOneAgent;
				var containerWidth = $('.teamschedule-scroll-block-container').width();
				var scrollBlockWidth = $('.teamschedule-scroll-block').width();
				if (containerWidth - scrollBlockWidth - newLeft < scrollIntervalOfPixelsRepresentingOneAgent) {
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

		calculateTheScrollingRatio();

		$('.teamschedule-scroll-block').draggable({
			axis: 'x',
			containment: 'parent',
			scrollSpeed: 1,
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
		$(window).on('resize', function() {
			calculateTheScrollingRatio();
		});
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

			if (scrollBlockWidth < MIN_SCROLL_BLOCK_WIDTH) {
				scrollBlockWidth = MIN_SCROLL_BLOCK_WIDTH;
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

	function setDraggableTimeSlider() {
		var timelineStartInMinute = 0;
		var timelineEndInMinute = 24 * 60;
		var containerWidth = $('.new-teamschedule-time-slider-line').width();
		var minutesOfOnePixel = (timelineEndInMinute - timelineStartInMinute) / containerWidth;
		var marginInterval = 2 * 60;
		var moveStep = 15;

		var filteredStartTime = getFilteredTime(vm.filter.filteredStartTimes);
		var filteredEndTime = getFilteredTime(vm.filter.filteredEndTimes);

		$('.start-time-slider').slider({
			animate: 'fast',
			min: timelineStartInMinute,
			max: timelineEndInMinute,
			step: moveStep,
			range: true,
			values: [filteredStartTime.start, filteredStartTime.end],
			slide: function(event, ui) {
				return setStartSliderTime(ui);
			}
		});

		setStartSliderTime();

		$('.end-time-slider').slider({
			animate: 'fast',
			min: timelineStartInMinute,
			max: timelineEndInMinute,
			step: moveStep,
			range: true,
			values: [filteredEndTime.start, filteredEndTime.end],
			slide: function(event, ui) {
				return setEndSliderTime(ui);
			}
		});

		setEndSliderTime();

		function setStartSliderTime(ui) {
			if (ui && ui.values) {
				startTimeStartInMinute = ui.values[0];
				startTimeEndInMinute = ui.values[1];
			} else {
				startTimeStartInMinute = $('.start-time-slider').slider('values', 0);
				startTimeEndInMinute = $('.start-time-slider').slider('values', 1);
			}

			if (startTimeStartInMinute != 0 && startTimeEndInMinute - startTimeStartInMinute < marginInterval)
				return false;

			var hours0 = Math.floor(startTimeStartInMinute / 60),
				minutes0 = parseInt(startTimeStartInMinute % 60),
				hours1 = Math.floor(startTimeEndInMinute / 60),
				minutes1 = parseInt(startTimeEndInMinute % 60);

			setStartInterval(hours0, minutes0, hours1, minutes1, startTimeStartInMinute, startTimeEndInMinute);
			return true;
		}

		function setStartInterval(hours0, minutes0, hours1, minutes1, startValue, endValue) {
			var startTimeStart =
				(hours0 < 10 ? '0' + hours0 : hours0) + ':' + (minutes0 < 10 ? '0' + minutes0 : minutes0);
			var startTimeEnd =
				(hours1 < 10 ? '0' + hours1 : hours1) + ':' + (minutes1 < 10 ? '0' + minutes1 : minutes1);

			vm.startTimeStart(startTimeStart);
			vm.startTimeEnd(startTimeEnd);

			if (startTimeStartInMinute == 0 && startTimeEndInMinute - startTimeStartInMinute < marginInterval)
				vm.showStartTimeStart(false);
			else vm.showStartTimeStart(true);

			$('.start-time-slider-start-label').css({
				left: (startValue / minutesOfOnePixel / containerWidth) * 100 + '%'
			});
			$('.start-time-slider-end-label').css({
				left: (endValue / minutesOfOnePixel / containerWidth) * 100 + '%'
			});
		}

		function setEndSliderTime(ui) {
			if (ui && ui.values) {
				endTimeStartInMinute = ui.values[0];
				endTimeEndInMinute = ui.values[1];
			} else {
				endTimeStartInMinute = $('.end-time-slider').slider('values', 0);
				endTimeEndInMinute = $('.end-time-slider').slider('values', 1);
			}

			if (endTimeStartInMinute != 0 && endTimeEndInMinute - endTimeStartInMinute < marginInterval) return false;

			var hours0 = Math.floor(endTimeStartInMinute / 60),
				minutes0 = parseInt(endTimeStartInMinute % 60),
				hours1 = Math.floor(endTimeEndInMinute / 60),
				minutes1 = parseInt(endTimeEndInMinute % 60);

			setEndInterval(hours0, minutes0, hours1, minutes1, endTimeStartInMinute, endTimeEndInMinute);

			return true;
		}

		function setEndInterval(hours0, minutes0, hours1, minutes1, startValue, endValue) {
			var endTimeStart =
				(hours0 < 10 ? '0' + hours0 : hours0) + ':' + (minutes0 < 10 ? '0' + minutes0 : minutes0);
			var endTimeEnd = (hours1 < 10 ? '0' + hours1 : hours1) + ':' + (minutes1 < 10 ? '0' + minutes1 : minutes1);

			vm.endTimeStart(endTimeStart);
			vm.endTimeEnd(endTimeEnd);

			if (startValue == 0 && endValue - startValue < marginInterval) vm.showEndTimeStart(false);
			else vm.showEndTimeStart(true);

			$('.end-time-slider-start-label').css({
				left: (startValue / minutesOfOnePixel / containerWidth) * 100 + '%'
			});
			$('.end-time-slider-end-label').css({ left: (endValue / minutesOfOnePixel / containerWidth) * 100 + '%' });
		}

		function getFilteredTime(filteredTimeString) {
			var filteredTimeArr = filteredTimeString.split('-');
			var startValue =
				Math.floor(moment.duration(filteredTimeArr[0]).asHours()) * 60 +
				moment.duration(filteredTimeArr[0]).minutes();
			var endValue =
				Math.floor(moment.duration(filteredTimeArr[1]).asHours()) * 60 +
				moment.duration(filteredTimeArr[1]).minutes();

			return {
				start: startValue,
				end: endValue
			};
		}

		$('.start-time-clear-button').click(function() {
			startTimeStartInMinute = 0;
			startTimeEndInMinute = 0;
			$('.start-time-slider').slider('values', [startTimeStartInMinute, startTimeEndInMinute]);
			setStartInterval(0, 0, 0, 0, startTimeStartInMinute, startTimeEndInMinute);
		});

		$('.end-time-clear-button').click(function() {
			endTimeStartInMinute = 0;
			endTimeEndInMinute = 0;
			$('.end-time-slider').slider('values', [endTimeStartInMinute, endTimeEndInMinute]);
			setEndInterval(0, 0, 0, 0, endTimeStartInMinute, endTimeEndInMinute);
		});
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

	function filterChangedCallback(momentDate) {
		showLoadingGif();
		vm.isAgentScheduleLoaded(false);

		var dateStr =
			(momentDate && momentDate.format('YYYY/MM/DD')) ||
			Teleopti.MyTimeWeb.Portal.ParseHash().dateHash ||
			vm.selectedDate().format('YYYY/MM/DD');

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
			setupFilterClickBindingFns();
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
					filterChangedCallback(vm.selectedDate());
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
