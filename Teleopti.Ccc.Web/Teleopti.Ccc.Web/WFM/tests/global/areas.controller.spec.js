'use strict';
describe('AreasCtrl', function() {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(function () {
		module('wfm.areas');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
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
});
