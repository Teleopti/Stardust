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
		getFormattedTimeSpan = Teleopti.MyTimeWeb.Common.FormatTimeSpan,
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
				filterChangedCallback(initialMomentDate || moment());
			});
		});
	}

	function setupFilterClickFn() {
		$('.new-teamschedule-time-filter').click(function(e) {
			vm.isPanelVisible(!vm.isPanelVisible());
			setDraggableTimeSlider();
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

		$('.teamschedule-filter-component').on('mousedown', function(e) {
			e.preventDefault();
			e.stopPropagation();
		});

		$('body').on('mousedown', function(event) {
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
			$(window).on('resize', function() {
				calculateTheScrollingRatio();
			});
		}, 0);
	}

	function setDraggableTimeSlider() {
		var timelineStartInMinute = 0;
		var timelineEndInMinute = 24 * 60;
		var containerWidth = $('.new-teamschedule-time-slider-line').width();
		var minutesOfOnePixel = (timelineEndInMinute - timelineStartInMinute) / containerWidth;

		var startTimeStartInMinute = 0;
		var startTimeEndInMinute = 0;
		var marginInterval = 1 * 60;
		$('.start-time-start-slider').draggable({
			axis: 'x',
			containment: 'parent',
			drag: function(event, ui) {
				var left = ui.position.left;
				if (left < 0 || left > containerWidth) return false;

				startTimeStartInMinute = parseInt(left * minutesOfOnePixel);
				setStartTimeInterval(startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
				vm.startTimeStart(getFormattedTimeSpan(startTimeStartInMinute));

				if (startTimeStartInMinute > startTimeEndInMinute - marginInterval) {
					startTimeStartInMinute = startTimeEndInMinute - marginInterval;
					setStartTimeStartPosition(this, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
					return false;
				}
			},
			stop: function(event, ui) {
				setStartTimeStartPosition(this, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
			}
		});
		$('.start-time-end-slider').draggable({
			axis: 'x',
			containment: 'parent',
			start: function() {
				if (vm.startTimeStart() == '') vm.startTimeStart(0);
			},
			drag: function(event, ui) {
				var left = ui.position.left;
				if (left < 0 || left > containerWidth) return false;
				startTimeEndInMinute = parseInt(left * minutesOfOnePixel);

				setStartTimeInterval(startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
				vm.startTimeEnd(getFormattedTimeSpan(startTimeEndInMinute));

				if (startTimeStartInMinute > 0 && startTimeEndInMinute < startTimeStartInMinute + marginInterval) {
					startTimeEndInMinute = startTimeStartInMinute + marginInterval;
					setStartTimeEndPosition(this, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);

					return false;
				}
			},
			stop: function(event, ui) {
				setStartTimeEndPosition(this, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
			}
		});
	}

	function setStartTimeStartPosition(slider, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel) {
		$(slider).css({ left: startTimeStartInMinute / minutesOfOnePixel });
		setStartTimeInterval(startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);

		vm.startTimeStart(getFormattedTimeSpan(startTimeStartInMinute));
	}
	function setStartTimeEndPosition(slider, startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel) {
		$(slider).css({ left: startTimeEndInMinute / minutesOfOnePixel });
		setStartTimeInterval(startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel);
		vm.startTimeEnd(getFormattedTimeSpan(startTimeEndInMinute));
	}

	function setStartTimeInterval(startTimeStartInMinute, startTimeEndInMinute, minutesOfOnePixel) {
		$('.start-time-interval-line').css({
			left: parseInt(startTimeStartInMinute / minutesOfOnePixel),
			width: parseInt((startTimeEndInMinute - startTimeStartInMinute) / minutesOfOnePixel)
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
			function() {
				setDraggableScrollBlockOnDesktop();
			}
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

	function filterChangedCallback(momentDate) {
		showLoadingGif();
		vm.isAgentScheduleLoaded(false);

		var dateStr = getDateStr(momentDate);
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
