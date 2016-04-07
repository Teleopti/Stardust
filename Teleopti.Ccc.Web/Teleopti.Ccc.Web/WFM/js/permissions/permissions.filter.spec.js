﻿'use strict';
describe('PermissionsFilters', function() {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(function(){
		module('wfm.permissions');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it('should filter on the selected functions only', inject(function ($filter) {
		var allFunctions = [{ id: 1, selected: false, nmbSelectedChildren: 1 },
			{ id: 2, selected: false }, { id: 3, selected: true }];
		var filter = $filter('selectedFunctions');

		var filteredArray = filter(allFunctions, true);

		expect(filteredArray.length).toEqual(2);
	}));

	it('should filter on the selected data only', inject(function ($filter) {
	    var allData = [{ id: 1, selected: true, nmbSelectedChildren: 1, Name: 'Name', Type: 'Site',
	             ChildNodes: [{ id: 2, selected: true, Name: 'Name', Type: 'Team' },
                 { id: 3, selected: false, Name: 'Name', Type: 'Team' }]
	    }];
	    var filter = $filter('selectedData');

	    var filteredArray = filter(allData, true);
	    expect(filteredArray.length).toEqual(1);
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

	it('should filter on the unselected data only', inject(function ($filter) {
	    var allData = [
		{
		    id: 1,
		    Type: 'Site',
            Name: 'London',
		    selected: true,
		    ChildNodes: [
				{ id: 4, selected: false }
		    ]
		}];
	    var filter = $filter('unselectedData');

	    var filteredArray = filter(allData, true);

	    expect(filteredArray.length).toEqual(1);
	}));

	it('should find matched parent', inject(function($filter) {
		var nodes = [
			{
				SearchableName: "test",
				ChildNodes: [
					{ SearchableName: "test confirmed" }
				]
			},
			{
				SearchableName: "thing"
			}
		];

		var filter =  $filter('nameFilter');

		var filteredNodes = filter(nodes, "test");
		console.log("Array ", filteredNodes);

		expect(filteredNodes.length).toEqual(1);
	}));

	it('should find matched child nodes', inject(function($filter) {
		var nodes = [
			{
				SearchableName: "test",
				ChildNodes: [
					{ SearchableName: "test confirmed" }
				]
			},
			{
				SearchableName: "thing"
			}
		];
	var filter = $filter('nameFilter');

	var filteredNodes = filter(nodes, "confirmed");

	expect(filteredNodes.length).toEqual(1);

	}));


	it('should find matched parent nodes', inject(function ($filter) {
	    var nodes = [
			{
			    LocalizedFunctionDescription: "test",
			    ChildFunctions: [
					{ LocalizedFunctionDescription: "confirmed" }
			    ]
			},
			{
			    LocalizedFunctionDescription: "thing"
			}
	    ];

	    var filter = $filter('descriptionFilter');

	    var filteredNodes = filter(nodes, "test");

	    expect(filteredNodes.length).toEqual(1);
	}));


	it('should find matched child nodes', inject(function ($filter) {
	    var nodes = [
			{
			    LocalizedFunctionDescription: "test",
			    ChildFunctions: [
					{ LocalizedFunctionDescription: "confirmed" }
			    ]
			},
			{
			    LocalizedFunctionDescription: "thing"
			}
	    ];
	    var filter = $filter('descriptionFilter');

	    var filteredNodes = filter(nodes, "confirmed");

	    expect(filteredNodes.length).toEqual(1);

	}));
});
