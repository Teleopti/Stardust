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
	}));

	it('should get a application function', function () {
		fakeBackend.withApplicationFunction({
			ChildFunctions:[],
            FunctionCode:'Raptor',
            FunctionDescription:'xxOpenRaptorApplication',
            FunctionId:'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled:false,
            LocalizedFunctionDescription:'Open Teleopti WFM'
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
			ChildFunctions:[],
            FunctionCode:'Raptor',
            FunctionDescription:'xxOpenRaptorApplication',
            FunctionId:'f19bb790-b000-4deb-97db-9b5e015b2e8c',
            IsDisabled:false,
            LocalizedFunctionDescription:'Open Teleopti WFM'
		}).withApplicationFunction({
			ChildFunctions:[],
            FunctionCode:'Anywhere',
            FunctionDescription:'xxAnywhere',
            FunctionId:'7884b7dd-31ea-4e40-b004-c7ce3b5deaf3',
            IsDisabled:false,
            LocalizedFunctionDescription:'Open Teleopti TEM'
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
});
