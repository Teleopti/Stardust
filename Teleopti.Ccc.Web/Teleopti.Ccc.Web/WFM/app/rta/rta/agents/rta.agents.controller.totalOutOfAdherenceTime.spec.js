'use strict';

rtaTester.describe('RtaAgentsController', function (it, fit, xit, _,
													 $fakeBackend,
													 $controllerBuilder,
													 stateParams) {

	var vm;
	
	it('should display time in rule', function () {
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

	it('should not display time in rule if not in alarm', function () {
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

	it('should display time bar based on rule time', function () {
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

	it('should not display time bar if not in alarm', function () {
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
