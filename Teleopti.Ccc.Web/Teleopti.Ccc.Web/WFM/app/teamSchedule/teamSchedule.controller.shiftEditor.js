
(function () {
	'use strict';

	angular.module("wfm.teamSchedule").controller("ShiftEditorViewController",
		['$stateParams',
			'TeamSchedule',
			'ShiftEditorViewModelFactory',
			'signalRSVC',
			'serviceDateFormatHelper',
			'guidgenerator',
			function ($stateParams) {
				var vm = this;
				vm.personId = $stateParams.personId;
				vm.timezone = decodeURIComponent($stateParams.timezone);
				vm.date = $stateParams.date;
			}]);

	angular.module("wfm.teamSchedule").component("shiftEditor", {
		controller: ShiftEditorController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/shiftEditor.html',
		bindings: {
			personId: '<',
			date: '<',
			timezone: '<'
		}
	});

	ShiftEditorController.$inject = ['$element', '$timeout', '$window', '$interval', '$filter', '$state', '$translate',
		'TeamSchedule', 'serviceDateFormatHelper', 'ShiftEditorViewModelFactory', 'TimezoneListFactory', 'ActivityService',
		'ShiftEditorService', 'CurrentUserInfo', 'guidgenerator', 'signalRSVC', 'NoticeService'];

	function ShiftEditorController($element, $timeout, $window, $interval, $filter, $state, $translate, TeamSchedule, serviceDateFormatHelper,
		ShiftEditorViewModelFactory, TimezoneListFactory, ActivityService, ShiftEditorService, CurrentUserInfo, guidgenerator,
		signalRSVC, NoticeService) {
		var doNotToggleSelection;

		var vm = this;
		var timeLineTimeRange = {
			Start: moment.tz(vm.date, vm.timezone).add(-1, 'days').hours(0),
			End: moment.tz(vm.date, vm.timezone).add(3, 'days').hours(0)
		};
		
		vm.showScrollLeftButton = false;
		vm.showScrollRightButton = false;
		vm.isInDifferentTimezone = false;
		vm.displayDate = moment(vm.date).format("L");
		vm.availableActivities = [];
		vm.trackId = guidgenerator.newGuid();
		vm.isSaving = false;

		vm.$onInit = function () {
			getSchedule();

			ActivityService.fetchAvailableActivities().then(function (data) {
				vm.availableActivities = data;
			});

			vm.timelineVm = ShiftEditorViewModelFactory.CreateTimeline(vm.date, vm.timezone, timeLineTimeRange);

			TimezoneListFactory.Create().then(function (timezoneList) {
				vm.timezoneName = timezoneList.GetShortName(vm.timezone);
			});

			subscribeToScheduleChange();
		}

		vm.gotoDayView = function () {
			$state.go('teams.dayView');
		}

		vm.isSameDate = function (interval) {
			return moment.tz(vm.date, vm.timezone).isSame(interval.Time, 'days');
		}

		vm.scroll = function (step) {
			var viewportEl = $element[0].querySelector('.viewport');
			viewportEl.scrollLeft += step;
			displayScrollButton();
		}

		vm.isNotAllowedToChange = function (shiftLayer) {
			return !shiftLayer.ShiftLayerIds || !shiftLayer.ShiftLayerIds.length;
		}


		vm.toggleSelection = function (shiftLayer, $event) {
			if (vm.isNotAllowedToChange(shiftLayer)) return;
			if (doNotToggleSelection) {
				doNotToggleSelection = false;
				return;
			}
			vm.selectedShiftLayer = shiftLayer !== vm.selectedShiftLayer ? shiftLayer : null;
			vm.selectedActivitiyId = shiftLayer.CurrentActivityId || shiftLayer.ActivityId;

			bindResizeEvent(shiftLayer, $event.target);
		}

		vm.changeActivityType = function () {
			var selectActivity = vm.availableActivities.filter(function (activity) {
				return vm.selectedActivitiyId == activity.Id;
			})[0];

			vm.selectedShiftLayer.Color = selectActivity.Color;
			vm.selectedShiftLayer.Description = selectActivity.Name;
			vm.selectedShiftLayer.CurrentActivityId = selectActivity.Id;
		}

		vm.saveChanges = function () {
			if (vm.scheduleChanged) {
				vm.showError = true;
				return;
			}
			vm.isSaving = true;
			ShiftEditorService.changeActivityType(vm.date, vm.personId, getChangedLayers(), { TrackId: vm.trackId }).then(function (response) {
				vm.isSaving = false;
				var errorMessages = getErrorMessagesFromActionResults(response.data);
				if (!!errorMessages.length) {
					showErrorNotice(errorMessages);
					return;
				}
				getSchedule();
				showSuccessNotice();
			}, function () {
				vm.isSaving = false;
			});
		}

		vm.isSaveButtonDisabled = function () {
			return !hasChanges() || vm.isSaving || vm.showError;
		}

		vm.refreshData = function () {
			if (vm.scheduleChanged) {
				getSchedule();
			}
		};

		vm.getTimeSpan = function () {
			var layer = vm.selectedShiftLayer;
			return (layer.Current || {}).TimeSpan || layer.TimeSpan;
		}

		function bindResizeEvent(shiftLayer, shiftLayerEl) {
			!shiftLayer.interact && (shiftLayer.interact = interact(shiftLayerEl))
				.resizable({
					allowFrom: '.selected',
					edges: { left: true, right: true },
					restrictSize: {
						min: { width: 5 },
					}
				})
				.on('resizemove', function (event) {
					var left = event.deltaRect.left;
					var width = event.rect.width;
					resizeLayer(event.target, function (x) { return x + left; }, function () { return width; });
				})
				.on('resizeend', function (event) {
					var target = event.target;
					var width = parseInt(target.style.width);

					resizeLayer(target,
						function (x) { return round5(x); },
						function () { return round5(width); },
						function (x, w, left) {

							var startMinutes = left + x;
							var endMinutes = startMinutes + w;

							var startTime = timeLineTimeRange.Start.clone().add(startMinutes, 'minutes');
							var endTime = timeLineTimeRange.Start.clone().add(endMinutes, 'minutes');

							shiftLayer.Current = shiftLayer.Current || {};
							shiftLayer.Current.Start = serviceDateFormatHelper.getDateTime(startTime);
							shiftLayer.Current.End = serviceDateFormatHelper.getDateTime(endTime);;
							shiftLayer.Current.TimeSpan = startTime.format('L LT') + ' - ' + endTime.format('L LT');
						});

					doNotToggleSelection = true;
				});
		}

		function resizeLayer(target, getX, getW, afterResize) {
			var dataX = (parseFloat(target.getAttribute('data-x')) || 0);

			var width = getW();
			var x = getX(dataX);

			target.style.width = width + 'px';
			target.style.webkitTransform = target.style.transform = 'translate(' + x + 'px, 0px)';
			target.setAttribute('data-x', x);

			afterResize && afterResize(x, width, parseInt(target.style.left));
		}

		function round5(number) {
			var a = number % 5;
			var b = number - a;
			var isMinus = number < 0;
			return a === 0 ? number : Math.abs(a) >= 3 ? (isMinus ? b - 5 : b + 5) : b;
		}

		function showErrorNotice(errorMessages) {
			angular.forEach(errorMessages, function (message) {
				NoticeService.error(message, null, true);
			});
		}

		function showSuccessNotice() {
			var successMessage = $translate.instant('SuccessfulMessageForSavingScheduleChanges');
			NoticeService.success(successMessage, 5000, true);
		}

		function getErrorMessagesFromActionResults(actionResults) {
			var errorMessages = [];
			actionResults.forEach(function (x) {
				if (x.ErrorMessages && x.ErrorMessages.length > 0) {
					x.ErrorMessages.forEach(function (message) {
						errorMessages.push(message);
					})
				}
			});
			return errorMessages;
		}

		function getSchedule() {
			initScheduleState();

			vm.isLoading = true;
			TeamSchedule.getSchedules(vm.date, [vm.personId]).then(function (data) {
				vm.scheduleVm = ShiftEditorViewModelFactory.CreateSchedule(vm.date, vm.timezone, data.Schedules[0]);
				vm.scheduleVm.ShiftLayers && vm.scheduleVm.ShiftLayers.forEach(function (layer) {
					layer.Width = getShiftLayerWidth(layer);
					layer.Left = getShiftLayerLeft(layer);
				});
				vm.isInDifferentTimezone = (vm.scheduleVm.Timezone !== vm.timezone);
				initAndBindScrollEvent();
				vm.isLoading = false;
			});
		}

		function getShiftLayerWidth(layer) {
			var start = (layer.Current || {}).Start || layer.Start;
			var startInTimezone = moment.tz(start, vm.timezone);
			return getDiffMinutes(layer.End, startInTimezone);
		}
		function getShiftLayerLeft(layer) {
			var start = (layer.Current || {}).Start || layer.Start;
			return getDiffMinutes(start, timeLineTimeRange.Start);
		}

		function initScheduleState() {
			vm.scheduleChanged = false;
			vm.selectedShiftLayer = null;
			vm.selectedActivitiyId = null;
			vm.showError = false;
		}

		function subscribeToScheduleChange() {
			signalRSVC.subscribeBatchMessage({ DomainType: 'IScheduleChangedInDefaultScenario' }, function (messages) {
				for (var i = 0; i < messages.length; i++) {
					var message = messages[i];
					if (message.DomainReferenceId === vm.personId
						&& moment(vm.date).isBetween(getMomentDate(message.StartDate), getMomentDate(message.EndDate), 'day', '[]')) {
						if (vm.trackId !== message.TrackId) {
							vm.scheduleChanged = true;
						}
						return;
					}
				}
			}, 300);
		}

		function getMomentDate(date) {
			return moment(serviceDateFormatHelper.getDateOnly(date.substring(1, date.length)));
		}

		function getChangedLayers() {
			var currentUserTimezone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var changedShiftLayers = vm.scheduleVm.ShiftLayers
				.filter(function (sl) { return !!sl.CurrentActivityId && sl.ActivityId !== sl.CurrentActivityId; });

			var sameShiftLayers = changedShiftLayers
				.filter(function (sl) {
					return !!sl.ShiftLayerIds.filter(function (id) {
						return vm.scheduleVm.ShiftLayers
							.filter(function (isl) { return isl.ShiftLayerIds && isl.ShiftLayerIds.indexOf(id) >= 0; }).length > 1;
					}).length;
				});

			return changedShiftLayers.map(function (sl) {
				if (!sl.TopShiftLayerId && sameShiftLayers.indexOf(sl) >= 0) {
					var startTime = moment.tz(sl.Start, vm.timezone).clone().tz(currentUserTimezone);
					var endTime = moment.tz(sl.End, vm.timezone).clone().tz(currentUserTimezone);
					return {
						ActivityId: sl.CurrentActivityId,
						ShiftLayerIds: [sl.ShiftLayerIds[0]],
						StartTime: serviceDateFormatHelper.getDateTime(startTime),
						EndTime: serviceDateFormatHelper.getDateTime(endTime),
						IsNew: true
					};
				} else {
					return {
						ActivityId: sl.CurrentActivityId,
						ShiftLayerIds: !!sl.TopShiftLayerId ? [sl.TopShiftLayerId] : sl.ShiftLayerIds
					};
				}
			});
		}


		function hasChanges() {
			return hasShift() && !!vm.scheduleVm.ShiftLayers.filter(function (layer) { return layer.CurrentActivityId && layer.CurrentActivityId !== layer.ActivityId; }).length;
		}


		function getSelectActivity(layer) {
			return vm.availableActivities.filter(function (activity) {
				return layer.Description == activity.Name;
			})[0];
		}

		function initAndBindScrollEvent() {
			var viewportEl = $element[0].querySelector('.viewport');

			if (!hasShift()) return;

			initScrollState();
			angular.element($window).bind('resize', initScrollState);

			bindScrollMouseEvent(viewportEl.querySelector('.left-scroll'), -20);
			bindScrollMouseEvent(viewportEl.querySelector('.right-scroll'), 20);
		}

		var scrollIntervalPromise = null;
		function cancelScrollIntervalPromise() {
			if (scrollIntervalPromise) {
				$interval.cancel(scrollIntervalPromise);
				scrollIntervalPromise = null;
			}
		}

		function bindScrollMouseEvent(el, step) {
			el.addEventListener('mousedown', function () {
				cancelScrollIntervalPromise();
				scrollIntervalPromise = $interval(function () {
					vm.scroll(step);
				}, 150);
			}, false);
			el.addEventListener('mouseup', function () {
				cancelScrollIntervalPromise();
			}, false);
			el.addEventListener('mouseleave', function () {
				cancelScrollIntervalPromise();
			}, false);

		}
		function initScrollState() {
			$timeout(function () {
				var viewportEl = $element[0].querySelector('.viewport');
				var shiftProjectionTimeRange = vm.scheduleVm.ProjectionTimeRange;
				var shiftStart = getDiffMinutes(shiftProjectionTimeRange.Start, timeLineTimeRange.Start);
				var shiftLength = getDiffMinutes(shiftProjectionTimeRange.End, timeLineTimeRange.Start) - shiftStart;
				var scrollTo = viewportEl.clientWidth <= shiftLength ?
					(shiftStart - 120) : (shiftStart - ((viewportEl.clientWidth - shiftLength) / 2));
				if (scrollTo <= 0) scrollTo = 0;
				viewportEl.scrollLeft = scrollTo;
				displayScrollButton();
			});
		}

		function displayScrollButton() {
			var viewportEl = $element[0].querySelector('.viewport');
			vm.showScrollLeftButton = viewportEl.scrollLeft > 0;
			vm.showScrollRightButton = viewportEl.scrollWidth > (viewportEl.scrollLeft + viewportEl.clientWidth);
		}

		function hasShift() {
			return !!vm.scheduleVm && !!vm.scheduleVm.ShiftLayers && !!vm.scheduleVm.ShiftLayers.length;
		}

		function getDiffMinutes(dateTime, fromTime) {
			var hours = 0;
			var startTime = fromTime.clone();
			var dateTimeMoment = moment.tz(dateTime, vm.timezone);

			while (dateTimeMoment.diff(startTime, 'hours') > 0) {
				hours += 1;
				startTime = startTime.add(1, 'hours');
			}
			return hours * 60 + dateTimeMoment.diff(startTime, 'minutes');
		}
	}


})();