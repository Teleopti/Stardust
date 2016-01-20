'use strict';
describe("PeopleSearchInputCtrl", function () {
	var $rootScope,
		controller;

	beforeEach(function(){
		module('wfm.people');
		module('externalModules');
	});

	beforeEach(inject(function(_$rootScope_, _$controller_) {
		$rootScope = _$rootScope_;
		controller = setUpController(_$controller_);
	}));
	function setUpController($controller) {
		var scope = $rootScope.$new();
		return $controller("PeopleSearchInputCtrl", {$scope:scope});
	}

	it("should parse terms correctly by search with option", inject(function () {
		if (controller.searchOptions == undefined)
			controller.searchOptions = {};
		controller.advancedSearchForm = {
			FirstName: "Ashley Smith",
			Organization: "London Shenzhen"
		};

		controller.advancedSearch();

		expect(controller.searchOptions.keyword).toEqual('firstName: Ashley Smith; organization: London Shenzhen;');
	}));

	it("should handle both single quote and double quote in search value correctly", inject(function () {
		if (controller.searchOptions == undefined)
			controller.searchOptions = {};
		controller.searchOptions.keyword = 'role: "London, Site Admin" \'Team Leader\' Agent; contract:\'Full Time"';

		controller.validateSearchKeywordChanged();

		expect(controller.advancedSearchForm.Role).toEqual('"London, Site Admin" "Team Leader" Agent');
		expect(controller.advancedSearchForm.Contract).toEqual('"Full Time"');
	}));

	it("should handle comma in search value correctly", inject(function () {
		if (controller.searchOptions == undefined)
			controller.searchOptions = {};
		controller.searchOptions.keyword = 'role: "London, Site Admin" Agent; contract:"Full Time"';

		controller.validateSearchKeywordChanged();

		expect(controller.advancedSearchForm.Role).toEqual('"London, Site Admin" Agent');
		expect(controller.advancedSearchForm.Contract).toEqual('"Full Time"');
	}));

	it("should change the advanced search field according to simple search input", inject(function () {
		if (controller.searchOptions == undefined)
			controller.searchOptions = {};
		controller.searchOptions.keyword = 'FirstName: Ashley Smith; Organization: London Shenzhen';

		controller.validateSearchKeywordChanged();

		expect(controller.advancedSearchForm.FirstName).toEqual('Ashley Smith');
		expect(controller.advancedSearchForm.Organization).toEqual('London Shenzhen');

		controller.searchOptions.keyword = 'FirstName: John King';
		controller.validateSearchKeywordChanged();

		expect(controller.advancedSearchForm.FirstName).toEqual('John King');
		expect(controller.advancedSearchForm.Organization).toEqual(undefined);
	}));
});
