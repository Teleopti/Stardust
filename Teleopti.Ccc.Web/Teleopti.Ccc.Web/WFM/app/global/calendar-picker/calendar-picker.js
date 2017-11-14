(function () {
    'use strict';

    angular
        .module('wfm.calendarPicker')
        .component('wfmCalendarPicker', {
            templateUrl: 'app/global/calendar-picker/calendar-picker.template.tpl.html',
            controller: 'PpDateRangeController',
            controllerAs: 'vm',
            bindings: {
                data: '=',
                pickStartDate: '=startDate',
                pickEndDate: '=endDate',
                showWeek: '<',
                disable: '@',
                singleDatePicker: '@'
            }
        })
        .controller('PpDateRangeController', PpDateRangeController);

    PpDateRangeController.$inject = ['$element'];

    function PpDateRangeController($element) {
        var vm = this;

        vm.resetStartDate = resetStartDate;
        vm.resetEndDate = resetEndDate;
        vm.resetDate = resetDate;
        vm.validate = undefined;
        vm.valid = false;
        vm.options = {
            customClass: renderRangeDate,
            showWeeks: !!vm.showWeek || vm.showWeek,
        };

        vm.$onInit = singleOrRangePicker();

        function singleOrRangePicker() {
            switch (vm.singleDatePicker) {
                default:
                    initSingleDatePicker();
                    break;
                case undefined:
                    initDateRangePicker();
                    break;
            }
        }

        function initSingleDatePicker() {
            vm.options.customClass = undefined;
            vm.switchDate = selectSingleDate;
        }

        function initDateRangePicker() {
            switch (vm.disable) {
                default:
                    vm.validate = validateDate;
                    vm.switchDate = switchDateForDateRangePicker;
                    break;
                case 'start-date':
                    vm.validate = validateEndDate;
                    vm.switchDate = selectEndDate;
                    break;
                case 'end-date':
                    vm.validate = validateStartDate;
                    vm.switchDate = selectStartDate;
                    break;
            }
            if (vm.validate() !== true)
                return vm.valid = true;
            return;
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
            if (vm.pickEndDate - vm.pickStartDate <= 0) {
                return vm.dateRangeText = 'End date should be later than start date';
            }
            return true;
        }

        function validateEndDate() {
            if (!vm.pickEndDate) {
                return vm.dateRangeText = 'Please select end date';
            }
            if (vm.pickEndDate - vm.pickStartDate <= 0) {
                return vm.dateRangeText = 'End date should be later than start date';
            }
            return true;
        }

        function validateStartDate() {
            if (!vm.pickStartDate) {
                return vm.dateRangeText = 'Please select start date';
            }
            if (vm.pickEndDate - vm.pickStartDate <= 0) {
                return vm.dateRangeText = 'Start date should be earlier than end date';
            }
            return true;
        }

        function resetDateRangeText() {
            vm.valid = false;
            return vm.dateRangeText = '';
        }

        function resetDate() {
            vm.date = undefined;
        }

        function resetStartDate() {
            vm.pickStartDate = undefined;
            vm.valid = true;
            return vm.validate();
        }

        function resetEndDate() {
            vm.pickEndDate = undefined;
            vm.valid = true;
            return vm.validate();
        }

        function selectSingleDate() {
            return vm.date = vm.pickDate; 
        }

        function selectStartDate() {
            vm.pickStartDate = vm.pickDate;
            return displayDateRange(vm.pickStartDate, vm.pickEndDate);
        }

        function selectEndDate() {
            vm.pickEndDate = vm.pickDate;
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

        function displayDateRange(a, b) {
            if (vm.validate() == true) {
                resetDateRangeText();
                var dateRangeText = generateDateRangeInfo(a, b);
                var keys = Object.keys(dateRangeText);
                for (var i = 0; i < keys.length; i++) {
                    if (dateRangeText[keys[i]] > 0)
                        vm.dateRangeText += dateRangeText[keys[i]] + keys[i] + '   ';
                }
                return vm.dateRangeText;
            }
            vm.valid = true;
            return;
        }

        function renderRangeDate(data) {
            var date = data.date,
                mode = data.mode;
            if (mode === 'day' && vm.validate() == true) {
                if (!moment(date).isBefore(vm.pickStartDate, 'day') && !moment(date).isAfter(vm.pickEndDate, 'day')) {
                    return 'in-date-range';
                }
            }
            return '';
        }
    }
})();
