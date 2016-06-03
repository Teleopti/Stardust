(function () {
	'use strict';

	angular.module('wfm.teamSchedule').directive('teamScheduleDatepicker', [teamScheduleDatePicker]);

	function teamScheduleDatePicker() {
		return {
			templateUrl: 'js/teamSchedule/html/teamscheduledatepicker.html',
			scope: {
				selectedDate: '=',
				onDateChange: '&'
			},
			controller: ['$timeout', teamScheduleDatePickerCtrl],
			controllerAs: 'vm',
			bindToController: true,
			compile: function (tElement, tAttrs) {
				var tabindex = angular.isDefined(tAttrs.tabindex) ? tAttrs.tabindex : '0';
				function addTabindexTo() {
					angular.forEach(arguments, function (arg) {
						angular.forEach(arg, function (elem) {
							elem.setAttribute('tabIndex', tabindex);
						});
					});
				}
				addTabindexTo(
					tElement[0].querySelectorAll('input'),
					tElement[0].querySelectorAll('button')
				);
				return function postLink(scope, element) {
					scope.$watch(function() {
						return scope.vm.selectedDate && scope.vm.selectedDate.toDateString();
					}, function() {
						scope.vm.shortDateFormat = moment(scope.vm.selectedDate).format('YYYY-MM-DD');
					});
					element.removeAttr('tabindex');
				};
			}
		};
	}

	function teamScheduleDatePickerCtrl($timeout) {
		var vm = this;

		vm.shortDateFormat = moment(vm.selectedDate).format('YYYY-MM-DD');

		vm.afterDateChangeDatePicker = function (curDate) {
			vm.toggleCalendar();
			vm.afterDateChange(curDate);
		};

		vm.afterDateChange = function(currentDate) {
			vm.selectedDate = new Date(currentDate);
			if (!isNaN(vm.selectedDate.getTime())) {
				vm.shortDateFormat = moment(vm.selectedDate).format('YYYY-MM-DD');
				vm.onDateChange && $timeout(function () { vm.onDateChange({ date: vm.selectedDate }) });
			}
		};

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
	}
})();
