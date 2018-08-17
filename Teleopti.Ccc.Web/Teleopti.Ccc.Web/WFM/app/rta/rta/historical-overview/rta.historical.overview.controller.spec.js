'use strict';

rtaTester.describe('RtaHistoricalOverviewController', function (it, fit, xit) {
	it('should build cards', function (t) {
		t.stateParams.teamIds = ['teamAvalancheId'];
		t.backend
			.withHistoricalTeamAdherence(
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
});