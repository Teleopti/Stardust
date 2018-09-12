﻿'use strict';
fdescribe('AreasController', function() {
	var $httpBackend, $controller, $q, $scope;

	beforeEach(function() {
		module('wfm.areas');
	});

	beforeEach(inject(function(_$httpBackend_, _$controller_, _$q_, _$rootScope_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$q = _$q_;
		$scope = _$rootScope_.$new();
	}));

	it('should get an area', inject(function() {
		var vm = $controller('AreasController', { $scope });
		var areas = [
			{
				Name: 'Forecaster',
				internalName: 'Forecaster',
				_links: []
			}
		];
		$httpBackend.whenGET('../api/Global/Application/WfmAreasWithPermission').respond(function(method, url, data) {
			return [200, areas, {}];
		});
		vm.loadAreas();
		$httpBackend.flush();

		expect(vm.areas[0].Name).toEqual('Forecaster');
	}));

	it('contains correct area', inject(function() {
		var vm = $controller('AreasController', { $scope });
		var areas = [
			{
				Name: 'Forecaster',
				internalName: 'Forecaster',
				_links: []
			},
			{
				Name: 'Schedules',
				InternalName: 'resourceplanner',
				_links: []
			}
		];
		$httpBackend.whenGET('../api/Global/Application/WfmAreasWithPermission').respond(function(method, url, data) {
			return [200, areas, {}];
		});
		vm.loadAreas();
		$httpBackend.flush();

		expect(vm.areas[1].Name).toEqual('Schedules');
	}));
});
