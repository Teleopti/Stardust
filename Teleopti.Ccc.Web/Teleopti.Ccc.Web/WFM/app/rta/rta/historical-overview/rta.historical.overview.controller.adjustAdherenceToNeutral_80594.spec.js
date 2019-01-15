'use strict';

rtaTester.fdescribe('RtaHistoricalOverviewController', function (it, fit, xit) {
	it('should have permission to view adjust adherence', function (t) {
		t.backend.with.permissions({
			AdjustAdherence: true
		});

		var c = t.createController();

		expect(c.hasAdjustAdherencePermission).toEqual(true);
	});	
	
	it('should not have permission to view adjust adherence', function (t) {
		t.backend.with.permissions({
            AdjustAdherence: false
		});

		var c = t.createController();

		expect(c.hasAdjustAdherencePermission).toEqual(false);
	});
});