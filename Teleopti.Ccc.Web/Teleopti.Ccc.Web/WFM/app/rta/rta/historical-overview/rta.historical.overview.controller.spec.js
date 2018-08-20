'use strict';

rtaTester.describe('RtaHistoricalOverviewController', function (it, fit, xit) {
	it('should build cards', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend.with.historicalOverview(
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
			vm.applyOrganizationSelection();
		});
		
		expect(vm.cards[0].Name).toBe('Denver/Avalanche');
		expect(vm.cards[0].TeamAdherence).toBe(74);
		expect(vm.cards[0].Agents[0].Name).toBe('Andeen Ashley');
		expect(vm.cards[0].Agents[0].IntervalAdherence).toBe(73);
		expect(vm.cards[0].Agents[0].Days[0].Date).toBe('17/08');
		expect(vm.cards[0].Agents[0].Days[0].Adherence).toBe(100);
		expect(vm.cards[0].Agents[0].Days[0].WasLateForWork).toBe(true);
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
			vm.applyOrganizationSelection();
		});

		expect(vm.cards.length).toBe(0);
	});

	it('should set background color', function (t) {
		var vm = t.createController();
		//maybe refactor implentation set toone on actual object
		var color;
		t.apply(function () {
			color = vm.toneAdherence(50);
		});

		expect(color).toBe('hsl(0,0%,70%)');
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
			vm.applyOrganizationSelection();
		});
		t.apply(function () {
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
			vm.applyOrganizationSelection();
		});
		t.apply(function () {
			vm.cards[0].toggle();
		});
		t.apply(function () {
			vm.cards[0].toggle();
		});

		expect(vm.cards[0].isOpen).toBe(false);
	});
	
	xit('should send request with siteIds', function (t) {
		t.stateParams.siteIds = ['LondonId'];
		expect(t.backend.lastHistoricalOverviewRequestParams.siteIds).toContain('LondonId');
	});

});