﻿'use strict';
describe('PermissionsFilters', function() {
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

	it('should filter on the selected functions only', inject(function ($filter) {
		var allFunctions = [{ id: 1, selected: false, nmbSelectedChildren: 1 },
			{ id: 2, selected: false }, { id: 3, selected: true }];
		var filter = $filter('selectedFunctions');

		var filteredArray = filter(allFunctions, true);

		expect(filteredArray.length).toEqual(2);
	}));


	it('should filter on the unselected functions only', inject(function ($filter) {
		var allFunctions = [
		{
			id: 1,
			selected: true,
			nmbSelectedChildren: 0,
			ChildFunctions: [
				{ id: 4, selected: false }
			]
		},
		{
			id: 2,
			selected: false
		},
		{
			id: 3,
			selected: true
		}];
		var filter = $filter('unselectedFunctions');

		var filteredArray = filter(allFunctions, true);

		expect(filteredArray.length).toEqual(2); 
	}));

	it('shold find matched parent nodes', inject(function($filter) {
		var nodes = [
			{
				Name: "test",
				ChildNodes: [
					{ Name: "confirmed" }
				]
			},
			{
				Name: "thing"
			}
		];

		var filter =  $filter('nameFilter');

		var filteredNodes = filter(nodes, "test");

		expect(filteredNodes.length).toEqual(1);
	}));


	it('should find matched child nodes', inject(function($filter) {
		var nodes = [
			{
				Name: "test",
				ChildNodes: [
					{ Name: "confirmed" }
				]
			},
			{
				Name: "thing"
			}
		];
	var filter = $filter('nameFilter');

	var filteredNodes = filter(nodes, "confirmed");

	expect(filteredNodes.length).toEqual(1);

	}));

});