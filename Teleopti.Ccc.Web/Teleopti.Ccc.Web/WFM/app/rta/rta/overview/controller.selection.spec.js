'use strict';

rtaTester.describe('RtaOverviewController', function (it, fit, xit) {
	
	it('should select site', function (t) {
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});
		var vm = t.createController();

		t.apply(function () {
			vm.siteCards[0].isSelected = true;
		});

		expect(vm.siteCards[0].isSelected).toEqual(true);
	});

	it('should select site from url', function (t) {
		t.stateParams.siteIds = ['parisId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].isSelected).toEqual(true);
	});

	it('should select team from url', function (t) {
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

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(true);
	});

	it('should open site when team in url', function (t) {
		t.stateParams.teamIds = ['redId'];
		t.backend
			.withSiteAdherence({
				Id: 'parisId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'redId'
			})
			.withTeamAdherence({
				SiteId: 'parisId',
				Id: 'greenId'
			});

		var vm = t.createController();

		expect(vm.siteCards[0].isOpen).toEqual(true);
	});

	it('should still display opened sites even though no team is selected', function (t) {
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

		expect(vm.siteCards[0].teams[0].isSelected).toEqual(false);
	});

});
