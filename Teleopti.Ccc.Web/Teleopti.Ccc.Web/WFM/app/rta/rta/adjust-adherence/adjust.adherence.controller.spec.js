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
    
    // it('should have preselected date', function (t) {
    //     var vm = t.createController();
    //    
    //     expect(vm.startDate).toBe(false);
    // });
    
    // it('should show foo', function (t) {
    //     t.backend.with.neutralizedAdherence([{
    //         StartTime: '2019-01-23T07:00:00',
    //         EndTime: '2019-01-23T08:00:00'
    //     }]);
    //     var vm = t.createController();
    //
    //     expect(vm.neutralizedPeriods[0].StartTime).toEqual(moment('2019-01-23T07:00:00').format('YYYY-MM-DD hh:mm'));
    //     expect(vm.neutralizedPeriods[0].EndTime).toEqual(moment('2019-01-23T07:00:00').format('YYYY-MM-DD hh:mm'));
    // });
});