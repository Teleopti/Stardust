'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should display current time', function (tester) {
		tester.backend.withHistoricalAdherence({
			Now: '2017-12-14T15:00:00',
			Timeline: {
				StartTime: '2017-12-14T10:00:00',
				EndTime: '2017-12-14T20:00:00'
			}
		});

		var controller = tester.createController();

		expect(controller.currentTimeOffset).toEqual('50%');
	});

	it('should display off screen far to the right when looking at yesterdays date ;)', function (tester) {
		tester.backend.withHistoricalAdherence({
			Now: '2017-12-14T15:00:00',
			Timeline: {
				StartTime: '2017-12-13T10:00:00',
				EndTime: '2017-12-13T20:00:00'
			}
		});

		var controller = tester.createController();

		expect(controller.currentTimeOffset).toBeGreaterThan('100%');
	});
	
});