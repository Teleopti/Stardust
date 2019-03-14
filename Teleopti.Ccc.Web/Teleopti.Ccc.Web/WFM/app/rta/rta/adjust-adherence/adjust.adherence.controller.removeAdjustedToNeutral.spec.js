'use strict';

rtaTester.describe('AdjustAdherenceController', function (it, fit, xit) {
    it('should remove adjusted period', function (t) {
        t.backend.with
            .adjustedPeriods({
                StartTime: '2019-03-13T09:00:00',
                EndTime: '2019-03-13T10:00:00'
            });
        var vm = t.createController();
        
        t.apply(function(){
            vm.adjustedPeriods[0].remove();
        });

        expect(t.backend.lastParams.cancelAdjustedPeriod()).toEqual({
            StartDateTime: '2019-03-13 09:00',
            EndDateTime: '2019-03-13 10:00'
        });
    });

    it('should load after removal', function (t) {
        t.backend.with
            .adjustedPeriods({
                StartTime: '2019-03-14T09:00:00',
                EndTime: '2019-03-14T10:00:00'
            });
        var vm = t.createController();

        t.apply(function(){
            t.backend.clear.adjustedPeriods();
            vm.adjustedPeriods[0].remove();
        });

        expect(vm.adjustedPeriods.length).toEqual(0);
    });
});