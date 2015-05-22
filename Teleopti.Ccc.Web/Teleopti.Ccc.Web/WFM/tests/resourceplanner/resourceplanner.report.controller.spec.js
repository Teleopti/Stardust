'use strict';
describe('ResourceplannerReportCtrl', function () {
	var $q,
	$rootScope,
	$httpBackend;;

	beforeEach(module('wfm'));
	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));


	it('no details are displayed when 0 issues', inject(function ($controller) {
		var scope = $rootScope.$new();
		var stateParams = {result:{BusinessRulesValidationResults:[]}};

		$controller('ResourceplannerReportCtrl', { $scope: scope, $stateParams: stateParams });
		expect(scope.issues.length).toEqual(0);
		expect(scope.hasIssues).toEqual(false);
	}));


});