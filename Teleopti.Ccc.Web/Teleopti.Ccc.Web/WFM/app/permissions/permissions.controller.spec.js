'use strict';
//DONT REMOVE X
xdescribe('PermissionsCtrlRefact', function () {
    var $httpBackend,
        fakeBackend,
        $timeout,
        $controller,
        permissionsDataService,
        vm;

    beforeEach(function () {
        module('wfm.permissions');
    });

    beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$controller_, _$timeout_, _permissionsDataService_) {
        $httpBackend = _$httpBackend_;
        fakeBackend = _fakePermissionsBackend_;
        $controller = _$controller_;
        $timeout = _$timeout_;
        permissionsDataService = _permissionsDataService_;

        fakeBackend.clear();
        vm = $controller('PermissionsCtrlRefact');

        $httpBackend.whenPOST('../api/Permissions/Roles').respond(function (method, url, data, headers) {
            return [201, { DescriptionText: 'rolename' }];
        });
        $httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64').respond(function (method, url, data, headers) {
            return 200;
        });
        $httpBackend.whenDELETE('../api/Permissions/Roles/f1e9237f-7bd8-4ca9-aebb-e49b80d8a4cf').respond(function (method, url, data, headers) {
            return 200;
        });
        $httpBackend.whenPUT('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64?newDescription=%7B%7D').respond(function (method, url, data, headers) {
            return [200, { newDescription: 'newRoleName' }];
        });
        $httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Copy').respond(function (method, url, data, headers) {
            return [201, { Name: 'Agent' }];
        });
        $httpBackend.whenPOST('../api/Permissions/Roles/4b102279-888a-45ee-b537-b48036bc27d0/Functions').respond(function (method, url, data, headers) {
            console.log(data);
            return 200;
        });

    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get an organization selection', function () {
        var BusinessUnit = {
            ChildNodes: [],
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
        fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);

        $httpBackend.flush();

        expect(vm.organizationSelection.BusinessUnit.ChildNodes).toEqual([]);
        expect(vm.organizationSelection.BusinessUnit.Id).toEqual("928dd0bc-bf40-412e-b970-9b5e015aadea");
        expect(vm.organizationSelection.BusinessUnit.Name).toEqual("TeleoptiCCCDemo");
        expect(vm.organizationSelection.BusinessUnit.Type).toEqual("BusinessUnit");
        expect(vm.organizationSelection.DynamicOptions[0].RangeOption).toEqual(0);
        expect(vm.organizationSelection.DynamicOptions[0].Name).toEqual("None");
    });

    it('should allways keep top node open', function () {
        fakeBackend.withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
            IsDisabled: false,
            LocalizedFunctionDescription: 'All'
        }).withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti WFM'
        });

        $httpBackend.flush();
        vm.prepareTree(vm.applicationFunctions);

        expect(vm.applicationFunctions[1].IsOpen).toEqual(true);
    });

    it('should remove function with the name All', function () {
        fakeBackend.withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
            IsDisabled: false,
            LocalizedFunctionDescription: 'All',
            IsHidden: false
        }).withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti WFM',
            IsHidden: false
        });

        $httpBackend.flush();
        vm.prepareTree(vm.applicationFunctions);

        expect(vm.applicationFunctions[0].IsHidden).toEqual(true);
    });

    it('should get an application function', function () {
        fakeBackend.withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti WFM'
        });

        $httpBackend.flush();

        expect(vm.applicationFunctions[0].ChildFunctions).toEqual([]);
        expect(vm.applicationFunctions[0].FunctionCode).toEqual('Raptor');
        expect(vm.applicationFunctions[0].FunctionDescription).toEqual('xxOpenRaptorApplication');
        expect(vm.applicationFunctions[0].FunctionId).toEqual('f19bb790-b000-4deb-97db-9b5e015b2e8c');
        expect(vm.applicationFunctions[0].IsDisabled).toEqual(false);
        expect(vm.applicationFunctions[0].LocalizedFunctionDescription).toEqual('Open Teleopti WFM');
    });

    it('should get application functions', function () {
        fakeBackend.withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti WFM'
        }).withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Anywhere',
            FunctionDescription: 'xxAnywhere',
            FunctionId: '7884b7dd-31ea-4e40-b004-c7ce3b5deaf3',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti TEM'
        });

        $httpBackend.flush();

        expect(vm.applicationFunctions.length).toEqual(2);
    });

    it('should get application functions child functions', function () {
        fakeBackend.withApplicationFunction({
            ChildFunctions: [{
                ChildFunctions: [],
                FunctionCode: 'Carl',
                FunctionDescription: 'CarlApplication',
                FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
                IsDisabled: false,
                LocalizedFunctionDescription: 'I am A child function'
            }],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled: false,
            LocalizedFunctionDescription: 'Open Teleopti WFM'
        });

        $httpBackend.flush();

        expect(vm.applicationFunctions[0].ChildFunctions[0].FunctionCode).toEqual('Carl');
    });

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

    it('should get roles', function () {
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

        expect(vm.roles.length).toEqual(2);
    });

    it('should create role', function () {
        var name = 'rolename';

        vm.createRole(name);
        $httpBackend.flush();

        expect(vm.roles.length).toBe(1);
    });

    it('should select newly created role', function () {
        var name = 'rolename';

        vm.createRole(name);
        $httpBackend.flush()
        console.log(vm)

        expect(vm.roles[0].IsSelected).toBe(true);
        expect(vm.selectedRole.DescriptionText).toEqual(name);
    });

    it('should unselect previous selected role when creating new role', function () {
        var name = 'rolename';
        fakeBackend
            .withRole({
                IsSelected: true,
                BuiltIn: false,
                DescriptionText: 'Agent',
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                IsAnyBuiltIn: true,
                IsMyRole: false,
                Name: 'Agent'
            });
        $httpBackend.flush();

        vm.createRole(name);
        $httpBackend.flush()

        expect(vm.selectedRole.IsSelected).toEqual(true);
        expect(vm.selectedRole.DescriptionText).toEqual(name);
        expect(vm.roles[0].IsSelected).toBe(true);
        expect(vm.roles[1].IsSelected).toBe(false);
    });

    it('should unselect new roll when selecting other role', function () {
        var name = 'rolename';
        fakeBackend
            .withRole({
                IsSelected: true,
                BuiltIn: false,
                DescriptionText: 'Agent',
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                IsAnyBuiltIn: true,
                IsMyRole: false,
                Name: 'Agent'
            });
        $httpBackend.flush();

        vm.createRole(name);
        $httpBackend.flush()
        vm.selectRole(vm.roles[1]);
        $httpBackend.flush();

        expect(vm.roles[0].IsSelected).toBe(false);
        expect(vm.roles[1].IsSelected).toBe(true);

    });

    it('should close create role modal on submit', function () {
        var name = 'rolename';

        vm.createRole(name);
        $httpBackend.flush();

        expect(vm.showCreateModal).toBe(false);
    });

    it('should clear new role name on submit', function () {
        var name = 'rolename';

        vm.createRole(name);
        $httpBackend.flush();

        expect(vm.roleName).toBe('');
    });

    it('should put newly created roll on top of roles list', function () {
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

        expect(vm.roles[0].DescriptionText).toBe(name);
    });

    xit('should be able to edit the name of a role', function (done) {
        fakeBackend.withRole({
            BuiltIn: false,
            DescriptionText: 'Agent',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: false,
            Name: 'Agent'
        });
        $httpBackend.flush();

        vm.editRole('newRoleName', vm.roles[0]);
        $httpBackend.flush();

        expect(vm.roles[0].DescriptionText).toBe('newRoleName');
        done();
    });

    it('should select edited role after name change', function () {
        fakeBackend.withRole({
            BuiltIn: false,
            DescriptionText: 'Agent',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: false,
            Name: 'Agent'
        });
        $httpBackend.flush();

        vm.editRole('newRoleName', vm.roles[0]);
        $httpBackend.flush();

        expect(vm.roles[0].IsSelected).toBe(true);
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

    it('should unselect role functions when deleting that role', function () {
        fakeBackend.withRole({
            BuiltIn: false,
            DescriptionText: 'Agent',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: false,
            IsSelected: true,
            Name: 'Agent'
        }).withApplicationFunction({
            ChildFunctions: [],
            FunctionCode: 'Raptor',
            FunctionDescription: 'xxOpenRaptorApplication',
            FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
            IsDisabled: false,
            IsSelected: true,
            LocalizedFunctionDescription: 'Open Teleopti WFM'
        }).withRoleInfo({
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            AvailableFunctions: [
                {
                    Id: '63656f9a-be16-4bb7-9012-9bde01232075'
                }
            ]
        });
        $httpBackend.flush();
        vm.selectedRole = vm.roles[0];
        vm.deleteRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.applicationFunctions[0].IsSelected).toEqual(false);
    });

    it('should not unselect role functions for selected role when deleting other role', function () {
        fakeBackend
            .withRole({
                BuiltIn: false,
                DescriptionText: 'Agent',
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                IsAnyBuiltIn: true,
                IsMyRole: false,
                IsSelected: true,
                Name: 'Agent'
            })
            .withApplicationFunction({
                ChildFunctions: [],
                FunctionCode: 'Raptor',
                FunctionDescription: 'xxOpenRaptorApplication',
                FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                IsDisabled: false,
                IsSelected: true,
                LocalizedFunctionDescription: 'Open Teleopti WFM'
            })
            .withRoleInfo({
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                AvailableFunctions: [
                    {
                        Id: '63656f9a-be16-4bb7-9012-9bde01232075'
                    }
                ]
            })
            .withRole({
                BuiltIn: false,
                DescriptionText: 'Carl',
                Id: 'f1e9237f-7bd8-4ca9-aebb-e49b80d8a4cf',
                IsAnyBuiltIn: true,
                IsMyRole: false,
                IsSelected: false,
                Name: 'Agent'
            });
        $httpBackend.flush();
        vm.selectedRole = vm.roles[0];
        vm.deleteRole(vm.roles[1]);
        $httpBackend.flush();

        expect(vm.roles[0].IsSelected).toEqual(true);
        expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
        expect(vm.roles.length).toEqual(1);
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

    it('should not be able to modify Built in role', function () {
        fakeBackend.withRole({
            BuiltIn: true,
            DescriptionText: 'Systemadministator',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: true,
            Name: '_superUser'
        });
        $httpBackend.flush();

        var result = vm.checkMyRole(vm.roles[0]);

        expect(result).toEqual(false);
    });

    it('should be able to copy a role', function () {
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

        expect(vm.roles.length).toEqual(2);
        expect(vm.roles[0].Name).toEqual('Agent');
        expect(vm.roles[1].Name).toEqual('Agent');
    });

    it('should select newly copied role', function () {
        fakeBackend.withRole({
            BuiltIn: false,
            IsSelected: true,
            DescriptionText: 'Agent',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: false,
            Name: 'Agent'
        });
        $httpBackend.flush();

        vm.copyRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.roles[0].IsSelected).toBe(true);
    });

    it('should set newyly copied role to selected role', function () {
        fakeBackend
            .withRole({
                BuiltIn: false,
                DescriptionText: 'Agent',
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                IsAnyBuiltIn: true,
                IsMyRole: false,
                IsSelected: true,
                Name: 'Agent'
            })
            .withApplicationFunction({
                ChildFunctions: [],
                FunctionCode: 'Raptor',
                FunctionDescription: 'xxOpenRaptorApplication',
                FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                IsDisabled: false,
                IsSelected: true,
                LocalizedFunctionDescription: 'Open Teleopti WFM'
            })
            .withRoleInfo({
                Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
                AvailableFunctions: [
                    {
                        Id: '63656f9a-be16-4bb7-9012-9bde01232075'
                    }
                ]
            });
        $httpBackend.flush();
        vm.selectedRole = vm.roles[0];
        vm.copyRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.roles[1].IsSelected).toEqual(false);
        expect(vm.selectedRole.Id).not.toEqual(vm.roles[1].Id)
        expect(vm.roles.length).toEqual(2);
    });

    it('should unselect previously selected role when copying role', function () {
        fakeBackend.withRole({
            BuiltIn: false,
            IsSelected: true,
            DescriptionText: 'Agent',
            Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
            IsAnyBuiltIn: true,
            IsMyRole: false,
            Name: 'Agent'
        });
        $httpBackend.flush();

        vm.copyRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.roles[1].IsSelected).toBe(false);
    });

    it('should put copied role on top of roles list', function () {
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

        expect(vm.roles[0].Name).toEqual('Agent');
    });

    it('should be able to select a role', function () {
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
                AvailableFunctions: []
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
            })
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
            })
            .withApplicationFunction({
                FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                ChildFunctions: [
                    {
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
                AvailableDataRange: 3,
                AvailableFunctions: [],
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

    it('should indicate available business unit for selected role', function () {
        fakeBackend
            .withRole({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0'
            })
            .withRoleInfo({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0',
                AvailableFunctions: [],
                AvailableBusinessUnits: [
                    {
                        Id: "127a343f-3f86-44ca-9b55-bcf04491ce7a",
                        Name: "CarlCCDemo"
                    },
                    {
                        Id: "928dd0bc-bf40-412e-b970-b48036bc27d0",
                        Name: "TeleoptiCCCDemo"
                    }
                ]
            })
            .withOrganizationSelection(
            {
                ChildNodes: [],
                Id: "928dd0bc-bf40-412e-b970-b48036bc27d0",
                Name: "TeleoptiCCCDemo"
            },
            []
            );
        $httpBackend.flush();

        vm.selectRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.organizationSelection.BusinessUnit.IsSelected).toBe(true);
    });

    it('should indicate available site for selected role', function () {
        fakeBackend
            .withRole({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0'
            })
            .withRoleInfo({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0',
                AvailableFunctions: [],
                AvailableBusinessUnits: [
                    {
                        Id: "928dd0bc-bf40-412e-b970-b48036bc27d0",
                        Name: "TeleoptiCCCDemo"
                    }
                ],
                AvailableDataRange: 3,
                AvailableSites: [
                    {
                        Id: 'd66f60f5-264c-4277-80eb-9b5e015ab495'
                    }
                ]
            })
            .withOrganizationSelection(
            {
                Id: "928dd0bc-bf40-412e-b970-b48036bc27d0",
                Name: "TeleoptiCCCDemo",
                ChildNodes: [
                    {
                        Id: 'd66f60f5-264c-4277-80eb-9b5e015ab495'
                    },
                    {
                        Id: '8c2dfdc5-6f92-4fab-9a39-676b88c26a53'
                    }
                ]
            },
            []
            );
        $httpBackend.flush();

        vm.selectRole(vm.roles[0]);
        $httpBackend.flush();

        expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toBe(false);
        expect(vm.organizationSelection.BusinessUnit.ChildNodes[1].IsSelected).toBe(true);
    });


    xit('should indicate available business unit when selecting a different role', function () {
        fakeBackend
            .withRole({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0',
                Name: 'Carl'
            })
            .withRoleInfo({
                Id: '4b102279-888a-45ee-b537-b48036bc27d0',
                AvailableFunctions: [],
                AvailableBusinessUnits: [
                    {
                        Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
                        Name: "CarlCCDemo"
                    }
                ],
                AvailableSites: [
                    {
                        Id: "b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1",
                        Name: "Site1",
                        ChildNodes: []
                    },
                    {
                        Id: "34f3c450-c206-454c-96ab-623c8daa7c53",
                        Name: "Site2",
                        ChildNodes: []
                    }
                ]
            })
            .withRole({
                Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
                Name: 'Herman'
            })
            .withRoleInfo({
                Id: '9c8e66ed-088e-41c8-8704-7110220f8fac',
                AvailableFunctions: [],
                AvailableBusinessUnits: [
                    {
                        Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
                        Name: "TeleoptiCCCDemo"
                    }
                ],
                AvailableSites: [
                    {
                        Id: "123f651b-5235-40d4-9d34-6be51b55df65",
                        Name: "SiteA",
                        ChildNodes: []
                    },
                    {
                        Id: "438ab59e-5c50-48c6-bd11-dc0ccd2af5d5",
                        Name: "SiteB",
                        ChildNodes: []
                    }
                ]
            })
            .withOrganizationSelection(
            {
                ChildNodes: [
                    {
                        Id: 'b36f9ab9-2f2a-472e-9a51-5a8dbf47b7b1',
                        ChildNodes: []
                    },
                    {
                        Id: '34f3c450-c206-454c-96ab-623c8daa7c53',
                        ChildNodes: []
                    },
                    {
                        Id: '123f651b-5235-40d4-9d34-6be51b55df65',
                        ChildNodes: []
                    },
                    {
                        Id: '438ab59e-5c50-48c6-bd11-dc0ccd2af5d5',
                        ChildNodes: []
                    }
                ],
                Id: "729b9399-d80a-46a7-a0dd-df18b89b12b9",
                Name: "TeleoptiCCCDemo"
            },
            []
            );
        $httpBackend.flush();

        vm.selectRole(vm.roles[0]);
        $httpBackend.flush();
        vm.selectRole(vm.roles[1]);
        $httpBackend.flush();

        expect(vm.organizationSelection.BusinessUnit.IsSelected).toBe(true);
        expect(vm.organizationSelection.ChildNodes[2].IsSelected).toBe(true);
        expect(vm.organizationSelection.ChildNodes[3].IsSelected).toBe(true);
    });

    it('should select all functions when toggle all function', function () {
        fakeBackend
            .withApplicationFunction({
                FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
                Name: 'All',
                ChildFunctions: [],
                IsSelected: false
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

        vm.toggleAllFunction();

        expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
        expect(vm.applicationFunctions[1].IsSelected).toEqual(true);
        expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].IsSelected).toEqual(true);
        expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].ChildFunctions[1].IsSelected).toEqual(true);
    });
    //HÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄÄR
    it('should save all functions for role when toggle all function', function () {
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
                AvailableFunctions: []
            })
            .withApplicationFunction({
                FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
                Name: 'All',
                ChildFunctions: [],
                IsSelected: false
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
        vm.toggleAllFunction();
        $httpBackend.flush();

        expect(permissionsDataService.selectAllFunction).toHaveBeenCalled();
    });

    it('should delete all functions for role when untoggle all function', function () {
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
                        Id: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7'
                    },
                    {
                        Id: '63656f9a-be16-4bb7-9012-9bde01232075'
                    },
                    {
                        Id: '7656320f-1391-4332-bcaa-6cf16cbcb8dc'
                    },
                    {
                        Id: '52b6f3de-55e0-45b2-922e-316f8538f60c'
                    },
                    {
                        Id: '6856320f-1391-4332-bcaa-6cf16cbcb8dc'
                    },
                    {
                        Id: '52b6f3de-55e0-45b2-922e-316f8538f60c'
                    },
                    {
                        Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
                    },
                    {
                        Id: 'f73154af-8d6d-4250-b066-d6ead56bfc16'
                    }
                ]
            })
            .withApplicationFunction({
                FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
                Name: 'All',
                ChildFunctions: [],
                IsSelected: true
            })
            .withApplicationFunction({
                FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                Name: 'FunctionTwo',
                ChildFunctions: [
                    {
                        FunctionId: '7656320f-1391-4332-bcaa-6cf16cbcb8dc',
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
        vm.toggleAllFunction();
        $httpBackend.flush();

        expect(permissionsDataService.selectAllFunction).toHaveBeenCalled();
    });

    it('should deselect all functions when toggle all function', function () {
        fakeBackend
            .withApplicationFunction({
                FunctionId: '6b4d0b6f-72c9-48a5-a0e6-d8d0d78126b7',
                Name: 'All',
                ChildFunctions: [],
                IsSelected: true
            })
            .withApplicationFunction({
                FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                Name: 'FunctionTwo',
                IsSelected: true,
                ChildFunctions: [
                    {
                        FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
                        IsSelected: true,
                        ChildFunctions: [
                            {
                                FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
                                IsSelected: true,
                                ChildFunctions: [
                                    {
                                        FunctionId: '6856320f-1391-4332-bcaa-6cf16cbcb8dc',
                                        IsSelected: true
                                    },
                                    {
                                        FunctionId: '52b6f3de-55e0-45b2-922e-316f8538f60c',
                                        IsSelected: true
                                    }
                                ]
                            },
                            {
                                FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
                                IsSelected: true
                            }
                        ]
                    },
                    {
                        FunctionId: 'f73154af-8d6d-4250-b066-d6ead56bfc16',
                        IsSelected: true
                    }
                ]
            });
        $httpBackend.flush();

        vm.toggleAllFunction();

        expect(vm.applicationFunctions[0].IsSelected).toEqual(false);
        expect(vm.applicationFunctions[1].IsSelected).toEqual(false);
        expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].IsSelected).toEqual(false);
        expect(vm.applicationFunctions[1].ChildFunctions[0].ChildFunctions[0].ChildFunctions[1].IsSelected).toEqual(false);
    });

    it('should only show selected function when pressing selected filter', function () {
        inject(function ($filter) {
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
                            IsSelected: true
                        }
                    ]
                })
                .withApplicationFunction({
                    FunctionId: '63656f9a-be16-4bb7-9012-9bde01232075',
                    Name: 'SelectedFunction',
                    ChildFunctions: [],
                    IsSelected: true
                })
                .withApplicationFunction({
                    FunctionId: '9995eba9-c304-40b7-866b-5c77b3e76f8d',
                    Name: 'UnselectedFunction',
                    ChildFunctions: [],
                    IsSelected: false
                });
            $httpBackend.flush();

            vm.selectRole(vm.roles[0]);
            $httpBackend.flush();
            vm.showOnlySelectedFunctionsFilter();

            expect(vm.filteredApplicationFunctions.length).toEqual(1);
        });
    });

});
