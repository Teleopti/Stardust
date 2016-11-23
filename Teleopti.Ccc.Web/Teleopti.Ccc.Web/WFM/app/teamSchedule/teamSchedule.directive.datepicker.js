﻿(function () {
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

		vm.afterDateChangeDatePicker = function () {
			vm.toggleCalendar();
			vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
		};

		vm.afterDateChangeInput = function (currentDateStr) {
			if (currentDateStr != vm.currentDateString) {
				vm.currentDateString = currentDateStr;
				var currentDay = new Date(currentDateStr);

				if (!isNaN(currentDay.getTime()) && currentDay.getTime() > 0) {
					vm.selectedDate = new Date(currentDateStr);
					vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
				} else {
					vm.isInputDateValid = false;
				}
			}
		};

		vm.gotoPreviousDate = function () {
			var currentDate = moment(vm.selectedDate).add(-(vm.step), "day").toDate();
			vm.afterDateChangeInput(currentDate);
		};

		vm.gotoNextDate = function () {
			var currentDate = moment(vm.selectedDate).add(vm.step, "day").toDate();
			vm.afterDateChangeInput(currentDate);
		};

		vm.toggleCalendar = function () {
			vm.isCalendarOpened = !vm.isCalendarOpened;
		};
	}
})();
