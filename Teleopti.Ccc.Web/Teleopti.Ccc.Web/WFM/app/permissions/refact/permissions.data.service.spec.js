'use strict';
//DONT REMOVE X
xdescribe('permissionsDataService', function () {
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

	var dynamicOption = {
		RangeOption: 0, 
		Name: "Test"
	}

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _permissionsDataService_) {
		$httpBackend = _$httpBackend_;
		fakeBackend = _fakePermissionsBackend_;
		permissionsDataService = _permissionsDataService_;

		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/AvailableData').respond(function (method, url, data, headers) {
			response = angular.fromJson(data);
			return 200;
		});
		//HÄR SKA NI FORTÄSTTA
		$httpBackend.whenPOST('../api/Permissions/Roles/e7f360d3-c4b6-41fc-9b2d-9b5e015aae64/DeleteData').respond(function (method, url, data, headers) {
			return 200;
		});

	}));

	afterEach(function () {
		response = null;
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should prepare all org data for sending to server when selecting bu', function () {
		permissionsDataService.setSelectedRole(role);

		var data = permissionsDataService.prepareData(BusinessUnit);

		expect(data.Id).toEqual('e7f360d3-c4b6-41fc-9b2d-9b5e015aae64');
		expect(data.BusinessUnits[0]).toEqual('928dd0bc-bf40-412e-b970-9b5e015aadea');
		expect(data.Sites[0]).toEqual('fe113bc0-979a-4b6c-9e7c-ef601c7e02d1');
		expect(data.Teams[0]).toEqual('e6377d56-277d-4c22-97f3-b218741b2480');
	});

	it('should send all org data to server when selecting bu', function () {
		permissionsDataService.setSelectedRole(role);
		var data = permissionsDataService.prepareData(BusinessUnit);

		permissionsDataService.selectOrganization(BusinessUnit);
		$httpBackend.flush();

		expect(response).toEqual(data);
	});

	it('should not persist previously sent bu', function () {
		permissionsDataService.setSelectedRole(role);
		var data = permissionsDataService.prepareData(BusinessUnit);

		permissionsDataService.selectOrganization(
			{
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
			}
		);
		$httpBackend.flush();

		expect(data.BusinessUnits.length).toEqual(1);
	});

	it('should send dynamic option data to server', function () {
		permissionsDataService.setSelectedRole(role);
		var data  = permissionsDataService.prepareDynamicOption(dynamicOption);

		permissionsDataService.selectDynamicOption(dynamicOption);
		$httpBackend.flush();

		expect(response).toEqual(data)
	});

});
