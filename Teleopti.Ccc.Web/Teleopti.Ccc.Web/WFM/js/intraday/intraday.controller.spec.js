'use strict';
describe('IntradayCtrl', function () {
	var $httpBackend,
		$controller,
		scope;

	var skillAreas = [];
	var skills = [];

	beforeEach(module('wfm.intraday'));

	beforeEach(function () {
		skills = [
		{
			Id: "5f15b334-22d1-4bc1-8e41-72359805d30f",
			Name: "skill x"
		}];

		skillAreas = [
		{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill area",
			Skills: skills
		}];
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();

		$httpBackend.whenGET("../api/intraday/skillarea")
			.respond(200, skillAreas);

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(200, skills);
	}));

	var createController = function () {
		$controller('IntradayCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should display list of skill areas', function () {
		createController();

		expect(scope.skillAreas[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skillAreas[0].Name).toEqual("my skill area");
		expect(scope.skillAreas[0].Skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skillAreas[0].Skills[0].Name).toEqual("skill x");
	});

	it('should display list of skills', function () {
		createController();

		expect(scope.skills[0].Id).toEqual("5f15b334-22d1-4bc1-8e41-72359805d30f");
		expect(scope.skills[0].Name).toEqual("skill x");
	});
});