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
			vm.roles.unshift(angular.fromJson(data));
			return [201, {}];
		});
		$httpBackend.whenDELETE('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64').respond(function(method, url, data, headers){
			return 200;
		});
		$httpBackend.whenPUT('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64?newDescription=%7B%7D').respond(function (method, url, data, headers) {
			var parsedObj = angular.fromJson(data);
			vm.roles[0].DescriptionText = parsedObj.newDescription;
			return 200;
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

	it('should get roles', function () {
		fakeBackend.withRole({
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

	it('should put newly created roll on top of roles list', function () {
		var name = 'rolename'
		fakeBackend.withRole({
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

		expect(vm.roles[0].Description).toBe(name);
	});

	it('should be able to edit the name of a role', function () {
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

		expect(vm.roles[0].DescriptionText).toBe('newRoleName');
	});

	it('should be able to delete a roll', function	(){
		var roleId = 'e7f360d3-c4b6-41fc-9b2d-9b5e015aae64';
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

});
