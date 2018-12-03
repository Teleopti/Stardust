'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should display late for work', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				LateForWorkMinutes: 30,
				Time: '2018-05-28T10:30:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork.minutes).toEqual(30);
	});

	it('should display late for work', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				LateForWorkMinutes: 60,
				Time: '2018-05-28T11:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork.minutes).toEqual(60);
	});

	it('should display late for work for first change', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				Time: '2018-05-28T10:00:00'
			}, {
				LateForWorkMinutes: 60,
				Time: '2018-05-28T11:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork.minutes).toEqual(60);
	});

	it('should not display late for work if there is none', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				Time: '2018-05-28T10:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork).toBeUndefined();
	});

	it('should display late for work positioned', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				LateForWorkMinutes: 60,
				Time: '2018-05-28T11:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork.offset).toEqual('10%');
	});

	it('should display late for work positioned', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				LateForWorkMinutes: 60,
				Time: '2018-05-28T12:00:00'
			}]
		});

		var vm = t.createController();

		expect(vm.lateForWork.offset).toEqual('20%');
	});

	it('should highlight diamond on click late for work', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				Time: '2018-05-28T10:00:00'
			}, {
				LateForWorkMinutes: 120,
				Time: '2018-05-28T12:00:00'
			}]
		});
		var vm = t.createController();

		vm.lateForWork.click();

		expect(vm.diamonds[1].highlight).toEqual(true);
		expect(vm.cards[0].isOpen).toEqual(true);
		expect(vm.cards[0].Items[0].highlight).toEqual(false);
		expect(vm.cards[0].Items[1].highlight).toEqual(true);
	});

	it('should display late for work on card item', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-05-28T10:00:00',
				EndTime: '2018-05-28T20:00:00'
			},
			Changes: [{
				LateForWorkMinutes: 30,
				Time: '2018-05-28T10:30:00'
			}]
		});

		var vm = t.createController();

		expect(vm.cards[0].Items[0].lateForWorkMinutes).toEqual(30);
	})
});