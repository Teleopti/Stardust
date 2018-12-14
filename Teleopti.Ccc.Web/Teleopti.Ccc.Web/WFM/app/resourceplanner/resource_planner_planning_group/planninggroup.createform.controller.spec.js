'use strict';
describe('planningGroupFormController', function () {
	var $httpBackend,
		$controller,
		$state,
		$rootScope,
		$injector,
		planningGroupService,
		debounceService,
		editPlanningGroup = {
			Id: "aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e",
			Name: "Plan Group 2",
			Filters: [{
				Id: "0ffeb898-11bf-43fc-8104-9b5e015ab3c2",
				Name: "Skill 1",
				FilterType: "Skill"
			}], 
			Settings: [{
				Id: '00e9d2f9-e35e-408a-9cef-a76cfc9f6d6c',
				PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
				Name: "Default",
				Default: true,
				MinDayOffsPerWeek: 1,
				MaxDayOffsPerWeek: 3,
				MinConsecutiveWorkdays: 2,
				MaxConsecutiveWorkdays: 6,
				MinConsecutiveDayOffs: 1,
				MaxConsecutiveDayOffs: 3,
				Priority: 0,
				Filters: []
			}]
		},
		stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' };

	beforeEach(function () {
		module('wfm.resourceplanner');
		module('localeLanguageSortingService');
	});

	beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _$rootScope_,_planningGroupService_, _debounceService_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		planningGroupService = _planningGroupService_;
		debounceService = _debounceService_;
		$state = _$state_;
		$rootScope = _$rootScope_;

		spyOn($state, 'go');
		spyOn(debounceService, 'debounce').and.callFake(function (cb) { return function () { cb(); } });
		spyOn(planningGroupService, 'getFilterData').and.callThrough();

		$httpBackend.whenGET(/.*?api\/filtersplanninggroup\?.*/).respond(function (method, url, data, headers) {
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

		$httpBackend.whenDELETE('../api/resourceplanner/planninggroup/aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e').respond(function (method, url, data, headers) {
			return [200, true];
		});

		$httpBackend.whenPOST('../api/resourceplanner/planninggroup').respond(function (method, url, data, headers) {
			return [200, true];
		});
	}));

	afterEach(function () {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should call function with debounce 250', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		expect(planningGroupService.getFilterData).toHaveBeenCalled();
	});

	it('should get filter results', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		expect(vm.filterResults.length).toEqual(2);
		expect(vm.filterResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
		expect(vm.filterResults[1].Id).toEqual('a98d2c45-a8f4-4c70-97f9-907ab364af75');
	});

	it('should add one filter from filter results', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);

		expect(vm.selectedResults.length).toEqual(1);
		expect(vm.selectedResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
	});

	it('should remove one filter from filter results', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);
		vm.removeSelectedFilter(vm.selectedResults[0]);

		expect(vm.selectedResults.length).toEqual(0);
	});

	it('should not create planning group when submit data is invalid', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.persist();

		expect($state.go).not.toHaveBeenCalledWith('resourceplanner.overview');
	});

	it('should create new planning group when submit data is valid', function () {
		var vm = $controller('planningGroupFormController', { editPlanningGroup: null });
		vm.searchString = "skill";
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);
		vm.name = 'Plan Group';
		vm.persist();
		$httpBackend.flush();

		expect($state.go).toHaveBeenCalledWith('resourceplanner.overview');
	});

	it('should load edit planning group', function () {
		var vm = $controller('planningGroupFormController', { $stateParams: stateparams, editPlanningGroup: editPlanningGroup });

		expect(vm.editPlanningGroup.Id).toEqual('aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e');
	});

	it('should save new name for selected edit planning group', function () {
		spyOn(planningGroupService, 'savePlanningGroup').and.callThrough();
		var vm = $controller('planningGroupFormController', { $stateParams: stateparams, editPlanningGroup: editPlanningGroup });
		
		var id = vm.editPlanningGroup.Id;
		var filter = vm.editPlanningGroup.Filters;
		var settings  = vm.editPlanningGroup.Settings;
		var preferencePercent = vm.editPlanningGroup.PreferencePercent;
		vm.name = 'Plan Group 3';
		vm.persist();
		$httpBackend.flush();

		expect(planningGroupService.savePlanningGroup).toHaveBeenCalledWith({ Id: id, Name: vm.name, Filters: filter, Settings: settings, PreferencePercent: preferencePercent });
		expect($state.go).toHaveBeenCalledWith('resourceplanner.overview');
	});

	it('should delete selected planning group', function () {
		spyOn(planningGroupService, 'removePlanningGroup').and.callThrough();
		var vm = $controller('planningGroupFormController', { $stateParams: stateparams, editPlanningGroup: editPlanningGroup });

		var id = vm.editPlanningGroup.Id;
		var filter = vm.editPlanningGroup.Filters;
		vm.name = 'Plan Group 3';
		vm.removePlanningGroup(id);
		$httpBackend.flush();

		expect(planningGroupService.removePlanningGroup).toHaveBeenCalledWith({ id: id });
		expect($state.go).toHaveBeenCalledWith('resourceplanner.overview');
	});

	it('should save planning group settings', function() {
		spyOn(planningGroupService, 'savePlanningGroup').and.callThrough();
		var vm = $controller('planningGroupFormController', { $stateParams: stateparams, editPlanningGroup: editPlanningGroup });

		for (var i = 0; i <vm.editPlanningGroup.Settings.length; i++) {
			if(vm.editPlanningGroup.Settings[i].Default){
				vm.editPlanningGroup.Settings[i].BlockFinderType = 1;
				vm.editPlanningGroup.Settings[i].BlockSameShiftCategory = true;
				vm.editPlanningGroup.Settings[i].BlockSameStartTime = false;
				vm.editPlanningGroup.Settings[i].BlockSameShift = false;
				vm.editPlanningGroup.Settings[i].MinDayOffsPerWeek = 2;
				vm.editPlanningGroup.Settings[i].MaxDayOffsPerWeek = 4;
				vm.editPlanningGroup.Settings[i].MinConsecutiveWorkdays = 1;
				vm.editPlanningGroup.Settings[i].MaxConsecutiveWorkdays = 5;
				vm.editPlanningGroup.Settings[i].MinConsecutiveDayOffs = 3;
				vm.editPlanningGroup.Settings[i].MaxConsecutiveDayOffs = 3;
				vm.editPlanningGroup.Settings[i].MinFullWeekendsOff = 1;
				vm.editPlanningGroup.Settings[i].MaxFullWeekendsOff = 4;
				vm.editPlanningGroup.Settings[i].MinWeekendDaysOff = 2;
				vm.editPlanningGroup.Settings[i].MaxWeekendDaysOff = 3;
			}
		}

		vm.persist();
		$httpBackend.flush();
		
		expect(planningGroupService.savePlanningGroup).toHaveBeenCalledWith({ Id: vm.editPlanningGroup.Id, Name: vm.name, Filters: vm.editPlanningGroup.Filters, Settings: vm.editPlanningGroup.Settings, PreferencePercent: vm.editPlanningGroup.PreferencePercent });

		expect($state.go).toHaveBeenCalledWith('resourceplanner.overview');
	});

	it('navigate to previous view after saving planning group settings', function() {
		spyOn(planningGroupService, 'savePlanningGroup').and.callThrough();
		var vm = $controller('planningGroupFormController', { $stateParams: {groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', planningPeriodId: 'a557210b-99cc-4128-8ae0-138d812974b6'}, editPlanningGroup: editPlanningGroup });

		vm.persist();
		$httpBackend.flush();

		expect(planningGroupService.savePlanningGroup).toHaveBeenCalledWith({ Id: vm.editPlanningGroup.Id, Name: vm.name, Filters: vm.editPlanningGroup.Filters, Settings: vm.editPlanningGroup.Settings, PreferencePercent: vm.editPlanningGroup.PreferencePercent });

		expect($state.go).toHaveBeenCalledWith('resourceplanner.planningperiodoverview', {groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e', ppId: 'a557210b-99cc-4128-8ae0-138d812974b6'});
	});
});
