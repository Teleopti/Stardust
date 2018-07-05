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
	
	it('should not display message until sites are returned', function (t) {
		var vm = t.createController({flush: false});

		expect(vm.displayNoSitesMessage()).toEqual(false);
	});

	it('should display loading until data is returned', function (t) {
		var vm = t.createController({flush: false});

		expect(vm.loading()).toEqual(true);
	});

	it('should not display loading after data is returned', function (t) {
		var vm = t.createController();

		expect(vm.loading()).toEqual(false);
	});
});
