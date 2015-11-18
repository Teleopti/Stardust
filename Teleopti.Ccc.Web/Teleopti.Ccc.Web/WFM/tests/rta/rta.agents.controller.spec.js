'use strict';
describe('RtaAgentsCtrl', function() {
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
		}];

		states = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStart: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#00FF00",
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

		$httpBackend.whenGET("../Adherence/ForToday?personId=11610fe4-0130-4568-97de-9b5e015b2564")
			.respond(function() {
				return [200, adherence];
			});
		$httpBackend.whenGET("../Agents/ForTeam?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, agents];
			});
		$httpBackend.whenGET("../Agents/GetStates?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(function() {
				return [200, states];
			});
	}));

	var createController = function() {
		$controller('RtaAgentsCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should get agent for team', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
	});

	it('should display site name London', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			SiteName: "London",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];

		createController();

		expect(scope.siteName).toEqual("London");
	});

	it('should display team name Team Preferences', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
			TeamName: "Team Preferences"
		}];

		createController();

		expect(scope.teamName).toEqual("Team Preferences");
	});

	it('should get agent states', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		states = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStart: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#00FF00",
			TimeInState: 15473
		}];

		createController();

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStart).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].AlarmColor).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should update agent state', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		states[0].State = "Ready";

		createController();
		states[0].State = "In Call";
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.agents[0].State).toEqual("In Call");
	});

	it('should set state to agent', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];
		states = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			State: "Ready",
			StateStart: "\/Date(1429254905000)\/",
			Activity: "Phone",
			NextActivity: "Short break",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "In Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#00FF00",
			TimeInState: 15473
		}];

		createController();

		expect(scope.agents[0].State).toEqual("Ready");
		expect(scope.agents[0].StateStart).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].AlarmColor).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);
	});

	it('should filter agent name with agentFilter', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			Name: "Ashley Andeen",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}, {
			Name: "Charley Caper",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];

		createController();
		scope.$apply('filterText = "Charley"');

		expect(scope.filteredData[0].Name).toEqual("Charley Caper");
	});

	it('should filter agent state updates with agentFilter ', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			Name: "Ashley Andeen",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];
		states[0].State = "In Call";

		createController();
		scope.$apply('filterText = "Ashley"');
		states[0].State = "Ready";
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
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

	it('should stop polling when page is about to destroy', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		agents = [{
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495"
		}];
		createController();
		$interval.flush(5000);
		$httpBackend.flush();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should not go back to sites overview when business unit is not initialized yet', function() {
		$sessionStorage.buid = undefined;

		createController();
		$sessionStorage.buid = "99a4b091-eb7a-4c2f-b5a6-a54100d88e8e";
		spyOn($state, 'go');
		scope.$digest();

		expect($state.go).not.toHaveBeenCalledWith('rta');
	});

	it('should select an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';

		createController();
		scope.selectAgent(personId);

		expect(scope.isSelected(personId)).toEqual(true);
	});

	it('should unselect an agent', function() {
		var personId = '11610fe4-0130-4568-97de-9b5e015b2564';

		createController();
		scope.selectAgent(personId);
		scope.selectAgent(personId);

		expect(scope.isSelected(personId)).toEqual(false);
	});

	it('should unselect previous selected agent', function() {
		var personId1 = '11610fe4-0130-4568-97de-9b5e015b2564';
		var personId2 = '6b693b41-e2ca-4ef0-af0b-9e06008d969b';

		createController();
		scope.selectAgent(personId1);
		scope.selectAgent(personId2);

		expect(scope.isSelected(personId1)).toEqual(false);
	});

	it('should not display adherence link when there is no adherence change', function() {
		createController();

		expect(scope.showAdherence(null)).toEqual(false);
	});

	it('should display adherence link when there is adherence change', function() {
		createController();

		expect(scope.showAdherence(0)).toEqual(true);
	});

	it('should not display last updated timestamp when there is no updates', function() {
		createController();

		expect(scope.showLastUpdate('')).toEqual(false);
		expect(scope.showLastUpdate(null)).toEqual(false);
	});

	it('should display last updated timestamp', function() {
		createController();

		expect(scope.showLastUpdate('2015-11-16 08:30')).toEqual(true);
	});
});
