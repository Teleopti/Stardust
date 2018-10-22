(function () {
	'use strict';

	angular.module('wfm.teamSchedule')
		.component('teamScheduleDatepicker',
		{
			templateUrl: 'app/teamSchedule/html/teamscheduledatepicker.html',
			require: {
				ngModelCtrl: 'ngModel'
			},
			bindings: {
				ngModel: '<',
				step: '<?',
				updateValue: '='
			},
			controller: teamScheduleDatePickerCtrl,
			controllerAs: 'vm'
		});

	teamScheduleDatePickerCtrl.$inject = ['$timeout', '$locale', 'serviceDateFormatHelper', 'CurrentUserInfo', 'throttleDebounce'];

	function teamScheduleDatePickerCtrl($timeout, $locale, serviceDateFormatHelper, CurrentUserInfo, throttleDebounce) {
		var vm = this;
		vm.dateFormat = $locale.DATETIME_FORMATS.shortDate;
		vm.step = parseInt(vm.step) || 1;
		vm.selectedDateObj = moment(vm.ngModel).toDate();
		vm.dateOptions = { startingDay: CurrentUserInfo.CurrentUserInfo().FirstDayOfWeek };

		vm.$onChanges = function () {
			vm.selectedDateObj = moment(vm.ngModel).toDate();
		}

		vm.onDateInputChange = throttleDebounce(changeDate, 300);

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

		function changeDate() {
			if (!vm.selectedDateObj || !moment(vm.selectedDateObj).isValid()) {
				vm.selectedDateObj = moment(vm.ngModel).toDate();
				return;
			}

			var date = serviceDateFormatHelper.getDateOnly(vm.selectedDateObj);
			if (vm.updateValue) {
				date = vm.updateValue(date);
			}
			if (date !== vm.ngModel)
				vm.ngModelCtrl.$setViewValue(date);
			else
				vm.selectedDateObj = moment(date).toDate();

		}
	}

})();

