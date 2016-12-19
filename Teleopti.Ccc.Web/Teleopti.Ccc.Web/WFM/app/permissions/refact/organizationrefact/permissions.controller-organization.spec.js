'use strict';

xdescribe('component: permissionsList', function() {
	var $httpBackend,
		fakeBackend,
		$controller,
		$componentController,
		permissionsDataService,
		ctrl,
		vm;

	beforeEach(function() {
		module('wfm.permissions');
	});

	beforeEach(inject(function(_$httpBackend_, _fakePermissionsBackend_, _$componentController_, _$controller_, _permissionsDataService_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$componentController = _$componentController_;
		$controller = _$controller_;
		permissionsDataService = _permissionsDataService_;

		fakeBackend.clear();
		vm = $controller('PermissionsCtrlRefact');

		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/1805c09e-39fe-4472-af28-ba9f3e8df759/AvailableData').respond(function(method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/DeleteData').respond(function(method, url, data, headers) {
			fakeBackend.deleteAllAvailableOrgData();
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData/Team/5d2cef4b-a994-4e6f-bb44-87c8306c2052').respond(function(method, url, data, headers) {
			fakeBackend.deleteUnselectedOrgData(data);
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData/Team/360bb004-356f-44fa-be18-cb92aa84c937').respond(function(method, url, data, headers) {
			fakeBackend.deleteUnselectedOrgData(data);
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData/Site/d970a45a-90ff-4111-bfe1-9b5e015ab45c').respond(function(method, url, data, headers) {
			fakeBackend.deleteUnselectedOrgData(data);
			return 200;
		});
	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should be able to select BusinessUnit', function() {
		var BusinessUnit = {
			ChildNodes: [],
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit"
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
	});

	it('should be able to deselect BusinessUnit', function() {
		var BusinessUnit = {
			ChildNodes: [],
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
	});

	it('should select child sites and teams when selecting a BusinessUnit', function() {
		var BusinessUnit = {
			ChildNodes: [{
				Id: 'fe113bc0-979a-4b6c-9e7c-ef601c7e02d1',
				Type: 'Site',
				Name: 'Site1',
				ChildNodes: [{
					Id: 'e6377d56-277d-4c22-97f3-b218741b2480',
					Type: 'Team',
					Name: 'Team1'
				}]
			}],
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit"
		};
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(true);
	});

	it('should deselect child sites and teams when deselecting a BusinessUnit', function() {
		var BusinessUnit = {
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
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true
		};
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
	});


	it('should select businessunit when selecting site', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: [{
				ChildNodes: [],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site"
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
	});

	it('should select businessunit and site when selecting team', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: [{
				ChildNodes: [{
					Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
					Name: "Team 1",
					Type: "Team"
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site"
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(true);
	});

	it('should select all teams when clicking parent site', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: [{
				ChildNodes: [{
					Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
					Name: "Team 1",
					Type: "Team"
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team"
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site"
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(true);
	});

	it('should deselect all teams when clicking parent site', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: [{
				ChildNodes: [{
					Id: "753a4452-0b5e-44d5-88db-1857d14c0c17",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: true
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(false);
	});

	it('should deselect parent when no children selected', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: true
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(false);
	});

	it('should save selected org data for selected role', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};

		var preparedObject = {
			Id: '928dd0bc-bf40-412e-b970-9b5e015aadea',
			Name: 'TeleoptiCCCDemo',
			Type: 'BusinessUnit',
			IsSelected: false,
			ChildNodes: [{
				ChildNodes: [],
				Id: 'd970a45a-90ff-4111-bfe1-9b5e015ab45c',
				Name: 'London',
				Type: 'Site',
				IsSelected: false
			}]
		};

		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		spyOn(permissionsDataService, 'selectOrganization');
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

		expect(permissionsDataService.selectOrganization).toHaveBeenCalledWith(preparedObject);
	});

	it('should delete all org data for selected role', function() {
		var BusinessUnit = {
			Id: "b2f7feae-d777-4f94-bf9d-d180a215ec09",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "JagÄrEttTeam",
					Type: "Team",
					IsSelected: true
				}],
				Id: "a2816f4d-691b-4e0b-9950-6f815259dc68",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: [],
				AvailableBusinessUnits: [{
					Name: "London",
					Id: "b2f7feae-d777-4f94-bf9d-d180a215ec09"
				}],
				AvailableSites: [{
					Name: "London",
					Id: "a2816f4d-691b-4e0b-9950-6f815259dc68"
				}],
				AvailableTeams: [{
					Name: "JagÄrEttTeam",
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052"
				}]
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);
		$httpBackend.flush();

		expect(fakeBackend.getInfoForSelecteRole().AvailableBusinessUnits.length).toEqual(0);
		expect(fakeBackend.getInfoForSelecteRole().AvailableSites.length).toEqual(0);
		expect(fakeBackend.getInfoForSelecteRole().AvailableTeams.length).toEqual(0);
	});

	//Måste fixa fakebakend för delete och post. Göra mer dynamiskt, inte hårdkoda idn/parametrar.
	it('should delete unselected data for selected role', function() {
		var BusinessUnit = {
			Id: "b2f7feae-d777-4f94-bf9d-d180a215ec09",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "JagÄrEttTeam",
					Type: "Team",
					IsSelected: true
				}],
				Id: "a2816f4d-691b-4e0b-9950-6f815259dc68",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: [],
				AvailableBusinessUnits: [{
					Name: "London",
					Id: "b2f7feae-d777-4f94-bf9d-d180a215ec09"
				}],
				AvailableSites: [{
					Name: "London",
					Id: "a2816f4d-691b-4e0b-9950-6f815259dc68"
				}],
				AvailableTeams: [{
					Name: "JagÄrEttTeam",
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052"
				}]
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();

		expect(fakeBackend.getInfoForSelecteRole().AvailableBusinessUnits.length).toEqual(1);
		expect(fakeBackend.getInfoForSelecteRole().AvailableSites.length).toEqual(1);
		expect(fakeBackend.getInfoForSelecteRole().AvailableTeams.length).toEqual(0);
	});

	it('should have correct avaiable number of teams for selected role', function() {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: [],
				AvailableBusinessUnits: [],
				AvailableSites: [],
				AvailableTeams: []
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent2',
				Id: '1805c09e-39fe-4472-af28-ba9f3e8df759',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: '1805c09e-39fe-4472-af28-ba9f3e8df759',
				AvailableFunctions: [],
				AvailableBusinessUnits: [],
				AvailableSites: [],
				AvailableTeams: []
			})
			.withOrganizationSelection({
				Id: "b2f7feae-d777-4f94-bf9d-d180a215ec09",
				Name: "TeleoptiCCCDemo",
				Type: "BusinessUnit",
				ChildNodes: [{
					Id: "a2816f4d-691b-4e0b-9950-6f815259dc68",
					Name: "London",
					Type: "Site",
					ChildNodes: [{
						Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
						Name: "London-Team1",
						Type: "Team"
					}, {
						Id: "c6326a58-8876-497f-be97-8dce62cc0c11",
						Name: "London-Team2",
						Type: "Team"
					}]
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		vm.selectRole(vm.roles[0]);
		$httpBackend.flush();

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();
		vm.selectRole(vm.roles[1]);
		$httpBackend.flush();
		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[1].IsSelected).toEqual(true);
	});

	it('should not unselect bu when unselecting site when other site is selected', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: true
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}, {
				ChildNodes: [{
					Id: "299ee5a5-8883-42e4-8c8b-9f82908375f4",
					Name: "Team A",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "d1b591a9-5ce6-4a4c-978e-caa79abf399e",
					Name: "Team B",
					Type: "Team",
					IsSelected: true
				}],
				Id: "ef49b09c-363e-43fd-9a1e-58ef1cf656dd",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
	});

	it('should not unselect site when unselecting team when other team in site is selected', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: true
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}, {
				ChildNodes: [{
					Id: "299ee5a5-8883-42e4-8c8b-9f82908375f4",
					Name: "Team A",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "d1b591a9-5ce6-4a4c-978e-caa79abf399e",
					Name: "Team B",
					Type: "Team",
					IsSelected: true
				}],
				Id: "ef49b09c-363e-43fd-9a1e-58ef1cf656dd",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
	});

	it('should unselect BU when deselecting last site', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: true
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}, {
				ChildNodes: [{
					Id: "299ee5a5-8883-42e4-8c8b-9f82908375f4",
					Name: "Team A",
					Type: "Team",
					IsSelected: false
				}, {
					Id: "d1b591a9-5ce6-4a4c-978e-caa79abf399e",
					Name: "Team B",
					Type: "Team",
					IsSelected: false
				}],
				Id: "ef49b09c-363e-43fd-9a1e-58ef1cf656dd",
				Name: "London",
				Type: "Site",
				IsSelected: false
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
	});

	it('should deselect BU when selecting last team in last site', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: false
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}, {
				ChildNodes: [{
					Id: "299ee5a5-8883-42e4-8c8b-9f82908375f4",
					Name: "Team A",
					Type: "Team",
					IsSelected: false
				}, {
					Id: "d1b591a9-5ce6-4a4c-978e-caa79abf399e",
					Name: "Team B",
					Type: "Team",
					IsSelected: false
				}],
				Id: "ef49b09c-363e-43fd-9a1e-58ef1cf656dd",
				Name: "London",
				Type: "Site",
				IsSelected: false
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
	});

	it('should not deselect BU when selecting last team in a site while there are other selected sites', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			IsSelected: true,
			ChildNodes: [{
				ChildNodes: [{
					Id: "5d2cef4b-a994-4e6f-bb44-87c8306c2052",
					Name: "Team 1",
					Type: "Team",
					IsSelected: true
				}, {
					Id: "360bb004-356f-44fa-be18-cb92aa84c937",
					Name: "Team 2",
					Type: "Team",
					IsSelected: false
				}],
				Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}, {
				ChildNodes: [{
					Id: "299ee5a5-8883-42e4-8c8b-9f82908375f4",
					Name: "Team A",
					Type: "Team",
					IsSelected: false
				}, {
					Id: "d1b591a9-5ce6-4a4c-978e-caa79abf399e",
					Name: "Team B",
					Type: "Team",
					IsSelected: true
				}],
				Id: "ef49b09c-363e-43fd-9a1e-58ef1cf656dd",
				Name: "London",
				Type: "Site",
				IsSelected: true
			}]
		};
		var DynamicOptions = [];
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, DynamicOptions);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0]);
		$httpBackend.flush();

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].IsSelected).toEqual(false);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[1].IsSelected).toEqual(true);
		expect(vm.organizationSelection.BusinessUnit.ChildNodes[0].ChildNodes[0].IsSelected).toEqual(false);
	});

	it('should not be able to select orgdata without selected role', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: []
		};

		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(undefined);
	});

	it('should not be able to select orgdata if role is built in', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: []
		};

		fakeBackend
			.withRole({
				BuiltIn: true,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(undefined);
	});

	it('should not be able to select orgdata if role is my role', function() {
		var BusinessUnit = {
			Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
			Name: "TeleoptiCCCDemo",
			Type: "BusinessUnit",
			ChildNodes: []
		};

		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: true,
				Name: 'Agent'
			})
			.withRoleInfo({
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				AvailableFunctions: []
			})
			.withOrganizationSelection(BusinessUnit, []);
		$httpBackend.flush();
		ctrl = $componentController('permissionsList', null, {
			org: vm.organizationSelection
		});
		permissionsDataService.setSelectedRole(vm.roles[0]);

		ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

		expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(undefined);
	});

});
