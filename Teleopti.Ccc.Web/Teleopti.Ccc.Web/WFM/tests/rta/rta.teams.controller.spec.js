'use strict';
describe('RtaTeamsCtrl', function() {
	var $q,
		$rootScope,
		$interval,
		$httpBackend,
		$controller,
		$resource,
		$state,
		scope;

	var stateParams = {};
	var teamAdherence = [];
	var sites = [];
	var teams = [];
	var rtaSvrc = {};

	beforeEach(module('wfm'));

	beforeEach(function() {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";

		teamAdherence = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565",
			OutOfAdherence: 1
		}, {
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
			OutOfAdherence: 5,
		}];

		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London",
			NumberOfAgents: 11
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			Name: "Paris",
			NumberOfAgents: 1
		}, {
			Id: "413157c4-74a9-482c-9760-a0a200d9f90f",
			Name: "Stores",
			NumberOfAgents: 98
		}];
		teams = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565",
			Name: "Green",
			NumberOfAgents: 1
		}, {
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
			Name: "Team 1",
			NumberOfAgents: 10,
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

		$httpBackend.expectGET("html/forecasting/forecasting.html").respond(200, 'mock'); // work around for ui-router bug with mocked states
		$httpBackend.whenGET("html/forecasting/forecasting-overview.html").respond(200);

		$httpBackend.whenGET("../api/Global/User/CurrentUser").respond(200, {
			Language: "en",
			DateFormat: "something"
		});
		// $httpBackend.whenGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		$httpBackend.whenGET("../api/Global/Language?lang=en").respond(200, '');


		rtaSvrc.getTeams = $resource('../Teams/ForSite?siteId=:siteId', {
			siteId: '@siteId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		//London
		$httpBackend.whenGET("../Teams/ForSite?siteId=d970a45a-90ff-4111-bfe1-9b5e015ab45c")
			.respond(200, teams);
		//Paris
		$httpBackend.whenGET("../Teams/ForSite?siteId=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(200, []);
		//Stores
		$httpBackend.whenGET("../Teams/ForSite?siteId=413157c4-74a9-482c-9760-a0a200d9f90f")
			.respond(200, []);

		rtaSvrc.getAdherenceForTeamsOnSite = $resource('../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=:siteId', {
			siteId: '@siteId'
		}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});
		//London
		$httpBackend.whenGET("../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=d970a45a-90ff-4111-bfe1-9b5e015ab45c")
			.respond(200, teamAdherence);
		//Paris
		$httpBackend.whenGET("../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=6a21c802-7a34-4917-8dfd-9b5e015ab461")
			.respond(200, []);
		//Stores
		$httpBackend.whenGET("../Teams/GetOutOfAdherenceForTeamsOnSite?siteId=413157c4-74a9-482c-9760-a0a200d9f90f")
			.respond(200, []);

		rtaSvrc.getSites = $resource('../Sites', {}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});
		$httpBackend.whenGET("../Sites")
			.respond(200, sites);
	}));

	var createController = function() {
		$controller('RtaTeamsCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should display team for site', function() {
		teams = [{
			Name: "Green",
			NumberOfAgents: 1
		}];
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";

		createController();

		expect(scope.teams[0].Name).toEqual("Green");
		expect(scope.teams[0].NumberOfAgents).toEqual(1);
	});

	it('should display agents out of adherence in the team', function() {
		teams = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565"
		}, {
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
		}];
		teamAdherence = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565",
			OutOfAdherence: 1
		}, {
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495",
			OutOfAdherence: 5,
		}];
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";

		createController();

		expect(scope.teams[0].OutOfAdherence).toEqual(1);
		expect(scope.teams[1].OutOfAdherence).toEqual(5);
	});

	it('should display site name London', function() {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London"
		}];

		createController();

		expect(scope.siteName).toEqual("London");
	});

	it('should display site name Paris', function() {
		stateParams.siteId = "6a21c802-7a34-4917-8dfd-9b5e015ab461";
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London"
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			Name: "Paris"
		}];

		createController();

		expect(scope.siteName).toEqual("Paris");
	});

	it('should update adhernce', function() {
		teamAdherence[0].OutOfAdherence = 1;
		createController();

		teamAdherence[0].OutOfAdherence = 3;
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.teams[0].OutOfAdherence).toEqual(3);
	});

	it('should go to agents', function() {
		stateParams.siteId = "d970a45a-90ff-4111-bfe1-9b5e015ab45c";
		teams = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565"
		}];
		createController();
		spyOn($state, 'go');

		scope.onTeamSelect(teams[0]);

		expect($state.go).toHaveBeenCalledWith('rta-agents', {
			siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c',
			teamId: '2d45a50e-db48-41db-b771-a53000ef6565'
		});
	});

	it('should go back to sites', function() {
		createController();
		spyOn($state, 'go');

		scope.goBack();

		expect($state.go).toHaveBeenCalledWith('rta-sites');
	});

	it('should go to agents for multiple teams', function() {
		teams = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565"
		}, {
			Id: "0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
		}];
		createController();
		spyOn($state, 'go');

		scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
		scope.toggleSelection("0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495");
		scope.openSelectedTeams();

		expect($state.go).toHaveBeenCalledWith('rta-agents-selected', {
			teamIds: ['2d45a50e-db48-41db-b771-a53000ef6565',
				"0a1cdb27-bc01-4bb9-b0b3-9b5e015ab495"
			]
		});
	});

	it('should go to agents after deselecting team', function() {
		teams = [{
			Id: "2d45a50e-db48-41db-b771-a53000ef6565"
		}];
		createController();
		spyOn($state, 'go');

		scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
		scope.toggleSelection("2d45a50e-db48-41db-b771-a53000ef6565");
		scope.openSelectedTeams();

		expect($state.go).toHaveBeenCalledWith('rta-agents-selected', {
			teamIds: []
		});
	});

	it('should display site name Stores', function() {
		stateParams.siteId = "413157c4-74a9-482c-9760-a0a200d9f90f";
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London"
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			Name: "Paris"
		}, {
			Id: "413157c4-74a9-482c-9760-a0a200d9f90f",
			Name: "Stores"
		}];

		createController();

		expect(scope.siteName).toEqual("Stores");
	});
});
