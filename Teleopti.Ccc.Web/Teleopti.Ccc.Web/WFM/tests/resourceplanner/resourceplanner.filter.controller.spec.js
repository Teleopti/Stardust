/// <reference path="planningperiods.controller.spec.js" />
'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(module('wfm.resourceplanner'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it('should load one search result', inject(function ($controller) {
		var searchString = 'something';
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, [singleResult]);
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.searchString = searchString;
		$httpBackend.flush();

		var result = scope.results[0];
		expect(result.Id).toEqual(singleResult.Id);
		expect(result.FilterType).toEqual(singleResult.FilterType);
		expect(result.Name).toEqual(singleResult.Name);
	}));

	it('should not put loaded item to selected array', inject(function ($controller) {
		var searchString = 'something';
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, [singleResult]);
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.searchString = searchString;
		$httpBackend.flush();

		expect(scope.selectedItems().length).toEqual(0);
	}));

	it('should not call service when model is undefined ', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.results).toEqual([]);
	}));

	it('should not call service when model is empty string', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });
		scope.searchString = '';

		expect(scope.results).toEqual([]);
	}));

	it('should work to call selectedItems before loaded', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.selectedItems().length).toEqual(0);
	}));

	it('should put clicked item in selected', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope});
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		scope.results = [singleResult];

		scope.selectResultItem(singleResult);

		expect(scope.selectedItems()[0]).toEqual(singleResult);
	}));

	var filterUrl = function (searchString) {
		return "../api/filters?searchString=" + searchString + "&maxHits=10";
	}
});
