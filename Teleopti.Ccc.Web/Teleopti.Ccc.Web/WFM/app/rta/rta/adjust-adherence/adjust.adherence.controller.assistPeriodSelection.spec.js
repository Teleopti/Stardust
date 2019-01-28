'use strict';

rtaTester.describe('AdjustAdherenceController', function (it, fit, xit) {
    it('should adjust start date', function (t) {
        var now = new Date('2019-01-25T12:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        var vm = t.createController();

        t.apply(function () {
            vm.endDate = new Date(2019, 0, 20);
        });

        expect(moment(vm.startDate).format('YYYY-MM-DD')).toEqual('2019-01-20');
    });    
    
    it('should adjust end date', function (t) {
        var now = new Date('2019-01-25T12:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 25);
        });

        expect(moment(vm.endDate).format('YYYY-MM-DD')).toEqual('2019-01-25');
    });

    it('should adjust start time', function (t) {
        var now = new Date('2019-01-25T12:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = new Date('2019-01-25T07:00:00');
        });

        expect(moment(vm.startTime).format('HH:mm')).toEqual('07:00');
    });

    it('should adjust end time', function (t) {
        var now = new Date('2019-01-25T12:00:00');
        jasmine.clock().mockDate(now);
        t.backend.with.currentUserInfo({
            DateFormatLocale: "sv-SE"
        });
        var vm = t.createController();

        t.apply(function () {
            vm.startTime = new Date('2019-01-25T19:00:00');
        });

        expect(moment(vm.endTime).format('HH:mm')).toEqual('19:00');
    });
});