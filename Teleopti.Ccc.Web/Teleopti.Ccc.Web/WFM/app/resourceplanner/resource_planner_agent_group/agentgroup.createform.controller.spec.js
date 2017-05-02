'use strict';
xdescribe('agentGroupFormController', function () {
	var $httpBackend,
		$controller,
		$state,
		fakeBackend,
		vm;

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _fakeResourcePlanningBackend_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		fakeBackend = _fakeResourcePlanningBackend_;
		$state = _$state_;

		spyOn($state, 'go');
		fakeBackend.clear();

		vm = $controller('agentGroupFormController');

		$httpBackend.whenGET(/.*?api\/filtersagentgroup\?.*/).respond(function (method, url, data, headers) {
			return [200, [{
				Id: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				Name: "Skill 1",
				FilterType: "Skill"
			}, {
				Id: "a98d2c45-a8f4-4c70-97f9-907ab364af75",
				Name: "Skill 2",
				FilterType: "Skill"
			}], {}];
		});
	}));

	afterEach(function () {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should get filter skills', function () {
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		console.log(vm.filterResults);

		expect(vm.filterResults.length).toEqual(2);
	});

	xit('should create new agentgroup when submit data is valid', function () {
		$httpBackend.whenPOST('../api/ResourcePlanner/AgentGroup').respond(function (method, url, data, headers) {
			return [200, {
				Id: "3d3d7ddc-27b4-4188-93c0-d986e93c9712",
				Name: "TEST agent group",
			}];
		});

		vm.name = 'Test';
		vm.selectedResults = [];
		vm.persist();
		$httpBackend.flush();

		expect($state.go).toHaveBeenCalledWith('resourceplanner.newoverview');
	});
});
