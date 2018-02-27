'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should remove approved out of adherences', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-26T07:00:00',
				EndTime: '2018-02-26T17:00:00'
			},
			ApprovedPeriods: [{
				StartTime: '2018-02-26T08:00:00',
				EndTime: '2018-02-26T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.approvedPeriods[0].remove();
		});

		expect(t.backend.lastParams.removeApprovedPeriod()).toEqual({
			PersonId: '1',
			StartDateTime: '2018-02-26 08:00:00',
			EndDateTime: '2018-02-26 09:00:00'
		});
	});

	it('should reload data after removal approved out of adherences ', function (t) {
		t.stateParams.personId = '2';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-26T07:00:00',
				EndTime: '2018-02-26T17:00:00'
			},
			ApprovedPeriods: [{
				StartTime: '2018-02-26T08:00:00',
				EndTime: '2018-02-26T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			t.backend.clear.historicalAdherence();
			c.approvedPeriods[0].remove();
		});
		
		expect(c.approvedPeriods.length).toEqual(0);
	});
	
});