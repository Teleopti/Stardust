'use strict';
describe('PlanningPeriodsCtrl', function () {
    var $q,
        $rootScope,
        $httpBackend,
        planningPeriodService,
        dayOffRuleService;

    beforeEach(function () {
        module('wfm.resourceplanner');
        module(function ($provide) {
            $provide.service('Toggle', function() {
                var queryDeferred = $q.defer();
                queryDeferred.resolve(true);
                return {
                    togglesLoaded: queryDeferred.promise,
                    Wfm_ChangePlanningPeriod_33043: true
                }
            });
        });
    });

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.whenGET('../api/resourceplanner/planningperiod').respond(200, {});

		dayOffRuleService = {
			getDayOffRules: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return { $promise: queryDeferred.promise };
			}
		}
		planningPeriodService = {
	    	getPlanningPeriod: function () {
	    		var queryDeferred = $q.defer();
	    		queryDeferred.resolve({});
	    		return { $promise: queryDeferred.promise };
	    	},
	    	
			status: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ IsRunning: false });
				return { $promise: queryDeferred.promise };
			}
		};
	}));

	it('not null', inject(function($controller) {
		var scope = $rootScope.$new();

		$controller('PlanningPeriodsCtrl', { $scope: scope, planningPeriodService: planningPeriodService, dayOffRuleService: dayOffRuleService });
		expect($controller).not.toBe(null);
	}));

	it('returns correct planning period', inject(function($controller) {
	    var scope = $rootScope.$new();

	    var queryDeferred = $q.defer();
	    queryDeferred.resolve({ StartDate: new Date(20150501), EndDate: new Date(20150531), Id: 'someguid' });
	    planningPeriodService.getPlanningPeriod = function () {
	        return { $promise: queryDeferred.promise };
	    }

	    $controller('PlanningPeriodsCtrl', { $scope: scope, planningPeriodService: planningPeriodService, dayOffRuleService: dayOffRuleService });
	    $rootScope.$apply();

		expect(scope.planningPeriod.StartDate).toEqual(new Date(20150501));
	    expect(scope.planningPeriod.EndDate).toEqual(new Date(20150531));
		expect(scope.planningPeriod.Id).toEqual('someguid');
	}));

	it('returns missing skills with content', inject(function($controller) {
		var scope = $rootScope.$new();

		var queryDeferred = $q.defer();
		queryDeferred.resolve({
		    Skills: [
                {
                    SkillName: 'Phone',
                    MissingRanges: [
                        { StartDate: new Date(20150502), EndDate: new Date(20150509) },
                        { StartDate: new Date(20150515), EndDate: new Date(20150517) }
                    ]
                }
		    ]
		});
		planningPeriodService.getPlanningPeriod = function () {
		    return { $promise: queryDeferred.promise };
		}
		
		$controller('PlanningPeriodsCtrl', { $scope: scope, planningPeriodService: planningPeriodService, dayOffRuleService: dayOffRuleService });
		$rootScope.$apply();

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

		expect(scope.dayOffRules[0].Id).toEqual('something');
	}));

	it('should set dayoffrules to empty array before loaded', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('PlanningPeriodsCtrl', { $scope: scope });

		expect(scope.dayOffRules.length).toEqual(0);
	}));
	it('should be able to edit each ruleset', inject(function ($controller, $state) {
		var scope = $rootScope.$new();
		spyOn($state, 'go');
		var stateParams = {id:2};

		$controller('PlanningPeriodsCtrl', { $scope: scope, $state: $state, $stateParams: stateParams });
		var filter = {id:1,default:false}
		scope.editRuleset(filter.id,filter.default);

		expect($state.go).toHaveBeenCalledWith('resourceplanner.filter',{filterId:filter.Id, periodId:stateParams.id, isDefault:filter.Default, groupId:undefined});

	}));
	it('should be able to delete each ruleset', inject(function ($controller, $state) {
		var scope = $rootScope.$new();
	    var result = [{ Id: 1 }, { Id: 3 }];

		$httpBackend.whenDELETE('../api/resourceplanner/dayoffrules/2').respond(200, {});
		$httpBackend.whenGET('../api/resourceplanner/dayoffrules')
			.respond(200, result);

		$controller('PlanningPeriodsCtrl', { $scope: scope, $state: $state });
		scope.dayOffRules = [{ Id: 1 }, { Id: 2 }, { Id: 3 }];
		scope.destoryRuleset(scope.dayOffRules[1]);

		$httpBackend.flush();

		expect(scope.dayOffRules[1].Id).toBe(3);

	}));

	it('should disable scheduling before planning period is loaded', inject(function ($controller) {
	    var scope = $rootScope.$new();
	    var loadedDayOffRules = [{ Id: 'something' }];

	    $httpBackend.whenGET('../api/resourceplanner/dayoffrules')
	        .respond(200, loadedDayOffRules);

	    var queryDeferred = $q.defer();
	    planningPeriodService.getPlanningPeriod = function () {
	        return { $promise: queryDeferred.promise };
	    }

	    $controller('PlanningPeriodsCtrl', { $scope: scope });
	    $rootScope.$apply();

	    expect(scope.disableSchedule()).toBe(true);

	    queryDeferred.resolve({ StartDate: new Date(20150501), EndDate: new Date(20150531), Id: 'someguid' });
	}));

	it('should enable scheduling when planning period is loaded', inject(function ($controller) {
		var scope = $rootScope.$new();
		var loadedDayOffRules = [{ Id: 'something' }];

	    $httpBackend.whenGET('../api/resourceplanner/dayoffrules')
	        .respond(200, loadedDayOffRules);

	    var queryDeferred = $q.defer();
	    planningPeriodService.getPlanningPeriod = function () {
	        return { $promise: queryDeferred.promise };
	    }
	    queryDeferred.resolve({ StartDate: new Date(20150501), EndDate: new Date(20150531), Id: 'someguid' });
	    $controller('PlanningPeriodsCtrl', { $scope: scope, planningPeriodService: planningPeriodService, dayOffRuleService: dayOffRuleService });

		$rootScope.$apply();

		expect(scope.disableSchedule()).toBe(false);
	}));

	it('should disable planningperiod select while servercall is made', inject(function($controller) {
		var scope = $rootScope.$new();
		var rangeDetails = {Number:10,}

		$controller('PlanningPeriodsCtrl', { $scope: scope });
		scope.rangeUpdated(10,rangeDetails);
		expect(scope.rangeDisabled).toBe(true);
	}));
	it('should call keep alive each 10th minute to make sure session is alive', inject(function($controller, $interval) {
		var scope = $rootScope.$new();

		var numberOfKeepAliveCalls = 0;

		planningPeriodService.keepAlive = function () { numberOfKeepAliveCalls++; };

		$controller('PlanningPeriodsCtrl', { $scope: scope, planningPeriodService: planningPeriodService, dayOffRuleService: dayOffRuleService });

		var twentyFiveMinutes = 1000 * 60 * 25;
		$interval.flush(twentyFiveMinutes);

		scope.$digest();

		expect(numberOfKeepAliveCalls).toBe(2);
	}));
	it('should output error if dayoffrules loading goes banana', inject(function ($controller) {
		//mainly put here for build server reporting, not really needed
		var scope = $rootScope.$new();
		$controller('PlanningPeriodsCtrl', { $scope: scope  });

		$httpBackend.whenGET("../api/resourceplanner/dayoffrules").respond(500, {});

		$httpBackend.flush();
		expect(scope.errorMessage).toBeTruthy();
	}));
});
