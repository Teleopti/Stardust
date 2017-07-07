'use strict';

describe('component: permissionsTree', function () {
	var $httpBackend,
		fakeBackend,
		$controller,
		$componentController,
		permissionsDataService,
		ctrl,
		vm,
		response;

	var allFunction;

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$componentController_, _$controller_, _permissionsDataService_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$componentController = _$componentController_;
		$controller = _$controller_;
		permissionsDataService = _permissionsDataService_;

		fakeBackend.clear();
		
		vm = $controller('PermissionsController');

		$httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');

		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Functions').respond(function (method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/5ad43bfa-7842-4cca-ae9e-8d03ddc789e9').respond(function (method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/f19bb790-b000-4deb-97db-9b5e015b2e8c').respond(function (method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/t19bb790-b000-4deb-97db-9b5e015b2e8c').respond(function (method, url, data, headers) {
			return 200;
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/8ecf6029-4f3c-409c-89db-46bd8d7d402d').respond(200);

		allFunction = {
			ChildFunctions: [],
			FunctionCode: 'All',
			FunctionDescription: 'xxAll',
			FunctionId: '8ecf6029-4f3c-409c-89db-46bd8d7d402d',
			IsDisabled: false,
			IsSelected: false,
			LocalizedFunctionDescription: 'All'
		};
	}));

	afterEach(function () {
		response = null;
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should be able to select a function', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(true);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(undefined);
	});

	it('should not be able to select a function without a selected role', function () {
		fakeBackend
			.withRole({
				BuiltIn: true,
				DescriptionText: 'Admin',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: true,
				IsMyRole: true,
				IsSelected: false,
				Name: 'Admin'
			})
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected
		});

		ctrl.toggleFunction(vm.applicationFunctions[0]);

		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(undefined);
	});

	it('should confirm before deselecting a parent', function () {
		fakeBackend.withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsOpen: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick
		});
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;

		ctrl.checkParent(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].multiDeselectModal).toEqual(true);
	});

	it('should be able to deselect a function', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(undefined);
	});

	it('should select parent function when selecting a child for that parent', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}],
				FunctionCode: 'Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj,
			parent: function () {
				return ctrl.onSelect.apply(null, arguments)
			}
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		ctrl.toggleFunction(vm.applicationFunctions[1].ChildFunctions[0]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(true);
		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(true);
	});

	it('should select only the parent function when selecting a child for that parent', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}],
				FunctionCode: 'Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false
			}).withApplicationFunction({
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'Second ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '8ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}],
				FunctionCode: 'Second Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'b19bb790-b000-4deb-97db-9b5e015b2e8d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});;
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj,
			parent: function () {
				return ctrl.onSelect.apply(null, arguments)
			}
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		ctrl.toggleFunction(vm.applicationFunctions[1].ChildFunctions[0]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['b19bb790-b000-4deb-97db-9b5e015b2e8d']).toEqual(undefined);
		expect(vm.selectedFunctions['8ad43bfa-7842-4cca-ae9e-8d03ddc789e0']).toEqual(undefined);
		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(true);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(true);
	});

	it('should not unselect other parents when selecting child for different parent', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			})
			.withApplicationFunction({
				FunctionCode: 'Second Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 't19bb790-b000-4deb-97db-9b5e015b2e8d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false,
				ChildFunctions: []
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj,
			parent: function () {
				return ctrl.onSelect.apply(null, arguments)
			}
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		vm.selectedFunctions['t19bb790-b000-4deb-97db-9b5e015b2e8d'] = true;

		ctrl.toggleFunction(vm.applicationFunctions[1].ChildFunctions[0]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(true);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(true);
		expect(vm.selectedFunctions['t19bb790-b000-4deb-97db-9b5e015b2e8d']).toEqual(true);
	});

	it('should not toggle unselected child when deselecting parent', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}, {
					ChildFunctions: [],
					FunctionCode: 'ChildFunction2',
					FunctionDescription: 'ChildFunction2',
					FunctionId: '6ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function2',
					IsOpen: false
				}]
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj,
			parent: function () {
				return ctrl.onSelect.apply(null, arguments)
			}
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9'] = true;

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(undefined);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(undefined);
		expect(vm.selectedFunctions['6ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(undefined);
	});

	it('should deselect matching children when deselecting parent', function () {
		fakeBackend
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 't19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false,
				ChildFunctions: [{
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '6ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false,
					ChildFunctions: [{
						ChildFunctions: [],
						FunctionCode: 'Grandchildfunction',
						FunctionDescription: 'GrandchildFunction',
						FunctionId: '7ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
						IsDisabled: false,
						LocalizedFunctionDescription: 'I am A grandchild function',
						IsOpen: false
					}]
				}]
			})
			.withApplicationFunction({
				FunctionCode: 'Second Parent',
				FunctionDescription: 'ParentFunction',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8d',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I Am A Parent Function',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'Second ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			})
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj,
			parent: function () {
				return ctrl.onSelect.apply(null, arguments)
			}
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		vm.selectedFunctions['t19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		vm.selectedFunctions['6ad43bfa-7842-4cca-ae9e-8d03ddc789e9'] = true;
		vm.selectedFunctions['7ad43bfa-7842-4cca-ae9e-8d03ddc789e0'] = true;
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8d'] = true;
		vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e0'] = true;

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['t19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(undefined);
		expect(vm.selectedFunctions['6ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(undefined);
		expect(vm.selectedFunctions['7ad43bfa-7842-4cca-ae9e-8d03ddc789e0']).toEqual(undefined);
		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8d']).toEqual(true);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e0']).toEqual(true);
	});

	it('should not be able to edit built in role or my role', function () {
		inject(function (NoticeService) {
			fakeBackend
				.withRole({
					BuiltIn: true,
					DescriptionText: 'Admin',
					Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
					IsAnyBuiltIn: true,
					IsMyRole: true,
					Name: 'Admin'
				})
				.withApplicationFunction({
					FunctionCode: 'Raptor',
					FunctionDescription: 'xxOpenRaptorApplication',
					FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
					IsDisabled: false,
					LocalizedFunctionDescription: 'Open Teleopti WFM',
					IsSelected: false
				});
			$httpBackend.flush();
			ctrl = $componentController('permissionsTree', null, {
				functions: vm.applicationFunctions,
				select: vm.selectFunction,
				isSelected: vm.selectFunction,
				onClick: vm.onFunctionClick,
				selectedRole: vm.selectedRole
			});
			spyOn(NoticeService, "warning");
			ctrl.selectedRole = vm.roles[0];

			ctrl.toggleFunction(vm.applicationFunctions[0]);

			expect(NoticeService.warning).toHaveBeenCalledWith("ChangesAreDisabled", 5000, true);
		});
	});

	it('should save selected function for selected role', function () {
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
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsSelected: false
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.selectFunction,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		spyOn(permissionsDataService, 'selectFunction');
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.expectDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/8ecf6029-4f3c-409c-89db-46bd8d7d402d').respond(200);
		$httpBackend.flush();

		expect(permissionsDataService.selectFunction).toHaveBeenCalled();
	});

	it('should delete unselected function for selected role', function () {
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
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsSelected: true
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.selectFunction,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		spyOn(permissionsDataService, 'selectFunction')

		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.expectDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/Function/8ecf6029-4f3c-409c-89db-46bd8d7d402d').respond(200);
		$httpBackend.flush();

		expect(permissionsDataService.selectFunction).toHaveBeenCalled();
	});

	it('should remove selected function when unselected functions filter is active', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];
		vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9'] = true;
		vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c'] = true;
		ctrl.filterFunc.isUnSelected = true;

		ctrl.toggleFunction(vm.applicationFunctions[1].ChildFunctions[0]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(true);
	});

	it('should be able to deselect functions while all function toggle is active', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];

		vm.toggleAllFunction();
		$httpBackend.flush();
		ctrl.toggleFunction(vm.applicationFunctions[1]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(undefined);
		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(undefined);
		expect(vm.isAllFunctionSelected).toEqual(false);
	});

	it('should select all toggle when all functions are selected', function () {
		fakeBackend
			.withRole({
				BuiltIn: false,
				DescriptionText: 'Agent',
				Id: 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64',
				IsAnyBuiltIn: false,
				IsMyRole: false,
				Name: 'Agent'
			})
			.withApplicationFunction(allFunction)
			.withApplicationFunction({
				FunctionCode: 'Raptor',
				FunctionDescription: 'xxOpenRaptorApplication',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'Open Teleopti WFM',
				IsOpen: false,
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'ChildFunction',
					FunctionDescription: 'ChildFunction',
					FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A child function',
					IsOpen: false
				}]
			});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {
			functions: vm.applicationFunctions,
			select: vm.selectFunction,
			isSelected: vm.isFunctionSelected,
			onClick: vm.onFunctionClick,
			selectedRole: vm.selectedRole,
			filterFunc: vm.functionsFilterObj
		});
		ctrl.selectedRole = vm.roles[0];
		vm.selectedRole = vm.roles[0];
		vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9'] = true;

		ctrl.toggleFunction(vm.applicationFunctions[1].ChildFunctions[0]);
		$httpBackend.flush();

		expect(vm.selectedFunctions['8ecf6029-4f3c-409c-89db-46bd8d7d402d']).toEqual(true);
		expect(vm.selectedFunctions['5ad43bfa-7842-4cca-ae9e-8d03ddc789e9']).toEqual(true);
		expect(vm.selectedFunctions['f19bb790-b000-4deb-97db-9b5e015b2e8c']).toEqual(true);
		expect(vm.isAllFunctionSelected).toEqual(true);
	});

});
