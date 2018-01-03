'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should link to previous day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {First: '20180101'}
		});

		var c = t.createController();

		expect(c.previousHref).toBe(t.href('rta-historical', {personId: 1, date: '20180102'}));
	});

	it('should link to next day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {Last: '20180104'}
		});

		var c = t.createController();

		expect(c.nextHref).toBe(t.href('rta-historical', {personId: 1, date: '20180104'}));
	});

	it('should not link too far in the past', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {First: '20180103'}
		});

		var c = t.createController();

		expect(c.previousHref).toBe(null);
	});

	it('should not link to the future', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180103';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {Last: '20180103'}
		});

		var c = t.createController();

		expect(c.nextHref).toBe(null);
	});

	it('should display next day in tooltip', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171220';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {First: '20171215', Last: '20171221'}
		});

		var c = t.createController();

		expect(c.nextTooltip).toEqual('2017-12-21');
	});

	it('should display previous day in tooltip', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171220';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Navigation: {First: '20171215', Last: '20171221'}
		});

		var c = t.createController();

		expect(c.previousTooltip).toEqual('2017-12-19');
	});

});