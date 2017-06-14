'use strict';
describe('permissionsDataService', function() {
	var $httpBackend,
		fakeBackend,
		permissionsDataService,
		response;

	var role = {
		BuiltIn: false,
		DescriptionText: 'Agent',
		Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
		IsAnyBuiltIn: true,
		IsMyRole: false,
		Name: 'Agent'
	};

	var BusinessUnit = {
		Id: '928dd0bc-bf40-412e-b970-9b5e015aadea',
		Name: 'TeleoptiCCCDemo',
		Type: 'BusinessUnit',
		IsSelected: true,
		ChildNodes: [{
			Id: 'fe113bc0-979a-4b6c-9e7c-ef601c7e02d1',
			Type: 'Site',
			Name: 'Site1',
			IsSelected: true,
			ChildNodes: [{
				Id: 'e6377d56-277d-4c22-97f3-b218741b2480',
				Type: 'Team',
				Name: 'Team1',
				IsSelected: true
			}]
		}]
	};

	var childFunction1 = {
		ChildFunctions: [],
		FunctionCode: 'child1',
		FunctionDescription: 'child1',
		FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
		IsDisabled: false,
		LocalizedFunctionDescription: 'child1'
	};
	var childFunction2 = {
		ChildFunctions: [],
		FunctionCode: 'child2',
		FunctionDescription: 'child2',
		FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
		IsDisabled: false,
		LocalizedFunctionDescription: 'child2'
	};
	var defaultApplicationFunction = {
		FunctionCode: 'Raptor',
		FunctionDescription: 'xxOpenRaptorApplication',
		FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
		IsDisabled: false,
		LocalizedFunctionDescription: 'Open Teleopti WFM',
		ChildFunctions: [childFunction1]
	};
	var defaultApplicationFunction2 = {
		FunctionCode: 'Anywhere',
		FunctionDescription: 'xxAnywhere',
		FunctionId: '7884b7dd-31ea-4e40-b004-c7ce3b5deaf3',
		IsDisabled: false,
		LocalizedFunctionDescription: 'Open Teleopti TEM',
		ChildFunctions: [childFunction2]
	};

	var dynamicOption = {
		RangeOption: 0,
		Name: "Test"
	};

	beforeEach(function() {
		module('wfm.permissions');
	});

	beforeEach(inject(function(_$httpBackend_, _fakePermissionsBackend_, _permissionsDataService_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		permissionsDataService = _permissionsDataService_;

		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Functions').respond(function(method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/f19bb790-b000-4deb-97db-9b5e015b2e8c').respond(function(method, url, data, headers) {
			response = true;
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData').respond(function(method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/DeleteData').respond(function(method, url, data, headers) {
			return 200;
		});

	}));

	afterEach(function() {
		response = null;
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should send functions to server', function() {
		var functions = [defaultApplicationFunction.FunctionId];

		permissionsDataService.selectFunction(role, functions, defaultApplicationFunction);
		$httpBackend.flush();

		expect(response).toEqual({
			Id: role.Id,
			Functions: functions
		});
	});

	it('should delete function', function() {
		var functions = [];

		permissionsDataService.selectFunction(role, functions, defaultApplicationFunction);
		$httpBackend.flush();

		expect(response).toEqual(true);
	});

	it('should prepare all org data for sending to server when selecting bu', function() {
		var data = permissionsDataService.prepareData(BusinessUnit, role);

		expect(data.Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		expect(data.BusinessUnits[0]).toEqual('928dd0bc-bf40-412e-b970-9b5e015aadea');
		expect(data.Sites[0]).toEqual('fe113bc0-979a-4b6c-9e7c-ef601c7e02d1');
		expect(data.Teams[0]).toEqual('e6377d56-277d-4c22-97f3-b218741b2480');
	});

	it('should send all org data to server when selecting bu', function() {
		var data = permissionsDataService.prepareData(BusinessUnit, role);

		permissionsDataService.selectOrganization(BusinessUnit, role, true);
		$httpBackend.flush();

		expect(response).toEqual(data);
	});

	it('should send site to server when selecting last team', function() {
		var data = permissionsDataService.prepareData(BusinessUnit, role);

		var obj = {
			Id: role.Id,
			Teams: [
				BusinessUnit.ChildNodes[0].ChildNodes[0].Id
			],
			Sites: [
				BusinessUnit.ChildNodes[0].Id
			]
		}

		permissionsDataService.setIdForSiteWithAllTeamsSelected(BusinessUnit.ChildNodes[0].Id);

		permissionsDataService.selectOrganization(BusinessUnit.ChildNodes[0].ChildNodes[0], role, true);
		$httpBackend.flush();

		expect(response).toEqual(obj);
	});


	it('should not persist previously sent bu', function() {
		var data = permissionsDataService.prepareData(BusinessUnit, role);

		permissionsDataService.selectOrganization({
				ChildNodes: [{
					Id: 'fe113bc0-979a-4b6c-9e7c-ef601c7e02d1',
					Type: 'Site',
					Name: 'Site1',
					IsSelected: true,
					ChildNodes: [{
						Id: 'e6377d56-277d-4c22-97f3-b218741b2480',
						Type: 'Team',
						Name: 'Team1',
						IsSelected: true
					}]
				}],
				Id: '928dd0bc-bf40-412e-b970-9b5e015aadea',
				Name: 'TeleoptiCCCDemo',
				Type: 'BusinessUnit',
				IsSelected: false
			},
			role
		);
		$httpBackend.flush();

		expect(data.BusinessUnits.length).toEqual(1);
	});

	it('should send dynamic option data to server', function() {
		var data = permissionsDataService.prepareDynamicOption(dynamicOption, role);

		permissionsDataService.selectDynamicOption(dynamicOption, role);
		$httpBackend.flush();

		expect(response).toEqual(data)
	});

	it('should find all child functions for a function', function() {
		var fn = {
			Name: "First",
			ChildFunctions: [{
				Name: "Second",
				ChildFunctions: [{
					Name: "Third"
				}]
			}]
		};

		var result = permissionsDataService.findChildFunctions(fn)
			.map(function(func) {
				return func.Name;
			});
		expect(result).toEqual(["Second", "Third"]);
	});

	it('should find all parent functions for a function', function() {
		var third = {
			FunctionId: '123',
			Name: "Third"
		};
		var fn = {
			Name: "First",
			FunctionId: '456',
			ChildFunctions: [{
				Name: "Second",
				FunctionId: '789',
				ChildFunctions: [third]
			}]
		};

		var result = permissionsDataService.findParentFunctions([fn], third)
			.map(function(func) {
				return func.Name;
			});
		expect(result).toEqual(["First", "Second"]);
	});

	it('should find all parent functions for a function', function() {
		var clicked = {
			Name: "D",
			FunctionId: '123',
			ChildFunctions: [{
				Name: "C",
				FunctionId: '456',
				ChildFunctions: []
			}]
		};
		var fn = {
			Name: "A",
			FunctionId: '789',
			ChildFunctions: [{
				Name: "B",
				FunctionId: '101',
				ChildFunctions: [{
					Name: "C-1",
					FunctionId: '102',
					ChildFunctions: [clicked]
				}, {
					Name: "C-2",
					FunctionId: '103',
					ChildFunctions: [{
						Name: "D",
						FunctionId: '104'
					}]
				}]
			}]
		};

		var result = permissionsDataService.findParentFunctions([fn], clicked)
			.map(function(func) {
				return func.Name;
			});
		expect(result).toEqual(["A", "B", "C-1"]);
	});

});
