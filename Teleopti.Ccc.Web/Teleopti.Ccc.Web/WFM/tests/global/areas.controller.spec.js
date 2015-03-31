'use strict';
describe('AreasCtrl', function() {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach( module('wfm'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, { "meta": { "code": 200, "errors": null }, "response": { "allowed": false } });
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { "meta": { "code": 200, "errors": null }, "response": { "allowed": false } });
	}));

	it('not null', inject(function($controller) {
		var scope = $rootScope.$new();

		var svrc = {
			getAreas: {
				query: function () {
					var queryDeferred = $q.defer();
					var result = [];
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('AreasCtrl', { $scope: scope, AreasSvrc: svrc });
		expect($controller).not.toBe(null);
	}));

	it('contains correct area', inject(function($controller) {
		var scope = $rootScope.$new();

		var svrc = {
			getAreas: {
				query: function () {
					var queryDeferred = $q.defer();
					var item = new Object();
					var result = [{ Name: 'Forecaster', internalName: "Forecaster", _links: [] }, { name: 'Scheduling', links: [item] }];
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('AreasCtrl', { $scope: scope, AreasSvrc: svrc });
		scope.loadAreas();
		scope.$digest();
		expect(scope.areas[1].name).toEqual('Scheduling');
	}));

	it('contains correct scheduling filter link', inject(function($controller) {
		var scope = $rootScope.$new();

		var svrc = {
			getAreas: {
				query: function () {
					var queryDeferred = $q.defer();
					var item = new Object();
					item.rel = "filters";
					item.href = "/api/ResourcePlanner/Filter";
					item.Methods = null;
					var result = [{ Name: 'Forecaster', internalName: "Forecaster", _links: [] }, { name: 'Scheduling', _links: [item] }];
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('AreasCtrl', { $scope: scope, AreasSvrc: svrc });
		scope.loadAreas();
		scope.$digest();
		expect(scope.areas[1]._links[0].href).toEqual("/api/ResourcePlanner/Filter");
	}));

	it('contains correct group pages for scheduling', inject(function ($controller) {
		var scope = $rootScope.$new();

		var svrc = {
			getFilters: {
				query: function () {
					var queryDeferred = $q.defer();
					var item = new Object();
					item.Name = "Car group";
					item.Id = "guid";
					var result = [{ Id: 'guid', Name: "Stockholm", Items: [item] }];
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('AreasCtrl', { $scope: scope, FilterSvrc: svrc });
		scope.loadFilters();
		scope.$digest();
		expect(scope.filters.length).toEqual(1);
	}));

});
