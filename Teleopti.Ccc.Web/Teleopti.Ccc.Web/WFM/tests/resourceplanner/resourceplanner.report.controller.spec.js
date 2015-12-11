'use strict';
describe('ResourceplannerReportCtrl', function () {
	var $q,
	$rootScope,
	$httpBackend;

	beforeEach(module('wfm'));
	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
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


});
