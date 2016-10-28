'use strict';
//DONT REMOVE X
xdescribe('PermissionsCtrlRefact', function () {
	var $httpBackend,
		fakeBackend,
		$controller,
		vm;

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$controller_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$controller = _$controller_;

		fakeBackend.clear();
		vm = $controller('PermissionsCtrlRefact');

		$httpBackend.whenPOST('../api/Permissions/Roles').respond(function (method, url, data, headers) {
			return [201, {}];
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64').respond(function (method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPUT('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64?newDescription=%7B%7D').respond(function (method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Copy').respond(function (method, url, data, headers) {
			return [201, angular.fromJson(data)];
		});

	}));

	it('should get a role', function () {
		fakeBackend.withRole({
			BuiltIn: false,
			DescriptionText: 'Agent',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'Agent'
		});

		$httpBackend.flush();

		expect(vm.roles[0].Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
	});

	it('should get roles', function (done) {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRole(
			{
				BuiltIn: true,
				DescriptionText: 'SuperAdmin',
				Id: '7afefc4f-3231-401b-8174-6525a5e47f23',
				IsAnyBuiltIn: true,
				IsMyRole: true,
				Name: '_superAdmin'
			});

		$httpBackend.flush();
		done();

		expect(vm.roles.length).toEqual(2);
	});

	it('should create role', function (done) {
		var name = 'rolename';

		vm.createRole(name);
		$httpBackend.flush();
		done();

		expect(vm.roles.length).toBe(1);
	});

	it('should put newly created roll on top of roles list', function (done) {
		var name = 'rolename'
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRole(
			{
				BuiltIn: true,
				DescriptionText: 'SuperAdmin',
				Id: '7afefc4f-3231-401b-8174-6525a5e47f23',
				IsAnyBuiltIn: true,
				IsMyRole: true,
				Name: '_superAdmin'
			});

		vm.createRole(name);
		$httpBackend.flush();
		done();

		expect(vm.roles[0].Description).toBe(name);
	});

	it('should be able to edit the name of a role', function (done) {
		fakeBackend.withRole({
			BuiltIn: false,
			DescriptionText: 'Agent',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'Agent'
		});

		vm.editRole('newRoleName', 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		$httpBackend.flush();
		done();

		expect(vm.roles[0].DescriptionText).toBe('newRoleName');
	});

	it('should be able to delete a roll', function () {
		fakeBackend.withRole({
			BuiltIn: false,
			DescriptionText: 'Agent',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'Agent'
		});
		$httpBackend.flush();

		vm.deleteRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.roles.length).toEqual(0);
	});

	it('should not be able to delete a built in roll', function () {
		fakeBackend.withRole({
			BuiltIn: true,
			DescriptionText: 'Systemadministator',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: '_superUser'
		});
		$httpBackend.flush();

		vm.deleteRole(vm.roles[0]);

		expect(vm.roles.length).toEqual(1);
	});

	it('should be able to copy a role', function (done) {
		fakeBackend.withRole({
			BuiltIn: false,
			DescriptionText: 'Agent',
			Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'Agent'
		});
		$httpBackend.flush();

		vm.copyRole(vm.roles[0]);
		$httpBackend.flush();
		done();
		expect(vm.roles.length).toEqual(2);
		expect(vm.roles[0].Name).toEqual('Agent');
		expect(vm.roles[1].Name).toEqual('Agent');
	});

	it('should put copied role on top of roles list', function (done) {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'HermansRoll',
				Id: '4b102279-888a-45ee-b537-b48036bc27d0',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'HermansRoll'
			}).withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();

		vm.copyRole(vm.roles[1]);
		$httpBackend.flush();
		done();

		expect(vm.roles[0].Name).toEqual('Agent');
	});

	it('should be able to select a role', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'CarlsRoll',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'CarlsRoll'
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.roles[0].IsSelected).toBe(true);
	});

	it('should deselect selcted role when selecting another role', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'CarlsRoll',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'CarlsRoll'
			}).withRole({
				BuiltIn: false,
				DescriptionText: 'CarlsRoll',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'CarlsRoll'
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();
		vm.selectRole(vm.roles[1]);
		$httpBackend.flush();


		expect(vm.roles[0].IsSelected).toBe(false);
		expect(vm.roles[1].IsSelected).toBe(true);
	});

	it('should fetch additional info for selected role', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}
				]
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.selectedRole.AvailableFunctions[0].Id).toEqual('63656f9a-be16-4bb7-9012-9bde01232075');
	});

	it('should indicate available functions for selected role', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}
				]
			}).withApplicationFunction({
				ChildFunctions: [],
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM'
			});;
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
	});

	it('should indicate available functions for selected role', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					}
				]
			}).withApplicationFunction({
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
				ChildFunctions: []
			}).withApplicationFunction({
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232074',
				ChildFunctions: []
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.applicationFunctions.find(function (fn) { return fn.FunctionId == '63656f9a-be16-4bb7-9012-9bde01232075' }).IsSelected).toEqual(true);
		expect(vm.applicationFunctions.find(function (fn) { return fn.FunctionId == '63656f9a-be16-4bb7-9012-9bde01232074' }).IsSelected).toEqual(false);
	});

	//JAAAO DET ÄR HÄR VI JOBBAR DUMMER
	it('should indicate all available functions for selected role', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075',
					},
					{
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
					}
				]
			}).withApplicationFunction({
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
				ChildFunctions: [{
					FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
				}
				]
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[0].ChildFunctions[0].IsSelected).toEqual(true);
	});

	it('should indicate all available functions for selected role but BETTER', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					},
					{
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}
				]
			})
			.withApplicationFunction({
				FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
				Name: 'FunctionOne',
				ChildFunctions: []
			})
			.withApplicationFunction({
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
				Name: 'FunctionTwo',
				ChildFunctions: [
					{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
					},
					{
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}
				]
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.applicationFunctions[1].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[1].ChildFunctions[0].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[1].ChildFunctions[1].IsSelected).toEqual(true);
	});

	it('should indicate all ApplicationFunctions and its ChildFunctions for selected role', function () {
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
				AvailableFunctions: [
					{
						Id: '63656f9a-be16-4bb7-9012-9bde01232075'
					},
					{
						Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					},
					{
						Id: '52b6f3de-55e0-45b2-922e-316f8538f60c'
					}
				]
			})
			.withApplicationFunction({
				FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
				Name: 'FunctionOne',
				ChildFunctions: []
			})
			.withApplicationFunction({
				FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
				Name: 'FunctionTwo',
				ChildFunctions: [
					{
						FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
						ChildFunctions: [
							{
								FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
								ChildFunctions: [
									{
										FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
									},
									{
										FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c'
									}
								]
							},
							{
								FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
							}
						]
					},
					{
						FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
					}
				]
			});
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.applicationFunctions[1].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].ChildFunctions[1].IsSelected).toEqual(true);
	});


	it('should fetch organization info for selected role', function () {
		fakeBackend.withRole({
			BuiltIn: false,
			DescriptionText: 'HermansRoll',
			Id: '4b102279-888a-45ee-b537-b48036bc27d0',
			IsAnyBuiltIn: true,
			IsMyRole: false,
			Name: 'HermansRoll'
		})
			.withRoleInfo({
				Id: '4b102279-888a-45ee-b537-b48036bc27d0',
				AvailableDataRange: 3,
				AvailableSites: [
					{
						Id: 'd66f60f5-264c-4277-80eb-9b5e015ab495'
					}
				],
				AvailableTeams: [
					{
						Id: '6a21c802-7a34-4917-8dfd-9b5e015ab461'
					}
				]
			})
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.selectedRole.AvailableDataRange).toEqual(3);
		expect(vm.selectedRole.AvailableSites[0].Id).toEqual('d66f60f5-264c-4277-80eb-9b5e015ab495');
		expect(vm.selectedRole.AvailableTeams[0].Id).toEqual('6a21c802-7a34-4917-8dfd-9b5e015ab461');
	});

	it('should indicate available site for selected role', function () {
		var BusinessUnit = {
			ChildNodes: [
				{
					Id: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c',
					Name: 'London',
					Type: 'Site'
				}
			],
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit"
		};
		var DynamicOptions = [
			{
				RangeOption: 0,
				Name: "None"
			}
		];
		fakeBackend
			.withRole({
				Id: '4b102279-888a-45ee-b537-b48036bc27d0',
				Name: 'HermansRoll'
			})
			.withRoleInfo({
				Id: '4b102279-888a-45ee-b537-b48036bc27d0',
				AvailableBusinessUnits: [
					{
						Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
						Name: "TeleoptiCCCDemo"
					}
				],
				AvailableDataRange: 3,
				AvailableFunctions: [],
				AvailableSites: [{
					Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
					Name: "London"
				}
				]
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();

		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
	});


});
