'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should link to previous day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.with.historicalAdherence({
			PersonId: '1',
			Navigation: {First: '20180101'}
		});

		var c = t.createController();

		expect(c.previousHref).toBe(t.href('rta-historical', {personId: 1, date: '20180102'}));
	});

	it('should link to next day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.with.historicalAdherence({
			PersonId: '1',
			Navigation: {Last: '20180104'}
		});

		var c = t.createController();

		expect(c.nextHref).toBe(t.href('rta-historical', {personId: 1, date: '20180104'}));
	});

	it('should not link too far in the past', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.with.historicalAdherence({
			PersonId: '1',
			Navigation: {First: '20180103'}
		});

		var c = t.createController();

		expect(c.previousHref).toBe(undefined);
	});

	it('should not link to the future', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.with.historicalAdherence({
			PersonId: '1',
			Navigation: {Last: '20180103'}
		});

		var c = t.createController();

		expect(c.nextHref).toBe(undefined);
	});
});