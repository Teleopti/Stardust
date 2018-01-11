'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {
	
	it('should open site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});

		expect(vm.siteCards[0].isOpen).toEqual(true);
		expect(vm.siteCards[0].teams[0].Id).toEqual("redId");
	});

	it('should close site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = true;
		});
		t.apply(function () {
			vm.siteCards[0].isOpen = false;
		});

		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

	it('should close site with selected team', function (t) {
		t.stateParams.teamIds = ['redId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isOpen = false;
		});

		expect(vm.siteCards[0].isOpen).toEqual(false);
	});

});
