'use strict';
describe("PeopleStartCtrl", function() {
	var $q,
		$rootScope,
		$httpBackend;
	var stateParams = { selectedPeopleIds: [], commandTag: "AdjustSkill", currentKeyword: '', paginationOptions: {} };

	beforeEach(function(){
		module('wfm.people');
		module('externalModules');
	});

	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_, _$controller_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	var mockUpload = {};

	var mockToggleService = {
		isFeatureEnabled: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					IsEnabled: true
				});
				return { $promise: queryDeferred.promise };
			}
		},
	}

	var mockPeopleService = {
		search: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					People: [
						{
							FirstName: "Ashley",
							LastName: "Andeen",
							EmploymentNumber: "12345",
							LeavingDate: "2015-04-09",
							OptionalColumnValues: [
								{
									"Key": "CellPhone",
									"Value": "123456"
								}
							],
							Team: "Paris/Team 1"
						}
					],
					OptionalColumns: ["CellPhone"]
				});
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it("should show agent by search function", inject(function($controller) {
		var scope = $rootScope.$new();

		$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService });

		scope.keyword = "ashley";
		scope.searchKeyword();
		scope.$digest(); // this is needed to resolve the promise

		expect(scope.searchResult.length).toEqual(1);
		expect(scope.searchResult[0].FirstName).toEqual("Ashley");
		expect(scope.optionalColumns.length).toEqual(1);
		expect(scope.optionalColumns[0]).toEqual("CellPhone");
		expect(scope.searchResult[0].OptionalColumnValues[0].Key).toEqual("CellPhone");
		expect(scope.searchResult[0].OptionalColumnValues[0].Value).toEqual("123456");
	}));

	//it("should show my team as default keyword", inject(function($controller) {
	//	var scope = $rootScope.$new();
	//	$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService });

	//	scope.searchKeyword();
	//	scope.$digest(); // this is needed to resolve the promise

	//	expect(scope.keyword).toEqual("\"Paris\" \"Team 1\"");
	//}));

	//it("should show agent by search with option", inject(function($controller) {
	//	var scope = $rootScope.$new();
	//	$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService });


	//	scope.advancedSearchForm = {
	//		FirstName: "Ashley Smith",
	//		Organization: "London Shenzhen"
	//	};

	//	scope.advancedSearch();
	//	scope.$digest(); // this is needed to resolve the promise

	//	expect(scope.searchResult.length).toEqual(1);

	//	var firstResult = scope.searchResult[0];
	//	expect(firstResult.FirstName).toEqual("Ashley");
	//	expect(firstResult.OptionalColumnValues[0].Key).toEqual("CellPhone");
	//	expect(firstResult.OptionalColumnValues[0].Value).toEqual("123456");

	//	expect(scope.optionalColumns.length).toEqual(1);
	//	expect(scope.optionalColumns[0]).toEqual("CellPhone");

	//	expect(scope.keyword).toEqual("firstName: Ashley Smith, organization: London Shenzhen");
	//}));

	//it("should change the advanced search field according to simple search input", inject(function($controller) {
	//	var scope = $rootScope.$new();
	//	$controller("PeopleStartCtrl", { $scope: scope, $stateParams: stateParams, Toggle: mockToggleService, People: mockPeopleService });

	//	scope.keyword = "FirstName: Ashley Smith, Organization: London Shenzhen";

	//	scope.validateSearchKeywordChanged();

	//	expect(scope.advancedSearchForm.FirstName).toEqual("Ashley Smith");
	//	expect(scope.advancedSearchForm.Organization).toEqual("London Shenzhen");

	//	scope.keyword = "FirstName: John King";
	//	scope.validateSearchKeywordChanged();

	//	expect(scope.advancedSearchForm.FirstName).toEqual("John King");
	//	expect(scope.advancedSearchForm.Organization).toEqual(undefined);
	//}));

});
