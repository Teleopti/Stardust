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
				options: '='
			},
			controller: ['$timeout', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true,
			link: function (scope, element) {
				scope.$watch(function () {
					return scope.vm.selectedDate && scope.vm.selectedDate.toDateString();
				}, function () {
					if (scope.vm.selectedDate && isValidDate(scope.vm.selectedDate))
						scope.vm.shortDateFormat = moment(scope.vm.selectedDate).format('YYYY-MM-DD');
				});
			}
		};
	}

	function teamScheduleDatePickerCtrl($timeout) {
		var vm = this;
		vm.shortDateFormat = moment(vm.selectedDate).format('YYYY-MM-DD');
		vm.step = parseInt(vm.step) || 1;
		vm.afterDateChangeDatePicker = function () {
			vm.toggleCalendar();
			vm.shortDateFormat = moment(vm.selectedDate).format('YYYY-MM-DD');
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
		};

		vm.onDateInputChange = function (currentDateStr) {
			if (!currentDateStr) {
				return;
			}

			var newDateObj = moment(currentDateStr).toDate();

			if (!isValidDate(newDateObj)) {
				return;
			}

			newDateObj.setHours(vm.selectedDate.getHours());
			newDateObj.setMinutes(vm.selectedDate.getMinutes());
			vm.selectedDate = newDateObj;
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }); });
		};

		vm.gotoPreviousDate = function () {
			var currentDate = moment(vm.selectedDate).add(-(vm.step), 'day').format('YYYY-MM-DD');
			vm.onDateInputChange(currentDate);
		};

		vm.gotoNextDate = function () {
			var currentDate = moment(vm.selectedDate).add(vm.step, 'day').format('YYYY-MM-DD');
			vm.onDateInputChange(currentDate);
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};
	}

	angular.module('wfm.teamSchedule').directive('teamsShortDate', function () {
		return {
			restrict: 'A',
			require: 'ngModel',
			link: function (scope, element) {
				var ngModelCtrl = element.controller('ngModel');
				ngModelCtrl.$validators.validFormat = function (modelValue, viewValue) {
					var value = modelValue || viewValue;
					return testDateStringFormat(value);
				};
			}
		};
	});

	function testDateStringFormat(dateStr) {
		if (!angular.isString(dateStr)) {
			return false;
		}
		var format = /^(\d{4})-(\d{2})-(\d{2})$/;
		var m = dateStr.match(format);
		return (!!m);
	}

	function isValidDate(dateObj) {
		return (!isNaN(dateObj.getTime()) && dateObj.getTime() > 0);
	}
})();
