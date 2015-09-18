'use strict';
describe('RtaTeamsCtrl', function () {
	var $q,
	    $rootScope,
        timerCallback,
        $interval,
	    $httpBackend;

	beforeEach(module('wfm'));
	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$interval_) {
		$q = _$q_;
		$interval = _$interval_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		$httpBackend.expectGET("html/forecasting/forecasting.html").respond(200, 'mock'); // work around for ui-router bug with mocked states
	}));

	it('should get the right teams for specific site', inject(function ($controller) {

		var scope = $rootScope.$new();

		var rtaSvrc = {
			getSites: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([
                        {
                        	"Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                        	"Name": "London",
                        	"NumberOfAgents": 46
                        }
					]);
					return { $promise: queryDeferred.promise };
				}
			},
			getAdherenceForAllSites: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([]);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		var orginaztionService = {

			organization:  [{ siteName: 'London', siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c', teams: [{ teamName: 'Preferences', teamId: 42 }, { teamName: 'NoPreferences', teamId: 43 }] }, { siteName: 'Paris', siteId: '6a21c802-7a34-4917-8dfd-9b5e015ab461', teams: [{ teamName: 'Agile', teamId: 40 }] }],

			getSites: function (id) {
				return orginaztionService.organization;
			},

			getTeams: function(siteId){
				return orginaztionService.organization.teams;
			}
		};

		$controller('RtaTeamsCtrl', { $scope: scope, RtaService: rtaSvrc, RtaOrganizationService: orginaztionService });

		expect(scope.teams).not.toBe(null);
		expect(scope.sites[1].teams.length).toBe(1);
		
	}));

});