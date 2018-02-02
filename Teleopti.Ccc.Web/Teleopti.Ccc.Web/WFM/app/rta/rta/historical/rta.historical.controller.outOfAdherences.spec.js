'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {

	it('should display out of adherence', function (t) {
		t.stateParams.personId = '1';
		t.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T08:15:00'
			}]
		});

		var controller = t.createController();

		expect(controller.outOfAdherences[0].StartTime).toEqual(moment('2016-10-10T08:00:00').format('LTS'));
		expect(controller.outOfAdherences[0].EndTime).toEqual(moment('2016-10-10T08:15:00').format('LTS'));
		expect(controller.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
		expect(controller.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
	});

	it('should display out of adherence', function (tester) {
		tester.stateParams.personId = '1';
		tester.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T08:15:00'
			}, {
				StartTime: '2016-10-10T09:15:00',
				EndTime: '2016-10-10T10:00:00'
			}]
		});

		var controller = tester.createController();

		expect(controller.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
		expect(controller.outOfAdherences[1].Width).toEqual((45 * 60) / (12 * 3600) * 100 + '%');
		expect(controller.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
		expect(controller.outOfAdherences[1].Offset).toEqual((15 * 60 + 7200) / (12 * 3600) * 100 + '%');
	});

	it('should display out of adherence start date when started a long time ago', function (tester) {
		tester.stateParams.personId = '1';
		tester.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-09T17:00:00'
			}]
		});

		var controller = tester.createController();

		expect(controller.outOfAdherences[0].StartTime).toEqual(moment('2016-10-09 17:00:00').format('LLL'));
	});

	it('should display out of adherence without end time', function (t) {
		t.stateParams.personId = '1';
		t.backend
			.withHistoricalAdherence({
				Now: '2016-10-10T15:00:00',
				PersonId: '1',
				AgentName: 'Mikkey Dee',
				Schedules: [{
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T09:00:00'
				}, {
					StartTime: '2016-10-10T15:00:00',
					EndTime: '2016-10-10T17:00:00'
				}],
				OutOfAdherences: [{
					StartTime: '2016-10-10T07:00:00',
					EndTime: null
				}]
			});

		var vm = t.createController();

		expect(vm.outOfAdherences.length).toEqual(1);
		expect(vm.outOfAdherences[0].Offset).toEqual('0%');
		expect(vm.outOfAdherences[0].Width).toEqual(8 / 11 * 100 + '%');
	});

	it('should display out of adherence started a long time ago within timeline ', function (t) {
		t.backend
			.withHistoricalAdherence({
				Timeline: {
					StartTime: '2018-02-02T10:00:00',
					EndTime: '2018-02-02T20:00:00'
				},
				Schedules: [{
					StartTime: '2018-02-02T11:00:00',
					EndTime: '2018-02-02T19:00:00'
				}],
				OutOfAdherences: [{
					StartTime: '2018-02-02T09:00:00',
					EndTime: '2018-02-02T11:00:00'
				}]
			});

		var vm = t.createController();

		expect(vm.outOfAdherences[0].Offset).toEqual("0%");
		expect(vm.outOfAdherences[0].Width).toEqual("10%");
	});

});