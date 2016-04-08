(function () {

	'use strict';

	angular.module('wfm.teamSchedule')
           .directive('tmpTimepickerWrap', ['$locale', timepickerWrap])
           .directive('activityTimeRangePicker', ['$filter', timeRangePicker]);

	var defaultTemplate = 'js/teamSchedule/html/addActivityTimeRangePicker.tpl.html';

	function timeRangePicker($filter) {
		return {
			templateUrl: function (element, attrs) {
				return attrs.templateUrl || defaultTemplate;
			},
			scope: {
				disableNextDay: '=?',
				referenceDay: '=?',
				isNextDay: '=?'
			},
			controller: ['$scope', '$element', timeRangePickerCtrl],
			require: ['ngModel', 'activityTimeRangePicker'],
			transclude: true,
			link: postlink
		};

		function timeRangePickerCtrl($scope, $element) {

			/* jshint validthis: true */

			var vm = this;
			$element.addClass('wfm-time-range-picker-wrap');

			vm.mutateMoment = mutateMoment;
			vm.sameDate = sameDate;

			if (!angular.isDefined(vm.disableNextDay)) {
				vm.disableNextDay = false;
			}

			function mutateMoment(mDate, date) {
				var hour = date.getHours(),
                    minute = date.getMinutes();

				mDate.set('hour', hour).set('minute', minute);
			}

			function sameDate(date1, date2) {
				return date1.toLocaleDateString() === date2.toLocaleDateString();
			}
		}

		function postlink(scope, elem, attrs, ctrls) {
			var ngModel = ctrls[0],
                timeRangeCtrl = ctrls[1];

			scope.$watch(watchUIChange, respondToUIChange, true);

			ngModel.$parsers.push(parseView);
			ngModel.$formatters.push(formatModel);
			ngModel.$render = render;

			function formatModel(modelValue) {
				if (!modelValue) {
					return undefined;
				}
				scope.disableNextDay =
                    !timeRangeCtrl.sameDate(modelValue.startTime, modelValue.endTime);

				var viewModel = makeViewValue(
                    modelValue.startTime, modelValue.endTime, scope.isNextDay);

				return viewModel;
			}

			function parseView(viewValue) {
				if (!viewValue) {
					return undefined;
				}
				return {
					startTime: viewValue.startTime.toDate(),
					endTime: viewValue.endTime.toDate()
				};
			}

			function render() {
				if (!ngModel.$viewValue) {
					return;
				}

				var mStartTime = ngModel.$viewValue.startTime,
                    mEndTime = ngModel.$viewValue.endTime;

				scope.startTime = mStartTime.toDate();
				scope.endTime = mEndTime.toDate();
				scope.disableNextDay =
                    !!mStartTime.isSame(mEndTime, 'day');
			}

			function makeViewValue(startTime, endTime, nextDay) {
				var viewValue = null;
				
				if (angular.isDefined(scope.referenceDay)) {
					viewValue = {
						startTime: moment(scope.referenceDay()),
						endTime: moment(scope.referenceDay())
					}; 
				} else {
					viewValue = {
						startTime: moment(),
						endTime: moment()
					};
				}
				if (angular.isDefined(scope.isNextDay)) {
					scope.isNextDay = nextDay;
				}


				timeRangeCtrl.mutateMoment(viewValue.startTime, startTime);
				timeRangeCtrl.mutateMoment(viewValue.endTime, endTime);
				if (viewValue.startTime > viewValue.endTime) {
					viewValue.endTime = viewValue.endTime.add(1, 'day');
				}
				if (nextDay) {
					viewValue.startTime = viewValue.startTime.add(1, 'days');
					viewValue.endTime = viewValue.endTime.add(1, 'days');
				}
				return viewValue;
			}

			function respondToUIChange(change, old) {
				if (!scope.startTime || !scope.endTime) {
					ngModel.$setViewValue(null);
					return;
				}

				if (change.boolDisableNextDay && change.boolNextDay) {
					scope.isNextDay = false;

				}
				
				scope.disableNextDay = scope.startTime > scope.endTime;
				ngModel.$setViewValue(
                    makeViewValue(scope.startTime, scope.endTime, scope.isNextDay));
			}

			function watchUIChange() {
				return {
					strStartTime: scope.startTime ? $filter('date')(scope.startTime, 'HH:mm') : '',
					strEndTime: scope.endTime ? $filter('date')(scope.endTime, 'HH:mm') : '',
					boolNextDay: scope.isNextDay,
					boolDisableNextDay: scope.disableNextDay
				};
			}
		}
	}

	function timepickerWrap($locale) {

		var meridianInfo = getMeridiemInfoFromMoment($locale);

		return {
			template: '<uib-timepicker></uib-timepicker>',
			controller: ['$scope', timepickerWrapCtrl],
			compile: compileFn
		};

		function compileFn(tElement, tAttributes) {
			var binding = tAttributes.ngModel;
			tElement.addClass('wfm-timepicker-wrap');

			var cellElement = tElement.find('timepicker');
			cellElement.attr('ng-model', binding);
			cellElement.attr('show-meridian', 'showMeridian');
			cellElement.attr('minute-step', 'minuteStep');

			if (meridianInfo.showMeridian) {
				cellElement.attr('meridians', 'meridians');
			}
		}

		function timepickerWrapCtrl($scope) {
			$scope.showMeridian = meridianInfo.showMeridian;
			$scope.minuteStep = 5;

			if (meridianInfo.showMeridian) {
				$scope.meridians = [meridianInfo.am, meridianInfo.pm];
			}
		}

	}

	function getMeridiemInfoFromMoment($locale) {
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

})();
