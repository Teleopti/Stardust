'use strict';

rtaTester.describe('AdjustAdherenceController', function (it, fit, xit) {
    it('should auto fix start date', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 21);
        });
        t.apply(function () {
            vm.endDate = new Date(2019, 0, 20);
        });

        expect(moment(vm.startDate).format('YYYY-MM-DD')).toEqual('2019-01-20');
    });

    it('should auto fix end date', function (t) {
        var vm = t.createController();
        t.apply(function () {
            vm.endDate = new Date(2019, 0, 24);
        });

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 25);
        });

        expect(moment(vm.endDate).format('YYYY-MM-DD')).toEqual('2019-01-25');
    });

    it('should not auto fix start time immediately', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = new Date('2019-02-01T01:00:00');
        });

        expect(moment(vm.startTime).format('HH:mm')).toEqual('08:00');
    });

    it('should not auto fix start time immediately', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = new Date('2019-02-01T01:00:00');
        });

        expect(moment(vm.startTime).format('HH:mm')).toEqual('08:00');
    });

    it('should auto fix start time', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 24);
            vm.endDate = new Date(2019, 0, 24);
            vm.startTime = new Date('2019-01-25T08:00:00');
            vm.endTime = new Date('2019-01-25T18:00:00');
        });
        t.apply(function () {
            vm.endTime = new Date('2019-01-25T07:00:00');
        }).wait(1000);

        expect(moment(vm.startTime).format('HH:mm')).toEqual('07:00');
    });

    it('should not auto fix end time immediately', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.startTime = new Date('2019-02-01T19:00:00');
        });

        expect(moment(vm.endTime).format('HH:mm')).toEqual('18:00');
    });

    it('should auto fix end time', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 24);
            vm.endDate = new Date(2019, 0, 24);
            vm.startTime = new Date('2019-01-25T08:00:00');
            vm.endTime = new Date('2019-01-25T18:00:00');
        });
        t.apply(function () {
            vm.startTime = new Date('2019-01-25T19:00:00');
        }).wait(1000);

        expect(moment(vm.endTime).format('HH:mm')).toEqual('19:00');
    });

    it('should auto fix end time on change of start date', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 28);
            vm.endDate = new Date(2019, 0, 29);
            vm.startTime = new Date('2019-01-29T19:00:00');
            vm.endTime = new Date('2019-01-29T08:00:00');
        });
        t.apply(function () {
            vm.startDate = new Date(2019, 0, 29);
        });

        expect(moment(vm.endTime).format('HH:mm')).toEqual('19:00');
    });

    it('should auto fix start time on change of end date', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 27);
            vm.endDate = new Date(2019, 0, 28);
            vm.startTime = new Date('2019-01-29T19:00:00');
            vm.endTime = new Date('2019-01-29T18:00:00');
        });
        t.apply(function () {
            vm.endDate = new Date(2019, 0, 27);
        });

        expect(moment(vm.startTime).format('HH:mm')).toEqual('18:00');
    });

    it('should not auto fix start time on invalid end time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = new Date('2019-02-01T01:00:00');
        })
            .apply(function () {
                vm.endTime = null;
            })
            .wait(1000);

        expect(moment(vm.startTime).format('HH:mm')).toEqual('08:00');
    });

    it('should not auto fix end time on invalid start time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.startTime = new Date('2019-02-01T00:00:00');
        })
            .apply(function () {
                vm.startTime = null;
            })
            .wait(1000);

        expect(moment(vm.endTime).format('HH:mm')).toEqual('18:00');
    });
});