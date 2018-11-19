'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {
	it('should have permission to access historical overview', function (t) {
		t.backend.with.permissions({
			HistoricalOverview: true
		});

		var vm = t.createController();

		expect(vm.hasHistoricalOverviewPermission).toEqual(true);
	});

	it('should not have permission to access historical overview', function (t) {
		t.backend.with.permissions({
			HistoricalOverview: false
		});

		var vm = t.createController();

		expect(vm.hasHistoricalOverviewPermission).toEqual(false);
	});
});