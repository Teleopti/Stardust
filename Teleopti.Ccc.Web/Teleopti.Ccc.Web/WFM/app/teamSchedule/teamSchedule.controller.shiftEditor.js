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

	ShiftEditorController.$inject = ['serviceDateFormatHelper'];

	function ShiftEditorController(serviceDateFormatHelper) {
		var vm = this;
		vm.intervals = [];

		vm.$onInit = function () {
			vm.intervals = getIntervals();
		}

		vm.getProjections = function () {
			var projections = !!vm.personSchedule && !!vm.personSchedule.Shifts ? vm.personSchedule.Shifts[0].Projections : [];
			projections.forEach(function (projection) {
				projection.Left = getDiffInHoursFromCurrentDate(projection.Start) * 60 + moment(projection.Start).minutes();
				projection.Width = projection.Minutes;
			});
			return projections;
		}

		function getDiffInHoursFromCurrentDate(dateTime) {
			var hours = 0;
			var currentDate = moment.tz(vm.date, vm.timezone);
			var dateTimeMoment = moment.tz(dateTime, vm.timezone);
			while (dateTimeMoment.diff(currentDate, 'hours') > 0) {
				hours += 1;
				currentDate = currentDate.add('hours', 1);
			}
			return hours;
		}

		function getIntervals() {
			var intervals = [];
			var startTime = moment.tz(vm.date, vm.timezone);
			var endTime = moment.tz(vm.date, vm.timezone).add(1, 'days').hours(12);
			while (startTime <= endTime) {
				intervals.push({ label: startTime.format('LT'), time: startTime, ticks: getTicks(startTime, startTime.isSame(endTime)) });
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
				ticks.push({ time: startTime, isHalfHour: isHalfHour, isHour: isHour });
				startTime.add(5, 'minutes');
			}
			return ticks;
		}
	}


})();