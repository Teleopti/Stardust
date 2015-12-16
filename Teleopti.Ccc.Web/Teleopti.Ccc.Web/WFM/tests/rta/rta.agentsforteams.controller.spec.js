'use strict';
describe('RtaAgentsCtrl for teams', function() {
	var $q,
		$rootScope,
		$interval,
		$httpBackend,
		$controller,
		$resource,
		$state,
		$sessionStorage,
		scope;

	var stateParams = {};
	var agents = [];
	var states = [];
	var adherence = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
		agents = [{
			Name: "Ashley Andeen",
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			SiteName: "London",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences"
		}, {
			Name: "Charley Caper",
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			SiteName: "Paris",
			TeamId: "103afc66-2bfa-45f4-9823-9e06008d5062",
			TeamName: "Team Backoffice"
		}];

		states = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#00FF00",
			TimeInState: 15473
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			State: "In Call",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Short break",
			NextActivity: "Phone",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "Out of Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#FF0000",
			TimeInState: 15473
		}];

		adherence = {
			AdherencePercent: 99,
			LastTimestamp: "16:34"
		};
	});

	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				return stateParams;
			});
		});
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$interval_, _$controller_, _$resource_, _$state_, _$sessionStorage_) {
		$controller = _$controller_;
		scope = _$rootScope_.$new();
		$q = _$q_;
		$interval = _$interval_;
		$rootScope = _$rootScope_;
		$resource = _$resource_;
		$state = _$state_;
		$sessionStorage = _$sessionStorage_;
		$httpBackend = _$httpBackend_;

		$httpBackend.whenGET("../api/Adherence/ForToday?personId=11610fe4-0130-4568-97de-9b5e015b2564")
			.respond(function() {
				return [200, adherence]
			});
		$httpBackend.whenGET("../api/Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, agents]
			});
		$httpBackend.whenGET("../api/Agents/ForTeam?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, agents]
			});
		$httpBackend.whenGET("../api/Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495&teamIds=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(function() {
				return [200, agents]
			});
		$httpBackend.whenGET("../api/Agents/GetStatesForTeams?ids=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, states]
			});
		$httpBackend.whenGET("../api/Agents/GetStates?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, states]
			});
		$httpBackend.whenGET("../api/Agents/GetStatesForTeams?ids=34590a63-6331-4921-bc9f-9b5e015ab495&ids=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(function() {
				return [200, states]
			});
		$httpBackend.whenGET(/ToggleHandler\/(.*)/).respond(function() {
			return [200, {
				IsEnabled: false
			}];
		});
	}));

	var createController = function() {
		$controller('RtaAgentsCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should get agents for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should get agent states for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		states = [{
			"PersonId": "11610fe4-0130-4568-97de-9b5e015b2564"
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should update agent states for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		states[0].State = "Ready";
		states[1].State = "In Call";

		createController();
		states[0].State = "In Call";
		states[1].State = "Ready";
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.agents[0].State).toEqual("In Call");
		expect(scope.agents[1].State).toEqual("Ready");
	});

	it('should set states to agents for multiple teams', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			TeamId: "103afc66-2bfa-45f4-9823-9e06008d5062"
		}];

		states = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#00FF00",
			TimeInState: 15473
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			State: "In Call",
			StateStartTime: "\/Date(1429254905000)\/",
			Activity: "Short break",
			NextActivity: "Phone",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "Out of Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#FF0000",
			TimeInState: 15473
		}];

		createController();

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].AlarmColor).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);

		expect(scope.agents[1].State).toEqual("In Call");
		expect(scope.agents[1].StateStartTime).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[1].Activity).toEqual("Short break");
		expect(scope.agents[1].NextActivity).toEqual("Phone");
		expect(scope.agents[1].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[1].Alarm).toEqual("Out of Adherence");
		expect(scope.agents[1].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[1].AlarmColor).toEqual("#FF0000");
		expect(scope.agents[1].TimeInState).toEqual(15473);
	});

	it('should filter agent state updates with agentFilter ', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		agents = [{
			Name: "Ashley Andeen",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}, {
			Name: "Charley Caper",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];
		states[1].State = "In Call";

		createController();
		scope.$apply("filterText = 'Caper'");
		states[1].State = "Ready";
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
		expect(scope.filteredData[0].State).toEqual("Ready");
	});

	it('should go back to sites when business unit is changed', function() {
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";

		createController();
		$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
		spyOn($state, 'go');
		scope.$digest();

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should stop polling when page is about to destroy', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495", "103afc66-2bfa-45f4-9823-9e06008d5062"];
		agents = [{
			Name: "Ashley Andeen",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}, {
			Name: "Charley Caper",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];
		createController();
		$interval.flush(5000);
		$httpBackend.flush();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should get adherence percentage for agent when clicked', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564"
		}];
		adherence = {
			AdherencePercent: 99,
			LastTimestamp: "16:34"
		};

		createController();
		scope.getAdherenceForAgent(agents[0].PersonId);
		$httpBackend.flush();

		expect(scope.adherence.AdherencePercent).toEqual(99);
		expect(scope.adherence.LastTimestamp).toEqual("16:34");
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		$sessionStorage.buid = undefined;

		createController();
		$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
		spyOn($state, 'go');
		scope.$digest();

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});
});
