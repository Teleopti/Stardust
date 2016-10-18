'use strict';
describe('RtaHistoricalController', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaHistoricalController');

		$fakeBackend.clear();
	}));

	it('should get agent', function() {
		var id = Math.random() * 1000 + 1
		stateParams.personId = id;
		$fakeBackend.withAgent({
			PersonId: id,
			AgentName: 'Mikkey Dee',
			Schedules: [],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.personId).toEqual(id);
		expect(vm.agentName).toEqual('Mikkey Dee');
	});

	it('should display schedule', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T17:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].DisplayStartTime).toEqual('2016-10-10 08:00:00');
		expect(vm.agentsFullSchedule[0].DisplayEndTime).toEqual('2016-10-10 17:00:00');
		expect(vm.agentsFullSchedule[0].Width).toEqual((9 / 11 * 100) + '%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual((1 / 11 * 100) + '%');
	});

	it('should display schedule', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T12:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10T12:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].Width).toEqual(4 / 12 * 100 + '%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual(1 / 12 * 100 + '%');
		expect(vm.agentsFullSchedule[1].Width).toEqual(6 / 12 * 100 + '%');
		expect(vm.agentsFullSchedule[1].Offset).toEqual(5 / 12 * 100 + '%');
	});

	it('should display schedule', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T12:30:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10T12:30:00',
				EndTime: '2016-10-10T18:30:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].Width).toEqual(4.5 / 12.5 * 100 + '%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual(1 / 12.5 * 100 + '%');
		expect(vm.agentsFullSchedule[1].Width).toEqual(6 / 12.5 * 100 + '%');
		expect(vm.agentsFullSchedule[1].Offset).toEqual(5.5 / 12.5 * 100 + '%');
	});

	it('should display out of adherence', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T08:15:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].DisplayStartTime).toEqual('2016-10-10 08:00:00');
		expect(vm.outOfAdherences[0].DisplayEndTime).toEqual('2016-10-10 08:15:00');
		expect(vm.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
		expect(vm.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
	});

	it('should display out of adherence', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
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

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].Width).toEqual((15 * 60) / (12 * 3600) * 100 + '%');
		expect(vm.outOfAdherences[0].Offset).toEqual(1 / 12 * 100 + '%');
		expect(vm.outOfAdherences[1].Width).toEqual((45 * 60) / (12 * 3600) * 100 + '%');
		expect(vm.outOfAdherences[1].Offset).toEqual((15 * 60 + 7200) / (12 * 3600) * 100 + '%');
	});

	it('should display full timeline', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			AgentName: 'Mikkey Dee',
			Schedules: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10T08:00:00',
				EndTime: '2016-10-10T09:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10T15:00:00',
				EndTime: '2016-10-10T17:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.fullTimeline[1].Time).toEqual('08:00');
		expect(vm.fullTimeline[vm.fullTimeline.length - 1].Time).toEqual('17:00');
	});

	it('should handle out of adherence without end time', function() {
		stateParams.personId = '1';
		$fakeBackend
			.withTime('2016-10-10T15:00:00')
			.withAgent({
				PersonId: '1',
				AgentName: 'Mikkey Dee',
				Schedules: [{
					Color: 'lightgreen',
					StartTime: '2016-10-10T08:00:00',
					EndTime: '2016-10-10T09:00:00'
				}, {
					Color: 'lightgreen',
					StartTime: '2016-10-10T15:00:00',
					EndTime: '2016-10-10T17:00:00'
				}],
				OutOfAdherences: [{
					StartTime: '2016-10-10T07:00:00',
					EndTime: null
				}]
			});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences.length).toEqual(1);
		expect(vm.outOfAdherences[0].Offset).toEqual('0%');
		expect(vm.outOfAdherences[0].Width).toEqual(8 / 11 * 100 + '%');
	});

	it('should display current date', function() {
		stateParams.personId = '1';
		$fakeBackend
			.withTime('2016-10-10T15:00:00')
			.withAgent({
				PersonId: '1',
				Schedules: [],
				OutOfAdherences: []
			});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.date).toBe('2016-10-10');
	});
});
