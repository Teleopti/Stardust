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
		var scope = $rootScope.$new();
		$httpBackend.whenGET(filterUrl(searchString, 5))
			.respond(200, [singleResult]);
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
		$httpBackend.whenGET(filterUrl(searchString, 5))
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

	it('should show that more results exists', inject(function($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		for (var i = 0; i < scope.maxHits; i++) {
			scope.results.push({ Id: i });
		}

		expect(scope.moreResultsExists()).toEqual(true);
	}));

	it('should not show that more results exists', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		for (var i = 0; i < scope.maxHits - 1; i++) {
			scope.results.push({ Id: i });
		}

		expect(scope.moreResultsExists()).toEqual(false);
	}));

	var filterUrl = function (searchString, maxHits) {
		return "../api/filters?searchString=" + searchString + "&maxHits=" + maxHits;
	}
});
