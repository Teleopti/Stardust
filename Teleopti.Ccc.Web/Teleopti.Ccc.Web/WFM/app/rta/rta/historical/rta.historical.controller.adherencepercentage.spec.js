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

});