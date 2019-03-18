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
	
	var createController = function(){
		var $scope = $rootScope.$new();
		return $controller('statusController', { $scope: $scope });
	};

	it("should return no custom steps", function() {
		var controller = createController();

		expect(controller.statusSteps).toEqual([]);
	});

	it("should return data from backend", function() {
		var serverJson = [{
				Id : 18,
				Name : 'some name',
				Description : 'some desc',
				PingUrl : 'http://something.com/hejhej/ping/18',
				Limit : 15,
				CanBeDeleted: true
			}];
		$httpBackend.expectGET('./status/listCustom').respond(serverJson);

		var controller = createController();
		$httpBackend.flush();

		expect(controller.statusSteps[0].id).toBe(serverJson[0].Id);
		expect(controller.statusSteps[0].name).toBe(serverJson[0].Name);
		expect(controller.statusSteps[0].description).toBe(serverJson[0].Description);
		expect(controller.statusSteps[0].pingUrl).toBe(serverJson[0].PingUrl);
		expect(controller.statusSteps[0].limit).toBe(serverJson[0].Limit);
		expect(controller.statusSteps[0].canBeDeleted).toBe(serverJson[0].CanBeDeleted);
	});
	
	it("should handle multiple items", function(){
		var serverJson = [{	Id : 18}, {Id :1}];
		$httpBackend.expectGET('./status/listCustom').respond(serverJson);

		var controller = createController();
		$httpBackend.flush();

		expect(controller.statusSteps.length).toBe(2);
	});
	
	it("should successfully send add to backend", function(){
		$httpBackend.whenGET('./status/listCustom').respond([]);

		var controller = createController();
		controller.newStatusStep = {
			name:'some name',
			description: 'some desc',
			limit: 19
		};

		$httpBackend.whenPOST('./status/Add').respond(function (method, url, data) {
			var sent = JSON.parse(data);
			expect(sent.Name).toBe(controller.newStatusStep.name);
			expect(sent.Description).toBe(controller.newStatusStep.description);
			expect(sent.LimitInSeconds).toBe(controller.newStatusStep.limit);
			return [200];
		});
		controller.storeNew();
		$httpBackend.flush();
	});
	
	it("limit should default to valid number", function(){
		$httpBackend.whenGET('./status/listCustom').respond([]);

		var controller = createController();
		
		expect(controller.newStatusStep.limit).toBe(60);
	});
	
	it("should not be valid if limit is a not number", function(){
		$httpBackend.whenGET('./status/listCustom').respond([]);

		var controller = createController();
		controller.newStatusStep.limit = 'roger';

		expect(controller.newStatusStepValid()).toBe(false);
	});

	it("should not be valid if name is empty", function(){
		$httpBackend.whenGET('./status/listCustom').respond([]);

		var controller = createController();
		controller.newStatusStep.name = '';

		expect(controller.newStatusStepValid()).toBe(false);
	});
	
	it("should not call back end if not valid", function(){
		$httpBackend.whenGET('./status/listCustom').respond([]);

		var controller = createController();
		controller.newStatusStep = {
			limit: 'wrong'
		};
		controller.storeNew();

		expect($httpBackend.flush).not.toThrow();
	});
});