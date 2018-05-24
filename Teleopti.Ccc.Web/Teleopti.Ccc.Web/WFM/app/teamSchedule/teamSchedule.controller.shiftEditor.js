
(function () {
	'use strict';

	angular.module("wfm.teamSchedule").controller("ShiftEditorViewController", ['$stateParams', 'serviceDateFormatHelper', function ($stateParams, serviceDateFormatHelper) {
		var vm = this;
		vm.personSchedule = $stateParams.personSchedule;
		vm.date = serviceDateFormatHelper.getDateOnly($stateParams.date);
	}]);

	angular.module("wfm.teamSchedule").component("shiftEditor", {
		controller: ShiftEditorController,
		controllerAs: 'vm',
		templateUrl: 'app/teamSchedule/html/shiftEditor.html',
		bindings: {
			personSchedule: '<',
			date: '<',
			timezone: '<'
		}
	});

	ShiftEditorController.$inject = ['$element', '$timeout', '$window', '$interval', 'serviceDateFormatHelper'];

	function ShiftEditorController($element, $timeout, $window, $interval, serviceDateFormatHelper) {
		var vm = this;
		var viewportClipEl = $element[0].querySelector('.shift-container .viewport-clip');
		var viewportEl = viewportClipEl.querySelector('.viewport');

		vm.intervals = [];
		vm.projections = [];
		vm.showScrollLeftButton = false;
		vm.showScrollRightButton = false;

		vm.$onInit = function () {
			if (!hasShift()) return;
			vm.intervals = getIntervals();
			vm.projections = getProjections();
			initScrollState();
			angular.element($window).bind('resize', initScrollState);
			bindScrollMouseEvent(viewportEl.querySelector('.left-scroll'), -20);
			bindScrollMouseEvent(viewportEl.querySelector('.right-scroll'), 20);
		}

		vm.scroll = function (step) {
			viewportEl.scrollLeft += step;
			displayScrollButton();
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
			el.addEventListener('mouseleave', function () {
				cancelScrollIntervalPromise();
			}, false);
		
		}

		function getProjections() {
			var projections = vm.personSchedule.Shifts[0].Projections || [];
			projections.forEach(function (projection) {
				projection.Left = getTotalMinutes(projection.Start);
				projection.Width = getTotalMinutes(projection.End) - getTotalMinutes(projection.Start);
			});

			return projections;
		}

		function initScrollState() {
			$timeout(function () {
				var shiftProjectionTimeRange = vm.personSchedule.Shifts[0].ProjectionTimeRange;
				var shiftStart = getTotalMinutes(shiftProjectionTimeRange.Start);
				var shiftLength = getTotalMinutes(shiftProjectionTimeRange.End) - shiftStart;
				var scrollTo = viewportEl.clientWidth <= shiftLength ?
					(shiftStart - 120) : (shiftStart - ((viewportEl.clientWidth - shiftLength) / 2));
				if (scrollTo <= 0) return;
				viewportEl.scrollLeft = scrollTo;
				displayScrollButton();
			});
		}

		function displayScrollButton() {
			vm.showScrollLeftButton = viewportEl.scrollLeft > 0;
			vm.showScrollRightButton = viewportEl.scrollWidth > (viewportEl.scrollLeft + viewportEl.clientWidth);
		}

		function hasShift() {
			return !!vm.personSchedule && !!vm.personSchedule.Shifts;
		}


		function getTotalMinutes(dateTime) {
			var hours = 0;
			var currentDate = moment.tz(vm.date, vm.timezone);
			var dateTimeMoment = moment.tz(dateTime, vm.timezone);
			while (dateTimeMoment.diff(currentDate, 'hours') > 0) {
				hours += 1;
				currentDate = currentDate.add('hours', 1);
			}
			return hours * 60 + moment(dateTime).minutes();
		}


		function getIntervals() {
			var intervals = [];
			var startTime = moment.tz(vm.date, vm.timezone);
			var endTime = moment.tz(vm.date, vm.timezone).add(1, 'days').hours(12);
			while (startTime <= endTime) {
				intervals.push({
					label: startTime.format('LT'),
					time: startTime.clone(),
					ticks: getTicks(startTime, startTime.isSame(endTime))
				});
				startTime = startTime.add(1, 'hours');
			}
			return intervals;
		}

		function getTicks(dateTime, isTheEnd) {
			var ticks = [];
			if (isTheEnd) {
				ticks.push({ time: startTime, isHalfHour: false, isHour: true });
				return ticks;
			}
			var startTime = dateTime.clone();
			var endTime = dateTime.clone().add(1, 'hours');

			while (startTime < endTime) {
				var minutes = startTime.minutes();
				var isHalfHour = minutes !== 0 && minutes % 30 === 0;
				var isHour = minutes === 0;
				ticks.push({ time: startTime.clone(), isHalfHour: isHalfHour, isHour: isHour });
				startTime.add(5, 'minutes');
			}
			return ticks;
		}
	}


})();