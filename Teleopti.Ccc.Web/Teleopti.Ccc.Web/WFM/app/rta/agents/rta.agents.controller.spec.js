'use strict';
describe('RtaAgentsController', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		$fakeBackend,
		$controllerBuilder,
		vm,
		scope;

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
		spyOn($state, 'go');

		scope = $controllerBuilder.setup('RtaAgentsController');
	}));

	it('should get agent for team', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		});

		vm = $controllerBuilder.createController().vm;

		expect(vm.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get agent states', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamName: "Team Preferences",
				SiteName: "London"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].TeamName).toEqual("Team Preferences");
		expect(vm.agents[0].SiteName).toEqual("London");
		expect(vm.agents[0].State).toEqual("Ready");
		expect(vm.agents[0].Activity).toEqual("Phone");
		expect(vm.agents[0].NextActivity).toEqual("Short break");
		expect(vm.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agents[0].Alarm).toEqual("In Adherence");
		expect(vm.agents[0].Color).toEqual("#00FF00");
		expect(vm.agents[0].TimeInState).toEqual(15473);
	});

	it('should update agent state', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamName: "Team Preferences"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		$fakeBackend
			.clearStates()
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "In Call"
			});
		c.wait(5000);

		expect(vm.agents[0].State).toEqual("In Call");
	});

	it('should set state to agent', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready",
				Activity: "Phone",
				NextActivity: "Short break",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "In Adherence",
				Color: "#00FF00",
				TimeInState: 15473
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].State).toEqual("Ready");
		expect(vm.agents[0].Activity).toEqual("Phone");
		expect(vm.agents[0].NextActivity).toEqual("Short break");
		expect(vm.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(vm.agents[0].Alarm).toEqual("In Adherence");
		expect(vm.agents[0].Color).toEqual("#00FF00");
		expect(vm.agents[0].TimeInState).toEqual(15473);
	});

	it('should display in the same order as states received', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].Name).toEqual("Charley Caper");
		expect(vm.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
		expect(vm.agents[0].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
		expect(vm.agents[1].Name).toEqual("Ashley Andeen");
		expect(vm.agents[1].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(vm.agents[1].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
	});

	it('should display agent without state received', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];

		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences",
			SiteName: "London"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.agentsInAlarm = false);

		expect(vm.agents[0].Name).toEqual("Ashley Andeen");
		expect(vm.agents[0].TeamName).toEqual("Team Preferences");
		expect(vm.agents[0].SiteName).toEqual("London");
		expect(vm.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(vm.agents[0].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
	});

	it('should go back to sites when business unit is changed', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		});

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should get adherence percentage for agent when clicked', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
		}).withAdherence({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			AdherencePercent: 99,
			LastTimestamp: "16:34"
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.getAdherenceForAgent("11610fe4-0130-4568-97de-9b5e015b2564");
		});

		expect(vm.adherence.AdherencePercent).toEqual(99);
		expect(vm.adherence.LastTimestamp).toEqual("16:34");
	});

	it('should stop polling when page is about to destroy', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController()
			.wait(5000);

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		$sessionStorage.buid = undefined;

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});

	it('should select an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';
		$fakeBackend.withAgent({
			PersonId: personId
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.selectAgent(personId);
		});

		expect(vm.isSelected(personId)).toEqual(true);
	});

	it('should unselect an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';
		$fakeBackend.withAgent({
			PersonId: personId
		});

		var c = $controllerBuilder.createController();
		vm = c.vm;

		c.apply(function() {
				vm.selectAgent(personId);
			})
			.apply(function() {
				vm.selectAgent(personId);
			});

		expect(vm.isSelected(personId)).toEqual(false);
	});

	it('should unselect previous selected agent', function() {
		var personId1 = '11610fe4-0130-4568-97de-9b5e015b2564';
		var personId2 = '6b693b41-e2ca-4ef0-af0b-9e06008d969b';
		$fakeBackend.withAgent({
				PersonId: personId1
			})
			.withAgent({
				PersonId: personId2
			});

		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(function() {
			vm.selectAgent(personId1);
		})
		c.apply(function() {
			vm.selectAgent(personId2);
		});

		expect(vm.isSelected(personId1)).toEqual(false);
	});

	it('should not show adherence updated when there is no adherence value', function() {
		var c = $controllerBuilder.createController();
		vm = c.vm;

		expect(vm.showAdherenceUpdates()).toEqual(false);
	});

	it('should display adherence updated when there is adherence value', function() {
		var c = $controllerBuilder.createController();
		vm = c.vm;
		c.apply(vm.adherencePercent = 0);

		expect(vm.showAdherenceUpdates()).toEqual(true);
	});
});
