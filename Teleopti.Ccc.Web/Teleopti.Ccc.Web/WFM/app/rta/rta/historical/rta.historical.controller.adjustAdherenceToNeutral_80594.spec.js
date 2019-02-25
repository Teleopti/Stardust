'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should not display adjusted to neutral adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2019-02-25T07:00:00',
				EndTime: '2019-02-25T19:00:00'
			}
		});

		var vm = t.createController();

		expect(vm.showAdjustedToNeutralAdherences).toBe(false);
	});

	it('should display adjusted to neutral adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2019-02-25T07:00:00',
				EndTime: '2019-02-25T19:00:00'
			},
			AdjustedToNeutralAdherences: [{
				StartTime: '2019-02-25T08:00:00',
				EndTime: '2019-02-25T09:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.showAdjustedToNeutralAdherences).toBe(true);
	});
	
	it('should have history with adjusted to neutral adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2019-02-25T07:00:00',
				EndTime: '2019-02-25T19:00:00'
			},
			AdjustedToNeutralAdherences: [{
				StartTime: '2019-02-25T08:00:00',
				EndTime: '2019-02-25T09:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.adjustedToNeutralAdherences[0].StartTime).toEqual(moment('2019-02-25T08:00:00').format('LTS'));
		expect(vm.adjustedToNeutralAdherences[0].EndTime).toEqual(moment('2019-02-25T09:00:00').format('LTS'));
	});

	it('should have history with multiple adjusted to neutral adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2019-02-25T07:00:00',
				EndTime: '2019-02-25T19:00:00'
			},
			AdjustedToNeutralAdherences: [{
				StartTime: '2019-02-25T09:00:00',
				EndTime: '2019-02-25T10:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.adjustedToNeutralAdherences[0].StartTime).toEqual(moment('2019-02-25T09:00:00').format('LTS'));
		expect(vm.adjustedToNeutralAdherences[0].EndTime).toEqual(moment('2019-02-25T10:00:00').format('LTS'));
	});

	it('should not have history with adjusted to neutral adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2019-02-25T07:00:00',
				EndTime: '2019-02-25T19:00:00'
			}
		});

		var vm = t.createController();

		expect(vm.adjustedToNeutralAdherences.length).toEqual(0);
	});
	
	
});