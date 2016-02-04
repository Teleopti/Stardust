'use strict';
describe('IntradayConigfCtrl', function () {
	var $httpBackend,
		$controller,
		scope;

	var skills = [];

	beforeEach(module('wfm.intraday'));

	beforeEach(function() {
		skills = [
		{
			Id: "fa9b5393-ef48-40d1-b7cc-09e797589f81",
			Name: "my skill"
		}];
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$rootScope_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();

		$httpBackend.whenGET("../api/intraday/skills")
			.respond(200, skills);
	}));

	var createController = function () {
		$controller('IntradayConigfCtrl', {
			$scope: scope
		});
		scope.$digest();
		$httpBackend.flush();
	}

	it('should display list of skills', function () {
		createController();

		expect(scope.skills[0].Id).toEqual("fa9b5393-ef48-40d1-b7cc-09e797589f81");
		expect(scope.skills[0].Name).toEqual("my skill");
		
	});
});