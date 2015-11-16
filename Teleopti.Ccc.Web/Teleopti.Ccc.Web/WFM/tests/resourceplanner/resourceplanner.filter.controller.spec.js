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
		scope.searchInput='';

		scope.$digest();

		expect(scope.result).toEqual(undefined);
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

		scope.searchInput=searchString;
		$httpBackend.flush();
		scope.$digest();

		var result = scope.result[0];
		expect(result.Id).toEqual(singleResult.Id);
		expect(result.FilterType).toEqual(singleResult.FilterType);
		expect(result.Name).toEqual(singleResult.Name);
	}));

	var filterUrl = function(searchString){
		return "../api/filters?searchString=" + searchString + "&maxResult=10";
	}
});
