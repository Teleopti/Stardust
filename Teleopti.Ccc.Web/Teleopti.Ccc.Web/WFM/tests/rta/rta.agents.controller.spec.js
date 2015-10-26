'use strict';
describe('RtaAgentsCtrl', function() {
	var $q,
		$rootScope,
		$interval,
		$httpBackend,
		$controller,
		$resource,
		$state,
		scope;

	var stateParams = {};
	var agents = [];
	var states = [];
	var rtaSvrc = {};

	beforeEach(module('wfm'));

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

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$interval_, _$controller_, _$resource_, _$state_) {
		$controller = _$controller_;
		scope = _$rootScope_.$new();
		$q = _$q_;
		$interval = _$interval_;
		$rootScope = _$rootScope_;
		$resource = _$resource_;
		$state = _$state_;
		$httpBackend = _$httpBackend_;

		$httpBackend.whenGET("html/forecasting/forecasting.html").respond(200, 'mock'); // work around for ui-router bug with mocked states
		$httpBackend.whenGET("html/forecasting/forecasting-overview.html").respond(200);
		$httpBackend.whenGET("html/main.html").respond(200);

		$httpBackend.whenGET("../api/Global/User/CurrentUser").respond(200, {
			Language: "en",
			DateFormat: "something"
		});
		$httpBackend.whenGET("../api/Global/Language?lang=en").respond(200, '');

		rtaSvrc.getAgents = $resource('../Agents/ForTeam?teamId=:teamId', {
			teamId: '@teamId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		rtaSvrc.getAgentsForSites = $resource('../Agents/ForSites', {}, {
			query: {
				method: 'GET',
				params: {
					siteIds: []
				},
				isArray: true
			}
		});

		// rtaSvrc.getAgentsForTeams = $resource('../Agents/ForTeams', {}, {
		// 	query: {
		// 		method: 'GET',
		// 		params: {
		// 			teamIds: []
		// 		},
		// 		isArray: true
		// 	}
		// });

		rtaSvrc.getStates = $resource('../Agents/GetStates?teamId=:teamId', {
			teamId: '@teamId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		rtaSvrc.getStatesForSites = $resource('../Agents/GetStatesForSites', {}, {
			query: {
				method: 'GET',
				params: {
					siteIds: []
				},
				isArray: true
			}
		});

		// rtaSvrc.getStatesForTeams = $resource('../Agents/GetStatesForTeams', {}, {
		// 	query: {
		// 		method: 'GET',
		// 		params: {
		// 			teamIds: []
		// 		},
		// 		isArray: true
		// 	}
		// });

		$httpBackend.whenGET("../Agents/ForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c&siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(200, agents);
		$httpBackend.whenGET("../Agents/ForTeam?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(200, [agents[0]]);
		$httpBackend.whenGET("../Agents/ForTeam?teamId=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(200, [agents[1]]);
		// $httpBackend.whenGET("../Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495")
		// 	.respond(200, agents);
		// $httpBackend.whenGET("../Agents/ForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495&teamIds=103afc66-2bfa-45f4-9823-9e06008d5062")
		// 	.respond(200, agents);
		$httpBackend.whenGET("../Agents/GetStates?teamId=34590a63-6331-4921-bc9f-9b5e015ab495")
			.respond(200, [states[0]]);
		$httpBackend.whenGET("../Agents/GetStates?teamId=103afc66-2bfa-45f4-9823-9e06008d5062")
			.respond(200, [states[1]]);
		$httpBackend.whenGET("../Agents/GetStatesForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c&siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(200, states);
		// $httpBackend.whenGET("../Agents/GetStatesForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495")
		// 	.respond(200, states);
		// $httpBackend.whenGET("../Agents/GetStatesForTeams?teamIds=34590a63-6331-4921-bc9f-9b5e015ab495&teamIds=103afc66-2bfa-45f4-9823-9e06008d5062")
		// 	.respond(200, states);
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

	it('should display in correct time format', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		states = [{
			"StateStart": "\/Date(1429254905000)\/",
			"NextActivityStartTime": "\/Date(1432109700000)\/",
			"AlarmStart": "\/Date(1432105910000)\/"
		}];

		var baseTime = new Date(2015, 4, 17);
		jasmine.clock().mockDate(baseTime);

		createController();

		expect(scope.format(scope.agents[0].StateStart)).toEqual("07:15");
		expect(scope.format(scope.agents[0].NextActivityStartTime)).toEqual("2015-05-20 08:15:00");
		expect(scope.format(scope.agents[0].AlarmStart)).toEqual("2015-05-20 07:11:50");
	});

	it('should display in correct time duration format', function() {
		stateParams.teamId = "34590a63-6331-4921-bc9f-9b5e015ab495";
		states = [{
			"TimeInState": 15473
		}];

		createController();

		expect(scope.formatDuration(scope.agents[0].TimeInState)).toEqual("4:17:53");
	});

	it('should get agents for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[1].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
	});

	it('should get agent states for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		states = [{
			"PersonId": "11610fe4-0130-4568-97de-9b5e015b2564"
		}, {
			"PersonId": "6b693b41-e2ca-4ef0-af0b-9e06008d969b"
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("11610fe4-0130-4568-97de-9b5e015b2564");
		expect(scope.agents[1].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
	});

	it('should update agent states for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
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

	it('should go back to teams', function() {
		stateParams.siteId = 'd970a45a-90ff-4111-bfe1-9b5e015ab45c'
		createController();
		spyOn($state, 'go');

		scope.goBack();

		expect($state.go).toHaveBeenCalledWith('rta-teams', 'd970a45a-90ff-4111-bfe1-9b5e015ab45c');
	});

	it('should go back to sites', function() {
		createController();
		spyOn($state, 'go');

		scope.goBackToRoot();

		expect($state.go).toHaveBeenCalledWith('rta-sites');
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


	it('should set states to agents for multiple sites', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
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
		scope.filterText = 'Ashley';
		scope.filterData();

		expect(scope.gridOptions.data[0].Name).toEqual("Ashley Andeen");
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
});
