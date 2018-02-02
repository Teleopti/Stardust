'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should get agent', function (t) {
		var id = t.randomId();
		t.stateParams.personId = id;
		t.backend.withHistoricalAdherence({
			PersonId: id,
			AgentName: 'Mikkey Dee',
			Schedules: [],
			OutOfAdherences: []
		});

		var c = t.createController();

		expect(c.personId).toEqual(id);
		expect(c.agentName).toEqual('Mikkey Dee');
	});

	it('should request with personid', function (t) {
		var id = t.randomId();
		t.stateParams.personId = id;

		t.createController();

		expect(t.backend.lastHistoricalAdherenceForPersonRequestParams.personId).toBe(id);
	});

	it('should request with date', function (t) {
		t.stateParams.date = '20171213';

		t.createController();

		expect(t.backend.lastHistoricalAdherenceForPersonRequestParams.date).toBe('20171213');
	});

	it('should display date', function (t) {
		t.stateParams.date = '20171214';
		t.backend
			.withHistoricalAdherence({
				Now: '2017-12-15T15:00:00',
			});

		var vm = t.createController();

		expect(vm.date).toBe(moment('2017-12-14').format('L'));
	});
	
	it('should display diamonds', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [],
				OutOfAdherences: [],
				Changes: [{
					Time: '2016-10-10T08:00:00',
					RuleColor: 'green'
				}]
			});

		var vm = t.createController();

		expect(vm.diamonds[0].Color).toEqual('green');
	});

	it('should display diamonds at offset', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [],
				OutOfAdherences: [],
				Changes: [{
					Time: '2016-10-10T08:00:00',
					Activity: 'phone',
					Rule: 'In Call',
					State: 'Ready',
					Adherence: 'In adherence'
				}],
				Timeline: {
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T17:00:00'
				}
			});

		var vm = t.createController();

		expect(vm.diamonds[0].Offset).toEqual(1 / 11 * 100 + '%')
	});

	it('should display cards with changes', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					Name: 'phone',
					Color: 'lightgreen',
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}, {
					Name: 'break',
					Color: 'red',
					StartTime: '2016-10-10T11:00:00',
					EndTime: '2016-10-10T11:10:00'
				}, {
					Name: 'phone',
					Color: 'lightgreen',
					StartTime: '2016-10-10T11:10:00',
					EndTime: '2016-10-10T14:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T08:00:00',
					Activity: 'phone',
					ActivityColor: 'lightgreen',
					Rule: 'In Call',
					RuleColor: 'darkgreen',
					State: 'Ready',
					Adherence: 'In adherence',
					AdherenceColor: 'green'
				}, {
					Time: '2016-10-10T08:15:00',
					Activity: 'phone',
					ActivityColor: 'lightgreen',
					Rule: 'ACW',
					RuleColor: 'darkgreen',
					State: 'Ready',
					Adherence: 'In adherence',
					AdherenceColor: 'green'
				}, {
					Time: '2016-10-10T11:02:00',
					Activity: 'break',
					ActivityColor: 'red',
					Rule: 'Short Break',
					RuleColor: 'darkred',
					State: 'Logged off',
					Adherence: 'In adherence',
					AdherenceColor: 'green'
				}, {
					Time: '2016-10-10T11:10:00',
					Activity: 'phone',
					ActivityColor: 'lightgreen',
					Rule: 'In Call',
					RuleColor: 'darkgreen',
					State: 'Ready',
					Adherence: 'In adherence',
					AdherenceColor: 'green'
				}]
			});

		var vm = t.createController();

		expect(vm.cards.length).toEqual(3);
		expect(vm.cards[0].Header).toEqual('phone 08:00 - 11:00');
		expect(vm.cards[0].Color).toEqual('lightgreen');
		expect(vm.cards[1].Header).toEqual('break 11:00 - 11:10');
		expect(vm.cards[1].Color).toEqual('red');
		expect(vm.cards[2].Header).toEqual('phone 11:10 - 14:00');
		expect(vm.cards[2].Color).toEqual('lightgreen');

		expect(vm.cards[0].Items.length).toEqual(2);
		expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T08:00:00');
		expect(vm.cards[0].Items[0].Activity).toEqual('phone');
		expect(vm.cards[0].Items[0].ActivityColor).toEqual('lightgreen');
		expect(vm.cards[0].Items[0].Rule).toEqual('In Call');
		expect(vm.cards[0].Items[0].RuleColor).toEqual('darkgreen');
		expect(vm.cards[0].Items[0].State).toEqual('Ready');
		expect(vm.cards[0].Items[0].Adherence).toEqual('In adherence');
		expect(vm.cards[0].Items[0].AdherenceColor).toEqual('green');
		expect(vm.cards[0].Items[1].Time).toEqual('2016-10-10T08:15:00');
		expect(vm.cards[0].Items[1].Activity).toEqual('phone');
		expect(vm.cards[0].Items[1].ActivityColor).toEqual('lightgreen');
		expect(vm.cards[0].Items[1].Rule).toEqual('ACW');
		expect(vm.cards[0].Items[1].RuleColor).toEqual('darkgreen');
		expect(vm.cards[0].Items[1].State).toEqual('Ready');
		expect(vm.cards[0].Items[1].Adherence).toEqual('In adherence');
		expect(vm.cards[0].Items[1].AdherenceColor).toEqual('green');

		expect(vm.cards[1].Items.length).toEqual(1);
		expect(vm.cards[1].Items[0].Time).toEqual('2016-10-10T11:02:00');
		expect(vm.cards[1].Items[0].Activity).toEqual('break');
		expect(vm.cards[1].Items[0].ActivityColor).toEqual('red');
		expect(vm.cards[1].Items[0].Rule).toEqual('Short Break');
		expect(vm.cards[1].Items[0].RuleColor).toEqual('darkred');
		expect(vm.cards[1].Items[0].State).toEqual('Logged off');
		expect(vm.cards[1].Items[0].Adherence).toEqual('In adherence');
		expect(vm.cards[1].Items[0].AdherenceColor).toEqual('green');

		expect(vm.cards[2].Items.length).toEqual(1);
		expect(vm.cards[2].Items[0].Time).toEqual('2016-10-10T11:10:00');
		expect(vm.cards[2].Items[0].Activity).toEqual('phone');
		expect(vm.cards[2].Items[0].ActivityColor).toEqual('lightgreen');
		expect(vm.cards[2].Items[0].Rule).toEqual('In Call');
		expect(vm.cards[2].Items[0].RuleColor).toEqual('darkgreen');
		expect(vm.cards[2].Items[0].State).toEqual('Ready');
		expect(vm.cards[2].Items[0].Adherence).toEqual('In adherence');
		expect(vm.cards[2].Items[0].AdherenceColor).toEqual('green');
	});

	it('should display changes before shift start in its own card', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					Name: 'phone',
					Color: 'lightgreen',
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T07:54:00',
					Activity: null,
					ActivityColor: null,
					Rule: 'Positive',
					RuleColor: 'orange',
					State: 'Ready',
					Adherence: 'Out adherence',
					AdherenceColor: 'red'
				}]
			});

		var vm = t.createController();

		expect(vm.cards[0].Header).toEqual('BeforeShiftStart');
		expect(vm.cards[0].Color).toEqual('black');
		expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T07:54:00');
		expect(vm.cards[0].Items[0].Activity).toEqual(null);
		expect(vm.cards[0].Items[0].ActivityColor).toEqual(null);
		expect(vm.cards[0].Items[0].Rule).toEqual('Positive');
		expect(vm.cards[0].Items[0].RuleColor).toEqual('orange');
		expect(vm.cards[0].Items[0].State).toEqual('Ready');
		expect(vm.cards[0].Items[0].Adherence).toEqual('Out adherence');
		expect(vm.cards[0].Items[0].AdherenceColor).toEqual('red');
	});

	it('should display changes after shift end in its own card', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					Name: 'phone',
					Color: 'lightgreen',
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T11:54:00',
					Activity: null,
					ActivityColor: null,
					Rule: 'Positive',
					RuleColor: 'orange',
					State: 'Ready',
					Adherence: 'Out adherence',
					AdherenceColor: 'red'
				}]
			});

		var vm = t.createController();

		expect(vm.cards[0].Header).toEqual('AfterShiftEnd');
		expect(vm.cards[0].Color).toEqual('black');
		expect(vm.cards[0].Items[0].Time).toEqual('2016-10-10T11:54:00');
		expect(vm.cards[0].Items[0].Activity).toEqual(null);
		expect(vm.cards[0].Items[0].ActivityColor).toEqual(null);
		expect(vm.cards[0].Items[0].Rule).toEqual('Positive');
		expect(vm.cards[0].Items[0].RuleColor).toEqual('orange');
		expect(vm.cards[0].Items[0].State).toEqual('Ready');
		expect(vm.cards[0].Items[0].Adherence).toEqual('Out adherence');
		expect(vm.cards[0].Items[0].AdherenceColor).toEqual('red');
	});

	it('should display changes on shift end in the after card', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2017-12-07T17:10:00',
				PersonId: '1',
				Schedules: [{
					StartTime: '2017-12-07T08:00:00',
					EndTime: '2017-12-07T17:00:00'
				}],
				Changes: [{
					Time: '2017-12-07T17:00:00'
				}]
			});

		var vm = t.createController();

		expect(vm.cards[0].Header).toEqual('AfterShiftEnd');
		expect(vm.cards[0].Items[0].Time).toEqual('2017-12-07T17:00:00');
	});

	it('should display closed cards by default', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T08:00:00',
				}]
			});

		var vm = t.createController();

		expect(vm.cards[0].isOpen).toEqual(false);
	});

	it('should highlight diamond and card item on click', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T08:00:00'
				}]
			});
		var vm = t.createController();

		vm.diamonds[0].click();

		expect(vm.diamonds[0].highlight).toEqual(true);
		expect(vm.cards[0].isOpen).toEqual(true);
		expect(vm.cards[0].Items[0].highlight).toEqual(true);
	});

	it('should remove highlight from other diamond and card item on click', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T09:00:00'
				}, {
					StartTime: '2016-10-10T09:00:00',
					EndTime: '2016-10-10T10:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T08:00:00'
				}, {
					Time: '2016-10-10T09:00:00'
				}]
			});
		var vm = t.createController();

		vm.diamonds[0].click();
		vm.diamonds[1].click();

		expect(vm.diamonds[0].highlight).toEqual(false);
		expect(vm.cards[0].isOpen).toEqual(true);
		expect(vm.cards[0].Items[0].highlight).toEqual(false);
		expect(vm.diamonds[1].highlight).toEqual(true);
		expect(vm.cards[1].isOpen).toEqual(true);
		expect(vm.cards[1].Items[0].highlight).toEqual(true);
	});

	it('should highlight diamond and card item on click', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				Schedules: [{
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T11:00:00'
				}],
				Changes: [{
					Time: '2016-10-10T08:00:00'
				}]
			});
		var vm = t.createController();

		vm.cards[0].Items[0].click();

		expect(vm.diamonds[0].highlight).toEqual(true);
		expect(vm.cards[0].Items[0].highlight).toEqual(true);
	});

});