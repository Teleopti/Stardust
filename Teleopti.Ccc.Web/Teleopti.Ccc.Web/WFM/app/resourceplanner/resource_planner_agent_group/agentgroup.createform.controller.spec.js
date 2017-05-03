'use strict';
xdescribe('agentGroupFormController', function () {
	var $httpBackend,
		$controller,
		$state,
		fakeBackend;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _fakeResourcePlanningBackend_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		$state = _$state_;
		spyOn($state, 'go');
		fakeBackend = _fakeResourcePlanningBackend_;

		fakeBackend.clear();
	}));


	it('should not create new agentgroup when submit data is not valid', function () {	
		var vm = $controller('agentGroupFormController');
		vm.persist();

		expect($state.go).not.toHaveBeenCalledWith('resourceplanner.newoverview');
	});

	xit('should create new agentgroup when submit data is valid', function () {
		var vm = $controller('agentGroupFormController');
		vm.name = 'Test';
		vm.persist();

		expect($state.go).toHaveBeenCalledWith('resourceplanner.newoverview');
	});

	xit('should load exit agentgroup', function () {
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

	xit('should not save edit agentgroup when submit data is not valid', function () {
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
