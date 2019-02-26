'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	it('should display recorded out of adherences', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T19:00:00'
			},
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
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});

		var c = t.createController();

		expect(c.recordedOutOfAdherences[0].Offset).toEqual("10%");
		expect(c.recordedOutOfAdherences[0].Width).toEqual("10%");
	});

	it('should highlight recorded out of adherence and open card when clicked', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
		});

		expect(c.recordedOutOfAdherences[0].highlight).toBe(true);
		expect(c.openRecordedAdherences).toBe(true);
	});

	it('should remove highlight from other recorded out of adherences when clicked', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}, {
				StartTime: '2018-01-29T09:00:00',
				EndTime: '2018-01-29T10:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
			c.recordedOutOfAdherences[1].click();
		});

		expect(c.recordedOutOfAdherences[0].highlight).toBe(false);
		expect(c.recordedOutOfAdherences[1].highlight).toBe(true);
	});

	it('should display approved periods', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
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


	it('should highlight approved interval and open card when clicked', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			ApprovedPeriods: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.approvedPeriods[0].click();
		});

		expect(c.approvedPeriods[0].highlight).toBe(true);
		expect(c.openRecordedAdherences).toBe(false);
		expect(c.openApprovedPeriods).toBe(true);
	});

	it('should remove highlight from other approved interval when clicked', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			ApprovedPeriods: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}, {
				StartTime: '2018-01-29T09:00:00',
				EndTime: '2018-01-29T10:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.approvedPeriods[0].click();
			c.approvedPeriods[1].click();
		});

		expect(c.approvedPeriods[0].highlight).toBe(false);
		expect(c.approvedPeriods[1].highlight).toBe(true);
	});

	it('should not remove highlight from approved when clicking recorded out of adherence', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}],
			ApprovedPeriods: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
			c.approvedPeriods[0].click();
		});

		expect(c.recordedOutOfAdherences[0].highlight).toBe(true);
		expect(c.openRecordedAdherences).toBe(true);
		expect(c.approvedPeriods[0].highlight).toBe(true);
		expect(c.openApprovedPeriods).toBe(true);
	});

	it('should set up approve form and open approved intervals when recorded out of adherence is clicked', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
		});

		expect(c.openRecordedAdherences).toBe(true);
		expect(c.openApprovedPeriods).toBe(true);
		expect(c.openApproveForm).toBe(true);
		expect(c.approveStartTime).toEqual(moment('2018-01-29T08:00:00').toDate());
		expect(c.approveEndTime).toEqual(moment('2018-01-29T09:00:00').toDate());
	});

	it('should close the approve form on cancel click', function (t) {
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
			c.cancelApprove();
		});

		expect(c.openApproveForm).toBe(false);
	});

	it('should submit approve form on submit click', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.recordedOutOfAdherences[0].click();
			c.submitApprove();
		});

		expect(t.backend.lastParams.approvePeriod()).toEqual({
			PersonId: '1',
			StartDateTime: '2018-01-29 08:00:00',
			EndDateTime: '2018-01-29 09:00:00'
		});
	});

	it('should refresh view after submit of approval', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			}
		});
		var vm = t.createController();

		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-01-29T07:00:00',
				EndTime: '2018-01-29T17:00:00'
			},
			ApprovedPeriods: [{
				StartTime: '2018-01-29T08:00:00',
				EndTime: '2018-01-29T09:00:00'
			}]
		});
		t.apply(function () {
			vm.approveStartTime = moment('2018-01-29T08:00:00').toDate();
			vm.approveEndTime = moment('2018-01-29T09:00:00').toDate();
			vm.submitApprove();
		});

		expect(vm.approvedPeriods[0].StartTime).toEqual(moment('2018-01-29T08:00:00').format('LTS'));
		expect(vm.approvedPeriods[0].EndTime).toEqual(moment('2018-01-29T09:00:00').format('LTS'));
	});

});