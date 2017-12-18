'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should display adherence percentage', function (t) {
		t.stateParams.personId = '1';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			AdherencePercentage: '90'
		});

		var c = t.createController();

		expect(c.adherencePercentage).toEqual("90");
	});

	it('should display adherence percentage 0', function (t) {
		t.stateParams.personId = '1';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			AdherencePercentage: '0'
		});

		var c = t.createController();

		expect(c.showAdherencePercentage).toEqual(true);
	});

	it('should not display adherence percentage when null', function (t) {
		t.stateParams.personId = '1';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			AdherencePercentage: null
		});

		var c = t.createController();

		expect(c.showAdherencePercentage).toEqual(false);
	});

});