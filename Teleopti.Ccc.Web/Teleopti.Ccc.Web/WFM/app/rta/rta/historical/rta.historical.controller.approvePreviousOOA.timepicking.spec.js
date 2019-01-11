'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should submit approve form start and end time entered manually', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T07:00:00',
				EndTime: '2018-02-06T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T16:00:00'
			}]
		});
		var c = t.createController();

		t.apply(function () {
			c.approveStartTime = moment('1970-01-01T09:00:00').toDate();
			c.approveEndTime = moment('1970-01-01T10:00:00').toDate();
		});
		t.apply(function () {
			c.submitApprove();
		});

		expect(t.backend.lastParams.approvePeriod()).toEqual({
			PersonId: '1',
			StartDateTime: '2018-02-06 09:00:00',
			EndDateTime: '2018-02-06 10:00:00'
		});
	});

	it('should fix start and end time entered manually', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T07:00:00',
				EndTime: '2018-02-06T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T09:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T10:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T09:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-06T10:00:00').toDate());
	});

	it('should fix start and end time entered manually - night shift', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T20:00:00',
				EndTime: '2018-02-07T04:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T21:00:00',
				EndTime: '2018-02-07T03:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T23:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T01:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T23:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-07T01:00:00').toDate());
	});

	it('should fix start and end time entered manually - night shift', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T20:00:00',
				EndTime: '2018-02-07T04:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T21:00:00',
				EndTime: '2018-02-07T03:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T22:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T23:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T22:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-06T23:00:00').toDate());
	});

	it('should fix start and end time entered manually - night shift', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T20:00:00',
				EndTime: '2018-02-07T04:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T21:00:00',
				EndTime: '2018-02-07T03:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T01:45:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T02:30:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-07T01:45:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-07T02:30:00').toDate());
	});

	it('should fix start time entered manually - long time line', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T00:00:00',
				EndTime: '2018-02-07T02:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T01:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T01:00:00').toDate());
	});

	it('should fix start time entered manually - long time line', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T00:00:00',
				EndTime: '2018-02-07T03:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T02:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T01:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T02:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-07T01:00:00').toDate());
	});

	it('should fix start and end time entered manually - after time line', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T21:00:00',
				EndTime: '2018-02-06T23:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T22:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T03:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T22:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-07T03:00:00').toDate());
	});

	it('should fix start and end time entered manually - before shift', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T18:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T09:00:00',
				EndTime: '2018-02-06T17:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T08:15:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T08:30:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T08:15:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-06T08:30:00').toDate());
	});

	it('should fix start time entered manually - plus 1 hour before shift', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T01:00:00',
				EndTime: '2018-02-06T12:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T02:00:00',
				EndTime: '2018-02-06T11:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T00:15:00').toDate();
		});
		t.apply(function () {
			vm.approveStartTime = moment(vm.approveStartTime).add(1, 'hour').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T01:15:00').toDate());
	});

	it('should fix start and end time entered manually - reverse order', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T18:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T09:00:00',
				EndTime: '2018-02-06T17:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T10:00:00').toDate();
		});
		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T09:00:00').toDate();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T09:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-06T10:00:00').toDate());
	});

	it('should fix start time - early recorded out of adherence start', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T18:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-02-05T03:00:00',
				EndTime: '2018-02-06T08:00:00'
			}],
			Schedules: [{
				StartTime: '2018-02-06T07:00:00',
				EndTime: '2018-02-06T17:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.recordedOutOfAdherences[0].click();
		});

		expect(vm.approveStartTime).toEqual(moment('2018-02-06T08:00:00').toDate());
	});

	it('should fix end time - future recorded out of adherence end', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T18:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-02-05T03:00:00',
				EndTime: '2018-02-07T12:00:00'
			}],
			Schedules: [{
				StartTime: '2018-02-06T07:00:00',
				EndTime: '2018-02-06T17:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.recordedOutOfAdherences[0].click();
		});

		expect(vm.approveEndTime).toEqual(moment('2018-02-06T18:00:00').toDate());
	});

	it('should display period to be approved', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T08:00:00',
				EndTime: '2018-02-06T18:00:00'
			},
			Schedules: [{
				StartTime: '2018-02-06T09:00:00',
				EndTime: '2018-02-06T17:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T10:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T11:00:00').toDate();
		});

		expect(vm.approveOffset).toEqual("20%");
		expect(vm.approveWidth).toEqual("10%");
	});

	it('should display period to be approved - changed reverse order', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T10:00:00',
				EndTime: '2018-02-06T20:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveEndTime = moment('1970-01-01T15:00:00').toDate();
		});
		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T13:00:00').toDate();
		});

		expect(vm.approveOffset).toEqual("30%");
		expect(vm.approveWidth).toEqual("20%");
	});

	it('should remove start and end time on cancel', function (t) {
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
		var vm = t.createController();

		t.apply(function () {
			vm.recordedOutOfAdherences[0].click();
		});
		t.apply(function () {
			vm.cancelApprove();
		});

		expect(vm.approveStartTime).toEqual(undefined);
		expect(vm.approveEndTime).toEqual(undefined);
		expect(vm.approveOffset).toEqual(undefined);
		expect(vm.approveWidth).toEqual(undefined);
	});

	it('should provide the same date object for angular binding', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T10:00:00',
				EndTime: '2018-02-06T20:00:00'
			}
		});
		var vm = t.createController();
		var startTime = moment('2018-02-06T11:00:00').toDate();
		var endTime = moment('2018-02-06T12:00:00').toDate();

		t.apply(function () {
			vm.approveStartTime = startTime;
		});
		t.apply(function () {
			vm.approveEndTime = endTime;
		});

		expect(vm.approveStartTime === startTime).toBe(true);
		expect(vm.approveEndTime === endTime).toBe(true);
	});

    it('should set end time to timeline end time when full day out of adherence', function (t) {
        t.stateParams.personId = '1';
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-01-10T08:00:00',
                EndTime: '2019-01-10T18:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: null,
                EndTime: null
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedOutOfAdherences[0].click();
        });

        expect(vm.approveEndTime).toEqual(moment('2019-01-10T18:00:00').toDate());
    });
	
});