'use strict';
describe('IntradayConfigController', function() {
	var $httpBackend, $controller, $translate, scope, vm;

	var skills = [];

	beforeEach(module('wfm.intraday'));

	beforeEach(function() {
		skills = [
			{
				Id: 'fa9b5393-ef48-40d1-b7cc-09e797589f81',
				Name: 'my skill'
			}
		];
	});

	beforeEach(inject(function(_$httpBackend_, _$controller_, _$rootScope_, _$translate_) {
		$controller = _$controller_;
		$httpBackend = _$httpBackend_;
		scope = _$rootScope_.$new();
		$translate = _$translate_;
		$httpBackend.whenGET('../api/skillgroup/skills').respond(200, skills);

		$httpBackend.whenGET('../ToggleHandler/AllToggles').respond(200, {});
	}));

	var createController = function(isNewlyCreatedSkillGroup) {
		vm = $controller('IntradayConfigController', {
			$scope: scope,
			$translate: $translate
		});
		scope.$digest();
		$httpBackend.flush();
	};

	it('should display list of skills', function() {
		createController();
		expect(vm.skills.data[0].Id).toEqual('fa9b5393-ef48-40d1-b7cc-09e797589f81');
		expect(vm.skills.data[0].Name).toEqual('my skill');
	});
});
