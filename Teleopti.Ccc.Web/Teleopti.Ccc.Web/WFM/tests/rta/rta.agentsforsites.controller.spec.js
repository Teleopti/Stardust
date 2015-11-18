'use strict';
describe('RtaAgentsCtrl for sites', function() {
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
		$httpBackend.whenGET("../Agents/ForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c&siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(function() {
				return [200, agents];
			});
		$httpBackend.whenGET("../Agents/GetStatesForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c&siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(function() {
				return [200, states];
			});
		$httpBackend.whenGET("../Agents/ForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c")
			.respond(function() {
				return [200, agents[0]];
			});
		$httpBackend.whenGET("../Agents/ForSites?siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(function() {
				return [200, agents];
			});
		$httpBackend.whenGET("../Agents/GetStatesForSites?siteIds=d970a45a-90ff-4111-bfe1-9b5e015ab45c")
			.respond(function() {
				return [200, states[0]];
			});
		$httpBackend.whenGET("../Agents/GetStatesForSites?siteIds=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(function() {
				return [200, states];
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

	it('should get agents for single selected site', function() {
		stateParams.siteIds = ["6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		agents = [{
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
		}];

		createController();

		expect(scope.agents[0].PersonId).toEqual("6b693b41-e2ca-4ef0-af0b-9e06008d969b");
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

	it('should filter agent name with agentFilter', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			Name: "Ashley Andeen",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			Name: "Charlie Caper",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
		}];

		createController();
		scope.$apply("filterText = 'Ashley'");

		expect(scope.filteredData[0].Name).toEqual("Ashley Andeen");
	});

	it('should stop polling when page is about to destroy', function() {
		stateParams.siteIds = ["d970a45a-90ff-4111-bfe1-9b5e015ab45c", "6a21c802-7a34-4917-8dfd-9b5e015ab461"];
		agents = [{
			PersonId: "11610fe4-0130-4568-97de-9b5e015b2564",
			SiteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}, {
			PersonId: "6b693b41-e2ca-4ef0-af0b-9e06008d969b",
			SiteId: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
		}];
		createController();
		$interval.flush(5000);
		$httpBackend.flush();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});
});
