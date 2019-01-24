'use strict';

rtaTester.fdescribe('AdjustAdherenceController', function (it, fit, xit) {
    it('should not show adjust section on load', function (t) {
        var vm = t.createController();

        expect(vm.showAdjustToNeutralForm).toBe(false);
    });

    it('should show adjust section', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.toggleAdjustToNeutralForm();
        });

        expect(vm.showAdjustToNeutralForm).toBe(true);
    });

    it('should not show adjust section', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.toggleAdjustToNeutralForm();
        });
        t.apply(function () {
            vm.toggleAdjustToNeutralForm();
        });

        expect(vm.showAdjustToNeutralForm).toBe(false);
    });

    it('should preselect yesterday', function (t) {
        var now = new Date('2019-01-22T08:00:00');
        jasmine.clock().mockDate(now);

        var vm = t.createController();

        expect(moment(vm.startDate).format('YYYY-MM-DD')).toEqual('2019-01-21');
        expect(moment(vm.endDate).format('YYYY-MM-DD')).toEqual('2019-01-21');
    });

    it('should preselect start and end time', function (t) {
        var vm = t.createController();

        expect(moment(vm.startTime).format('HH:mm')).toEqual('08:00');
        expect(moment(vm.endTime).format('HH:mm')).toEqual('18:00');
    });

    it('should show meridian', function (t) {
        t.backend.with.currentUserInfo({
            DateTimeFormat: {ShortTimePattern: "h:mm tt"}
        });
        
        var vm = t.createController();

        expect(vm.showMeridian).toBe(true);
    });

    it('should have preselected period', function (t) {
        var now = new Date('2019-01-22T08:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        
        var vm = t.createController();
        
        expect(vm.selectedPeriod).toEqual("2019-01-21 08:00 - 2019-01-21 18:00");
    });

    it('should update preselected period', function (t) {
        var now = new Date('2019-01-22T08:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        var vm = t.createController();
        
        t.apply(function () {
            vm.startDate = new Date(2019, 0, 20);
        });
        t.apply(function () {
            vm.startTime = new Date('2019-01-22T09:00:00');
        });
        t.apply(function () {
            vm.endDate = new Date(2019, 0, 20);
        });
        t.apply(function () {
            vm.endTime = new Date('2019-01-22T20:00:00');
        });

        expect(vm.selectedPeriod).toEqual("2019-01-20 09:00 - 2019-01-20 20:00");
    });
});