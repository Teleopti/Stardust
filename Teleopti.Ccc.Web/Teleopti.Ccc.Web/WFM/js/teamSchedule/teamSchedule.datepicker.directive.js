'use strict';

(function () {

	angular.module('wfm.teamSchedule').directive('teamScheduleDatepicker', ['$locale', teamScheduleDatePicker]);

	function teamScheduleDatePicker($locale) {
		return {
			templateUrl: 'js/teamSchedule/html/teamscheduledatepicker.html',
			scope: {
				selectedDate: '=',
				onDateChange: '&'
			},
			controller: ['$timeout', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element, attr) {
				scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				scope.$on('$localeChangeSuccess', function() {
					scope.vm.shortDateFormat = $locale.DATETIME_FORMATS.shortDate;
				});
				scope.vm.isMiniMode = 'mini' in attr;
			}
		};
	};

	function teamScheduleDatePickerCtrl($timeout) {
		var vm = this;
		
		vm.afterDateChange = function (currentDate) {
			vm.selectedDate = currentDate;

			vm.onDateChange
			&& $timeout(function () { vm.onDateChange({ date: currentDate }) });
		}

		vm.gotoPreviousDate = function () {
			var currentDate = moment(vm.selectedDate).add(-1, "day").toDate();
			vm.afterDateChange(currentDate);
		};

		vm.gotoNextDate = function () {
			var currentDate = moment(vm.selectedDate).add(1, "day").toDate();
			vm.afterDateChange(currentDate);
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};

	};

})();