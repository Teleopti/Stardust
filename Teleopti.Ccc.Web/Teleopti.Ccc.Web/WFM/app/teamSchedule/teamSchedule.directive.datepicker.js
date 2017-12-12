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
			controller: ['$timeout', '$locale', 'CurrentUserInfo', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function teamScheduleDatePickerCtrl($timeout, $locale, CurrentUserInfo) {
		var vm = this;
		vm.isJalaali = CurrentUserInfo.CurrentUserInfo().DateFormatLocale === 'fa-IR' ? true : false;

		vm.dateFormat = vm.isJalaali ? "YYYY/MM/DD" : $locale.DATETIME_FORMATS.shortDate;

		vm.step = parseInt(vm.step) || 1;

		vm.onDateInputChange = function () {
			if (!vm.selectedDate || !moment(vm.selectedDate).isValid()) {
				return;
			}
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }); });
		};
		vm.onSelectedDateChange = function () {
			vm.isCalendarOpened = false;
			vm.onDateInputChange();
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

