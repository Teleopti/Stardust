'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should display active buttons when in period', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171217';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
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
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledNext).toEqual(true);
	});

	it('should display disabled Next when on date after end date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171222';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledNext).toEqual(true);
	});

	it('should display disabled Prev when on start date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171215';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledPrev).toEqual(true);
	});

	it('should display disabled Prev when on date before start date', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171214';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.disabledPrev).toEqual(true);
	});

	it('should display next day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171220';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.nextDay).toEqual('2017-12-21');
	});

	it('should display previous day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171220';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();

		expect(c.previousDay).toEqual('2017-12-19');
	});


	it('should go to next day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171217';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();
		t.apply(function () {
			c.goToNext();
		});
		expect(t.lastGoParams.date).toBe('20171218');
	});
	
	it('should go to previous day', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20171217';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Period: { StartDate:'20171215', EndDate: '20171221'}
		});

		var c = t.createController();
		t.apply(function () {
			c.goToPrevious();
		});
		expect(t.lastGoParams.date).toBe('20171216');
	});
});