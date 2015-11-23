'use strict';
describe('PlanningPeriodsCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(module('wfm.resourceplanner'));

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		//$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		//$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	it('not null', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockPlanningPeriodSvrc = {
			getPlanningPeriod: {
				query: function() {}
			},
			isEnabled: {
				query: function(param) {
					var queryDeferred = $q.defer();
					queryDeferred.resolve('true');
					return { $promise: queryDeferred.promise };
				}
			},
            status: {
                get: function() {
                    var queryDeferred = $q.defer();
                    queryDeferred.resolve({IsRunning:false});
                    return { $promise: queryDeferred.promise };
                }
            }
		};

		$controller('PlanningPeriodsCtrl', { $scope: scope, PlanningPeriodSvrc: mockPlanningPeriodSvrc });
		expect($controller).not.toBe(null);
	}));

	it('returns correct planning period', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockPlanningPeriodSvrc = {
			getPlanningPeriod: {
				query: function() {
					return { StartDate: new Date(20150501), EndDate: new Date(20150531), Id: 'someguid' };
				}
			},
			isEnabled: {
				query: function (param) {
					var queryDeferred = $q.defer();
					queryDeferred.resolve('true');
					return { $promise: queryDeferred.promise };
				}
			},
			status: {
			    get: function () {
			        var queryDeferred = $q.defer();
			        queryDeferred.resolve({ IsRunning: false });
			        return { $promise: queryDeferred.promise };
			    }
			}
		};

		$controller('PlanningPeriodsCtrl', { $scope: scope, PlanningPeriodSvrc: mockPlanningPeriodSvrc });
		expect(scope.planningPeriod.StartDate).toEqual(new Date(20150501));
		expect(scope.planningPeriod.EndDate).toEqual(new Date(20150531));
		expect(scope.planningPeriod.Id).toEqual('someguid');
	}));

	it('returns missing skills with content', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockPlanningPeriodSvrc = {
			getPlanningPeriod: {
				query: function() {	
					return {
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
				}
			},
			isEnabled: {
				query: function (param) {
					var queryDeferred = $q.defer();
					queryDeferred.resolve('true');
					return { $promise: queryDeferred.promise };
				}
			},
			status: {
			    get: function () {
			        var queryDeferred = $q.defer();
			        queryDeferred.resolve({ IsRunning: false });
			        return { $promise: queryDeferred.promise };
			    }
			}
		};

		$controller('PlanningPeriodsCtrl', { $scope: scope, PlanningPeriodSvrc: mockPlanningPeriodSvrc });

		expect(scope.planningPeriod.Skills[0].SkillName).toEqual('Phone');
		expect(scope.planningPeriod.Skills[0].MissingRanges.length).toEqual(2);
	}));

	it('should fetch all dayoffrules', inject(function ($controller) {
		var loadedDayOffRules = [{ Id: 'something' }];
		var scope = $rootScope.$new();

		$httpBackend.whenGET('../api/Status/Scheduling').respond(200, {});
		$httpBackend.whenGET('../ToggleHandler/IsEnabled?toggle=Wfm_ChangePlanningPeriod_33043').respond(200, {});
		$httpBackend.whenGET('../api/resourceplanner/planningperiod').respond(200, {});

		$httpBackend.whenGET('../api/resourceplanner/dayoffrules')
			.respond(200, loadedDayOffRules);

		$controller('PlanningPeriodsCtrl', { $scope: scope});

		$httpBackend.flush();

		expect(scope.dayoffRules[0].Id).toEqual('something');
	}));

	it('should set dayoffrules to empty array before loaded', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('PlanningPeriodsCtrl', { $scope: scope });

		expect(scope.dayoffRules.length).toEqual(0);
	}));
});
