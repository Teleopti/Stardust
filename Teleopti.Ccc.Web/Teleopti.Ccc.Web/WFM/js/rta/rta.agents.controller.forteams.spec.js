'use strict';
describe('RtaAgentsCtrl for teams', function() {
	var $interval,
		$httpBackend,
		$state,
		$sessionStorage,
		scope,
		$fakeBackend,
		$controllerBuilder;

	var stateParams = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function () {
		module(function ($provide) {
			$provide.service('$stateParams', function () {
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

		scope = $controllerBuilder.setup('RtaAgentsCtrl');

		$fakeBackend.clear();

	}));

	it('should get agents for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get agent states for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
			});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should update agent states for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "103afc66-2bfa-45f4-9823-9e06008d5062",
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready",
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call",
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply(function () {
				$fakeBackend.clearStates()
					.withState({
						PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
						State: "In Call",
					})
					.withState({
						PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
						State: "Ready",
					});
			})
			.wait(5000);

		expect(scope.agents[0].State).toEqual("In Call");
		expect(scope.agents[1].State).toEqual("Ready");
	});

	it('should set states to agents for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "103afc66-2bfa-45f4-9823-9e06008d5062"
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
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call",
				Activity: "Short break",
				NextActivity: "Phone",
				NextActivityStartTime: "\/Date(1432109700000)\/",
				Alarm: "Out of Adherence",
				Color: "#FF0000",
				TimeInState: 15473
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false");

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);

		expect(scope.agents[1].State).toEqual("In Call");
		expect(scope.agents[1].Activity).toEqual("Short break");
		expect(scope.agents[1].NextActivity).toEqual("Phone");
		expect(scope.agents[1].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[1].Alarm).toEqual("Out of Adherence");
		expect(scope.agents[1].Color).toEqual("#FF0000");
		expect(scope.agents[1].TimeInState).toEqual(15473);
	});

	it('should filter agent state updates with agentFilter ', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withState({
				PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
				State: "In Call",
			});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply("filterText = 'Caper'")
			.apply(function() {
				$fakeBackend.clearStates()
					.withState({
						PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
						State: "Ready",
					});
			})
			.wait(5000);

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Ready");
	});

	it('should go back to sites when business unit is changed', function() {
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should stop polling when page is about to destroy', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		$fakeBackend.withAgent({
				Name: "Ashley Andeen",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			})
			.withAgent({
				Name: "Charley Caper",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
			});

		$controllerBuilder.createController();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should get adherence percentage for agent when clicked', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
			})
			.withAdherence({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				AdherencePercent: 99,
				LastTimestamp: "16:34"
			});

		$controllerBuilder.createController()
			.apply(function() {
				scope.getAdherenceForAgent("11610fe4-0130-4568-97de-9b5e015b2564");
			});

		expect(scope.adherence.AdherencePercent).toEqual(99);
		expect(scope.adherence.LastTimestamp).toEqual("16:34");
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		$sessionStorage.buid = undefined;
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});

});
