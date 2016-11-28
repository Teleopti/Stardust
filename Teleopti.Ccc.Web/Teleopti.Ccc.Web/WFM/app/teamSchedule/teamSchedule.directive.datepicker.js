(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('teamScheduleDatepicker', teamScheduleDatePicker);

	function teamScheduleDatePicker() {
		return {
			templateUrl: 'app/teamSchedule/html/teamscheduledatepicker.html',
			scope: {
				selectedDate: '=',
				step: '@?',
				isCalendarOpened: '=?status',
				onDateChange: '&'
			},
			controller: ['$timeout', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element) {
				scope.$watch(function () {
					return scope.vm.selectedDate && scope.vm.selectedDate.toDateString();
				}, function () {
					if (scope.vm.selectedDate != null && !isNaN(scope.vm.selectedDate.getTime()) && scope.vm.selectedDate.getTime() > 0)
						scope.vm.shortDateFormat = moment(scope.vm.selectedDate).format('YYYY-MM-DD');
				});
			}
		};
	}

	function teamScheduleDatePickerCtrl($timeout) {
		var vm = this;

		vm.shortDateFormat = moment(vm.selectedDate).format('YYYY-MM-DD');

		vm.currentDateString = vm.shortDateFormat;
		vm.step = parseInt(vm.step) || 1;
		vm.dateInputValid = true;

		vm.afterDateChangeDatePicker = function () {
			vm.toggleCalendar();
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
		};

		vm.onDateInputChange = function (currentDateStr) {

			if (!testDateStringFormat(currentDateStr)) {
				vm.dateInputValid = false;
				return;
			}

			var newDateObj = new Date(currentDateStr);

			if (!isNaN(newDateObj.getTime()) && newDateObj.getTime() > 0) {
				newDateObj.setHours(vm.selectedDate.getHours());
				newDateObj.setMinutes(vm.selectedDate.getMinutes());
				vm.dateInputValid = true;
				vm.currentDateString = currentDateStr;
				vm.selectedDate = newDateObj;
				vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
			} else {
				vm.dateInputValid = false;
			}

		};

		vm.gotoPreviousDate = function () {
			var currentDate = moment(vm.selectedDate).add(-(vm.step), "day").toDate();
			vm.onDateInputChange(currentDate);
		};

		vm.gotoNextDate = function () {
			var currentDate = moment(vm.selectedDate).add(vm.step, "day").toDate();
			vm.onDateInputChange(currentDate);
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};

		function testDateStringFormat(dateStr) {
			if (typeof dateStr !== 'string') {
				return false;
			}
			var format = /^(\d{4})-(\d{2})-(\d{2})$/;
			var m = dateStr.match(format);
			return (!!m);
		}
	}
})();
