'use strict';

rtaTester.describe('RtaHistoricalController', function (it, fit, xit) {
	
	it('should display schedule', function (tester) {
		tester.stateParams.personId = '1';
		tester.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T17:00:00'
			}],
			Timeline: {
				StartTime: '2016-10-10T07:00:00',
				EndTime: '2016-10-10T18:00:00'
			}
		});

		var controller = tester.createController();

		expect(controller.agentsFullSchedule[0].Color).toEqual('lightgreen');
		expect(controller.agentsFullSchedule[0].StartTime.format('HH:mm:ss')).toEqual('08:00:00');
		expect(controller.agentsFullSchedule[0].EndTime.format('HH:mm:ss')).toEqual('17:00:00');
		expect(controller.agentsFullSchedule[0].Width).toEqual((9 / 11 * 100) + '%');
		expect(controller.agentsFullSchedule[0].Offset).toEqual((1 / 11 * 100) + '%');
	});

	it('should display schedule', function (tester) {
		tester.stateParams.personId = '1';
		tester.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T12:00:00'
			}, {
				StartTime: '2016-10-10T12:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			Timeline: {
				StartTime: '2016-10-10T07:00:00',
				EndTime: '2016-10-10T19:00:00'
			}
		});

		var controller = tester.createController();

		expect(controller.agentsFullSchedule[0].Width).toEqual(4 / 12 * 100 + '%');
		expect(controller.agentsFullSchedule[0].Offset).toEqual(1 / 12 * 100 + '%');
		expect(controller.agentsFullSchedule[1].Width).toEqual(6 / 12 * 100 + '%');
		expect(controller.agentsFullSchedule[1].Offset).toEqual(5 / 12 * 100 + '%');
	});

	it('should display schedule', function (tester) {
		tester.stateParams.personId = '1';
		tester.backend.withHistoricalAdherence({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T12:30:00'
			}, {
				StartTime: '2016-10-10T12:30:00',
				EndTime: '2016-10-10T18:30:00'
			}],
			Timeline: {
				StartTime: '2016-10-10T07:00:00',
				EndTime: '2016-10-10T19:30:00'
			}
		});

		var controller = tester.createController();

		expect(controller.agentsFullSchedule[0].Width).toEqual(4.5 / 12.5 * 100 + '%');
		expect(controller.agentsFullSchedule[0].Offset).toEqual(1 / 12.5 * 100 + '%');
		expect(controller.agentsFullSchedule[1].Width).toEqual(6 / 12.5 * 100 + '%');
		expect(controller.agentsFullSchedule[1].Offset).toEqual(5.5 / 12.5 * 100 + '%');
	});

});