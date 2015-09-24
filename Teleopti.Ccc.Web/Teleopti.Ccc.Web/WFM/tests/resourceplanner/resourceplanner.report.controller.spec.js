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
    var mockStateParams ={result:{BusinessRulesValidationResults:[]},interResult:{_skillResultList:[{SkillName:'test',SkillDetails:[{Date:{Date:'2015-11-14'}}]}]}};

    it ('should only get the number of days until next monday',inject(function($controller){
       var scope = $rootScope.$new();
        var day = new Date(2015,10,10);
        $controller('ResourceplannerReportCtrl', { $scope: scope,  $stateParams: mockStateParams});

        var result = scope.getdaysuntilnextmonday(day);

        expect(result).toEqual(2);
    }));

	it('no details are displayed when 0 issues', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams: mockStateParams});

		expect(scope.issues.length).toEqual(0);
		expect(scope.hasIssues).toEqual(false);
	}));

    it('details are displayed when issues', inject(function ($controller) {
        var scope = $rootScope.$new();
         var mockStateParams = {result:{BusinessRulesValidationResults:[[]]},interResult:{_skillResultList:[{SkillName:'test',SkillDetails:[{Date:{Date:'2015-11-14'}}]}]}};
        $controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams: mockStateParams });

        expect(scope.issues.length).toEqual(1);
        expect(scope.hasIssues).toEqual(true);
    }));
    it('weekends are found', inject(function ($controller) {
        var scope = $rootScope.$new();
        $controller('ResourceplannerReportCtrl', { $scope: scope,$stateParams: mockStateParams });

        expect(scope.dayNodes[0].SkillDetails[0].weekend).toBe(true);
    }));
    it('non-weekend are found and ignored', inject(function ($controller) {
        var scope = $rootScope.$new();
        var mockStateParams = {result:{BusinessRulesValidationResults:[[]]},interResult:{_skillResultList:[{SkillName:'test',SkillDetails:[{Date:{Date:'2015-11-12'}}]}]}};

        $controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: mockStateParams });

        expect(scope.dayNodes[0].SkillDetails[0].weekend).toBe(undefined);
    }));
	it('should be possible to publish a schudule', inject(function ($controller) {
		var scope = $rootScope.$new();
		var mockStateParams = {planningperiod:{id:0},result:{BusinessRulesValidationResults:[[]]},interResult:{_skillResultList:[{SkillName:'test',SkillDetails:[{Date:{Date:'2015-11-12'}}]}]}};

		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: mockStateParams });

		scope.publishSchedule()
		expect(scope.publishedClicked).toBe(true);
	}));

});
