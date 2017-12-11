describe('CalendarPickerControllerBasicFeature', function () {
    var vm,
        $controller,
        $compile,
        $rootScope,
        attachedElements = [],
        monthNames = ["January", "February", "March", "April", "May", "June",
            "July", "August", "September", "October", "November", "December"
        ],
        preSetLength = 14,
        calendarView,
        data;

    beforeEach(function () {
        module('wfm.templates', 'wfm.calendarPicker', 'externalModules');
        inject(function (_$controller_, _$compile_, _$rootScope_) {
            $controller = _$controller_;
            $compile = _$compile_;
            $rootScope = _$rootScope_;
        });

        data = {
            startDate: moment(),
            endDate: moment().add(preSetLength - 1, 'day')
        }

        $rootScope.data = data;

        pickerWithPresetDateRange = setupPicker('ng-model="data"');
        vm = pickerWithPresetDateRange.find('wfm-calendar-picker-header').scope().vm;
        calendarView = pickerWithPresetDateRange.find('table')[0];
    });

    afterEach(function () {
        attachedElements.forEach(function (element) {
            var scope = element.scope();
            scope && scope.$destroy();
            element.remove();
        });
        attachedElements = [];
    });

    function setupPicker(attrs, scope, optCompileOpts) {
        var el;
        var template = '' +
            '<wfm-calendar-picker ' + (attrs || '') + '>' +
            '</wfm-calendar-picker>';

        el = $compile(template)(scope || $rootScope);

        $rootScope.$digest();
        attachedElements.push(el);

        return el;
    }

    it('should be able to prepare data form other controller to component while picker was init', function () {
        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(vm.pickEndDate).toEqual(data.endDate);
    });

    it('should be able to reset startDate from component', function () {
        vm.resetStartDate();

        expect(vm.pickStartDate).toEqual(null);
        expect(vm.pickEndDate).toEqual(data.endDate);
    });

    it('should be able to reset endDate from component', function () {
        vm.resetEndDate();

        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(vm.pickEndDate).toEqual(null);
    });

    it('should be able to reset the display of date range on calendar view while start date is set to null', function () {
        vm.resetStartDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickStartDate).toEqual(null);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(range.length).not.toEqual(preSetLength);
        expect(range.length).toEqual(0);
    });

    it('should be able to reset the display of date range on calendar view while end date is set to null', function () {
        vm.resetEndDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).toEqual(null);
        expect(vm.pickStartDate).not.toEqual(null);
        expect(range.length).not.toEqual(preSetLength);
        expect(range.length).toEqual(0);
    });

    it('should be able to reset the display of date range on calendar view while start date and end date are both set to null', function () {
        vm.resetStartDate();
        vm.resetEndDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).toEqual(null);
        expect(vm.pickStartDate).toEqual(null);
        expect(range.length).not.toEqual(preSetLength);
        expect(range.length).toEqual(0);
    });

    it('should be able to display date range on calendar view while start date and end date are not null', function () {
        var month = monthNames[vm.pickStartDate.get('month')];
        var year = vm.pickStartDate.get('year');
        var startDay = vm.pickStartDate.get('date');
        var endDay = vm.pickEndDate.get('date');
        var monthOnCalendar = calendarView.getElementsByTagName('strong')[0].innerHTML;
        var range = calendarView.getElementsByClassName('in-date-range');
        var rangeStartDate = Math.floor(range[0].getElementsByTagName('span')[0].innerHTML);
        var rangeEndDate = Math.floor(range[range.length - 1].getElementsByTagName('span')[0].innerHTML);

        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(monthOnCalendar).toEqual(month + ' ' + year);
        expect(range.length).toEqual(preSetLength);
        expect(rangeStartDate).toEqual(startDay);
        expect(rangeEndDate).toEqual(endDay);
    });

    it('should be able to update the display of date range on calendar view while start date is reset and update', function () {
        var moveDays = 3;
        vm.resetStartDate();
        vm.pickDate = moment().add(moveDays, 'day');
        vm.switchDate();

        var month = monthNames[vm.pickStartDate.get('month')];
        var year = vm.pickStartDate.get('year');
        var startDay = vm.pickStartDate.get('date');
        var endDay = vm.pickEndDate.get('date');
        var monthOnCalendar = calendarView.getElementsByTagName('strong')[0].innerHTML;
        var range = calendarView.getElementsByClassName('in-date-range');
        var rangeStartDate = Math.floor(range[0].getElementsByTagName('span')[0].innerHTML);
        var rangeEndDate = Math.floor(range[range.length - 1].getElementsByTagName('span')[0].innerHTML);

        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).not.toEqual(data.startDate);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).toEqual(data.endDate);
        expect(monthOnCalendar).toEqual(month + ' ' + year);
        expect(range.length).toEqual(preSetLength - moveDays);
        expect(rangeStartDate).toEqual(startDay);
        expect(rangeEndDate).toEqual(endDay);
    });

    it('should be able to update the display of date range on calendar view while end date is reset and update', function () {
        var moveDays = 3;
        vm.resetEndDate();
        vm.pickDate = moment().add(preSetLength + moveDays - 1, 'day');
        vm.switchDate();

        var month = monthNames[vm.pickStartDate.get('month')];
        var year = vm.pickStartDate.get('year');
        var startDay = vm.pickStartDate.get('date');
        var endDay = vm.pickEndDate.get('date');
        var monthOnCalendar = calendarView.getElementsByTagName('strong')[0].innerHTML;
        var range = calendarView.getElementsByClassName('in-date-range');
        var rangeStartDate = Math.floor(range[0].getElementsByTagName('span')[0].innerHTML);
        var rangeEndDate = Math.floor(range[range.length - 1].getElementsByTagName('span')[0].innerHTML);

        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).not.toEqual(data.endDate);
        expect(monthOnCalendar).toEqual(month + ' ' + year);
        expect(range.length).toEqual(preSetLength + moveDays);
        expect(rangeStartDate).toEqual(startDay);
        expect(rangeEndDate).toEqual(endDay);
    });

    it('should be able to auto update new start date after both start date and end date are set to none', function () {
        vm.resetStartDate();
        vm.resetEndDate();
        vm.pickDate = moment().add((preSetLength / 2 - 2), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).toEqual(null);
        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).toEqual(vm.pickDate);
        expect(range.length).not.toEqual(preSetLength);
        expect(range.length).toEqual(0);
    });

    it('should be able to auto update new start date while new pick date is near to original start date', function () {
        vm.pickDate = moment().add((preSetLength / 2 - 2), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).not.toEqual(data.startDate);
        expect(vm.pickStartDate).toEqual(vm.pickDate);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).toEqual(data.endDate);
        expect(range.length).toEqual(preSetLength / 2 + 2);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Week1Day');
    });

    it('should be able to auto update new end date while new pick date is near to original end date', function () {
        vm.pickDate = moment().add((preSetLength / 2 + 3), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).not.toEqual(data.endDate);
        expect(vm.pickEndDate).toEqual(vm.pickDate);
        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(range.length).toEqual(preSetLength / 2 + 4);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Week4Day');
    });

    it('should be able to auto update new end date while new pick date is the middle date between the original start date and end date', function () {
        vm.pickDate = moment().add((preSetLength / 2 - 1), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).not.toEqual(data.endDate);
        expect(vm.pickEndDate).toEqual(vm.pickDate);
        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(range.length).toEqual(preSetLength / 2);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Week');
    });

    it('should be able to auto update new start date while new pick date is near to original start date', function () {
        vm.pickDate = moment().add((preSetLength / 2 - 2), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).not.toEqual(data.startDate);
        expect(vm.pickStartDate).toEqual(vm.pickDate);
        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).toEqual(data.endDate);
        expect(range.length).toEqual(preSetLength / 2 + 2);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Week1Day');
    });

    it('should be able to auto update new end date while new pick date is near to original end date', function () {
        vm.pickDate = moment().add((preSetLength / 2 + 3), 'day');
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(vm.pickEndDate).not.toEqual(null);
        expect(vm.pickEndDate).not.toEqual(data.endDate);
        expect(vm.pickEndDate).toEqual(vm.pickDate);
        expect(vm.pickStartDate).not.toEqual(null);
        expect(vm.pickStartDate).toEqual(data.startDate);
        expect(range.length).toEqual(preSetLength / 2 + 4);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Week4Day');
    });

    it('should be able to understand the interval length between 2018-01-31 to 2018-02-27 is one month', function () {
        vm.resetStartDate();
        vm.resetEndDate();
        vm.pickDate = moment([2018, 0, 31]);
        vm.switchDate();
        vm.pickDate = moment([2018, 1, 27]);
        vm.switchDate();
        var range = calendarView.getElementsByClassName('in-date-range');

        expect(range.length).toEqual(28);
        expect(vm.dateRangeText.replace(/\s/g, '')).toEqual('1Month');
    });
});