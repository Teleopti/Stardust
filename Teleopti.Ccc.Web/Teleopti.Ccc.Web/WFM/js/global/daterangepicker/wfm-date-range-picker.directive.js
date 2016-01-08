(function () {

	'use strict';

	angular.module('wfm.daterangepicker', ['angularMoment']).directive('dateRangePicker', ['$filter', dateRangePicker]);

	function dateRangePicker($filter) {
		return {
			template: getHtmlTemplate(),
			scope: {
				startDate: '=',
				endDate: '=',
				disabled: '=?',
				errors: '=?',
				templateType: '@'
			},
			controller: ['$scope', '$element', dateRangePickerCtrl],
			require: ["dateRangePicker"],
			link: postlink
		}

		function dateRangePickerCtrl($scope, $element) {
			if (!$scope.templateType) $scope.templateType = "inline";
			$element.addClass('wfm-date-range-picker-wrap');
			$scope.setRangeClass = setRangeClass;
			$scope.isInvalid = isInvalid;
			if (!angular.isDefined($scope.errors) || !angular.isArray($scope.errors)) $scope.errors = [];

			this.refreshDatepickers = refreshDatepickers;
			this.checkValidity = checkValidity;

			function setRangeClass(date, mode) {
				if (mode === 'day') {
					var startDate = $scope.startDate,
						endDate = $scope.endDate;

					if (startDate && endDate && startDate <= endDate) {
						var dayToCheck = new Date(date).setHours(12, 0, 0, 0);
						var start = new Date(startDate).setHours(12, 0, 0, 0);
						var end = new Date(endDate).setHours(12, 0, 0, 0);

						if (dayToCheck >= start && dayToCheck <= end) {
							return 'in-date-range';
						}
					}
				}
				return '';
			}

			function isInvalid(symbol) {
				if (!$scope.errors) $scope.errors = [];
				if (symbol) {
					return $scope.errors.indexOf(symbol) >= 0;
				} else {
					return $scope.errors.length > 0;
				}
			}

			function checkValidity() {
				if (!$scope.errors) $scope.errors = [];
				var errors = [];
				if (!$scope.startDate || !$scope.endDate) {
					errors.push('empty');
				} else if ($scope.startDate > $scope.endDate) {
					errors.push('order');
				}

				if (errors.length != $scope.errors.length || !(function compareArray(a1, a2) {
                    for (var i = 0; i < a1.length; i++) {
                        if (a1[i] !== a2[i]) return false;
				}
                    return true;
				})(errors, $scope.errors)) {
					$scope.errors = errors;
				}

			}

			function refreshDatepickers(startDate, endDate) {
				$scope.startDate = angular.copy($scope.startDate);
				$scope.endDate = angular.copy($scope.endDate);
			}
		}

		function postlink(scope, elem, attrs, ctrls) {
			var ctrl = ctrls[0];
			scope.$watch(function () {
				return {
					startDate: scope.startDate ? $filter('date')(scope.startDate, 'yyyy-mm-dd') : '',
					endDate: scope.endDate ? $filter('date')(scope.endDate, 'yyyy-mm-dd') : ''
				}
			}, function (newValue) {

				ctrl.checkValidity();
				ctrl.refreshDatepickers();
			}, true);

			scope.showMessage = !angular.isDefined(attrs.hideMessage);
			scope.AllDatePickerSwitch=function() {
				scope.showAllDatePickers = !scope.showAllDatePickers;
				if (scope.showStartDatePicker == false && scope.showAllDatePickers == true) scope.showStartDatePicker = true;
				if (scope.showEndDatePicker == false && scope.showAllDatePickers == true) scope.showEndDatePicker = true;
				if (scope.showAllDatePickers == false) {
					scope.showStartDatePicker = false;
					scope.showEndDatePicker = false;
				}
			}
		}
		
	}

	function getHtmlTemplate() {
		return "<div ng-show=\"templateType=='inline'\" class=\"wfm-block clearfix picker-template-inline\" >" +
			"<div ng-class=\"{ 'ng-valid': !isInvalid(), 'ng-invalid': isInvalid(), 'ng-invalid-order': isInvalid('order'), 'ng-invalid-empty': isInvalid('empty')}\">" +
			"    <div class=\"wfm-datepicker-wrap no-boxshadow\">" +
			"      <div class=\"sub-header\">" +
			"	<span translate>From</span> <strong>{{ startDate | date: \"longDate\" }}</strong>" +
			"	<div class=\"icon-set form-validation-sign datepickerfix\">" +
			"	  <i class=\"mdi mdi-check right-sign \"></i>" +
			"	  <i class=\"mdi mdi-close wrong-sign\"></i>" +
			"	</div>" +
			"      </div>" +
			"      <datepicker name=\"startDatePicker\" show-weeks=\"true\" class=\"wfm-datepicker datepicker-start-date\" ng-model=\"startDate\" ng-disabled=\"disabled\" custom-class=\"setRangeClass(date, mode)\"></datepicker>" +
			"    </div>" +
			"    <div class=\"wfm-datepicker-wrap no-boxshadow\">" +
			"      <div class=\"sub-header\">" +
			"	<span translate>To</span> <strong>{{ endDate | date: \"longDate\" }}</strong>" +
			"	<div class=\"icon-set form-validation-sign datepickerfix\">" +
			"	  <i class=\"mdi mdi-check right-sign \"></i>" +
			"	  <i class=\"mdi mdi-close wrong-sign\"></i>" +
			"	</div>" +
			"      </div>" +
			"      <datepicker show-weeks=\"true\" class=\"wfm-datepicker datepicker-end-date\" ng-model=\"endDate\" ng-disabled=\"disabled\" custom-class=\"setRangeClass(date, mode)\"></datepicker>" +
			"    </div>" +
			"<div class=\"error-msg-container ng-invalid-order alert-error notice-spacer\" ng-if=\"showMessage\"><i class='mdi mdi-alert-octagon'></i> <span translate>StartDateMustBeEqualToOrEarlierThanEndDate</span></div>" +
			"<div class=\"error-msg-container ng-invalid-empty alert-error notice-spacer\" ng-if=\"showMessage\"><i class='mdi mdi-alert-octagon'></i> <span translate>StartDateAndEndDateMustBeSet</span></div>" +
			"  </div>" +
			"  </div>" +
			"<div ng-show=\"templateType=='dropdown'\" class=\"inline-block picker-template-dropdown\" ng-init=\"showAllDatePickers=false;showStartDatePicker=false;showEndDatePicker=false;hoverShow=false\">" +
			"<div  ng-class=\"{'ng-valid': !isInvalid(), 'ng-invalid': isInvalid(), 'ng-invalid-order': isInvalid('order'), 'ng-invalid-empty': isInvalid('empty')}\">" +
			"<span translate>From</span>" +
			"<div class=\"inline-block relative\">" +
			"<input class=\"pointer\" type=\"text\" ng-click=\"showStartDatePicker=!showStartDatePicker\" value=\"{{startDate | amDateFormat : 'LL' }}\" readonly />" +
			"<div class=\"picker-wrapper \" ng-show=\"showStartDatePicker||showAllDatePickers\">" +
			"<datepicker show-weeks=\"true\" ng-model=\"startDate\" class=\"wfm-datepicker\" ng-change=\"showStartDatePicker=false\" custom-class=\"setRangeClass(date, mode)\"></datepicker>" +
			"</div>" +
			"</div>" +
			"<span translate class=\"hor-m-5\">To</span>" +
			"<div class=\"inline-block relative\">" +
			"<input class=\"pointer\" type=\"text\" ng-click=\"showEndDatePicker=!showEndDatePicker\" value=\"{{endDate| amDateFormat : 'LL' }}\" readonly/>" +
			"<div class=\"picker-wrapper\" ng-show=\"showEndDatePicker||showAllDatePickers\">" +
			"<datepicker show-weeks=\"true\" ng-model=\"endDate\" class=\"wfm-datepicker\" ng-change=\"showEndDatePicker=false;\" custom-class=\"setRangeClass(date, mode)\"></datepicker>" +
			"</div>" +
			"</div>" +
			"<span class='pointer iconfix' ng-mouseenter=\"hoverShow=true\" ng-mouseleave=\"hoverShow=false\" ng-click=\"AllDatePickerSwitch()\" ng-class={'pin-down':showAllDatePickers==true}><i class=\"mdi mdi-calendar middle\"></i></span>" +
			"<div class=\"error-msg-container invalid-order alert-error notice-spacer\" ng-if=\"hoverShow\"><i class='mdi mdi-alert-octagon'></i> <span translate>StartDateMustBeEqualToOrEarlierThanEndDate</span></div>" +
			"<div class=\"error-msg-container invalid-empty alert-error notice-spacer\" ng-if=\"hoverShow\"><i class='mdi mdi-alert-octagon'></i> <span translate>StartDateAndEndDateMustBeSet</span></div>" +
			"</div>"+
			"</div>";

	}

})();