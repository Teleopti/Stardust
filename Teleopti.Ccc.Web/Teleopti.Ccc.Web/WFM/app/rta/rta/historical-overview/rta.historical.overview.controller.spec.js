'use strict';

rtaTester.describe('RtaHistoricalOverviewController', function (it, fit, xit) {
	it('should build cards', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend.with.historicalOverview(
				{
					Name: 'Denver/Avalanche',
					DisplayDays: ['20/8', '21/8', '22/8', '23/8', '24/8', '25/8', '26/8'],
					Agents: [{
						Id: '625295cf-9b4c-4915-ba66-9b5e015b257c',
						Name: 'Andeen Ashley',
						IntervalAdherence: 73,
						Days: [
							{
								Date: '20180820',
								Adherence: 50,
								WasLateForWork: true
							}
						],
						LateForWork:
							{
								Count: 2,
								TotalMinutes: 24
							}
					}
					]
				});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
		});
		
		expect(vm.cards[0].Name).toBe('Denver/Avalanche');
		expect(vm.cards[0].DisplayDays.length).toBe(7);
		expect(vm.cards[0].DisplayDays[0]).toBe('20/8');
		expect(vm.cards[0].Agents[0].Name).toBe('Andeen Ashley');
		expect(vm.cards[0].Agents[0].IntervalAdherence).toBe(73);
		expect(vm.cards[0].Agents[0].Days[0].Adherence).toBe(50);
		expect(vm.cards[0].Agents[0].Days[0].WasLateForWork).toBe(true);
		expect(vm.cards[0].Agents[0].Days[0].Color).toBe('hsl(0,0%,70%)');
		expect(vm.cards[0].Agents[0].Days[0].HistoricalAdherenceUrl).toBe(t.href('rta-historical', {personId: '625295cf-9b4c-4915-ba66-9b5e015b257c', date: '20180820'}));
		expect(vm.cards[0].Agents[0].LateForWork.Count).toBe(2);
		expect(vm.cards[0].Agents[0].LateForWork.TotalMinutes).toBe(24);
	});

	it('should empty cards when no selection', function (t) {
		t.backend
			.with.historicalOverview(
				{
					Name: 'Denver/Avalanche',
					TeamAdherence: 74,
					Agents: [{
						Name: 'Andeen Ashley',
						IntervalAdherence: 73,
						Days: [
							{
								Date: '17/08',
								Adherence: 100,
								WasLateForWork: true
							}
						],
						LateForWork:
							{
								Count: 2,
								TotalMinutes: 24
							}
					}
					]
				});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
		});

		expect(vm.cards.length).toBe(0);
	});
	
	it('should open card', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend
			.with.historicalOverview(
				{
					Name: 'Denver/Avalanche'
				});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
			vm.cards[0].toggle();
		});

		expect(vm.cards[0].isOpen).toBe(true);
	});

	it('should close card', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend
			.with.historicalOverview(
				{
					Name: 'Denver/Avalanche'
				});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
			vm.cards[0].toggle();
			vm.cards[0].toggle();
		});

		expect(vm.cards[0].isOpen).toBe(false);
	});

	it('should open organization picker', function (t) {

		var vm = t.createController();
		t.apply(function () {
			vm.toggleOrganizationPicker();
		});

		expect(vm.organizationPickerOpen).toBe(true);
	});

	it('should close organization picker', function (t) {

		var vm = t.createController();
		t.apply(function () {
			vm.toggleOrganizationPicker();
			vm.toggleOrganizationPicker();
		});

		expect(vm.organizationPickerOpen).toBe(false);
	});

	it('should send request with siteIds', function (t) {
		t.stateParams.siteIds = ['LondonId'];
		t.createController();
		expect(t.backend.lastParams.historicalOverview().siteIds).toContain('LondonId');
	});

	it('should display -- when adherence is empty', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend.with.historicalOverview(
			{
				Name: 'Denver/Avalanche',
				DisplayDays: ['2/10', '3/10', '4/10', '5/10', '6/10', '7/10', '8/10'],
				Agents: [{
					Name: 'Andeen Ashley',
					IntervalAdherence: null,
					Days: [
						{
							Date: '20180820',
							Adherence: null,
							WasLateForWork: false
						}
					],
					LateForWork:
						{
							Count: 0,
							TotalMinutes: 0
						}
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
		});

		expect(vm.cards[0].Agents[0].Days[0].Adherence).toBe('--');
	});
	
	it('should display white background when adherence is empty', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend.with.historicalOverview(
			{
				Name: 'Denver/Avalanche',
				DisplayDays: ['2/10', '3/10', '4/10', '5/10', '6/10', '7/10', '8/10'],
				Agents: [{
					Name: 'Andeen Ashley',
					IntervalAdherence: null,
					Days: [
						{
							Date: '20180820',
							Adherence: null,
							WasLateForWork: false
						}
					],
					LateForWork:
						{
							Count: 0,
							TotalMinutes: 0
						}
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
		});

		expect(vm.cards[0].Agents[0].Days[0].Color).toBe('hsl(0,0%,100%)');
	});

	it('should display white when adherence is empty', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend.with.historicalOverview(
			{
				Name: 'Denver/Avalanche',
				DisplayDays: ['2/10', '3/10', '4/10', '5/10', '6/10', '7/10', '8/10'],
				Agents: [{
					Name: 'Andeen Ashley',
					IntervalAdherence: null,
					Days: [
						{
							Date: '20180820',
							Adherence: 0,
							WasLateForWork: false
						}
					],
					LateForWork:
						{
							Count: 0,
							TotalMinutes: 0
						}
				}]
			});

		var vm = t.createController();
		t.apply(function () {
			vm.closeOrganizationPicker();
		});

		expect(vm.cards[0].Agents[0].AdherenceBarWidth).toBe(0);
	});

	it('should display loading until data is returned on page load', function (t) {
		var vm = t.createController({flush: false});

		expect(vm.loading).toEqual(true);
	});

	it('should not display loading after data is returned', function (t) {
		var vm = t.createController();

		expect(vm.loading).toEqual(false);
	});
});