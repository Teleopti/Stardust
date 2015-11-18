﻿/// <reference path="planningperiods.controller.spec.js" />
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

		expect(scope.selectedResults.length).toEqual(0);
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

		expect(scope.selectedResults.length).toEqual(0);
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

		expect(scope.selectedResults[0]).toEqual(singleResult);
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
	it('should clear result when no input', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		var loadedResult = {};
		scope.results = [loadedResult];
		scope.searchString = "";
		scope.$digest();

		expect(scope.results.length).toEqual(0);
	}));
	it('should not clear selected when no input', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		var loadedResult = {Name:'hello',Id:'3'};
		scope.results = [loadedResult];
		scope.selectResultItem(loadedResult);
		scope.searchString = "";
		scope.$digest();

		expect(scope.selectedResults.length).toEqual(1);
	}));
	it('should have a name', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.name = "default name";

		expect(scope.name).toBe("default name");
	}));


	it('should set default value for days of per week', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.isValidDayOffsPerWeek()).toBe(true);
		expect(scope.dayOffsPerWeek.MinDayOffsPerWeek).toBe(2); //?
		expect(scope.dayOffsPerWeek.MaxDayOffsPerWeek).toBe(3); //?
	}));

	it('should set invalid if MaxDayOffsPerWeek is smaller than MinDayOffsPerWeek', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 5,
			MaxDayOffsPerWeek: 4
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidDayOffsPerWeek()).toBe(false);
	}));

	it('should set valid if MaxDayOffsPerWeek is equal or greater than MinDayOffsPerWeek', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 4,
			MaxDayOffsPerWeek: 4
		};
		expect(scope.isValidDayOffsPerWeek()).toBe(true);
		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 3,
			MaxDayOffsPerWeek: 4
		};
		expect(scope.isValidDayOffsPerWeek()).toBe(true);
	}));

	it('should set default value for consecutive days off', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.isValidConsecDaysOff()).toBe(true);
		expect(scope.consecDaysOff.MinConsecDaysOff).toBe(2); //?
		expect(scope.consecDaysOff.MaxConsecDaysOff).toBe(3); //?
	}));

	it('should set invalid if MaxConsecDaysOff is smaller than MinConsecDaysOff', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.consecDaysOff = {
			MinConsecDaysOff: 5,
			MaxConsecDaysOff: 4
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidConsecDaysOff()).toBe(false);
	}));

	it('should set valid if MaxConsecDaysOff is equal or greater than MinConsecDaysOff', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.consecDaysOff = {
			MinConsecDaysOff: 2,
			MaxConsecDaysOff: 2
		};
		expect(scope.isValidConsecDaysOff()).toBe(true);
		scope.consecDaysOff = {
			MinConsecDaysOff: 1,
			MaxConsecDaysOff: 3
		};
		expect(scope.isValidConsecDaysOff()).toBe(true);
	}));

	it('should set default value for consecutive work days', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.isValidConsecWorkDays()).toBe(true);
		expect(scope.consecWorkDays.MinConsecWorkDays).toBe(2); //?
		expect(scope.consecWorkDays.MaxConsecWorkDays).toBe(3); //?
	}));

	it('should set invalid if MaxConsecWorkDays is smaller than MinConsecWorkDays', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.consecWorkDays = {
			MinConsecWorkDays: 2,
			MaxConsecWorkDays: 1
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidConsecWorkDays()).toBe(false);
	}));

	it('should set valid if MaxConsecWorkDays is equal or greater than MinConsecWorkDays', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.consecWorkDays = {
			MinConsecWorkDays: 4,
			MaxConsecWorkDays: 4
		};
		expect(scope.isValidConsecWorkDays()).toBe(true);
		scope.consecWorkDays = {
			MinConsecWorkDays: 2,
			MaxConsecWorkDays: 5
		};
		expect(scope.isValidConsecWorkDays()).toBe(true);
	}));

	it('should be invalid if no selected result', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		
		expect(scope.isValid()).toBe(false);
		expect(scope.isValidFilters()).toBe(false);
	}));


	it('should be valid if at least one selected result', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		scope.selectedResults = [{}];

		expect(scope.isValidFilters()).toBe(true);
	}));

	it('should be able to save the dayoffrule', inject(function ($controller) {
		var scope = $rootScope.$new();
		var sentData;
		$controller('ResourceplannerFilterCtrl', { $scope: scope });
		scope.name = 'asdfasdf';
		scope.selectedResults = [{ FilterType: "contract", Id: "5BC5B983-281A-4B92-92D6-A54F00D63A78" }];

		$httpBackend.when('POST', '../api/resourceplanner/dayoffrules',
				function (postData) {
					sentData = JSON.parse(postData);
					return true;
				}).respond(200, true);

		var result = scope.persist();
		$httpBackend.flush();

		expect(sentData.Name).toEqual(scope.name);
		expect(sentData.MinDayOffsPerWeek).toEqual(scope.dayOffsPerWeek.MinDayOffsPerWeek);
		expect(sentData.MaxDayOffsPerWeek).toEqual(scope.dayOffsPerWeek.MaxDayOffsPerWeek);
		expect(sentData.MinConsecutiveWorkdays).toEqual(scope.consecWorkDays.MinConsecWorkDays);
		expect(sentData.MaxConsecutiveWorkdays).toEqual(scope.consecWorkDays.MaxConsecWorkDays);
		expect(sentData.MinConsecutiveDayOffs).toEqual(scope.consecDaysOff.MinConsecDaysOff);
		expect(sentData.MaxConsecutiveDayOffs).toEqual(scope.consecDaysOff.MaxConsecDaysOff);
		expect(sentData.Filters[0].FilterType).toEqual(scope.selectedResults[0].FilterType);
		expect(sentData.Filters[0].Id).toEqual(scope.selectedResults[0].Id);
		expect(result).toEqual(true);
	}));

	it('should return false if persist when invalid', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		expect(scope.persist()).toEqual(false);
	}));

	it('should not be possible to select item twice', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope });

		var item = {Id:'someGuid'};

		scope.selectResultItem(item);
		scope.selectResultItem(item);

		expect(scope.selectedResults.length).toEqual(1);
	}));



	var filterUrl = function (searchString, maxHits) {
		return "../api/filters?searchString=" + searchString + "&maxHits=" + maxHits;
	}
});
