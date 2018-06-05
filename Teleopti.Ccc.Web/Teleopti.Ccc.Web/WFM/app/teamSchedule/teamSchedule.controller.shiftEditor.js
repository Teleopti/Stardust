
(function () {
	'use strict';

	angular.module("wfm.teamSchedule").controller("ShiftEditorViewController",
		['$stateParams',
			'$state',
			'TeamSchedule',
			'ShiftEditorViewModelFactory',
			'signalRSVC',
			'serviceDateFormatHelper',
			function ($stateParams, $state, TeamSchedule, ShiftEditorViewModelFactory, signalRSVC, serviceDateFormatHelper) {
				var vm = this;
				vm.personId = $stateParams.personId;
				vm.timezone = decodeURIComponent($stateParams.timezone);
				vm.date = $stateParams.date;
				vm.gotoDayView = function () {
					$state.go('teams.dayView');
				}

				function init() {
					getSchedule();
					subscribeToScheduleChange();
				}

				function subscribeToScheduleChange() {
					var domainType = 'IScheduleChangedInDefaultScenario';
					signalRSVC.subscribeBatchMessage({ DomainType: domainType }, function (messages) {
						for (var i = 0; i < messages.length; i++) {
							var message = messages[i];
							if (message.DomainReferenceId === vm.personId
								&& serviceDateFormatHelper.getDateOnly(message.StartDate.substring(1, message.StartDate.length)) === vm.date) {
								getSchedule();
								return;
							}
						}
					}, 300);
				}

				function getSchedule() {
					TeamSchedule.getSchedules(vm.date, [vm.personId]).then(function (data) {
						vm.schedules = data.Schedules;
					});
				}
				init();
			}]);

	angular.module("wfm.teamSchedule").component("shiftEditor", {
		controller: ShiftEditorController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/shiftEditor.html',
		bindings: {
			personId: '<',
			schedules: '<',
			date: '<',
			timezone: '<'
		}
	});

	ShiftEditorController.$inject = ['$element', '$timeout', '$window', '$interval', '$filter', 'serviceDateFormatHelper', 'ShiftEditorViewModelFactory'];

	function ShiftEditorController($element, $timeout, $window, $interval, $filter, serviceDateFormatHelper, ShiftEditorViewModelFactory) {
		var vm = this;
		var timeLineTimeRange = {
			Start: moment.tz(vm.date, vm.timezone).add(-1, 'days').hours(0),
			End: moment.tz(vm.date, vm.timezone).add(2, 'days').hours(0)
		};
		vm.showScrollLeftButton = false;
		vm.showScrollRightButton = false;
		vm.selectedShiftLayer = null;
		vm.displayDate = moment(vm.date).format("YYYY-MM-DD");


		vm.$onInit = function () {
			vm.timelineVm = ShiftEditorViewModelFactory.CreateTimeline(vm.date, vm.timezone, timeLineTimeRange);
		}

		vm.$onChanges = function (changesObj) {
			if (!!changesObj.schedules.currentValue && changesObj.schedules.currentValue !== changesObj.schedules.previousValue) {
				vm.scheduleVm = ShiftEditorViewModelFactory.CreateSchedule(vm.date, vm.timezone, changesObj.schedules.currentValue[0]);
				initAndBindScrollEvent();
			}
		}

		vm.scroll = function (step) {
			var viewportEl = $element[0].querySelector('.viewport');
			viewportEl.scrollLeft += step;
			displayScrollButton();
		}

		vm.toggleSelection = function (shiftLayer) {
			shiftLayer.Selected = !shiftLayer.Selected;
			vm.selectedShiftLayer = shiftLayer.Selected ? shiftLayer : null;
			vm.scheduleVm.ShiftLayers.forEach(function (otherShiftLayer) {
				if (otherShiftLayer === shiftLayer) return;
				otherShiftLayer.Selected = false;
				//if (!!otherShiftLayer.ShiftLayerIds) {
				//	otherShiftLayer.Selected = otherShiftLayer.ShiftLayerIds.some(function (shiftLayerId) {
				//		return shiftLayer.ShiftLayerIds.indexOf(shiftLayerId) >= 0;
				//	});
				//	if (otherShiftLayer.Selected) {
				//		vm.selectedShiftLayer.End = otherShiftLayer.Start
				//	}
				//}
			});
		}

		vm.getShiftLayerWidth = function (layer) {
			var start = moment.tz(layer.Start, vm.timezone);
			return getDiffMinutes(layer.End, start);
		}
		vm.getShiftLayerLeft = function (layer) {
			return getDiffMinutes(layer.Start, timeLineTimeRange.Start);
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
				}, 300);
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