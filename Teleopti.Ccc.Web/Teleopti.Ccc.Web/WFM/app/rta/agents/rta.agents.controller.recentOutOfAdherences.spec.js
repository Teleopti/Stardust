'use strict';
describe('RtaAgentsController', function () {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		scope,
		vm;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.factory('$stateParams', function () {
				stateParams = {};
				return stateParams;
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$interval_, _$state_, _$sessionStorage_, _FakeRtaBackend_, _ControllerBuilder_) {
		$interval = _$interval_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;
		$fakeBackend = _FakeRtaBackend_;
		$controllerBuilder = _ControllerBuilder_;

		scope = $controllerBuilder.setup('RtaAgentsController');
		
		$fakeBackend.clear();
		$fakeBackend.withToggle('RTA_SeeRecentOutOfAdherenceOccurancesToday_39145');	
		spyOn($state, 'go');
	}));

	var minutesToPercent = function (minutes) {
		return (minutes * (25 / 60)) + "%";
	}

	it('should display out of adherence', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: "2016-06-15 07:45:00"
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);
		expect(vm.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(30));
		expect(vm.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
	});

	it('should display past out of adherences', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:15:00",
					EndTime: "2016-06-15 07:30:00"
				}, {
					StartTime: "2016-06-15 07:45:00",
					EndTime: "2016-06-15 08:00:00"
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(15));
		expect(vm.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
		expect(vm.agents[0].OutOfAdherences[1].Offset).toEqual(minutesToPercent(45));
		expect(vm.agents[0].OutOfAdherences[1].Width).toEqual(minutesToPercent(15));
	});

	it('should display on going out of adherence', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:45:00",
					EndTime: null
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(45));
		expect(vm.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(15));
	});

	it('should cut on going out of adherence', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 06:45:00",
					EndTime: null
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences[0].Offset).toEqual(minutesToPercent(0));
		expect(vm.agents[0].OutOfAdherences[0].Width).toEqual(minutesToPercent(60));
	});

	it('should display on going out of adherence time', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:50:00",
					EndTime: null
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].TimeOutOfAdherence).toEqual("0:10:00");
	});

	it('should not display out of adherence time when in adherence', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:15:00",
					EndTime: "2016-06-15 07:30:00"
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].TimeOutOfAdherence).toEqual(undefined);
	});

	it('should display when out of adherence started and ended', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: "2016-06-15 07:45:00"
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences[0].StartTime).toEqual("07:30:00");
		expect(vm.agents[0].OutOfAdherences[0].EndTime).toEqual("07:45:00");
	});

	it('should display when ongoing out of adherence started', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-15 08:00')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-15 07:30:00",
					EndTime: null
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences[0].StartTime).toEqual("07:30:00");
		expect(vm.agents[0].OutOfAdherences[0].EndTime).toEqual(null);
	});

	it('should not display out of adherence ending before time window', function () {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend
			.withTime('2016-06-29 08:55')
			.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				OutOfAdherences: [{
					StartTime: "2016-06-29T07:06:18",
					EndTime: "2016-06-29T07:29:27",
				}, {
					StartTime: "2016-06-29T07:29:41",
					EndTime: "2016-06-29T07:29:58",
				}]
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].OutOfAdherences.length).toEqual(0);
	});

});
