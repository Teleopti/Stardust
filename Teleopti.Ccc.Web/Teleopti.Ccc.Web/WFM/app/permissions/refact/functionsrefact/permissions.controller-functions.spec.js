'use strict';
//DONT REMOVE X
xdescribe('component: permissionsTree', function () {
	var $httpBackend,
		fakeBackend,
		$controller,
		$componentController,
		ctrl,
		vm;

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$componentController_, _$controller_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		$componentController = _$componentController_;
		$controller = _$controller_;

		fakeBackend.clear();
		vm = $controller('PermissionsCtrlRefact');
	}));

	afterEach(function () {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should be able to select a function', function () {
		fakeBackend.withApplicationFunction({
			FunctionCode: 'Raptor',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti WFM',
			IsSelected: false,
			IsOpen: false,
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: false,
				IsOpen: false
			}]
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });

		ctrl.toggleFunction(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
	});

	it('should be able to select a parent function', function () {
		fakeBackend.withApplicationFunction({
			FunctionCode: 'Raptor',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti WFM',
			IsSelected: false,
			IsOpen: false,
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: false,
				IsOpen: false
			}]
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });

		ctrl.checkParent(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
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
				IsSelected: false,
				IsOpen: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: true,
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });
		ctrl.checkParent(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].multiDeselectModal).toEqual(true);
	});

	it('should be able to deselect a function', function () {
		fakeBackend.withApplicationFunction({
			FunctionCode: 'Raptor',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti WFM',
			IsSelected: true,
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });

		ctrl.toggleFunction(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(false);
	});

	it('should select parent function when selecting a child for that parent', function () {
		fakeBackend.withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: false,
				IsOpen: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false,
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions, parent: function () { return ctrl.onSelect.apply(null, arguments) } });

		ctrl.toggleFunction(vm.applicationFunctions[0].ChildFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[0].ChildFunctions[0].IsSelected).toEqual(true);
	});

	it('should select only the parent function when selecting a child for that parent', function () {
		fakeBackend.withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: false,
				IsOpen: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false,
			IsOpen: false
		}).withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'Second ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: false,
				IsOpen: false
			}],
			FunctionCode: 'Second Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8d',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false,
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions, parent: function () { return ctrl.onSelect.apply(null, arguments) } });

		ctrl.toggleFunction(vm.applicationFunctions[0].ChildFunctions[0]);

		expect(vm.applicationFunctions[1].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[1].ChildFunctions[0].IsSelected).toEqual(false);
	});

	it('should deselect matching children when deselecting parent', function () {
		fakeBackend.withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [{
					ChildFunctions: [],
					FunctionCode: 'Grandchildfunction',
					FunctionDescription: 'GrandchildFunction',
					FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
					IsDisabled: false,
					LocalizedFunctionDescription: 'I am A grandchild function',
					IsSelected: true,
					IsOpen: false
				}],
				FunctionCode: 'ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e9',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: true,
				IsOpen: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: true,
			IsOpen: false
		}).withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'Second ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
				IsSelected: true,
				IsOpen: false
			}],
			FunctionCode: 'Second Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8d',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: true,
			IsOpen: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions, parent: function () { return ctrl.onSelect.apply(null, arguments) } });

		ctrl.toggleFunction(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[0].ChildFunctions[0].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[0].ChildFunctions[0].ChildFunctions[0].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[1].IsSelected).toEqual(true);
		expect(vm.applicationFunctions[1].ChildFunctions[0].IsSelected).toEqual(true);
	});

	it('should not be able to edit built in role or my role', function () {
		inject(function (permissionsDataService, NoticeService) {
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
			ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });
			spyOn(NoticeService, "warning");
			permissionsDataService.setSelectedRole(vm.roles[0]);
			ctrl.toggleFunction(vm.applicationFunctions[0]);

			expect(vm.applicationFunctions[0].IsSelected).toEqual(false);
			expect(NoticeService.warning).toHaveBeenCalledWith("ChangesAreDisabled", 5000, true);
		});
	});


	it('should save selected function for selected role', function () {
		inject(function (permissionsDataService) {
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
				.withApplicationFunction({
					FunctionCode: 'Raptor',
					FunctionDescription: 'xxOpenRaptorApplication',
					FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
					IsDisabled: false,
					LocalizedFunctionDescription: 'Open Teleopti WFM',
					IsSelected: false
				});
			$httpBackend.flush();
			ctrl = $componentController('permissionsTree', null, { functions: vm.applicationFunctions });
			spyOn(permissionsDataService, 'selectFunction');
			permissionsDataService.setSelectedRole(vm.roles[0]);

			ctrl.toggleFunction(vm.applicationFunctions[0]);

			expect(permissionsDataService.selectFunction).toHaveBeenCalled();
		});
	});

});
