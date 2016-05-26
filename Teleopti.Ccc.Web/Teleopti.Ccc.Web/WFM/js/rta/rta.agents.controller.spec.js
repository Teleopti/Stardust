'use strict';
describe('RtaAgentsCtrl', function() {
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

	it('should get agent for team', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		});

		$controllerBuilder.createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should display site name London', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			SiteName: "London",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController();

		expect(scope.siteName).toEqual("London");
	});

	it('should display team name Team Preferences', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences"
		});

		$controllerBuilder.createController();

		expect(scope.teamName).toEqual("Team Preferences");
	});

	it('should get agent states', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		})
		.withState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			Color: "#00FF00",
			TimeInState: 15473
		});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should update agent state', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
				TeamName: "Team Preferences"
			})
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});

		var c = $controllerBuilder.createController()
			.apply('agentsInAlarm = false');
		$fakeBackend
			.clearStates()
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "In Call"
			});
		c.wait(5000);

		expect(scope.agents[0].State).toEqual("In Call");
	});

	it('should set state to agent', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
		.withState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			Color: "#00FF00",
			TimeInState: 15473
		});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].Color).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should display in the same order as states received', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
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

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Name).toEqual("Charley Caper");
		expect(scope.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
		expect(scope.agents[0].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
		expect(scope.agents[1].Name).toEqual("Ashley Andeen");
		expect(scope.agents[1].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[1].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
	});

	it('should display agent without state received', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController()
			.apply('agentsInAlarm = false');

		expect(scope.agents[0].Name).toEqual("Ashley Andeen");
		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[0].TeamId).toEqual("34590a63-6331-4921-bc9f-9b5e015ab495");
	});

	it('should filter agent name with agentFilter', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		})
		.withAgent({
			Name: "Charley Caper",
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});

		$controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Charley"');

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent state updates with agentFilter ', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		});
		$fakeBackend.withState({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "In Call"
		});

		var c = $controllerBuilder.createController()
			.apply("agentsInAlarm = false")
			.apply('filterText = "Ashley"');
		$fakeBackend
			.clearStates()
			.withState({
				PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
				State: "Ready"
			});
		c.wait(5000);

		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
		expect(scope.filteredData[0].State).toEqual("Ready");
	});

	it('should go back to sites when business unit is changed', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		$fakeBackend.withAgent({
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		});
		spyOn($state, 'go');

		$controllerBuilder.createController()
			.apply(function() {
				$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
			});

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should get adherence percentage for agent when clicked', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		$fakeBackend.withAgent({
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
		}).withAdherence({
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

	it('should stop polling when page is about to destroy', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
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
		spyOn($state, 'go');

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

		$controllerBuilder.createController()
			.apply(function() { scope.selectAgent(personId); });

		expect(scope.isSelected(personId)).toEqual(true);
	});

	it('should unselect an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';
		$fakeBackend.withAgent({
			PersonId: personId
		});

		$controllerBuilder.createController()
			.apply(function () { scope.selectAgent(personId); })
			.apply(function () { scope.selectAgent(personId); });

		expect(scope.isSelected(personId)).toEqual(false);
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

		$controllerBuilder.createController()
			.apply(function () { scope.selectAgent(personId1); })
			.apply(function () { scope.selectAgent(personId2); });

		expect(scope.isSelected(personId1)).toEqual(false);
	});

	it('should not show adherence updated when there is no adherence value', function() {
		$controllerBuilder.createController();

		expect(scope.showAdherenceUpdates()).toEqual(false);
	});

	it('should display adherence updated when there is adherence value', function() {
		$controllerBuilder.createController()
			.apply('adherencePercent = 0');

		expect(scope.showAdherenceUpdates()).toEqual(true);
	});

});
