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
				onDateChange: '&',
				options: '=?'
			},
			controller: ['$timeout', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function teamScheduleDatePickerCtrl($timeout) {
		var vm = this;
		vm.format = 'YYYY-MM-DD';
		vm.step = parseInt(vm.step) || 1;

		vm.onDateInputChange = function () {
			if (!vm.selectedDate) {
				return;
			}
			var newDateObj = moment(vm.selectedDate).toDate();
			if (!isValidDate(newDateObj)) {
				return;
			}
			newDateObj.setHours(vm.selectedDate.getHours());
			newDateObj.setMinutes(vm.selectedDate.getMinutes());
			vm.selectedDate = newDateObj;
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }); });
		};

		vm.gotoPreviousDate = function () {
			var currentDate = moment(vm.selectedDate).add(-(vm.step), 'day').format(vm.format);
			vm.onDateInputChange(currentDate);
		};

		vm.gotoNextDate = function () {
			var currentDate = moment(vm.selectedDate).add(vm.step, 'day').format(vm.format);
			vm.onDateInputChange(currentDate);
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};
	}

	function isValidDate(dateObj) {
		return (!isNaN(dateObj.getTime()) && dateObj.getTime() > 0);
	}
})();
