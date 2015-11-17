/// <reference path="planningperiods.controller.spec.js" />
'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		mockResourceplannerFilterSvrc

	beforeEach(module('wfm.resourceplanner'));

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it('should not call service when model is blank ', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope, ResourcePlannerFilterSrvc: mockResourceplannerFilterSvrc });
		scope.searchString='';

		scope.$digest();

		expect(scope.results).toEqual(undefined);
	}));
	it('should fetch one search result', inject(function($controller) {
		var searchString = 'something';
		var singleResult = {
			Id : 'aölsdf',
			Name : 'asdfasdf',
			FilterType : 'asdfasdfasdf'
		};
		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, [singleResult]);
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope});

		scope.searchString=searchString;
		$httpBackend.flush();
		scope.$digest();

		var result = scope.results[0];
		expect(result.Id).toEqual(singleResult.Id);
		expect(result.FilterType).toEqual(singleResult.FilterType);
		expect(result.Name).toEqual(singleResult.Name);
	}));
	it('should not call service when model is undefined ', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope, ResourcePlannerFilterSrvc: mockResourceplannerFilterSvrc });

		scope.$digest();

		expect(scope.results).toEqual(undefined);
	}));
	it('should copy clicked item in result to selected', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope, ResourcePlannerFilterSrvc: mockResourceplannerFilterSvrc });

		var singleResult = {
			Id : 'aölsdf',
			Name : 'asdfasdf',
			FilterType : 'asdfasdfasdf'
		};
		scope.results = [singleResult];

		scope.selectResultItem(singleResult);

		expect(scope.selected[0]).toEqual(singleResult);
	}));
	var filterUrl = function(searchString){
		return "../api/filters?searchString=" + searchString + "&maxHits=10";
	}
});
