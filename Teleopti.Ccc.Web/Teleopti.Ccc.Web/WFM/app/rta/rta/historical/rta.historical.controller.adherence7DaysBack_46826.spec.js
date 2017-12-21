'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should display interval', function (t) {
		t.stateParams.personId = '1';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.interval.StartDate).toEqual("20171215");
		expect(c.interval.EndDate).toEqual("20171221");
	});

	it('should display active buttons when in interval', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171217';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledNext).toEqual(false);
		expect(c.disabledPrev).toEqual(false);
	});

	it('should display disabled Next when on end date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171221';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledNext).toEqual(true);
	});

	it('should display disabled Next when on date after end date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171222';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledNext).toEqual(true);
	});

	it('should display disabled Prev when on start date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171215';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledPrev).toEqual(true);
	});

	it('should display disabled Prev when on date before start date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171214';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Interval: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledPrev).toEqual(true);
	});
	
});