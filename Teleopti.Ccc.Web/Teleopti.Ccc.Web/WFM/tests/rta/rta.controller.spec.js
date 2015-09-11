'use strict';
describe('RtaCtrl', function () {
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

	it('should update out of adherence for all sites', inject( function ($controller) {
	    var rtaSvrc = {
	        getSites: {
	            query: function () {

	                var queryDeferred = $q.defer();
	                queryDeferred.resolve([
                        {
                            "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                            "Name": "London",
                            "NumberOfAgents": 46
                        },
                        {
                            "Id": "6a21c802-7a34-4917-8dfd-9b5e015ab461",
                            "Name": "Paris",
                            "NumberOfAgents": 50
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
	    var scope = $rootScope.$new();
		
	    $controller('RtaCtrl', { $scope: scope, RtaService: rtaSvrc });
		

		scope.$digest(); // this is needed to resolve the promise
		
		expect(scope.sites[0].OutOfAdherence).toEqual(3);
		expect(scope.sites[1].OutOfAdherence).toEqual(1);
	}));

	it('should have adherence 0 if no adherence provided for the site', inject(function ($controller) {
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
	                queryDeferred.resolve([
                        {
                            "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c"
                        }
	                ]);
	                return { $promise: queryDeferred.promise };
	            }
	        }
	    };
	    var scope = $rootScope.$new();
	    $controller('RtaCtrl', { $scope: scope, RtaService: rtaSvrc });

	    scope.$digest(); // this is needed to resolve the promise

	    expect(scope.sites[0].OutOfAdherence).toEqual(0);	    
	}));

	it('should have adherence 0 if no data provided', inject(function ($controller) {
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
	    var scope = $rootScope.$new();
	    $controller('RtaCtrl', { $scope: scope, RtaService: rtaSvrc });

	    scope.$digest(); // this is needed to resolve the promise

	    expect(scope.sites[0].OutOfAdherence).toEqual(0);
	}));

	it('should update OutOfAdherence data after every 5 sec', inject(function ($controller, $interval) {
	  
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
	                queryDeferred.resolve([
                        {
                            "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                            "OutOfAdherence": 3
                        }
	                ]);
	                return { $promise: queryDeferred.promise };
	            }
            }
	    };
	    var scope = $rootScope.$new();

	    $controller('RtaCtrl', { $scope: scope, RtaService: rtaSvrc, $interval: $interval});

	    rtaSvrc.getAdherenceForAllSites = {
	            query: function () {
	                var queryDeferred = $q.defer();
	                queryDeferred.resolve([
                        {
                            "Id": "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                            "OutOfAdherence": 5
                        }
	                ]);
	                return { $promise: queryDeferred.promise };
	            }
	        };

        $interval.flush(5000);
	    scope.$digest();
	    expect(scope.sites[0].OutOfAdherence).toEqual(5);
        
	}));
});
