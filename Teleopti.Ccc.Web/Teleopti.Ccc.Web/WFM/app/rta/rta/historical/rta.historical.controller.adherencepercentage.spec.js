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

});