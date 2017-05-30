'use strict';
describe('RtaAgentsController', function() {
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

	beforeEach(function() {
		module(function($provide) {
			$provide.factory('$stateParams', function() {
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

		$fakeBackend.clear();

		scope = $controllerBuilder.setup('RtaAgentsController');

		spyOn($state, 'go');
	}));

	it('should display time in rule', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInAlarm: 1 * 60,
				TimeInRule: 11 * 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates[0].TimeInRule).toEqual(11 * 60);
	});

	it('should not display time in rule if not in alarm', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInRule: 11 * 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].TimeInRule).toEqual(null);
	});

	it('should display time bar based on rule time', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInAlarm: 20 * 60,
				TimeInRule: 30 * 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates[0].ShiftTimeBar).toEqual("12.5%");
	});

	it('should not display time bar if not in alarm', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInRule: 30 * 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = false);

		expect(vm.agentStates[0].ShiftTimeBar).toEqual("0%");
	});

});
