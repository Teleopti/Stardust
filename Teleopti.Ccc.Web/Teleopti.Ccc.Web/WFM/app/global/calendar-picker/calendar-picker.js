(function () {
    'use strict';

    angular
        .module('wfm.calendarPicker')
        .component('wfmCalendarPicker', {
            templateUrl: 'app/global/calendar-picker/calendar-picker.template.tpl.html',
            require: {
                ngModel: 'ngModel'
            },
            controller: 'PpDateRangeController',
            controllerAs: 'vm',
            bindings: {
                showWeek: '<',
                disable: '@',
                intervalRule: '@',
                singleDatePicker: '@'
            }
        })
        .controller('PpDateRangeController', PpDateRangeController);

    PpDateRangeController.$inject = ['$scope', '$element'];

    function PpDateRangeController($scope, $element) {
        var vm = this;

        vm.resetStartDate = resetStartDate;
        vm.resetEndDate = resetEndDate;
        vm.resetDate = resetDate;
        vm.hightLightToday = hightLightToday;
        vm.validate = undefined;
        vm.isValid = true;
        vm.options = {
            customClass: renderRangeDate,
            showWeeks: !!vm.showWeek || vm.showWeek,
        };

        vm.$onInit = function () {
            switch (vm.singleDatePicker) {
                default:
                    vm.ngModel.$viewChangeListeners.push(onChangeForSingleDatePicker);
                    vm.ngModel.$render = onChangeForSingleDatePicker;
                    initSingleDatePicker();
                    break;
                case undefined:
                    vm.ngModel.$viewChangeListeners.push(onChangeForDateRangePicker);
                    vm.ngModel.$render = onChangeForDateRangePicker;
                    initDateRangePicker();
                    break;
            }
        }

        function initSingleDatePicker() {
            vm.options.customClass = undefined;
            vm.switchDate = selectSingleDate;
            return;
        }

        function initDateRangePicker() {
            switch (vm.disable) {
                default:
                    vm.validate = validateDate;
                    vm.switchDate = switchDateForDateRangePicker;
                    vm.displayCalenderView = displayCalenderViewDefault;
                    break;
                case 'start-date':
                    vm.validate = validateEndDate;
                    vm.switchDate = selectEndDate;
                    vm.displayCalenderView = displayCalenderViewForDisableView;
                    break;
                case 'end-date':
                    vm.validate = validateStartDate;
                    vm.switchDate = selectStartDate;
                    vm.displayCalenderView = displayCalenderViewForDisableView;
                    break;
                case 'all':
                    vm.validate = validateDate;
                    vm.switchDate = undefined;
                    vm.displayCalenderView = displayCalenderViewForDisableView;
                    break;
            }
            return;
        }

        function hightLightToday() {
            return vm.pickDate = new Date();
        }

        function onChangeForDateRangePicker() {
            var oldVal = [angular.copy(vm.pickStartDate), angular.copy(vm.pickEndDate)];
            var newVal = fetchNgModelDateForDateRangePicker();
            vm.displayCalenderView(oldVal, newVal);
            return displayDateRange(vm.pickStartDate, vm.pickEndDate);
        }

        function fetchNgModelDateForDateRangePicker() {
            vm.pickStartDate = !vm.ngModel.$modelValue ? null : vm.ngModel.$modelValue.startDate;
            vm.pickEndDate = !vm.ngModel.$modelValue ? null : vm.ngModel.$modelValue.endDate;
            return [angular.copy(vm.pickStartDate), angular.copy(vm.pickEndDate)];
        }

        function onChangeForSingleDatePicker() {
            return vm.pickDate = !vm.ngModel.$modelValue ? new Date() : vm.ngModel.$modelValue;
        }

        function displayCalenderViewForDisableView(oldVal, newVal) {
            if (!!(newVal[1] - oldVal[1]) && vm.disable == 'end-date')
                return vm.pickDate = vm.pickStartDate;
            if (!!(newVal[0] - oldVal[0]) && vm.disable == 'start-date')
                return vm.pickDate = vm.pickEndDate;
            if (vm.disable == 'all')
                return vm.pickDate = vm.pickStartDate;
        }

        function displayCalenderViewDefault(oldVal, newVal) {
            if (!!(newVal[0] - oldVal[0]))
                return vm.pickDate = vm.pickStartDate;
            if (!!(newVal[1] - oldVal[1]))
                return vm.pickDate = vm.pickEndDate;
        }

        function validateDate() {
            if (!vm.pickStartDate && !vm.pickEndDate) {
                return vm.dateRangeText = 'Please select start date and end date';
            }
            if (!vm.pickStartDate) {
                return vm.dateRangeText = 'Please select start date';
            }
            if (!vm.pickEndDate) {
                return vm.dateRangeText = 'Please select end date';
            }
            if (vm.pickEndDate - vm.pickStartDate < 0) {
                return vm.dateRangeText = 'End date should be later than start date';
            }
            return vm.dateRangeText = '';
        }

        function validateEndDate() {
            if (!vm.pickEndDate) {
                return vm.dateRangeText = 'Please select end date';
            }
            if (vm.pickEndDate - vm.pickStartDate <= 0) {
                return vm.dateRangeText = 'End date should be later than start date';
            }
            return vm.dateRangeText = '';
        }

        function validateStartDate() {
            if (!vm.pickStartDate) {
                return vm.dateRangeText = 'Please select start date';
            }
            if (vm.pickEndDate - vm.pickStartDate <= 0) {
                return vm.dateRangeText = 'Start date should be earlier than end date';
            }
            return vm.dateRangeText = '';
        }

        function resetDate() {
            vm.pickDate = null;
            return vm.ngModel.$setViewValue(vm.pickDate);
        }

        function resetStartDate() {
            vm.pickStartDate = null;
            vm.pickDate = vm.pickStartDate;
            updateNgModelDateForDateRangePicker();
            return vm.isValid = !vm.validate();
        }

        function resetEndDate() {
            vm.pickEndDate = null;
            vm.pickDate = vm.pickEndDate;
            updateNgModelDateForDateRangePicker();
            return vm.isValid = !vm.validate();
        }

        function selectSingleDate() {
            return vm.ngModel.$setViewValue(vm.pickDate);
        }

        function selectStartDate() {
            vm.pickStartDate = vm.pickDate;
            updateNgModelDateForDateRangePicker();
            return displayDateRange(vm.pickStartDate, vm.pickEndDate);
        }

        function selectEndDate() {
            vm.pickEndDate = vm.pickDate;
            updateNgModelDateForDateRangePicker();
            return displayDateRange(vm.pickStartDate, vm.pickEndDate);
        }

        function autoSelectDate() {
            var betweenToStart = vm.pickDate - vm.pickStartDate;
            var betweenToEnd = vm.pickDate - vm.pickEndDate;
            if (betweenToStart > 0 && betweenToEnd >= 0) {
                return selectEndDate();
            }
            if (betweenToStart <= 0 && betweenToEnd < 0) {
                return selectStartDate();
            }
            if (betweenToStart > 0 && betweenToEnd < 0) {
                if (Math.abs(betweenToStart) >= Math.abs(betweenToEnd))
                    return selectEndDate();
                return selectStartDate();
            }
            return;
        }

        function switchDateForDateRangePicker() {
            if (!vm.pickStartDate) {
                return selectStartDate();
            }
            if (!vm.pickEndDate) {
                return selectEndDate();
            }
            if (!!vm.pickStartDate && !!vm.pickStartDate) {
                return autoSelectDate();
            }
            return;
        }

        function generateWeeksOnlyDateRangeInfo(a, b) {
            var a = moment(a);
            var b = moment(b).add(1, 'day');
            var days = b.diff(a, 'day');
            if (days > 6) {
                var week = (days / 7).toString().split('.')[0];
                var day = b.subtract(week * 7, 'day').diff(a, 'day');
            } else {
                var week = 0;
                var day = days;
            }
            return {
                Week: week,
                Day: day
            }
        }

        function generateMonthsOnlyDateRangeInfo(a, b) {
            var a = moment(a);
            var b = moment(b);
            var month = b.add(1, 'day').diff(a, 'month');
            var days = b.subtract(month, 'month').diff(a, 'day');
            if (days > 6) {
                var week = (days / 7).toString().split('.')[0];
                var day = b.subtract(week * 7, 'day').diff(a, 'day');
            } else {
                var week = 0;
                var day = days;
            }
            return {
                Month: month,
                Week: week,
                Day: day
            }
        }

        function generateDateRangeInfo(a, b) {
            var a = moment(a);
            var b = moment(b);
            var year = b.diff(a, 'year');
            var month = b.subtract(year, 'year').add(1, 'day').diff(a, 'month');
            var days = b.subtract(month, 'month').diff(a, 'day');
            if (days > 6) {
                var week = (days / 7).toString().split('.')[0];
                var day = b.subtract(week * 7, 'day').diff(a, 'day');
            } else {
                var week = 0;
                var day = days;
            }
            return {
                Year: year,
                Month: month,
                Week: week,
                Day: day
            }
        }

        function createDateInterval(a, b, type) {
            var text = '';
            if (type == 'week') {
                var dateRangeText = generateWeeksOnlyDateRangeInfo(a, b);
            } else if (type == 'month') {
                var dateRangeText = generateMonthsOnlyDateRangeInfo(a, b);
            } else {
                var dateRangeText = generateDateRangeInfo(a, b);
            }
            var keys = Object.keys(dateRangeText);
            for (var i = 0; i < keys.length; i++) {
                if (dateRangeText[keys[i]] > 0)
                    text += dateRangeText[keys[i]] + keys[i] + '   ';
            }
            return text;
        }

        function displayDateRange(a, b) {
            vm.isValid = !vm.validate();
            if (vm.isValid) {
                switch (vm.intervalRule) {
                    default:
                        vm.dateRangeText = createDateInterval(a, b);
                        break;
                    case 'week':
                        vm.dateRangeText = createDateInterval(a, b, 'week');
                        break;
                    case 'month':
                        vm.dateRangeText = createDateInterval(a, b, 'month');
                        break;
                }
            }
            return vm.dateRangeText;
        }

        function updateNgModelDateForDateRangePicker() {
            return vm.ngModel.$setViewValue({ startDate: vm.pickStartDate, endDate: vm.pickEndDate });
        }

        function renderRangeDate(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === 'day' && vm.isValid) {
                if (!moment(date).isBefore(vm.pickStartDate, 'day') && !moment(date).isAfter(vm.pickEndDate, 'day')) {
                    return 'in-date-range';
                }
            }
            return '';
        }
    }
})();
