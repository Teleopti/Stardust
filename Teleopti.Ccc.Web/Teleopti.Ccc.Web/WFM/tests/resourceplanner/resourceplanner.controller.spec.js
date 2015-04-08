'use strict';
describe('ResourceplannerCtrl', function() {
	var $q,
		$rootScope,
		$httpBackend;;

	beforeEach(module('wfm'));

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	it('not null', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockResourcePlannerSvrc = {
			getPlanningPeriod: {
				query: function() {
					var queryDeferred = $q.defer();
					queryDeferred.resolve({});
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('ResourceplannerCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerSvrc });
		expect($controller).not.toBe(null);
	}));

	it('returns correct planning period', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockResourcePlannerSvrc = {
			getPlanningPeriod: {
				query: function() {
					var queryDeferred = $q.defer();
					var result = { StartDate: new Date(20150501), EndDate: new Date(20150531), Id: 'someguid' };
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('ResourceplannerCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerSvrc });
		scope.$digest();
		expect(scope.planningPeriod.StartDate).toEqual(new Date(20150501));
		expect(scope.planningPeriod.EndDate).toEqual(new Date(20150531));
		expect(scope.planningPeriod.Id).toEqual('someguid');
	}));

	it('returns missing skills with content', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockResourcePlannerSvrc = {
			getPlanningPeriod: {
				query: function() {
					var queryDeferred = $q.defer();
					var result = {
						Skills: [
							{
								SkillName: 'Phone',
								MissingRanges: [
									{ StartDate: new Date(20150502), EndDate: new Date(20150509) },
									{ StartDate: new Date(20150515), EndDate: new Date(20150517) }
								]
							}
						]
					};
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};

		$controller('ResourceplannerCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerSvrc });
		scope.$digest();
		expect(scope.planningPeriod.Skills[0].SkillName).toEqual('Phone');
		expect(scope.planningPeriod.Skills[0].MissingRanges.length).toEqual(2);
	}));

});
