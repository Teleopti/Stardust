'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		mockResourceplannerSvrc

	beforeEach(module('wfm'));


	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');

		mockResourceplannerSvrc = {
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
				deferred.resolve({Id:0, Default:true});
				return { $promise: deferred.promise};
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
		scope.validateInputAndSend(node)
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
		scope.validateInputAndSend(node)
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

			scope.validateInputAndSend(node)
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

			scope.validateInputAndSend(node)
	        expect(scope.isValid).toBe(false);
    }));

	it('should be invalid until filled', inject(function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});

		expect(scope.isValid).toBe(false);
	}));

	it('should be valid after loaded', inject(function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});

		scope.$digest();

		expect(scope.isValid).toBe(true);
	}));

	it('should save model when valid', inject(function($controller){
		spyOn(mockResourceplannerSvrc.saveDayoffRules, 'update');
		var scope = setupScope($controller);
		scope.$digest();

		var node = {
			MinConsecutiveDayOffs:1,
			MaxConsecutiveDayOffs:1,
			MinConsecutiveWorkdays:3,
			MaxConsecutiveWorkdays:4,
			MinDayOffsPerWeek:5,
			MaxDayOffsPerWeek:5
		};
		scope.validateInputAndSend(node);

		expect(mockResourceplannerSvrc.saveDayoffRules.update).toHaveBeenCalledWith(node);
	}));

	it('should not save model when invalid', inject(function($controller){
		spyOn(mockResourceplannerSvrc.saveDayoffRules, 'update');
		var scope = setupScope($controller);

		var node = {
			MinConsecutiveDayOffs:2,
			MaxConsecutiveDayOffs:1,
			MinConsecutiveWorkdays:17,
			MaxConsecutiveWorkdays:4,
			MinDayOffsPerWeek:24,
			MaxDayOffsPerWeek:5
		};

		scope.validateInputAndSend(node);

		expect(mockResourceplannerSvrc.saveDayoffRules.update).not.toHaveBeenCalledWith();
	}));
	it('should not enable user input if service is not called', inject(function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});

		expect(scope.isEnabled).toBe(false);
	}));
	it('should enable user input if service is successfully called', inject(function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		scope.$digest();

		expect(scope.isEnabled).toBe(true);
	}));

	var setupScope = function($controller){
		var scope = $rootScope.$new();
		$controller('ResourcePlannerCtrl', {$scope:scope,ResourcePlannerSvrc:mockResourceplannerSvrc});
		scope.$digest();
		return scope;
	}
});
