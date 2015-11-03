'use strict';
describe('RtaCtrl', function() {
	var $q,
		$rootScope,
		$interval,
		$httpBackend,
		$controller,
		$resource,
		$state,
		scope;

	var sites = [];
	var siteAdherence = [];
	var rtaSvrc = {};

	beforeEach(module('wfm.rta'));

	beforeEach(function() {
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
		siteAdherence = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			OutOfAdherence: 1
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			OutOfAdherence: 5,
		}];

	});

	beforeEach(function() {
		module(function($provide) {
			$provide.service('RtaService', function() {
				return rtaSvrc;
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

		rtaSvrc.getSites = $resource('../Sites', {}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		rtaSvrc.getAdherenceForAllSites = $resource('../Sites/GetOutOfAdherenceForAllSites', {}, {
			query: {
				method: 'GET',
				params: {},
				isArray: true
			}
		});

		$httpBackend.whenGET("../Sites")
			.respond(200, sites);

		$httpBackend.whenGET("../Sites/GetOutOfAdherenceForAllSites")
			.respond(200, siteAdherence);
	}));

	var createController = function() {
		$controller('RtaCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should display site', function() {
		sites = [{
			Name: "London",
			NumberOfAgents: 11
		}];

		createController();

		expect(scope.sites[0].Name).toEqual("London");
		expect(scope.sites[0].NumberOfAgents).toEqual(11);
	});

	it('should display agents out of adherence in sites', function() {
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			Name: "London",
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			Name: "Paris",
		}];
		siteAdherence = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
			OutOfAdherence: 1
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461",
			OutOfAdherence: 5,
		}];

		createController();

		expect(scope.sites[0].OutOfAdherence).toEqual(1);
		expect(scope.sites[1].OutOfAdherence).toEqual(5);
	});

	it('should update adhernce', function() {
		siteAdherence[0].OutOfAdherence = 1;
		createController();

		siteAdherence[0].OutOfAdherence = 3;
		$interval.flush(5000);
		$httpBackend.flush();

		expect(scope.sites[0].OutOfAdherence).toEqual(3);
	});

	it('should stop polling when page is about to destroy', function() {
		createController();
		$interval.flush(5000);
		$httpBackend.flush();

		scope.$emit('$destroy');
		$interval.flush(5000);
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should go to teams', function() {
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}];

		createController();
		spyOn($state, 'go');

		scope.onSiteSelect(sites[0]);

		expect($state.go).toHaveBeenCalledWith('rta-teams', {
			siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c'
		});
	});

	it('should go to agents for multiple sites', function() {
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
		}];
		createController();
		spyOn($state, 'go');

		scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
		scope.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
		scope.openSelectedSites();

		expect($state.go).toHaveBeenCalledWith('rta-agents-sites', {
			siteIds: ['d970a45a-90ff-4111-bfe1-9b5e015ab45c',
				"6a21c802-7a34-4917-8dfd-9b5e015ab461"
			]
		});
	});

	it('should go to agents after deselecting site', function() {
		sites = [{
			Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
		}, {
			Id: "6a21c802-7a34-4917-8dfd-9b5e015ab461"
		}];
		createController();
		spyOn($state, 'go');

		scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
		scope.toggleSelection("6a21c802-7a34-4917-8dfd-9b5e015ab461");
		scope.toggleSelection("d970a45a-90ff-4111-bfe1-9b5e015ab45c");
		scope.openSelectedSites();

		expect($state.go).toHaveBeenCalledWith('rta-agents-sites', {
			siteIds: ["6a21c802-7a34-4917-8dfd-9b5e015ab461"]
		});
	});

});
