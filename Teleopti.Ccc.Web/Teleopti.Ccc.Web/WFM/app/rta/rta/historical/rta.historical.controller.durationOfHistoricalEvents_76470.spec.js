'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should display duration', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				Duration: '01:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.cards[0].Items[0].Duration).toEqual('01:00:00');
	});

	it('should display durations', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				Duration: '01:00:00'
			},
				{
					Duration: '02:00:00'
				}]
		});

		var vm = t.createController();

		expect(vm.cards[0].Items[0].Duration).toEqual('01:00:00');
		expect(vm.cards[0].Items[1].Duration).toEqual('02:00:00');
	});
});