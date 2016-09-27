'use strict';
describe('ResourceplannerReportCtrl', function () {
	var $q,
	$rootScope,
	$httpBackend;

	beforeEach(module('wfm.resourceplanner'));
	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		$httpBackend.whenGET("../ToggleHandler/AllToggles").respond(200, {});
	}));
	var result = {};

	result.withNoIssues = function(){
		return {result:{BusinessRulesValidationResults:[]},interResult:{SkillResultList:[{SkillName:'test',SkillDetails:[{Date:'2015-11-14'}]}]}};
	};
	result.withIssues = function(){
		return {result:{BusinessRulesValidationResults:[[]]},interResult:{SkillResultList:[{SkillName:'test',SkillDetails:[{Date:'2015-11-14'}]}]}};
	};
	result.withNoWeekends = function(){
		return {result:{BusinessRulesValidationResults:[[]]},interResult:{SkillResultList:[{SkillName:'test',SkillDetails:[{Date:'2015-11-12'}]}]}};
	};
	result.withPeriodId = function(){
		return {planningperiod:{id:0},result:{BusinessRulesValidationResults:[[]]},interResult:{SkillResultList:[{SkillName:'test',SkillDetails:[{Date:'2015-11-12'}]}]}}
	};

	it('should not detect any issues', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams: result.withNoIssues()});

		expect(scope.issues.length).toEqual(0);
		expect(scope.hasIssues).toEqual(false);
	}));
    it('should detect and process issues', inject(function ($controller) {
        var scope = $rootScope.$new();
        $controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams: result.withIssues() });

        expect(scope.issues.length).toEqual(1);
        expect(scope.hasIssues).toEqual(true);
    }));
    it('should find weekends', inject(function ($controller) {
        var scope = $rootScope.$new();
        $controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams:  result.withNoIssues() });

        expect(scope.dayNodes[0].SkillDetails[0].weekend).toBe(true);
    }));
    it('should find and ignore weekends', inject(function ($controller) {
        var scope = $rootScope.$new();
        $controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: result.withNoWeekends() });

        expect(scope.dayNodes[0].SkillDetails[0].weekend).toBe(undefined);
    }));
	it('should be possible to publish a schedule', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: result.withPeriodId() });

		scope.publishSchedule()
		expect(scope.publishedClicked).toBe(true);
	}));
	it('should be possible to publish a schedule only once', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: result.withPeriodId() });

		scope.publishSchedule()
		expect(scope.publishedClicked).toBe(true);
	}));
	it('should default values if none are loaded', inject(function ($controller) {
		var scope = $rootScope.$new();
		var mockstateParams = {result:{},interResult:[],planningperiod:{}}
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams:mockstateParams  });

		expect(scope.dayNodes.length).toEqual(0);
	}));
	it('should return false if no params are provided', inject(function ($controller) {
		var scope = $rootScope.$new();
		var mockstateParams = {id:"",result:{},interResult:[],planningperiod:{}}
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: mockstateParams});

		expect(scope.optimizeDayOffIsEnabled()).toBe(false);
	}));
	it('should return true if params are provided', inject(function ($controller) {
		var scope = $rootScope.$new();
		var deferred = $q.defer();
		deferred.resolve();
		var mockToggleService = {
			togglesLoaded: deferred.promise,
			Scheduler_IntradayOptimization_36617:function(){
				return true;
			}
		};
		var mockstateParams = {id:"111-111",result:{},interResult:[],planningperiod:{}}
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: mockstateParams, Toggle:mockToggleService});
		scope.$digest();
		expect(scope.optimizeDayOffIsEnabled()).toBe(true);
	}));
	it('should call keep alive each 10th minute to make sure session is alive', inject(function ($controller, $interval, PlanningPeriodSvrc) {
		var scope = $rootScope.$new();

		var numberOfKeepAliveCalls = 0;

		PlanningPeriodSvrc.keepAlive = function () { numberOfKeepAliveCalls++; };

		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: result.withNoIssues() });

		var twentyFiveMinutes = 1000 * 60 * 25;
		$interval.flush(twentyFiveMinutes);

		expect(numberOfKeepAliveCalls).toBe(2);
	}));
	it('should update relative difference when new data is available', inject(function ($controller, $interval, PlanningPeriodSvrc) {
		var scope = $rootScope.$new();
		var mockstateParams = {id:"",result:{},planningperiod:{},interResult:{SkillResultList:[{SkillName:'test',SkillDetails:[{RelativeDifference:2,Date:'2015-11-14'}]}]}}
		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: mockstateParams });
		scope.$digest();
		scope.dayNodes = [{SkillDetails:[{RelativeDifference:1,Date:'2015-11-14'}]}];
		scope.$digest();
		expect(scope.dayNodes[0].SkillDetails[0].parseDif).toBe("100.0" );
	}));
});
