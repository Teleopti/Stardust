(function() {
	'use strict';

	angular
		.module('wfm.timerangepicker', ['pascalprecht.translate'])
		.controller('timeRangePickerController', ['$scope', '$translate', '$locale', timeRangePickerController])
		.directive('timeRangePicker', timeRangePickerDirective);

	function timeRangePickerDirective() {
		return {
			templateUrl: 'app/styleguide/time-range-picker/time-range-picker.tpl.html',
			scope: {
				disableNextDay: '=?',
				maxHoursRange: '=?',
				ngModel: '=ngModel'
			},
			controller: 'timeRangePickerController',
			controllerAs: 'vm',
			bindToController: true
		};
	}

	function timeRangePickerController($scope, $translate, $locale) {
		var vm = this;

		var meridianInfo = getMeridiemInfoByLocale($locale);

		vm.showMeridian = meridianInfo.showMeridian;
		vm.nextDay = !isSameDate(vm.ngModel.startTime, vm.ngModel.endTime);
		vm.meridians = [meridianInfo.am, meridianInfo.pm];
		vm.minuteStep = 5;
		vm.errorMessage = '';
		vm.showInvalidError = false;
		vm.showOrderError = false;
		vm.showEmptyError = false;

		vm.toggleNextDay = function() {
			if (!vm.disableNextDay) {
				vm.nextDay = !vm.nextDay;

				if (vm.nextDay) {
					vm.ngModel.endTime = moment(vm.ngModel.startTime)
						.startOf('day')
						.add(1, 'days')
						.add(vm.ngModel.endTime.getHours(), 'hours')
						.add(vm.ngModel.endTime.getMinutes(), 'minutes')
						.toDate();
				} else {
					vm.ngModel.endTime = moment(vm.ngModel.startTime)
						.startOf('day')

						.add(vm.ngModel.endTime.getHours(), 'hours')
						.add(vm.ngModel.endTime.getMinutes(), 'minutes')
						.toDate();
				}
			}
		};

		$scope.$watch(
			function() {
				return angular.toJson({
					startTime: moment(vm.ngModel.startTime),
					endTime: moment(vm.ngModel.endTime),
					nextDay: vm.nextDay
				});
			},
			function(newVal, oldVal) {
				if (!vm.ngModel || !vm.ngModel.startTime || !vm.ngModel.endTime) {
					vm.errorMessage = $translate.instant('StartTimeAndEndTimeMustBeSet');
				} else if (vm.ngModel.endTime.getHours() - vm.ngModel.startTime.getHours() > vm.maxHoursRange) {
					vm.errorMessage = $translate.instant('InvalidHoursRange').replace('{0}', vm.maxHoursRange);
				} else if (vm.ngModel.startTime > vm.ngModel.endTime) {
					vm.errorMessage = $translate.instant('EndTimeMustBeGreaterOrEqualToStartTime');
				} else {
					vm.errorMessage = '';
				}
			}
		);

		function isSameDate(date1, date2) {
			return date1.toLocaleDateString() === date2.toLocaleDateString();
		}

		function getMeridiemInfoByLocale($locale) {
			var timeFormat = $locale.DATETIME_FORMATS.shortTime;
			var info = {};

			if (/h:/.test(timeFormat)) {
				info.showMeridian = true;
				info.am = $locale.DATETIME_FORMATS.AMPMS[0];
				info.pm = $locale.DATETIME_FORMATS.AMPMS[1];
			} else {
				info.showMeridian = false;
			}

			return info;
		}
	}
})();
