'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should fix start and end time entered manually as text', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-06T07:00:00',
				EndTime: '2018-02-06T17:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTimeString = '09:00:00';
		});
		t.apply(function () {
			vm.approveEndTimeString = '10:00:00';
		});

		expect(vm.approveStartTimeString).toEqual('09:00:00');
		expect(vm.approveEndTimeString).toEqual('10:00:00');
		expect(vm.approveStartTime).toEqual(moment('2018-02-06T09:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-06T10:00:00').toDate());
	});

	it('should fix start and end time entered manually as text', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-15T07:00:00',
				EndTime: '2018-02-15T17:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTimeString = '09:00';
		});
		t.apply(function () {
			vm.approveEndTimeString = '10:00';
		});

		expect(vm.approveStartTimeString).toEqual('09:00');
		expect(vm.approveEndTimeString).toEqual('10:00');
		expect(vm.approveStartTime).toEqual(moment('2018-02-15T09:00:00').toDate());
		expect(vm.approveEndTime).toEqual(moment('2018-02-15T10:00:00').toDate());
	});

	it('should not have start and end time when invalid input', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-15T07:00:00',
				EndTime: '2018-02-15T17:00:00'
			}
		});
		var vm = t.createController();

		t.apply(function () {
			vm.approveStartTimeString = '09:00';
		});
		t.apply(function () {
			vm.approveEndTimeString = '10:00';
		});
		t.apply(function () {
			vm.approveStartTimeString = 'invalid';
		});
		t.apply(function () {
			vm.approveEndTimeString = 'invalid';
		});

		expect(vm.approveStartTimeString).toEqual('invalid');
		expect(vm.approveEndTimeString).toEqual('invalid');
		expect(vm.approveStartTime).toEqual(undefined);
		expect(vm.approveEndTime).toEqual(undefined);
		expect(vm.approveOffset).toEqual(undefined);
		expect(vm.approveWidth).toEqual(undefined);
	});

	it('should start and end time text values when selecting recorded out of adherence', function (t) {
		t.stateParams.personId = '1';
		t.backend.with.historicalAdherence({
			Timeline: {
				StartTime: '2018-02-15T07:00:00',
				EndTime: '2018-02-15T17:00:00'
			},
			RecordedOutOfAdherences: [{
				StartTime: '2018-02-15T08:00:00',
				EndTime: '2018-02-15T09:00:00'
			}]
		});
		var vm = t.createController();

		t.apply(function () {
			vm.recordedOutOfAdherences[0].click();
		});

		expect(vm.approveStartTimeString).toEqual(moment('8', 'H').format('LTS'));
		expect(vm.approveEndTimeString).toEqual(moment('9', 'H').format('LTS'));
	});


    it('should have start time as timeline start when there is no recorded out of adherence start time', function (t) {
        t.stateParams.personId = '1';
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-01-08T07:00:00',
                EndTime: '2019-01-08T17:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: null,
                EndTime: '2019-01-08T09:00:00'
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedOutOfAdherences[0].click();
        });

        expect(vm.approveStartTimeString).toEqual(moment('7', 'H').format('LTS'));
        expect(vm.approveEndTimeString).toEqual(moment('9', 'H').format('LTS'));
    });

    it('should have start time as timeline start when there is no recorded out of adherence end time', function (t) {
        t.stateParams.personId = '1';
        t.backend.with.historicalAdherence({
            Timeline: {
                StartTime: '2019-01-08T07:00:00',
                EndTime: '2019-01-08T17:00:00'
            },
            RecordedOutOfAdherences: [{
                StartTime: '2019-01-08T08:00:00',
                EndTime: null
            }]
        });
        var vm = t.createController();

        t.apply(function () {
            vm.recordedOutOfAdherences[0].click();
        });

        expect(vm.approveStartTimeString).toEqual(moment('8', 'H').format('LTS'));
        expect(vm.approveEndTimeString).toEqual(moment('17', 'H').format('LTS'));
    });
});