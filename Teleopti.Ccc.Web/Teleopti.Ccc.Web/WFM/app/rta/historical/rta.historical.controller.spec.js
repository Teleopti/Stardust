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
			Name: 'Mikkey Dee',
			Schedule: [],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.personId).toEqual(id);
		expect(vm.agentName).toEqual('Mikkey Dee');
	});

	it('should display schedule', function () {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 17:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].StartTime).toEqual('2016-10-10 08:00:00');
		expect(vm.agentsFullSchedule[0].EndTime).toEqual('2016-10-10 17:00:00');
		expect(vm.agentsFullSchedule[0].Width).toEqual('100%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual('0%');
	});

	it('should display schedule', function () {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 12:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10 12:00:00',
				EndTime: '2016-10-10 18:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].Width).toEqual('40%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual('0%');
		expect(vm.agentsFullSchedule[1].Width).toEqual('60%');
		expect(vm.agentsFullSchedule[1].Offset).toEqual('40%');
	});

	it('should display schedule', function () {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 12:30:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10 12:30:00',
				EndTime: '2016-10-10 18:30:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.agentsFullSchedule[0].Width).toEqual(4.5 / 10.5 * 100 + '%');
		expect(vm.agentsFullSchedule[0].Offset).toEqual('0%');
		expect(vm.agentsFullSchedule[1].Width).toEqual(6 / 10.5 * 100 + '%');
		expect(vm.agentsFullSchedule[1].Offset).toEqual(4.5 / 10.5 * 100 + '%');
	});

	it('should display out of adherence', function () {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 08:15:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].StartTime).toEqual('2016-10-10 08:00:00');
		expect(vm.outOfAdherences[0].EndTime).toEqual('2016-10-10 08:15:00');
		expect(vm.outOfAdherences[0].Width).toEqual('2.5%');
		expect(vm.outOfAdherences[0].Offset).toEqual('0%');
	});

	it('should display out of adherence', function () {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 18:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 08:15:00'
			}, {
				StartTime: '2016-10-10 09:15:00',
				EndTime: '2016-10-10 10:00:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.outOfAdherences[0].Width).toEqual('2.5%');
		expect(vm.outOfAdherences[0].Offset).toEqual('0%');
		expect(vm.outOfAdherences[1].Width).toEqual('7.5%');
		expect(vm.outOfAdherences[1].Offset).toEqual('12.5%');
	});

	it('should display full timeline', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 09:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10 15:00:00',
				EndTime: '2016-10-10 17:00:00'
			}],
			OutOfAdherences: []
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.fullTimeline[0].Time).toEqual('08:00');
		expect(vm.fullTimeline[vm.fullTimeline.length - 1].Time).toEqual('17:00');
	});

	it('should display longer timeline if out of adherence after of shift', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 09:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10 15:00:00',
				EndTime: '2016-10-10 17:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10 18:00:00',
				EndTime: '2016-10-10 18:05:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.fullTimeline[0].Time).toEqual('08:00');
		expect(vm.fullTimeline[vm.fullTimeline.length - 1].Time).toEqual('18:00');
	});

	it('should display longer timeline if out of adherence before of shift', function() {
		stateParams.personId = '1';
		$fakeBackend.withAgent({
			PersonId: '1',
			Name: 'Mikkey Dee',
			Schedule: [{
				Color: 'lightgreen',
				StartTime: '2016-10-10 08:00:00',
				EndTime: '2016-10-10 09:00:00'
			}, {
				Color: 'lightgreen',
				StartTime: '2016-10-10 15:00:00',
				EndTime: '2016-10-10 17:00:00'
			}],
			OutOfAdherences: [{
				StartTime: '2016-10-10 07:00:00',
				EndTime: '2016-10-10 07:05:00'
			}]
		});

		var vm = $controllerBuilder.createController().vm;

		expect(vm.fullTimeline[0].Time).toEqual('07:00');
		expect(vm.fullTimeline[vm.fullTimeline.length - 1].Time).toEqual('17:00');
	});
});
