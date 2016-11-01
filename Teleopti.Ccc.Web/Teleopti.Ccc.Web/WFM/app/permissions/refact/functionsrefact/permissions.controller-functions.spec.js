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
			IsSelected: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {functions: vm.applicationFunctions});

		ctrl.toggleFunction(vm.applicationFunctions[0]);

		expect(vm.applicationFunctions[0].IsSelected).toEqual(true);
	});

	it('should be able to deselect a function', function () {
		fakeBackend.withApplicationFunction({
			FunctionCode: 'Raptor',
			FunctionDescription: 'xxOpenRaptorApplication',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'Open Teleopti WFM',
			IsSelected: true
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {functions: vm.applicationFunctions});

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
				IsSelected: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {functions: vm.applicationFunctions, parent: function() { return ctrl.onSelect.apply(null, arguments) }});

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
			IsSelected: false
			}],
			FunctionCode: 'Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8c',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false
		}).withApplicationFunction({
			ChildFunctions: [{
				ChildFunctions: [],
				FunctionCode: 'Second ChildFunction',
				FunctionDescription: 'ChildFunction',
				FunctionId: '5ad43bfa-7842-4cca-ae9e-8d03ddc789e0',
				IsDisabled: false,
				LocalizedFunctionDescription: 'I am A child function',
			IsSelected: false
			}],
			FunctionCode: 'Second Parent',
			FunctionDescription: 'ParentFunction',
			FunctionId: 'f19bb790-b000-4deb-97db-9b5e015b2e8d',
			IsDisabled: false,
			LocalizedFunctionDescription: 'I Am A Parent Function',
			IsSelected: false
		});
		$httpBackend.flush();
		ctrl = $componentController('permissionsTree', null, {functions: vm.applicationFunctions, parent: function() { return ctrl.onSelect.apply(null, arguments) }});

		ctrl.toggleFunction(vm.applicationFunctions[0].ChildFunctions[0]);

		expect(vm.applicationFunctions[1].IsSelected).toEqual(false);
		expect(vm.applicationFunctions[1].ChildFunctions[0].IsSelected).toEqual(false);
	});
});
