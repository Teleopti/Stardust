'use strict';

rtaTester.describe('RtaHistoricalOverviewController', function (it, fit, xit) {
	it('should disable clear if no organization', function (t) {
		var vm = t.createController();

		expect(vm.clearEnabled).toBe(false);
	});

	it('should disable clear if no selection', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		});

		var vm = t.createController();

		expect(vm.clearEnabled).toBe(false);
	});

	it('should enable clear if a site selection is made', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});

		expect(vm.clearEnabled).toBe(true);
	});

	it('should disable clear when all organization is deselected', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});
		t.apply(function () {
			vm.sites[0].toggle();
		});

		expect(vm.clearEnabled).toBe(false);
	});

	it('should enable clear when multiple sites are selected', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		})
			.withOrganization({
				Id: 'ParisId'
			});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});
		t.apply(function () {
			vm.sites[1].toggle();
		});

		expect(vm.clearEnabled).toBe(true);
	});

	it('should enable clear if a team is selected', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId',
			Name: 'London',
			Teams: [{
				Id: '1',
				Name: 'Team Preferences'
			}, {
				Id: '2',
				Name: 'Team Students'
			}]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
		});

		expect(vm.clearEnabled).toBe(true);
	});

	it('should deselect all organization when clear all', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});
		t.apply(function () {
			vm.clearOrganizationSelection();
		});

		expect(vm.sites[0].isChecked).toBe(false);
	});

	it('should display empty selection text when clear all', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId',
			Name: 'London'
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});
		t.apply(function () {
			vm.clearOrganizationSelection();
		});

		expect(vm.organizationPickerSelectionText).toEqual("");
	});
	
	it('should close organization picker', function (t) {
		var vm = t.createController();
		t.apply(function () {
			vm.applyOrganizationSelection();
		});

		expect(vm.organizationPickerOpen).toBe(false);
	});

	it('should send request with siteIds', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId'
		});
		
		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].toggle();
		});
		t.apply(function () {
			vm.applyOrganizationSelection();
		});

		expect(t.backend.lastHistoricalOverviewRequestParams.siteIds).toContain('LondonId');
	});

	it('should send request with teamIds', function (t) {
		t.backend.withOrganization({
			Id: 'LondonId',
			Teams: [
				{
					Id: 'RedTeamId'
				},
				{
					Id: 'GreenTeamId'
				}
			]
		});

		var vm = t.createController();
		t.apply(function () {
			vm.sites[0].Teams[0].toggle();
		});
		t.apply(function () {
			vm.applyOrganizationSelection();
		});

		expect(t.backend.lastHistoricalOverviewRequestParams.teamIds).toContain('RedTeamId');
	});
});