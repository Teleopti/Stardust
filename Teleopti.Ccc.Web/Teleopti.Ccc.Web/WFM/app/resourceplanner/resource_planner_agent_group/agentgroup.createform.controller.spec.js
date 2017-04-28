'use strict';
describe('agentGroupFormController', function () {
	var $httpBackend,
		$controller,
		fakeBackend;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _fakeSkillPrioBackend_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		fakeBackend = _fakeSkillPrioBackend_;

		fakeBackend.clear();
	}));


	it('should get activities', function () {
		fakeBackend.withActivity({
			ActivityGuid: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
			ActivityName: "Phone"
		}).withActivity({
			ActivityGuid: "a98d2c45-a8f4-4c70-97f9-907ab364af75",
			ActivityName: "Lunch"
		});

		var vm = $controller('skillPrioControllerNew');
		$httpBackend.flush();

		expect(vm.activites.length).toEqual(2);
	});



});
