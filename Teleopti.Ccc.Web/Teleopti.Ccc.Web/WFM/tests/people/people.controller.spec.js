'use strict';
describe("PeopleCtrl", function () {
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
	 
	var mockSearchService = {
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
							}],
							Team: "Paris/Team 1"
						}
					],
					OptionalColumns:["CellPhone"]
				});
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it("should show agent by search function", inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller("PeopleCtrl", { $scope: scope, PeopleSearch: mockSearchService });

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
	it("should show my team as default keyword", inject(function($controller) {
		var scope = $rootScope.$new();

		$controller("PeopleCtrl", { $scope: scope, PeopleSearch: mockSearchService });

		scope.searchKeyword();
		scope.$digest(); // this is needed to resolve the promise

		expect(scope.keyword).toEqual("Paris/Team 1");
	})); 
});
