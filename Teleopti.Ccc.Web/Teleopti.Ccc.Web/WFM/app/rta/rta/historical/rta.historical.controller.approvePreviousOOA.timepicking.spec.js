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
			personId: '1',
			startTime: '2018-02-06 09:00:00',
			endTime: '2018-02-06 10:00:00'
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
});