'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {
	
	it('should display message if there are no sites', function (t) {
		var vm = t.createController();

		expect(vm.displayNoSitesMessage()).toEqual(true);
	});

	it('should not display message if there are sites', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});
		var vm = t.createController();

		expect(vm.displayNoSitesMessage()).toEqual(false);
	});
});
