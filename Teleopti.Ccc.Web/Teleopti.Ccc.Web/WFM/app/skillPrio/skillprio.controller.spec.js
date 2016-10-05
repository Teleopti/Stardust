'use strict';
describe('skillPrioController', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(function () {
		module('wfm.skillPrio');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
        $httpBackend.expectGET("../ToggleHandler/AllToggles").respond(200, 'mock');
	}));


	xit('should move a skill to sorted', inject( function ($controller) {
		var scope = $rootScope.$new();

		var vm = $controller('skillPrioController', { $scope: scope });
		scope.$digest();
        vm.skills = [{name:'test'}];
        vm.sortedSkills = [];
        vm.addSkill(vm.skills[0])
		expect(vm.sortedSkills.length).toEqual(1);
	}));
});

