/// <reference path="dependencies.js" />

describe("statusController", function() {
	var $controller, $rootScope, $httpBackend;

	beforeEach(function(){
		module('adminApp');

		inject(function(_$controller_, _$rootScope_, _$httpBackend_){
			$controller = _$controller_;
			$rootScope = _$rootScope_;
			$httpBackend = _$httpBackend_;
		})
	});

	it("should return no custom steps", function() {
		var $scope = $rootScope.$new();
		var controller = $controller('statusController', { $scope: $scope });

		expect(controller.statusSteps).toEqual([]);
	});

	it("should return data from backend", function() {
		var serverJson = [{
				Id : 18,
				Name : 'some name',
				Description : 'some desc',
				PingUrl : 'http://something.com/hejhej/ping/18',
				Limit : 15
			}];
		$httpBackend.expectGET('./status/listCustom').respond(serverJson);

		var $scope = $rootScope.$new();
		var controller = $controller('statusController', { $scope: $scope });
		$httpBackend.flush();

		expect(controller.statusSteps[0].id).toBe(serverJson[0].Id);
		expect(controller.statusSteps[0].name).toBe(serverJson[0].Name);
		expect(controller.statusSteps[0].description).toBe(serverJson[0].Description);
		expect(controller.statusSteps[0].pingUrl).toBe(serverJson[0].PingUrl);
		expect(controller.statusSteps[0].limit).toBe(serverJson[0].Limit);
	});
	
	it("should handle multiple items", function(){
		var serverJson = [{	Id : 18}, {Id :1}];
		$httpBackend.expectGET('./status/listCustom').respond(serverJson);

		var $scope = $rootScope.$new();
		var controller = $controller('statusController', { $scope: $scope });
		$httpBackend.flush();

		expect(controller.statusSteps.length).toBe(2);
	})
});