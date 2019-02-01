'use strict';

rtaTester.describe('AdjustAdherenceController', function (it, fit, xit) {
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

        var vm = t.createController();

        expect(vm.selectedPeriod).toEqual("2019-01-21 08:00 - 2019-01-21 18:00");
    });

    it('should update preselected period', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 20);
            vm.startTime = new Date('2019-01-22T09:00:00');
            vm.endDate = new Date(2019, 0, 20);
            vm.endTime = new Date('2019-01-22T20:00:00');
        });

        expect(vm.selectedPeriod).toEqual("2019-01-20 09:00 - 2019-01-20 20:00");
    });

    it('should submit adjusted period that is preselected', function (t) {
        var now = new Date('2019-01-22T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.adjustToNeutral();
        });

        expect(t.backend.lastParams.adjustPeriod()).toEqual({
            StartDateTime: '2019-01-21 08:00',
            EndDateTime: '2019-01-21 18:00'
        });
    });

    it('should submit adjusted period', function (t) {
        var vm = t.createController();

        t.apply(function () {
            vm.startDate = new Date(2019, 0, 20);
            vm.startTime = new Date('2019-01-22T09:00:00');
            vm.endDate = new Date(2019, 0, 20);
            vm.endTime = new Date('2019-01-22T20:00:00');
        });
        t.apply(function () {
            vm.adjustToNeutral();
        });

        expect(t.backend.lastParams.adjustPeriod()).toEqual({
            StartDateTime: '2019-01-20 09:00',
            EndDateTime: '2019-01-20 20:00'
        });
    });

    it('should handle empty adjusted periods', function (t) {
        var vm = t.createController();

        expect(vm.adjustedPeriods.length).toEqual(0);
    });

    it('should display multiple adjusted periods', function (t) {
        t.backend.with
            .adjustedPeriods({
                StartTime: '2019-01-25T07:00:00',
                EndTime: '2019-01-25T09:00:00'
            })
            .adjustedPeriods({
                StartTime: '2019-01-25T10:00:00',
                EndTime: '2019-01-25T11:00:00'
            });

        var vm = t.createController();

        expect(vm.adjustedPeriods[0].StartTime).toEqual('2019-01-25 07:00');
        expect(vm.adjustedPeriods[0].EndTime).toEqual('2019-01-25 09:00');
        expect(vm.adjustedPeriods[1].StartTime).toEqual('2019-01-25 10:00');
        expect(vm.adjustedPeriods[1].EndTime).toEqual('2019-01-25 11:00');
    });

    it('should refresh view after adjust to neutral', function (t) {
        var vm = t.createController();
        t.backend.with
            .adjustedPeriods({
                StartTime: '2019-01-28T09:00:00',
                EndTime: '2019-01-28T10:00:00'
            });
        t.apply(function () {
            vm.startDate = new Date(2019, 0, 28);
            vm.startTime = new Date('2019-01-28T09:00:00');
            vm.endDate = new Date(2019, 0, 28);
            vm.endTime = new Date('2019-01-28T10:00:00');
        });
        
        t.apply(function () {
            vm.adjustToNeutral();
        });

        expect(vm.adjustedPeriods[0].StartTime).toEqual('2019-01-28 09:00');
        expect(vm.adjustedPeriods[0].EndTime).toEqual('2019-01-28 10:00');
    });

    it('should display ? if invalid start time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.startTime = null;
        });

        expect(vm.selectedPeriod).toEqual("2019-01-31 ? - 2019-01-31 18:00");
    });

    it('should display ? if invalid end time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = null;
        });

        expect(vm.selectedPeriod).toEqual("2019-01-31 08:00 - 2019-01-31 ?");
    });

    it('should not submit if invalid start time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.startTime = null;
        });
        t.apply(function () {
            vm.adjustToNeutral();
        });

        expect(t.backend.lastParams.adjustPeriod()).toBeUndefined();
    });

    it('should not submit if invalid end time', function (t) {
        var now = new Date('2019-02-01T08:00:00');
        jasmine.clock().mockDate(now);
        var vm = t.createController();

        t.apply(function () {
            vm.endTime = null;
        });
        t.apply(function () {
            vm.adjustToNeutral();
        });

        expect(t.backend.lastParams.adjustPeriod()).toBeUndefined();
    });
});