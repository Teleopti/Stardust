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
		vm.step = parseInt(vm.step) || 1;

		vm.onDateInputChange = function () {
			if (!vm.selectedDate || !moment(vm.selectedDate).isValid()) {
				return;
			}
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }); });
		};

		vm.gotoPreviousDate = function () {
			vm.selectedDate = moment(vm.selectedDate).add(-(vm.step), 'day').toDate();
			vm.onDateInputChange();
		};

		vm.gotoNextDate = function () {
			vm.selectedDate = moment(vm.selectedDate).add(vm.step, 'day').toDate();
			vm.onDateInputChange();
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};
	}

})();

