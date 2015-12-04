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

		$httpBackend.whenGET('../api/Status/Scheduling').respond(200, {});
		$httpBackend.whenGET('../ToggleHandler/IsEnabled?toggle=Wfm_ChangePlanningPeriod_33043').respond(200, {});
		$httpBackend.whenGET('../api/resourceplanner/planningperiod').respond(200, {});

	}));

	it('not null', inject(function($controller) {
		var scope = $rootScope.$new();
		var mockPlanningPeriodSvrc = {
			getPlanningPeriod: {
				query: function() {}
			},
			getDayOffRules: function () { return { then: function () { } } },
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
			getDayOffRules: function () { return { then: function () { } } },
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
			getDayOffRules : function(){return {then:function(){}}},
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
	it('should be able to edit each ruleset', inject(function ($controller, $state) {
		var scope = $rootScope.$new();
		spyOn($state, 'go');
		var stateParams = {id:2};

		$controller('PlanningPeriodsCtrl', { $scope: scope, $state: $state, $stateParams:stateParams });
		var filter = {id:1,default:false}
		scope.editRuleset(filter.id,filter.default);

		expect($state.go).toHaveBeenCalledWith('resourceplanner.filter',{filterId:filter.Id, periodId:stateParams.id, isDefault:filter.Default});

	}));
	it('should be able to delete each ruleset', inject(function ($controller, $state) {
		var scope = $rootScope.$new();
		var result = [{Id:1},{Id:3}]
		$httpBackend.whenDELETE('../api/resourceplanner/dayoffrules/2').respond(200, {});
		$httpBackend.whenGET('../api/resourceplanner/dayoffrules')
			.respond(200, result);

		$controller('PlanningPeriodsCtrl', { $scope: scope, $state: $state });
		scope.dayoffRules = [{Id:1},{Id:2},{Id:3}];
		scope.destoryRuleset(scope.dayoffRules[1]);

		$httpBackend.flush();

		expect(scope.dayoffRules[1].Id).toBe(3);

	}));
	it('should disable scheduling before planning period is loaded', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('PlanningPeriodsCtrl', { $scope: scope });

		expect(scope.disableSchedule()).toBe(true);
	}));
	it('should disable planningperiod select while servercall is made', inject(function($controller) {
		var scope = $rootScope.$new();
		var rangeDetails = {Number:10,}
		$controller('PlanningPeriodsCtrl', { $scope: scope });
		scope.rangeUpdated(10,rangeDetails);
		expect(scope.rangeDisabled).toBe(true);
	}));
});
