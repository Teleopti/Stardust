'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should not display validation message', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-12-17T07:00:00',
				EndTime: '2018-12-17T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-12-17T08:00:00',
				EndTime: '2018-12-17T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('1970-01-01T09:00:00').toDate();
			vm.approveEndTime = moment('1970-01-01T10:00:00').toDate();
		});
		t.apply(function () {
			vm.submitApprove();
		});

		expect(vm.invalidTime).toBe(false);
		expect(vm.invalidTimeMessage).toBe(undefined);
	});

	it('should display validation message when start time after end time', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-12-17T07:00:00',
				EndTime: '2018-12-17T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-12-17T08:00:00',
				EndTime: '2018-12-17T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveEndTime = moment('2018-12-17T10:00:00').toDate();
		});
		t.apply(function () {
			vm.approveStartTime = moment('2018-12-17T11:00:00').toDate();
		});
		t.apply(function () {
			vm.submitApprove();
		});

		expect(vm.invalidTime).toBe(true);
		expect(vm.invalidTimeMessage).toBe("EndTimeMustBeGreaterOrEqualToStartTime");
	});	
	
	it('should display validation message when start time is null', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-12-17T07:00:00',
				EndTime: '2018-12-17T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-12-17T08:00:00',
				EndTime: '2018-12-17T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveEndTime = moment('2018-12-17T09:00:00').toDate();
		});
		t.apply(function () {
			vm.approveStartTime = null;
		});
		t.apply(function () {
			vm.submitApprove();
		});

		expect(vm.invalidTime).toBe(true);
		expect(vm.invalidTimeMessage).toBe("IllegalTimeInput");
	});

	it('should display validation message when end time is null', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-12-17T07:00:00',
				EndTime: '2018-12-17T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-12-17T08:00:00',
				EndTime: '2018-12-17T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveEndTime = null;
		});
		t.apply(function () {
			vm.approveStartTime = moment('2018-12-17T09:00:00').toDate();
		});
		t.apply(function () {
			vm.submitApprove();
		});

		expect(vm.invalidTime).toBe(true);
		expect(vm.invalidTimeMessage).toBe("IllegalTimeInput");

	});

	it('should display validation message when end time is undefined', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-12-17T07:00:00',
				EndTime: '2018-12-17T17:00:00'
			},
			Schedules: [{
				StartTime: '2018-12-17T08:00:00',
				EndTime: '2018-12-17T16:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTime = moment('2018-12-17T09:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = moment('2018-12-17T10:00:00').toDate();
		});
		t.apply(function () {
			vm.approveEndTime = undefined;
		});
		t.apply(function () {
			vm.submitApprove();
		});

		expect(vm.invalidTime).toBe(true);
		expect(vm.invalidTimeMessage).toBe("IllegalTimeInput");
	});
});