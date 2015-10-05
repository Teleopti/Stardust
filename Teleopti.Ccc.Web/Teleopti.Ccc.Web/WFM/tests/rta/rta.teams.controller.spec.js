'use strict';
describe('RtaTeamsCtrl', function () {
	var $q,
	    $rootScope,
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

    xit('should update out of adherence for all teams on site', inject( function ($controller) {
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
            getTeams: {
                query: function () {

                    var queryDeferred = $q.defer();
                    queryDeferred.resolve([
                        {
                            "Id": "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
                            "Name": "Students",
                            "NumberOfAgents": 7
                        },
                        {
                            "Id": "d7a9c243-8cd8-406e-9889-9b5e015ab495",
                            "Name": "Team Flexible",
                            "NumberOfAgents": 10
                        }
                    ]);
                }
            },
            getAdherenceForTeamsOnSite: {
                query: function () {
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve([
                        {
                            "Id": "e5f968d7-6f6d-407c-81d5-9b5e015ab495",
                            "OutOfAdherence": 3
                        },
                        {
                            "Id": "d7a9c243-8cd8-406e-9889-9b5e015ab495",
                            "OutOfAdherence": 1
                        }
                    ]);
                    return { $promise: queryDeferred.promise };
                }
            },
            getAdherenceForAllSites: {
                query: function () {
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve([
                        {
                            "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                            "OutOfAdherence": 3
                        },
                        {
                            "Id": "6a21c802-7a34-4917-8dfd-9b5e015ab461",
                            "OutOfAdherence": 1
                        }
                    ]);
                    return { $promise: queryDeferred.promise };
                }
            }
        };

        var rtaOrgSvrc = {

            siteId: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",

            getSites: function () {
                return [];
            },

            getTeams: function (siteId) {
                return [];
            }
        };

        var scope = $rootScope.$new();

        $controller('RtaCtrl', { $scope: scope, $interval: $interval, RtaService: rtaSvrc, RtaOrganizationService: rtaOrgSvrc });

        scope.$digest(); // this is needed to resolve the promise

        expect(scope.teams.length).not.toEqual(0);
        expect(scope.teams[0].OutOfAdherence).toEqual(3);
        expect(scope.teams[1].OutOfAdherence).toEqual(1);
    }));
		
	xit('should get the right teams for specific site', inject(function ($controller) {

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

			var rtaOrgSvrc = {

			organization:  [{ siteName: 'London', siteId: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c', teams: [{ teamName: 'Preferences', teamId: 42 }, { teamName: 'NoPreferences', teamId: 43 }] },
				{ siteName: 'Paris', siteId: '6a21c802-7a34-4917-8dfd-9b5e015ab461', teams: [{ teamName: 'Agile', teamId: 40 }] }],

			getSites: function (id) {
				return rtaOrgSvrc.organization;
			},

			getTeams: function(siteId){
				return rtaOrgSvrc.organization.teams;
			},
			getSiteName: function (siteIdParam) {
				return '';
			}

		};

		$controller('RtaTeamsCtrl', { $scope: scope, RtaService: rtaSvrc, RtaOrganizationService: rtaOrgSvrc });

		expect(scope.teams).not.toBe(null);

	}));

});
