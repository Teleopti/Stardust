'use strict';

describe('PermissionsController', function() {

	var $httpBackend,
		fakeBackend,
		$timeout,
		$controller,
		permissionsDataService,
		vm,
		response;

	var defaultRole,
		defaultRole2,
		adminRole;

	var allFunction,
		defaultApplicationFunction,
		defaultApplicationFunction2,
		childFunction1,
		childFunction2;

	var organizationSelection;

	beforeEach(function() {
		module('wfm.permissions');
	});

	beforeEach(inject(function(_$httpBackend_, _fakePermissionsBackend_, _$controller_, _$timeout_, _permissionsDataService_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$controller = _$controller_;
		$timeout = _$timeout_;
		permissionsDataService = _permissionsDataService_;

		fakeBackend.clear();
		vm = $controller('PermissionsRefactController');

		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');

		$httpBackend.whenPOST('../api/Permissions/Roles').respond(function(method, url, data, headers) {
			return [201, {
				DescriptionText: 'rolename',
				Id: '123'
			}];
		});

		$httpBackend.whenDELETE('../api/Permissions/Roles/AvailableData/41815804-7c51-4f43-a1ae-ecb2a17a2177').respond(function(method, url, data, headers) {
			return 200;
		});

		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/f1e9237f-7bd8-4ca9-aebb-e49b80d8a4cf').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPUT('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64?newDescription=%7B%7D').respond(function(method, url, data, headers) {
			response = angular.fromJson(data);
			fakeBackend.setName(response.Id, response.newDescription)
			return [200, {
				newDescription: response.newDescription
			}];
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Copy').respond(function(method, url, data, headers) {
			return [200, {
				Name: 'Agent',
				Id: '123'
			}];
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/DeleteFunction/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/DeleteFunction/8ecf6029-4f3c-409c-89db-46bd8d7d402d').respond(function(method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/Functions').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/4b102279-888a-45ee-b537-b48036bc27d0/Functions').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Functions').respond(function(method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});

		allFunction = {
			ChildFunctions: [],
			FunctionCode: 'All',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
			IsDisabled: false,
			IsSelected: false,
			LocalizedFunctionDescription: 'All'
		};
		childFunction1 = {
			ChildFunctions: [],
			FunctionCode: 'child1',
			FunctionDescription: 'child1',
			FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
			IsDisabled: false,
			LocalizedFunctionDescription: 'child1'
		};
		childFunction2 = {
			ChildFunctions: [],
			FunctionCode: 'child2',
			FunctionDescription: 'child2',
			FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
			IsDisabled: false,
			LocalizedFunctionDescription: 'child2'
		};
		defaultApplicationFunction = {
			FunctionCode: 'Raptor',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti WFM',
			ChildFunctions: [childFunction1]
		};
		defaultApplicationFunction2 = {
			FunctionCode: 'Anywhere',
			FunctionDescription: 'xxAnywhere',
			FunctionId: '7884b7dd-31ea-4e40-b004-c7ce3b5deaf3',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti TEM',
			ChildFunctions: [childFunction2]
		};
		defaultRole = {
			BuiltIn: false,
			DescriptionText: 'Agent',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'Agent',
			IsSelected: true
		};
		defaultRole2 = {
			BuiltIn: false,
			DescriptionText: 'Carl',
			Id: 'f1e9237f-7bd8-4ca9-aebb-e49b80d8a4cf',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			IsSelected: false,
			Name: 'Agent'
		};
		adminRole = {
			BuiltIn: true,
			DescriptionText: 'SuperAdmin',
			Id: '7afefc4f-3231-401b-8174-6525a5e47f23',
			IsAnyBuiltIn: true,
			IsMyRole: true,
			Name: '_superAdmin'
		};
		organizationSelection = {
			BusinessUnit: {
				Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
				Name: "TeleoptiCCCDemo",
				Type: "BusinessUnit",
				ChildNodes: [{
					Id: '41815804-7c51-4f43-a1ae-ecb2a17a2177',
					Name: 'Site1',
					ChildNodes: [{
						Id: '58643cfb-6f7d-4128-a014-0c8f82209d68',
						Name: 'Team1',
					}]
				}]
			},
			DynamicOptions: [{
				RangeOption: 0,
				Name: "None"
			}]
		};
	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	describe('PermissionsController - roles', function() {

		it('should get a role', function() {
			fakeBackend
				.withRole(defaultRole);

			$httpBackend.flush();

			expect(vm.roles[0].Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		});

		it('should get roles', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRole(adminRole);

			$httpBackend.flush();

			expect(vm.roles.length).toEqual(2);
		});

		it('should create role', function() {
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush();

			expect(vm.roles.length).toBe(1);
		});

		it('should not be able to create role with no name', function() {
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole();

			expect(vm.roles.length).toBe(0);
		});

		it('should select newly created role', function() {
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush()

			expect(vm.roles[0].IsSelected).toBe(true);
		});

		it('should toggle input if role has all function available', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '123',
						FunctionCode: 'All'
					}]
				})
				.withApplicationFunction(defaultRole);
			$httpBackend.flush();

			vm.selectRole(defaultRole);
			$httpBackend.flush()

			expect(vm.isAllFunctionSelected).toEqual(true);
		});

		it('should unselect previous selected role when creating new role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush()

			expect(vm.roles[0].IsSelected).toBe(true);
			expect(vm.roles[1].IsSelected).toBe(false);
		});

		it('should unselect new roll when selecting other role', function() {
			var name = 'rolename';
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush()
			vm.selectRole(vm.roles[1]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toBe(false);
			expect(vm.roles[1].IsSelected).toBe(true);

		});

		it('should close create role modal on submit', function() {
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush();

			expect(vm.showCreateModal).toBe(false);
		});

		it('should clear new role name on submit', function() {
			var name = 'rolename';
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush();

			expect(vm.roleName).toBe('');
		});

		it('should put newly created roll on top of roles list', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRole(adminRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.createRole('rolename');
			$httpBackend.flush();

			expect(vm.roles[0].DescriptionText).toBe('rolename');
		});

		it('should be able to edit the name of a role', function() {
			fakeBackend
				.withRole(defaultRole);
			$httpBackend.flush();

			vm.editRole('newRoleName', vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[0].DescriptionText).toBe('newRoleName');
		});

		it('should select edited role after name change', function() {
			fakeBackend.withRole(defaultRole);
			$httpBackend.flush();

			vm.editRole('newRoleName', vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toBe(true);
		});

		it('should be able to delete a roll', function() {
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();

			vm.deleteRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles.length).toEqual(0);
		});

		it('should clear selected role when deleting selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();
			vm.selectRole = vm.roles[0];

			vm.deleteRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedRole.Id).not.toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		});

		it('should not be able to delete a built in roll', function() {
			fakeBackend
				.withRole(adminRole);
			$httpBackend.flush();

			vm.deleteRole(vm.roles[0]);

			expect(vm.roles.length).toEqual(1);
		});

		it('should not be able to modify Built in role', function() {
			fakeBackend.withRole(adminRole);
			$httpBackend.flush();

			var result = vm.checkMyRole(vm.roles[0]);

			expect(result).toEqual(false);
		});

		it('should be able to copy a role', function() {
			fakeBackend
				.withRole(defaultRole);
			$httpBackend.flush();

			vm.copyRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles.length).toEqual(2);
			expect(vm.roles[0].Name).toEqual('Agent');
			expect(vm.roles[1].Name).toEqual('Agent');
		});

		it('should select newly copied role', function() {
			fakeBackend
				.withRole(defaultRole);
			$httpBackend.flush();

			vm.copyRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[1].IsSelected).toBe(true);
		});

		it('should set newly copied role to selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}]
				});
			$httpBackend.flush();
			vm.selectedRole = vm.roles[0];
			vm.copyRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[1].IsSelected).toEqual(true);
			expect(vm.selectedRole.Id).not.toEqual(vm.roles[1].Id)
			expect(vm.roles.length).toEqual(2);
		});

		it('should unselect previously selected role when copying role', function() {
			fakeBackend
				.withRole(defaultRole);
			$httpBackend.flush();

			vm.copyRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toBe(false);
		});

		it('should put copied role under the copied role', function() {
			fakeBackend
				.withRole(defaultRole2)
				.withRole(defaultRole);
			$httpBackend.flush();

			vm.copyRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[1].Name).toEqual('Agent');
		});

		it('should be able to select a role', function() {
			fakeBackend
				.withRole(defaultRole);

			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toBe(true);
		});

		it('should deselect selcted role when selecting another role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRole(defaultRole2)
				.withRoleInfo({
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					AvailableFunctions: []
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();
			vm.selectRole(vm.roles[1]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toBe(false);
			expect(vm.roles[1].IsSelected).toBe(true);
		});

		it('should fetch additional info for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}]
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedRole.AvailableFunctions[0].Id).toEqual('63656f9a-be16-4bb7-9012-9bde01232075');
			expect(vm.selectedRole.Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		});

	});

	describe('PermissionsController - functions', function() {

		it('should get an application function', function() {
			fakeBackend
				.withApplicationFunction(defaultApplicationFunction);

			$httpBackend.flush();

			expect(vm.applicationFunctions[0].ChildFunctions[0]).toEqual(childFunction1);
			expect(vm.applicationFunctions[0].FunctionCode).toEqual('Raptor');
			expect(vm.applicationFunctions[0].FunctionDescription).toEqual('xxOpenRaptorApplication');
			expect(vm.applicationFunctions[0].FunctionId).toEqual('f19bb790-b000-4deb-97db-9b5e015b2e8c');
			expect(vm.applicationFunctions[0].IsDisabled).toEqual(false);
			expect(vm.applicationFunctions[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
		});

		it('should get application functions', function() {
			fakeBackend
				.withApplicationFunction(defaultApplicationFunction)
				.withApplicationFunction(defaultApplicationFunction2);

			$httpBackend.flush();

			expect(vm.applicationFunctions.length).toEqual(2);
		});

		it('should allways keep top node open', function() {
			fakeBackend
				.withApplicationFunction(allFunction)
				.withApplicationFunction(defaultApplicationFunction);

			$httpBackend.flush();
			vm.prepareTree(vm.applicationFunctions);

			expect(vm.applicationFunctions[1].IsOpen).toEqual(true);
		});

		it('should remove function with the name All', function() {
			fakeBackend
				.withApplicationFunction(allFunction)
				.withApplicationFunction(defaultApplicationFunction);

			$httpBackend.flush();
			vm.prepareTree(vm.applicationFunctions);

			expect(vm.applicationFunctions[0].IsHidden).toEqual(true);
		});

		it('should unselect role functions when deleting that role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withApplicationFunction(defaultApplicationFunction)
				.withRoleInfo({
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}]
				})
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();
			vm.selectedRole = vm.roles[0];
			vm.deleteRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(undefined);
		});

		it('should not unselect role functions for selected role when deleting other role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withApplicationFunction(defaultApplicationFunction)
				.withRoleInfo({
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}]
				})
				.withRole(defaultRole2)
				.withOrganizationSelection(organizationSelection.BusinessUnit);
			$httpBackend.flush();
			vm.selectRole(vm.roles[0]);

			vm.deleteRole(vm.roles[1]);
			$httpBackend.flush();

			expect(vm.roles[0].IsSelected).toEqual(true);
			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(true);
			expect(vm.roles.length).toEqual(1);
		});

		it('should indicate available functions for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '7884b7dd-31ea-4e40-b004-c7ce3b5deaf3'
					}]
				})
				.withApplicationFunction(defaultApplicationFunction2);
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions['7884b7dd-31ea-4e40-b004-c7ce3b5deaf3']).toEqual(true);
		});

		it('should indicate available functions for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: defaultApplicationFunction2.Id
					}]
				}).withApplicationFunction({
					FunctionId: defaultApplicationFunction2.Id,
					ChildFunctions: []
				}).withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232074',
					ChildFunctions: []
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions[defaultApplicationFunction2.Id]).toEqual(true);
			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232074']).toEqual(undefined);
		});

		it('should indicate all available functions for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075',
					}, {
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
					}]
				})
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					ChildFunctions: [{
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(true);
			expect(vm.selectedFunctions['f73154af-8d6d-4250-b066-d6ead56bfc16']).toEqual(true);
		});

		it('should not select all functions checkbox when selecting second to last unselected function', function() {
			$httpBackend.whenDELETE('../api/Permissions/Roles/Function/5ad43bfa-7842-4cca-ae9e-8d03ddc789e9').respond(200);
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
						Id: defaultRole.Id
					})
				.withApplicationFunction(allFunction)
				.withApplicationFunction(defaultApplicationFunction)
				.withApplicationFunction(defaultApplicationFunction2)

				$httpBackend.flush();
				vm.onFunctionClick(vm.applicationFunctions[1].ChildFunctions[0]);
				$httpBackend.flush();

				expect(vm.isAllFunctionSelected).toEqual(false);
		});

		it('should indicate all available functions for selected role but BETTER', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}, {
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				})
				.withApplicationFunction({
					FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
					Name: 'FunctionOne',
					ChildFunctions: []
				})
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(true);
			expect(vm.selectedFunctions['6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7']).toEqual(undefined);
			expect(vm.selectedFunctions['f73154af-8d6d-4250-b066-d6ead56bfc16']).toEqual(true);
		});

		it('should indicate all ApplicationFunctions and its ChildFunctions for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}, {
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}, {
						Id: '52b6f3de-55e0-45b2-922e-316f8538f60c'
					}]
				})
				.withApplicationFunction({
					FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
					Name: 'FunctionOne',
					ChildFunctions: []
				})
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [{
							FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
							ChildFunctions: [{
								FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
							}, {
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
							}]
						}, {
							FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
						}]
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(true);
			expect(vm.selectedFunctions['f73154af-8d6d-4250-b066-d6ead56bfc16']).toEqual(true);
			expect(vm.selectedFunctions['52b6f3de-55e0-45b2-922e-316f8538f60c']).toEqual(true);
		});

		it('should select all functions when toggle all function', function() {
			fakeBackend
				.withApplicationFunction(allFunction)
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [{
							FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
							ChildFunctions: [{
								FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
							}, {
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
							}]
						}, {
							FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
						}]
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();

			vm.toggleAllFunction();
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(true);
			expect(vm.selectedFunctions['6856320f-1391-4332-bcaa-6cf16cbcb8dc']).toEqual(true);
			expect(vm.selectedFunctions['52b6f3de-55e0-45b2-922e-316f8538f60c']).toEqual(true);
			expect(vm.selectedFunctions['f73154af-8d6d-4250-b066-d6ead56bfc16']).toEqual(true);
		});

		it('should save all functions for role when toggle all function', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: []
				})
				.withApplicationFunction(allFunction)
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [{
							FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
							ChildFunctions: [{
								FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
							}, {
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
							}]
						}, {
							FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
						}]
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();
			var expectedResponse = {
				Id: "e7f360d3-c4b6-41fc-9b2d-9b5e015aae64",
				Functions: ["8ecf6029-4f3c-409c-89db-46bd8d7d402d",
					"63656f9a-be16-4bb7-9012-9bde01232075",
					"6856320f-1391-4332-bcaa-6cf16cbcb8dc",
					"52b6f3de-55e0-45b2-922e-316f8538f60c",
					"6856320f-1391-4332-bcaa-6cf16cbcb8dc",
					"52b6f3de-55e0-45b2-922e-316f8538f60c",
					"f73154af-8d6d-4250-b066-d6ead56bfc16",
					"f73154af-8d6d-4250-b066-d6ead56bfc16"
				]
			};

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();
			vm.toggleAllFunction();
			$httpBackend.flush();

			expect(response).toEqual(expectedResponse);
		});

		it('should delete all functions for role when untoggle all function', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: []
				})
				.withApplicationFunction(allFunction)
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '7656320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [{
							FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
							ChildFunctions: [{
								FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
							}, {
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
							}]
						}, {
							FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
						}]
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();
			var expectedResponse = {
				Id: "e7f360d3-c4b6-41fc-9b2d-9b5e015aae64",
				FunctionId: "8ecf6029-4f3c-409c-89db-46bd8d7d402d",
				Functions: ["8ecf6029-4f3c-409c-89db-46bd8d7d402d",
					"63656f9a-be16-4bb7-9012-9bde01232075",
					"7656320f-1391-4332-bcaa-6cf16cbcb8dc",
					"52b6f3de-55e0-45b2-922e-316f8538f60c",
					"6856320f-1391-4332-bcaa-6cf16cbcb8dc",
					"52b6f3de-55e0-45b2-922e-316f8538f60c",
					"f73154af-8d6d-4250-b066-d6ead56bfc16",
					"f73154af-8d6d-4250-b066-d6ead56bfc16"
				]
			}

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();
			vm.toggleAllFunction();
			$httpBackend.flush();
			vm.toggleAllFunction();
			$httpBackend.flush();

			expect(response).toEqual(expectedResponse);
		});

		it('should deselect all functions when toggle all function', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}, {
						Id: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
					}, {
						Id: '52b6f3de-55e0-45b2-922e-316f8538f60c'
					}, {
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				})
				.withApplicationFunction({
					FunctionId: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					Name: 'All',
					ChildFunctions: []
				})
				.withApplicationFunction({
					FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
					Name: 'FunctionTwo',
					ChildFunctions: [{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [{
							FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
							ChildFunctions: [{
								FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
							}, {
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
							}]
						}, {
							FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
						}]
					}, {
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}]
				});
			$httpBackend.flush();

			vm.isAllFunctionSelected = true;
			vm.toggleAllFunction();
			$httpBackend.flush();

			expect(vm.selectedFunctions['63656f9a-be16-4bb7-9012-9bde01232075']).toEqual(undefined);
			expect(vm.selectedFunctions['6856320f-1391-4332-bcaa-6cf16cbcb8dc']).toEqual(undefined);
			expect(vm.selectedFunctions['52b6f3de-55e0-45b2-922e-316f8538f60c']).toEqual(undefined);
			expect(vm.selectedFunctions['f73154af-8d6d-4250-b066-d6ead56bfc16']).toEqual(undefined);
		});
	});

	describe('PermissionsController - organization', function() {

		it('should get an organization selection', function() {
			fakeBackend
				.withOrganizationSelection(organizationSelection.BusinessUnit, organizationSelection.DynamicOptions);
			$httpBackend.flush();

			expect(vm.organizationSelection.BusinessUnit.Id).toEqual("928dd0bc-bf40-412e-b970-9b5e015aadea");
			expect(vm.organizationSelection.BusinessUnit.Name).toEqual("TeleoptiCCCDemo");
			expect(vm.organizationSelection.BusinessUnit.Type).toEqual("BusinessUnit");
			expect(vm.organizationSelection.DynamicOptions[0].RangeOption).toEqual(0);
			expect(vm.organizationSelection.DynamicOptions[0].Name).toEqual("None");
		});

		it('should send selected dynamic option to the server', function() {
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit, organizationSelection.DynamicOptions);
			$httpBackend.flush();
			vm.selectedRole = vm.roles[0];
			spyOn(permissionsDataService, 'selectDynamicOption');

			vm.selectDynamicOption(organizationSelection.DynamicOptions[0]);

			expect(permissionsDataService.selectDynamicOption).toHaveBeenCalledWith(organizationSelection.DynamicOptions[0], vm.selectedRole);
		});

		it('should unselect selected org data for previous role when creating new role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [],
					AvailableBusinessUnits: [{
						Id: organizationSelection.BusinessUnit.Id,
						Name: organizationSelection.BusinessUnit.Name
					}],
					AvailableSites: organizationSelection.BusinessUnit.ChildNodes,
					AvailableTeams: organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes
				})
				.withOrganizationSelection(organizationSelection.BusinessUnit, organizationSelection.DynamicOptions);
			$httpBackend.flush();
			vm.selectedOrgData['928dd0bc-bf40-412e-b970-9b5e015aadea'] = true;
			vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177'] = true;
			vm.selectedOrgData['58643cfb-6f7d-4128-a014-0c8f82209d68'] = true;

			vm.createRole('Johan Dam');
			$httpBackend.flush();

			expect(vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177']).toEqual(undefined);
			expect(vm.selectedOrgData['58643cfb-6f7d-4128-a014-0c8f82209d68']).toEqual(undefined);
		});

		it('should unselect selected org data for previous role when deleting that role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [],
					AvailableBusinessUnits: organizationSelection.BusinessUnit,
					AvailableSites: organizationSelection.BusinessUnit.ChildNodes,
					AvailableTeams: organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes
				})
				.withOrganizationSelection(organizationSelection.BusinessUnit, organizationSelection.DynamicOptions);
			$httpBackend.flush();
			vm.selectedOrgData['928dd0bc-bf40-412e-b970-9b5e015aadea'] = true;
			vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177'] = true;
			vm.selectedOrgData['58643cfb-6f7d-4128-a014-0c8f82209d68'] = true;

			vm.deleteRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177']).toEqual(undefined);
			expect(vm.selectedOrgData['58643cfb-6f7d-4128-a014-0c8f82209d68']).toEqual(undefined);
		});

		it('should fetch organization info for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withOrganizationSelection(organizationSelection.BusinessUnit)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableDataRange: 3,
					AvailableFunctions: [],
					AvailableBusinessUnits: [],
					AvailableSites: [{
						Id: 'd66f60f5-264c-4277-80eb-9b5e015ab495'
					}],
					AvailableTeams: [{
						Id: '6a21c802-7a34-4917-8dfd-9b5e015ab461'
					}]
				})
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedRole.AvailableDataRange).toEqual(3);
			expect(vm.selectedRole.AvailableSites[0].Id).toEqual('d66f60f5-264c-4277-80eb-9b5e015ab495');
			expect(vm.selectedRole.AvailableTeams[0].Id).toEqual('6a21c802-7a34-4917-8dfd-9b5e015ab461');
		});

		it('should indicate available site for selected role', function() {
			fakeBackend
				.withRole(defaultRole)
				.withRoleInfo({
					Id: defaultRole.Id,
					AvailableFunctions: [],
					AvailableBusinessUnits: [{
						Id: organizationSelection.BusinessUnit.Id
					}],
					AvailableDataRange: 3,
					AvailableSites: [{
						Id: organizationSelection.BusinessUnit.ChildNodes[0].Id
					}],
					AvailableTeams: [{
						Id: organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].Id
					}]
				})
				.withOrganizationSelection(organizationSelection.BusinessUnit, organizationSelection.DynamicOptions);
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();

			expect(vm.selectedOrgData['928dd0bc-bf40-412e-b970-9b5e015aadea']).toBe(true);
			expect(vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177']).toBe(true);
			expect(vm.selectedOrgData[organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].Id]).toBe(true);
		});

		it('should indicate available org data when selecting a different role', function() {
			fakeBackend
				.withRole({
					Id: '4b102279-888a-45ee-b537-b48036bc27d0',
					Name: 'Carl'
				})
				.withRoleInfo({
					Id: '4b102279-888a-45ee-b537-b48036bc27d0',
					AvailableFunctions: [],
					AvailableBusinessUnits: [{
						Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
						Name: "TeleoptiCCCDemo"
					}],
					AvailableSites: [{
						Id: "b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1",
						Name: "London",
					}],
					AvailableTeams: [{
						Id: "95de4e34-6f18-4129-a962-7cf0d57f1ac4",
						Name: "London-Team1",
					}, {
						Id: "6f4cec84-f5ce-4e62-9fc2-274172ec385b",
						Name: "London-Team2",
					}]
				})
				.withRole({
					Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
					Name: 'Herman'
				})
				.withRoleInfo({
					Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
					AvailableFunctions: [],
					AvailableBusinessUnits: [{
						Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
						Name: "TeleoptiCCCDemo"
					}],
					AvailableSites: [{
						Id: "123f651b-5235-40d4-9d34-6be51b55df65",
						Name: "Paris",
					}],
					AvailableTeams: [{
						Id: "000478e4-d11f-42bc-9ead-435f2eab160a",
						Name: "Paris-Team1",
					}, {
						Id: "930addd3-79c6-47ef-96e2-6c68c8f522f3",
						Name: "Paris-Team2",
					}]
				})
				.withOrganizationSelection({
					Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
					Name: "TeleoptiCCCDemo",
					ChildNodes: [{
						Id: 'b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1',
						Name: 'London',
						ChildNodes: [{
							Id: "95de4e34-6f18-4129-a962-7cf0d57f1ac4",
							Name: "London-Team1"
						}, {
							Id: "6f4cec84-f5ce-4e62-9fc2-274172ec385b",
							Name: "London-Team2"
						}]
					}, {
						Id: '123f651b-5235-40d4-9d34-6be51b55df65',
						Name: 'Paris',
						ChildNodes: [{
							Id: "000478e4-d11f-42bc-9ead-435f2eab160a",
							Name: "Paris-Team1",
						}, {
							Id: "930addd3-79c6-47ef-96e2-6c68c8f522f3",
							Name: "Paris-Team2",
						}]
					}]
				}, []);
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();
			vm.selectRole(vm.roles[1]);
			$httpBackend.flush();

			expect(vm.selectedOrgData['729b9399-d80a-46a7-a0dd-df18b89b12b9']).toBe(true);
			expect(vm.selectedOrgData['b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1']).toBe(undefined);
			expect(vm.selectedOrgData['95de4e34-6f18-4129-a962-7cf0d57f1ac4']).toBe(undefined);
			expect(vm.selectedOrgData['6f4cec84-f5ce-4e62-9fc2-274172ec385b']).toBe(undefined);
			expect(vm.selectedOrgData['123f651b-5235-40d4-9d34-6be51b55df65']).toBe(true);
			expect(vm.selectedOrgData['000478e4-d11f-42bc-9ead-435f2eab160a']).toBe(true);
			expect(vm.selectedOrgData['930addd3-79c6-47ef-96e2-6c68c8f522f3']).toBe(true);
		});

		it('should indicate available org data when selecting a different role without selected BU', function() {
			fakeBackend
				.withRole({
					Id: '4b102279-888a-45ee-b537-b48036bc27d0',
					Name: 'Carl'
				})
				.withRoleInfo({
					Id: '4b102279-888a-45ee-b537-b48036bc27d0',
					AvailableFunctions: [],
					AvailableBusinessUnits: [],
					AvailableSites: [],
					AvailableTeams: [{
						Id: "95de4e34-6f18-4129-a962-7cf0d57f1ac4",
						Name: "London-Team1"
					}]
				})
				.withRole({
					Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
					Name: 'Herman'
				})
				.withRoleInfo({
					Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
					AvailableFunctions: [],
					AvailableBusinessUnits: [],
					AvailableSites: [],
					AvailableTeams: [{
						Id: "000478e4-d11f-42bc-9ead-435f2eab160a",
						Name: "Paris-Team1"
					}, {
						Id: "930addd3-79c6-47ef-96e2-6c68c8f522f3",
						Name: "Paris-Team2"
					}]
				})
				.withOrganizationSelection({
					Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
					Name: "TeleoptiCCCDemo",
					ChildNodes: [{
						Id: 'b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1',
						Name: 'London',
						ChildNodes: [{
							Id: "95de4e34-6f18-4129-a962-7cf0d57f1ac4",
							Name: "London-Team1"
						}, {
							Id: "6f4cec84-f5ce-4e62-9fc2-274172ec385b",
							Name: "London-Team2"
						}]
					}, {
						Id: '123f651b-5235-40d4-9d34-6be51b55df65',
						Name: 'Paris',
						ChildNodes: [{
							Id: "000478e4-d11f-42bc-9ead-435f2eab160a",
							Name: "Paris-Team1",
						}, {
							Id: "930addd3-79c6-47ef-96e2-6c68c8f522f3",
							Name: "Paris-Team2",
						}]
					}]
				}, []);
			$httpBackend.flush();

			vm.selectRole(vm.roles[0]);
			$httpBackend.flush();
			vm.selectRole(vm.roles[1]);
			$httpBackend.flush();

			expect(vm.selectedOrgData['95de4e34-6f18-4129-a962-7cf0d57f1ac4']).toBe(undefined);
			expect(vm.selectedOrgData['000478e4-d11f-42bc-9ead-435f2eab160a']).toBe(true);
			expect(vm.selectedOrgData['930addd3-79c6-47ef-96e2-6c68c8f522f3']).toBe(true);
		});
	});

	describe('PermissionsController - filters', function() {

		it('should only show selected function when pressing selected filter', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableFunctions: [{
							Id: defaultApplicationFunction.FunctionId
						}]
					})
					.withApplicationFunction(defaultApplicationFunction)
					.withApplicationFunction(defaultApplicationFunction2);
				$httpBackend.flush();
				vm.selectedFunctions[defaultApplicationFunction.FunctionId] = true;

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.selectedFunctionsFilter();

				expect(vm.componentFunctions.length).toEqual(1);
			});
		});

		it('should only show unselected function when pressing unselected filter', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableFunctions: [{
							Id: defaultApplicationFunction.FunctionId
						}]
					})
					.withApplicationFunction(defaultApplicationFunction)
					.withApplicationFunction(defaultApplicationFunction2);
				$httpBackend.flush();
				vm.selectedFunctions[defaultApplicationFunction.FunctionId] = true;

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.unSelectedFunctionsFilter();

				expect(vm.componentFunctions.length).toEqual(2);
			});
		});

		it('should reset filters', function() {
			inject(function($filter) {
				fakeBackend
					.withRole({
						BuiltIn: false,
						DescriptionText: 'HermansRoll',
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						IsAnyBuiltIn: true,
						IsMyRole: false,
						Name: 'HermansRoll'
					})
					.withRoleInfo({
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						AvailableFunctions: [{
							Id: '63656f9a-be16-4bb7-9012-9bde01232075'
						}]
					})
					.withApplicationFunction({
						FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
						Name: 'SelectedFunction',
						ChildFunctions: []
					})
					.withApplicationFunction({
						FunctionId: '9995eba9-c304-40b7-866b-5c77b3e76f8d',
						Name: 'UnselectedFunction',
						ChildFunctions: [{
							FunctionId: 'be3321cd-96f1-450c-9aaf-7c5fcaf259aa',
							ChildFunctions: []
						}, {
							FunctionId: 'f68a4cec-4274-455f-acaf-958f79da1db6',
							ChildFunctions: []
						}]
					});
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.unSelectedFunctionsFilter();
				vm.allFunctionsFilter();

				expect(vm.componentFunctions.length).toEqual(2);
				expect(vm.componentFunctions[1].ChildFunctions.length).toEqual(2);
			});
		});

		it('should filter on selected org data', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withOrganizationSelection(organizationSelection.BusinessUnit)
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableDataRange: 3,
						AvailableFunctions: [],
						AvailableBusinessUnits: [],
						AvailableSites: [],
						AvailableTeams: []
					})
				$httpBackend.flush();
				vm.selectedOrgData['928dd0bc-bf40-412e-b970-9b5e015aadea'] = true
				vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177'] = true
				vm.selectedOrgData['58643cfb-6f7d-4128-a014-0c8f82209d68'] = true

				vm.selectedDataFilter();

				expect(vm.filteredOrganizationSelection.BusinessUnit).not.toEqual(null);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(1);
			});
		});

		it('should filter on unselected org data', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withOrganizationSelection({
						Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
						Name: "TeleoptiCCCDemo",
						Type: "BusinessUnit",
						IsSelected: true,
						ChildNodes: [{
							Id: '41815804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site1',
							IsSelected: true,
							ChildNodes: [{
								Id: '58643cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team1',
								IsSelected: false,
							}, {
								Id: '65894cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team2',
								IsSelected: true,
							}]
						}, {
							Id: '85725804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site2',
							IsSelected: false,
							ChildNodes: [{
								Id: '87590cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team3',
								IsSelected: false,
							}]
						}]
					})
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableDataRange: 3,
						AvailableFunctions: [],
						AvailableBusinessUnits: [],
						AvailableSites: [],
						AvailableTeams: []
					})
				$httpBackend.flush();
				vm.selectedOrgData['928dd0bc-bf40-412e-b970-9b5e015aadea'] = true;
				vm.selectedOrgData['41815804-7c51-4f43-a1ae-ecb2a17a2177'] = true;
				vm.selectedOrgData['65894cfb-6f7d-4128-a014-0c8f82209d68'] = true;

				vm.unselectedDataFilter();

				expect(vm.filteredOrganizationSelection.BusinessUnit).not.toEqual(null);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes.length).toEqual(2);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[1].ChildNodes.length).toEqual(1);
			});
		});

		it('should filter with searchString', function() {
			inject(function($filter) {
				fakeBackend
					.withRole({
						BuiltIn: false,
						DescriptionText: 'HermansRoll',
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						IsAnyBuiltIn: true,
						IsMyRole: false,
						Name: 'HermansRoll'
					})
					.withRoleInfo({
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						AvailableFunctions: [{
							Id: '63656f9a-be16-4bb7-9012-9bde01232075'
						}]
					})
					.withApplicationFunction({
						ChildFunctions: [],
						FunctionCode: 'Raptor',
						FunctionDescription: 'xxOpenRaptorApplication',
						FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
						IsDisabled: false,
						LocalizedFunctionDescription: 'All'
					})
					.withApplicationFunction({
						FunctionCode: 'Raptor',
						FunctionDescription: 'xxOpenRaptorApplication',
						FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
						IsDisabled: false,
						LocalizedFunctionDescription: 'Open Teleopti WFM',
						ChildFunctions: [{
							FunctionId: '4bd70a0c-8da7-42fa-8400-4c438375540f',
							LocalizedFunctionDescription: 'Child1',
							ChildFunctions: [{
								FunctionId: '88bd5120-c698-4ccf-8833-884389ab3efe',
								LocalizedFunctionDescription: 'Somethingelse',
								ChildFunctions: []
							}, {
								FunctionId: '24817ca2-8d5d-44de-9edb-bb464e8b2bbf',
								LocalizedFunctionDescription: 'Child1-a',
								ChildFunctions: []
							}]
						}, {
							FunctionId: 'e1214feb-9d8a-47a9-8563-6ef5cec1c3fb',
							LocalizedFunctionDescription: 'SomethingElse2',
							ChildFunctions: [{
								FunctionId: 'ac496ff5-a448-493b-91b8-ac095053f73e',
								LocalizedFunctionDescription: 'ImAlsoAChild',
								ChildFunctions: []
							}]
						}]
					});
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.functionsDescriptionFilter('child');

				expect(vm.componentFunctions.length).toEqual(1);
				expect(vm.componentFunctions[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
				expect(vm.componentFunctions[0].ChildFunctions.length).toEqual(2);
				expect(vm.componentFunctions[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Child1');
				expect(vm.componentFunctions[0].ChildFunctions[1].LocalizedFunctionDescription).toEqual('SomethingElse2');
				expect(vm.componentFunctions[0].ChildFunctions[0].ChildFunctions.length).toEqual(2);
				expect(vm.componentFunctions[0].ChildFunctions[1].ChildFunctions.length).toEqual(1);
				expect(vm.componentFunctions[0].ChildFunctions[0].ChildFunctions[0].LocalizedFunctionDescription).toEqual('Somethingelse');
				expect(vm.componentFunctions[0].ChildFunctions[0].ChildFunctions[1].LocalizedFunctionDescription).toEqual('Child1-a');
				expect(vm.componentFunctions[0].ChildFunctions[1].ChildFunctions[0].LocalizedFunctionDescription).toEqual('ImAlsoAChild');
			});
		});

		it('should find org data with search string', function() {
			inject(function($filter) {
				fakeBackend
					.withRole({
						BuiltIn: false,
						DescriptionText: 'HermansRoll',
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						IsAnyBuiltIn: true,
						IsMyRole: false,
						Name: 'HermansRoll'
					})
					.withRoleInfo({
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						AvailableFunctions: [{
							Id: '63656f9a-be16-4bb7-9012-9bde01232075'
						}]
					})
					.withOrganizationSelection({
						Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
						Name: 'BU',
						Type: 'BusinessUnit',
						IsSelected: false,
						ChildNodes: [{
							Id: '17f20e39-3d42-422c-94ba-305966612cb5',
							Name: 'Site1',
							IsSelected: false,
							ChildNodes: [{
								Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
								Name: 'Team1',
								IsSelected: false
							}, {
								Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
								Name: 'Somethingelse',
								IsSelected: false
							}]
						}, {
							Id: '586591fc-1708-45fa-a967-6d7f71162072',
							Name: 'Site2',
							IsSelected: false,
							ChildNodes: [{
								Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
								Name: 'Child',
								IsSelected: false
							}]
						}]
					}, []);
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.orgDataDescriptionFilter('team');

				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].Name).toEqual('Team1');
			});
		});

		it('should show all children under found bu', function() {
			inject(function($filter) {
				fakeBackend
					.withRole({
						BuiltIn: false,
						DescriptionText: 'HermansRoll',
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						IsAnyBuiltIn: true,
						IsMyRole: false,
						Name: 'HermansRoll'
					})
					.withRoleInfo({
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						AvailableFunctions: [{
							Id: '63656f9a-be16-4bb7-9012-9bde01232075'
						}]
					})
					.withOrganizationSelection({
						Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
						Name: 'BU',
						Type: 'BusinessUnit',
						IsSelected: false,
						ChildNodes: [{
							Id: '17f20e39-3d42-422c-94ba-305966612cb5',
							Name: 'Site1',
							IsSelected: false,
							ChildNodes: [{
								Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
								Name: 'Team1',
								IsSelected: false
							}, {
								Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
								Name: 'Somethingelse',
								IsSelected: false
							}]
						}, {
							Id: '586591fc-1708-45fa-a967-6d7f71162072',
							Name: 'Site2',
							IsSelected: false,
							ChildNodes: [{
								Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
								Name: 'Child',
								IsSelected: false
							}]
						}]
					}, []);
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.orgDataDescriptionFilter('bu');

				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes.length).toEqual(2);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(2);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[1].ChildNodes.length).toEqual(1);
			});
		});

		it('should show all children under found site', function() {
			inject(function($filter) {
				fakeBackend
					.withRole({
						BuiltIn: false,
						DescriptionText: 'HermansRoll',
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						IsAnyBuiltIn: true,
						IsMyRole: false,
						Name: 'HermansRoll'
					})
					.withRoleInfo({
						Id: '4b102279-888a-45ee-b537-b48036bc27d0',
						AvailableFunctions: [{
							Id: '63656f9a-be16-4bb7-9012-9bde01232075'
						}]
					})
					.withOrganizationSelection({
						Id: '40b52c33-fe20-4913-a651-dcf214b88dc3',
						Name: 'BU',
						Type: 'BusinessUnit',
						IsSelected: false,
						ChildNodes: [{
							Id: '17f20e39-3d42-422c-94ba-305966612cb5',
							Name: 'Site1',
							IsSelected: false,
							ChildNodes: [{
								Id: 'c2bbd355-0fcf-402e-858e-ca2bd0f02175',
								Name: 'Team1',
								IsSelected: false
							}, {
								Id: 'fc1645f3-8a86-49da-81d5-8b954da51fe2',
								Name: 'Somethingelse',
								IsSelected: false
							}]
						}, {
							Id: '586591fc-1708-45fa-a967-6d7f71162072',
							Name: 'Site2',
							IsSelected: false,
							ChildNodes: [{
								Id: '4ee60742-dfa5-403c-9758-d7c5d82f6ea5',
								Name: 'Child',
								IsSelected: false
							}]
						}]
					}, []);
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.orgDataDescriptionFilter('site1');

				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].Name).toEqual('Site1');
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes.length).toEqual(2);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].Name).toEqual('Somethingelse');
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].Name).toEqual('Team1');
			});
		});

		it('should show correct selected filtered functions when switching role', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRole(defaultRole2)
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableFunctions: [{
							Id: defaultApplicationFunction.FunctionId
						}]
					})
					.withRoleInfo({
						Id: defaultRole2.Id,
						AvailableFunctions: [{
							Id: defaultApplicationFunction2.FunctionId
						}]
					})
					.withApplicationFunction(defaultApplicationFunction)
					.withApplicationFunction(defaultApplicationFunction2);
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.selectedFunctionsFilter();
				vm.selectRole(vm.roles[1]);
				$httpBackend.flush();

				expect(vm.componentFunctions.length).toEqual(1);
				expect(vm.componentFunctions[0].FunctionId).toEqual(defaultApplicationFunction2.FunctionId);
			});
		});

		it('should show correct unselected filtered functions when switching role', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRole(defaultRole2)
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableFunctions: [{
							Id: '111bb790-b000-4deb-97db-9b5e015b2e8c'
						}, {
							Id: '222bb790-b000-4deb-97db-9b5e015b2e8c'
						}, {
							Id: '333bb790-b000-4deb-97db-9b5e015b2e8c'
						}]
					})
					.withRoleInfo({
						Id: defaultRole2.Id,
						AvailableFunctions: [{
							Id: '444bb790-b000-4deb-97db-9b5e015b2e8c'
						}, {
							Id: '555bb790-b000-4deb-97db-9b5e015b2e8c'
						}]
					})
					.withApplicationFunction({
						FunctionId: '111bb790-b000-4deb-97db-9b5e015b2e8c',
						LocalizedFunctionDescription: 'Parent1',
						ChildFunctions: [{
							FunctionId: '222bb790-b000-4deb-97db-9b5e015b2e8c',
							LocalizedFunctionDescription: 'Child1',
							ChildFunctions: []
						}, {
							FunctionId: '333bb790-b000-4deb-97db-9b5e015b2e8c',
							LocalizedFunctionDescription: 'Child2',
							ChildFunctions: []
						}]
					})
					.withApplicationFunction({
						FunctionId: '444bb790-b000-4deb-97db-9b5e015b2e8c',
						LocalizedFunctionDescription: 'Parent2',
						ChildFunctions: [{
							FunctionId: '555bb790-b000-4deb-97db-9b5e015b2e8c',
							LocalizedFunctionDescription: 'ChildA',
							ChildFunctions: []
						}, {
							FunctionId: '666bb790-b000-4deb-97db-9b5e015b2e8c',
							LocalizedFunctionDescription: 'ChildB',
							ChildFunctions: []
						}]
					});
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.unSelectedFunctionsFilter();
				vm.selectRole(vm.roles[1]);
				$httpBackend.flush();

				expect(vm.componentFunctions[1].ChildFunctions.length).toEqual(1);
			});
		});

		it('should show correct selected filtered org data when switching role', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRole(defaultRole2)
					.withOrganizationSelection({
						Id: "111dd0bc-bf40-412e-b970-9b5e015aadea",
						Name: "TestParent",
						Type: "BusinessUnit",
						ChildNodes: [{
							Id: '22225804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site1',
							ChildNodes: [{
								Id: '3333cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team1'
							}, {
								Id: '4444cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team2'
							}]
						}, {
							Id: '66625804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site2',
							ChildNodes: [{
								Id: '7773cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'TeamA'
							}, {
								Id: '88884cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'TeamB'
							}]
						}]
					})
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableBusinessUnits: [{
							Id: "111dd0bc-bf40-412e-b970-9b5e015aadea"
						}],
						AvailableSites: [{
							Id: '22225804-7c51-4f43-a1ae-ecb2a17a2177'
						}],
						AvailableTeams: [{
							Id: '3333cfb-6f7d-4128-a014-0c8f82209d68'
						}]
					})
					.withRoleInfo({
						Id: defaultRole2.Id,
						AvailableBusinessUnits: [{
							Id: "111dd0bc-bf40-412e-b970-9b5e015aadea"
						}],
						AvailableSites: [{
							Id: '66625804-7c51-4f43-a1ae-ecb2a17a2177'
						}],
						AvailableTeams: [{
							Id: '7773cfb-6f7d-4128-a014-0c8f82209d68'
						}]
					})
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.selectedDataFilter();
				vm.selectRole(vm.roles[1]);
				$httpBackend.flush();

				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[0].Id).toEqual('66625804-7c51-4f43-a1ae-ecb2a17a2177');
			});
		});

		it('should show correct unselected filtered org data when switching role', function() {
			inject(function($filter) {
				fakeBackend
					.withRole(defaultRole)
					.withRole(defaultRole2)
					.withOrganizationSelection({
						Id: "111dd0bc-bf40-412e-b970-9b5e015aadea",
						Name: "TestParent",
						Type: "BusinessUnit",
						ChildNodes: [{
							Id: '22225804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site1',
							ChildNodes: [{
								Id: '3333cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team1'
							}, {
								Id: '4444cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'Team2'
							}]
						}, {
							Id: '66625804-7c51-4f43-a1ae-ecb2a17a2177',
							Name: 'Site2',
							ChildNodes: [{
								Id: '7773cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'TeamA'
							}, {
								Id: '88884cfb-6f7d-4128-a014-0c8f82209d68',
								Name: 'TeamB'
							}]
						}]
					})
					.withRoleInfo({
						Id: defaultRole.Id,
						AvailableBusinessUnits: [{
							Id: "111dd0bc-bf40-412e-b970-9b5e015aadea"
						}],
						AvailableSites: [{
							Id: '22225804-7c51-4f43-a1ae-ecb2a17a2177'
						}],
						AvailableTeams: [{
							Id: '3333cfb-6f7d-4128-a014-0c8f82209d68'
						}]
					})
					.withRoleInfo({
						Id: defaultRole2.Id,
						AvailableBusinessUnits: [{
							Id: "111dd0bc-bf40-412e-b970-9b5e015aadea"
						}],
						AvailableSites: [{
							Id: '66625804-7c51-4f43-a1ae-ecb2a17a2177'
						}],
						AvailableTeams: [{
							Id: '7773cfb-6f7d-4128-a014-0c8f82209d68'
						}]
					})
				$httpBackend.flush();

				vm.selectRole(vm.roles[0]);
				$httpBackend.flush();
				vm.unselectedDataFilter();
				vm.selectRole(vm.roles[1]);
				$httpBackend.flush();

				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[1].ChildNodes.length).toEqual(1);
				expect(vm.filteredOrganizationSelection.BusinessUnit.ChildNodes[1].ChildNodes[0].Id).toEqual('88884cfb-6f7d-4128-a014-0c8f82209d68');
			});
		});

	});
});
