'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;;

		var mockResourceplannerSvrc = {
			getPlanningPeriod:{
				query: function() {
					var deferred = $q.defer();
					deferred.resolve();
					return deferred.promise;
			}
		},
		getDayoffRules:{
			query: function() {
				var deferred = $q.defer();
				deferred.resolve();
				return { $promise: deferred.promise };
		}
	},
		saveDayoffRules:{
			update: function() {
				var deferred = $q.defer();
				deferred.resolve();
				return { $promise: deferred.promise };

		}
	}

		};
	beforeEach(module('wfm'));

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	it('should recive valid input', inject(function($controller) {
        var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		var node = {
			MinConsecutiveDayOffs:1,
			MaxConsecutiveDayOffs:2
			};
			scope.dayoffRules = {Id:0};
		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(true);

	}));
    it('should recive input and invalidate it', inject(function($controller) {
        var scope = $rootScope.$new();

        $controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		var node = {
			MinConsecutiveDayOffs:2,
			MaxConsecutiveDayOffs:1
			};

		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(false);
    }))

});
