'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should display recorded out of adherences', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180129';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Schedules: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T18:00:00'
			}],
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T08:15:00'
			}]
		});

		var c = t.createController();

		expect(c.recordedOutOfAdherences[0].StartTime).toEqual(moment('2018-01-29T08:00:00').format('LTS'));
		expect(c.recordedOutOfAdherences[0].EndTime).toEqual(moment('2018-01-29T08:15:00').format('LTS'));
	});

	it('should display recorded out of adherences positioned', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180129';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Schedules: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T16:00:00'
			}],
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});

		var c = t.createController();

		expect(c.recordedOutOfAdherences[0].Offset).toEqual("10%");
		expect(c.recordedOutOfAdherences[0].Width).toEqual("10%");
	});

	it('should display approved periods', function (t) {
		t.stateParams.personId = '1';
		t.stateParams.date = '20180129';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			Schedules: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T16:00:00'
			}],
			ApprovedPeriods: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});

		var c = t.createController();

		expect(c.approvedPeriods[0].StartTime).toEqual(moment('2018-01-29T08:00:00').format('LTS'));
		expect(c.approvedPeriods[0].EndTime).toEqual(moment('2018-01-29T09:00:00').format('LTS'));
		expect(c.approvedPeriods[0].Offset).toEqual("10%");
		expect(c.approvedPeriods[0].Width).toEqual("10%");
	});

});