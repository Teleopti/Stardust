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
			controller: ['$timeout', '$locale', '$scope', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function teamScheduleDatePickerCtrl($timeout, $locale, $scope) {
		var vm = this;
		vm.dateFormat = $locale.DATETIME_FORMATS.shortDate;
		vm.step = parseInt(vm.step) || 1;
		vm.selectedDateObj = moment(vm.selectedDate).toDate();
		var date = vm.selectedDate;

		vm.onDateInputChange = function () {
			if (!vm.selectedDateObj || !moment(vm.selectedDateObj).isValid()) {
				vm.selectedDate = date;
				return;
			}
			date = moment(vm.selectedDateObj).format('YYYY-MM-DD HH:mm');
			vm.selectedDate = date;
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: date }); });
		};

		vm.gotoPreviousDate = function () {
			vm.selectedDateObj = moment(vm.selectedDateObj).add(-(vm.step), 'day').toDate();
			vm.onDateInputChange();
		};

		vm.gotoNextDate = function () {
			vm.selectedDateObj = moment(vm.selectedDateObj).add(vm.step, 'day').toDate();
			vm.onDateInputChange();
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};
	}

})();

