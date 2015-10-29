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

	it('should validate correct input', inject(function($controller) {
		var scope = setupScope($controller);

		var node = {
			MinConsecutiveDayOffs:1,
			MaxConsecutiveDayOffs:1,
			MinConsecutiveWorkdays:3,
			MaxConsecutiveWorkdays:4,
			MinDayOffsPerWeek:5,
			MaxDayOffsPerWeek:5
		};
		scope.dayoffRules = {Id:0};
		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(true);
	}));

    it('should be invalid if MaxConsecutiveDayOffs is smaller than MinConsecutiveDayOffs', inject(function($controller) {
		var scope = setupScope($controller);

		var node = {
			MinConsecutiveDayOffs:2,
			MaxConsecutiveDayOffs:1,
			MinConsecutiveWorkdays:3,
			MaxConsecutiveWorkdays:4,
			MinDayOffsPerWeek:5,
			MaxDayOffsPerWeek:6
			};

		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(false);
    }));
	it('should be invalid if MaxConsecutiveWorkdays is smaller than MinConsecutiveWorkdays', inject(function($controller) {
		var scope = setupScope($controller);

		var node = {
			MinConsecutiveDayOffs:2,
			MaxConsecutiveDayOffs:2,
			MinConsecutiveWorkdays:3,
			MaxConsecutiveWorkdays:1,
			MinDayOffsPerWeek:5,
			MaxDayOffsPerWeek:6
			};

		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(false);
    }));

	it('should be invalid if MaxDayOffsPerWeek is smaller than MinDayOffsPerWeek', inject(function($controller) {
		var scope = setupScope($controller);

		var node = {
			MinConsecutiveDayOffs:2,
			MaxConsecutiveDayOffs:2,
			MinConsecutiveWorkdays:3,
			MaxConsecutiveWorkdays:5,
			MinDayOffsPerWeek:5,
			MaxDayOffsPerWeek:4
			};

		scope.validateInput(node)
		scope.$digest();

        expect(scope.isValid).toBe(false);
    }));

	it('should be invalid until filled', inject(function($controller){
		var scope = setupScope($controller);

		expect(scope.isValid).toBe(false);
	}));

	var setupScope = function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		return scope;
	}
});
