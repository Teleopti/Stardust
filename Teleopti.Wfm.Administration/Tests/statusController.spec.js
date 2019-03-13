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

	it("should return id from backend", function() {
		var serverJson = [{id :18}];
		$httpBackend.expectGET('./status/list').respond(serverJson);

		var $scope = $rootScope.$new();
		var controller = $controller('statusController', { $scope: $scope });
		//$scope.$digest();
		$httpBackend.flush();

		expect(controller.statusSteps[0].id).toBe(serverJson[0].id);
	});
});