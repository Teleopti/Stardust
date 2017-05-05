'use strict';
describe('agentGroupFormController', function () {
	var $httpBackend,
		$controller,
		$state,
		$injector,
		$q,
		agentGroupService,
		debounceService,
		stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' };

	beforeEach(function () {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _$q_, _agentGroupService_, _debounceService_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		agentGroupService = _agentGroupService_;
		debounceService = _debounceService_;
		$q = _$q_;
		$state = _$state_;

		spyOn($state, 'go');
		spyOn(debounceService, 'debounce').and.callFake(function (cb) { return function () { cb(); } });
		spyOn(agentGroupService, 'getFilterData').and.callThrough();

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

		$httpBackend.whenGET('../api/ResourcePlanner/AgentGroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e').respond(function (method, url, data, headers) {
			return [200, {
				Id: "aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e",
				Name: "Agent Group 2",
				Filters: [{
					Id: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
					Name: "Skill 1",
					FilterType: "Skill"
				}]
			}, {}];
		});

		$httpBackend.whenDELETE('../api/ResourcePlanner/AgentGroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e').respond(function (method, url, data, headers) {
			return [200, true];
		});

		$httpBackend.whenPOST('../api/ResourcePlanner/AgentGroup').respond(function (method, url, data, headers) {
			return [200, true];
		});		
	}));

	afterEach(function () {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should call function with debounce 250', function () {
		var vm = $controller('agentGroupFormController');
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		expect(agentGroupService.getFilterData).toHaveBeenCalled();
	});

	it('should get filter results', function () {
		var vm = $controller('agentGroupFormController');
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		expect(vm.filterResults.length).toEqual(2);
		expect(vm.filterResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
		expect(vm.filterResults[1].Id).toEqual('a98d2c45-a8f4-4c70-97f9-907ab364af75');
	});

	it('should add one filter from filter results', function () {
		var vm = $controller('agentGroupFormController');
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);

		expect(vm.selectedResults.length).toEqual(1);
		expect(vm.selectedResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
	});

	it('should remove one filter from filter results', function () {
		var vm = $controller('agentGroupFormController');
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);
		vm.removeSelectedFilter(vm.selectedResults[0]);

		expect(vm.selectedResults.length).toEqual(0);
	});

	it('should not create agent group when submit data is invalid', function () {
		var vm = $controller('agentGroupFormController');
		vm.persist();

		expect($state.go).not.toHaveBeenCalledWith('resourceplanner.newoverview');
	});

	it('should create new agentgroup when submit data is valid', function () {
		var vm = $controller('agentGroupFormController');
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);
		vm.name = 'Agent Group';
		vm.persist();
		$httpBackend.flush();

		expect($state.go).toHaveBeenCalledWith('resourceplanner.newoverview');
	});

	it('should load edit agent group', function () {
		var vm = $controller('agentGroupFormController', { $stateParams: stateparams });
		$httpBackend.flush();

		expect(vm.editAgentGroup.Id).toEqual('aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e');
	});

	it('should load edit agent group', function () {
		var vm = $controller('agentGroupFormController', { $stateParams: stateparams });
		$httpBackend.flush();

		expect(vm.editAgentGroup.Id).toEqual('aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e');
	});

	it('should save new name for selected edit agent group', function () {
		spyOn(agentGroupService, 'saveAgentGroup').and.callThrough();
		var vm = $controller('agentGroupFormController', { $stateParams: stateparams });
		$httpBackend.flush();
		
		var id = vm.editAgentGroup.Id;
		var filter = vm.editAgentGroup.Filters;		
		vm.name = 'Agent Group 3';
		vm.persist();
		$httpBackend.flush();

		expect(agentGroupService.saveAgentGroup).toHaveBeenCalledWith({Id:id, Name: vm.name, Filters: filter });
		expect($state.go).toHaveBeenCalledWith('resourceplanner.newoverview');
	});

	it('should delete selected agent group', function () {
		spyOn(agentGroupService, 'removeAgentGroup').and.callThrough();
		var vm = $controller('agentGroupFormController', { $stateParams: stateparams });
		$httpBackend.flush();
		
		var id = vm.editAgentGroup.Id;
		var filter = vm.editAgentGroup.Filters;		
		vm.name = 'Agent Group 3';
		vm.removeAgentGroup(id);
		$httpBackend.flush();

		expect(agentGroupService.removeAgentGroup).toHaveBeenCalledWith({id:id});
		expect($state.go).toHaveBeenCalledWith('resourceplanner.newoverview');
	});
});
