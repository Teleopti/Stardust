'use strict';
rtaTester.describe('RtaAgentsController', function(it, fit, xit, _,
												   $state,
												   $fakeBackend,
												   $controllerBuilder,
												   stateParams) {
	var vm;

	it('should display states in alarm only', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should display states in alarm only for site', function() {
		stateParams.siteIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "Break",
				TimeInAlarm: 0
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				SiteId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "Break",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(1);
		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[0].State).toEqual("Break");
	});

	it('should display nothing', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				State: "Break",
				TimeInAlarm: 0
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates.length).toEqual(0);
	});

	it('should display states with alarm time in desc order when agentsInAlarm is turned on', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgentState({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInAlarm: 30
			})
			.withAgentState({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TimeInAlarm: 60
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.showInAlarm = true);

		expect(vm.agentStates[0].Name).toEqual("Charley Caper");
		expect(vm.agentStates[1].Name).toEqual("Ashley Andeen");
	});

	it('should show all agents if it is specified by url', function() {
		stateParams.showAllAgents = true;

		vm = $controllerBuilder.createController().vm;

		expect(vm.showInAlarm).toEqual(false);
	});

});
