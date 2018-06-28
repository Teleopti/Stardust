
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

	ShiftEditorController.$inject = ['$element', '$timeout', '$window', '$interval', '$filter', '$state',
		'TeamSchedule', 'serviceDateFormatHelper', 'ShiftEditorViewModelFactory', 'TimezoneListFactory', 'ActivityService',
		'ShiftEditorService', 'CurrentUserInfo', 'guidgenerator', 'signalRSVC', 'teamScheduleNotificationService'];

	function ShiftEditorController($element, $timeout, $window, $interval, $filter, $state, TeamSchedule, serviceDateFormatHelper,
		ShiftEditorViewModelFactory, TimezoneListFactory, ActivityService, ShiftEditorService, CurrentUserInfo, guidgenerator,
		signalRSVC, teamScheduleNotificationService) {
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
		var isSaving = false;

		vm.$onInit = function () {
			initScheduleState();

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

		vm.toggleSelection = function (shiftLayer) {
			if (vm.isNotAllowedToChange(shiftLayer)) return;
			vm.selectedShiftLayer = shiftLayer !== vm.selectedShiftLayer ? shiftLayer : null;
			vm.selectedActivitiyId = shiftLayer.CurrentActivityId || shiftLayer.ActivityId;

		}

		vm.getShiftLayerWidth = function (layer) {
			var start = moment.tz(layer.Start, vm.timezone);
			return getDiffMinutes(layer.End, start);
		}
		vm.getShiftLayerLeft = function (layer) {
			return getDiffMinutes(layer.Start, timeLineTimeRange.Start);
		}

		vm.changeActivityType = function () {
			var selectActivity = vm.availableActivities.filter(function (activity) {
				return vm.selectedActivitiyId == activity.Id;
			})[0];

			vm.selectedShiftLayer.Color = selectActivity.Color;
			vm.selectedShiftLayer.Description = selectActivity.Name;
			vm.selectedShiftLayer.CurrentActivityId = selectActivity.Id;
			vm.hasChanges = vm.selectedShiftLayer.CurrentActivityId !== vm.selectedShiftLayer.ActivityId;
		}

		vm.saveChanges = function () {
			isSaving = true;
			ShiftEditorService.changeActivityType(vm.date, vm.personId, getChangedLayers(), { TrackId: vm.trackId }).then(function (response) {
				initScheduleState();
				teamScheduleNotificationService.reportActionResult({
					success: 'SuccessfulMessageForSavingScheduleChanges'
				},
					[{
						PersonId: vm.personId,
						Name: vm.scheduleVm.Name
					}],
					response.data);
			});
		}

		vm.isSaveButtonDisabled = function () {
			return !vm.hasChanges || vm.scheduleChanged || isSaving;
		}


		vm.refreshData = function () {
			if (vm.scheduleChanged) {
				getSchedule();
				initScheduleState();
			}
		};


		function getSchedule() {
			TeamSchedule.getSchedules(vm.date, [vm.personId]).then(function (data) {
				vm.scheduleVm = ShiftEditorViewModelFactory.CreateSchedule(vm.date, vm.timezone, data.Schedules[0]);
				vm.isInDifferentTimezone = (vm.scheduleVm.Timezone !== vm.timezone);
				initAndBindScrollEvent();
			});
		}

		function initScheduleState() {
			isSaving = false;
			vm.hasChanges = false;
			vm.scheduleChanged = false;
			vm.selectedShiftLayer = null;
		}

		function subscribeToScheduleChange() {
			signalRSVC.subscribeBatchMessage({ DomainType: 'IScheduleChangedInDefaultScenario' }, function (messages) {
				for (var i = 0; i < messages.length; i++) {
					var message = messages[i];
					if (message.DomainReferenceId === vm.personId
						&& serviceDateFormatHelper.getDateOnly(message.StartDate.substring(1, message.StartDate.length)) === vm.date
						&& vm.trackId !== message.TrackId) {
						vm.scheduleChanged = true;
						return;
					}
				}
			}, 300);
		}

		function getChangedLayers() {
			var currentUserTimezone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
			var changedShiftLayers = vm.scheduleVm.ShiftLayers
				.filter(function (sl) { return !!sl.CurrentActivityId && sl.ActivityId !== sl.CurrentActivityId; });

			var sameShiftLayers = changedShiftLayers
				.filter(function (sl) {
					return !!sl.ShiftLayerIds.filter(function (id) {
						return vm.scheduleVm.ShiftLayers
							.filter(function (isl) { return isl.ShiftLayerIds.indexOf(id) >= 0; }).length > 1;
					}).length;
				});

			return changedShiftLayers.map(function (sl) {
				if (sameShiftLayers.indexOf(sl) >= 0) {
					var startTime = moment.tz(sl.Start, vm.timezone).clone().tz(currentUserTimezone);
					var endTime = moment.tz(sl.End, vm.timezone).clone().tz(currentUserTimezone);
					return {
						ActivityId: sl.CurrentActivityId,
						ShiftLayerIds: sl.ShiftLayerIds,
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