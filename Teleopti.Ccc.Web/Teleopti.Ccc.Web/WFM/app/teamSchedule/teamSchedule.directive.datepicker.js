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
			controller: teamScheduleDatePickerCtrl,
			controllerAs: 'vm',
			bindToController: true
		};
	}

	teamScheduleDatePickerCtrl.$inject = ['$timeout', '$locale', 'serviceDateFormatHelper', 'CurrentUserInfo', 'throttleDebounce'];
	function teamScheduleDatePickerCtrl($timeout, $locale, serviceDateFormatHelper, CurrentUserInfo, throttleDebounce) {
		var vm = this;
		vm.dateFormat = $locale.DATETIME_FORMATS.shortDate;
		vm.step = parseInt(vm.step) || 1;
		vm.selectedDateObj = moment(vm.selectedDate).toDate();
		vm.dateOptions = { startingDay: CurrentUserInfo.CurrentUserInfo().FirstDayOfWeek };

		var date = vm.selectedDate;

		vm.onDateInputChange = throttleDebounce(
			function () {
			if (!vm.selectedDateObj || !moment(vm.selectedDateObj).isValid()) {
				vm.selectedDate = date;
				return;
			}
			date = vm.selectedDateObj;
			vm.selectedDate = serviceDateFormatHelper.getDateOnly(vm.selectedDateObj);
			vm.onDateChange && vm.onDateChange({ date: serviceDateFormatHelper.getDateOnly(vm.selectedDateObj) });
		}, 300);

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

