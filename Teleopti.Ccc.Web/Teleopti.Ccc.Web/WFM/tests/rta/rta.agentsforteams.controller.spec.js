'use strict';
describe('RtaAgentsForTeamsCtrl', function() {
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
	var rtaSvrc = {};

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
			StateStart: "\/Date(1429254905000)\/",
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
			StateStart: "\/Date(1429254905000)\/",
			Activity: "Short break",
			NextActivity: "Phone",
			NextActivityStartTime: "\/Date(1432109700000)\/",
			Alarm: "Out of Adherence",
			AlarmStart: "\/Date(1432105910000)\/",
			AlarmColor: "#FF0000",
			TimeInState: 15473
		}];
	});

	beforeEach(function() {
		module(function($provide) {
			$provide.service('RtaService', function() {
				return rtaSvrc;
			});
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

		rtaSvrc.getAgentsForTeams = $resource('../Agents/ForTeams', {}, {
			query: {
				method: 'GET',
				params: {
					teamIds: []
				},
				isArray: true
			}
		});

		rtaSvrc.getStatesForTeams = $resource('../Agents/GetStatesForTeams', {}, {
			query: {
				method: 'GET',
				params: {
					teamIds: []
				},
				isArray: true
			}
		});

		$httpBackend.whenGET("../Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(200, agents);
		$httpBackend.whenGET("../Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495&teamIds=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(200, agents);
		$httpBackend.whenGET("../Agents/GetStatesForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(200, states);
		$httpBackend.whenGET("../Agents/GetStatesForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495&teamIds=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(200, states);
	}));

	var createController = function() {
		$controller('RtaAgentsForTeamsCtrl', {
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

	it('should go back to sites', function() {
		createController();
		spyOn($state, 'go');

		scope.goBackToRoot();

		expect($state.go).toHaveBeenCalledWith('rta');
	});

	it('should display in correct time format', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		states = [{
			"StateStart": "\/Date(1429254905000)\/",
			"NextActivityStartTime": "\/Date(1432109700000)\/",
			"AlarmStart": "\/Date(1432105910000)\/"
		}];

		var baseTime = new Date('2015-04-17');
		jasmine.clock().mockDate(baseTime);

		createController();

		expect(scope.format(scope.agents[0].StateStart)).toEqual("07:15");
		expect(scope.format(scope.agents[0].NextActivityStartTime)).toEqual("2015-05-20 08:15:00");
		expect(scope.format(scope.agents[0].AlarmStart)).toEqual("2015-05-20 07:11:50");
	});

	it('should display in correct time duration format', function() {
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		states = [{
			"TimeInState": 15473
		}];

		createController();

		expect(scope.formatDuration(scope.agents[0].TimeInState)).toEqual("4:17:53");
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
			StateStart: "\/Date(1429254905000)\/",
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
			StateStart: "\/Date(1429254905000)\/",
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
		expect(scope.agents[0].StateStart).toEqual("\/Date(1429254905000)\/");
		expect(scope.agents[0].Activity).toEqual("Phone");
		expect(scope.agents[0].NextActivity).toEqual("Short break");
		expect(scope.agents[0].NextActivityStartTime).toEqual("\/Date(1432109700000)\/");
		expect(scope.agents[0].Alarm).toEqual("In Adherence");
		expect(scope.agents[0].AlarmStart).toEqual("\/Date(1432105910000)\/");
		expect(scope.agents[0].AlarmColor).toEqual("#00FF00");
		expect(scope.agents[0].TimeInState).toEqual(15473);

		expect(scope.agents[1].State).toEqual("In Call");
		expect(scope.agents[1].StateStart).toEqual("\/Date(1429254905000)\/");
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
		scope.filterText = 'Caper';
		scope.filterData();
		states[1].State = "Ready";
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.gridOptions.data[0].Name).toEqual("Charley Caper");
		expect(scope.gridOptions.data[0].State).toEqual("Ready");
	});

	it('should get change schedule url for agent', function() {
		$sessionStorage.buid = "928dd0bc-bf40-412e-b970-9b5e015aadea";
		stateParams.teamIds = ["34590a63-6331-4921-bc9f-9b5e015ab495"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			TeamId: "34590a63-6331-4921-bc9f-9b5e015ab495",
		}];
		var fakeDate = new Date('2015-10-28');
		jasmine.clock().mockDate(fakeDate);

		createController();

		expect(scope.changeScheduleUrl("34590a63-6331-4921-bc9f-9b5e015ab495", "11610fe4-0130-4568-97de-9b5e015b2564"))
			.toEqual("/Anywhere#teamschedule/928dd0bc-bf40-412e-b970-9b5e015aadea/34590a63-6331-4921-bc9f-9b5e015ab495/11610fe4-0130-4568-97de-9b5e015b2564/20151028");
	});

});
