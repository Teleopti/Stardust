'use strict';
describe('planningGroupSettingEditController', function() {
	var $httpBackend,
		$controller,
		$state,
		$injector,
		PlanGroupSettingService,
		debounceService,
		stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' },
		stateparamsForDefaultDo = {
			groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			filterId: '33f52ff4-0314-4a9e-80fa-5c958c57c92f'
		},
		stateparamsForUndefaultDo = {
			groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			filterId: '8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb'
		},
		defaultSettingInfo,
		nonDefaultSettingInfo;

	beforeEach(function() {
		module('wfm.resourceplanner');
	});

	beforeEach(inject(function(_$httpBackend_, _$controller_, _$state_, _PlanGroupSettingService_, _debounceService_) {
		$httpBackend = _$httpBackend_;
		$controller = _$controller_;
		PlanGroupSettingService = _PlanGroupSettingService_;
		debounceService = _debounceService_;
		$state = _$state_;

		spyOn($state, 'go');
		spyOn(debounceService, 'debounce').and.callFake(function(cb) {
			return function() {
				cb();
			};
		});
		spyOn(PlanGroupSettingService, 'getFilterData').and.callThrough();

		$httpBackend.whenGET(/.*?api\/filters\?.*/).respond(function(method, url, data, headers) {
			return [
				200,
				[
					{
						Id: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
						Name: 'Skill 1',
						FilterType: 'Skill'
					},
					{
						Id: 'a98d2c45-a8f4-4c70-97f9-907ab364af75',
						Name: 'Skill 2',
						FilterType: 'Skill'
					}
				],
				{}
			];
		});

		$httpBackend.whenPOST('../api/resourceplanner/plangroupsetting').respond(function(method, url, data, headers) {
			return [200, true];
		});

		defaultSettingInfo = {
			BlockFinderType: 0,
			BlockSameShift: false,
			BlockSameShiftCategory: false,
			BlockSameStartTime: false,
			Id: '33f52ff4-0314-4a9e-80fa-5c958c57c92f',
			PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			Name: 'Default',
			MinDayOffsPerWeek: 1,
			MaxDayOffsPerWeek: 3,
			MinConsecutiveWorkdays: 2,
			MaxConsecutiveWorkdays: 6,
			MinConsecutiveDayOffs: 1,
			MaxConsecutiveDayOffs: 3,
			MinFullWeekendsOff: 0,
			MaxFullWeekendsOff: 8,
			MinWeekendDaysOff: 0,
			MaxWeekendDaysOff: 16,
			Filters: [],
			Priority: 0,
			PreferencePercent: 23,
			Default: true
		};
		
		nonDefaultSettingInfo = {
			BlockFinderType: 0,
			BlockSameShift: false,
			BlockSameShiftCategory: false,
			BlockSameStartTime: false,
			Id: '8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb',
			PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
			Name: 'Scheduling setting 101',
			Default: false,
			MinDayOffsPerWeek: 1,
			MaxDayOffsPerWeek: 3,
			MinConsecutiveWorkdays: 2,
			MaxConsecutiveWorkdays: 6,
			MinConsecutiveDayOffs: 1,
			MaxConsecutiveDayOffs: 3,
			MinFullWeekendsOff: 0,
			MaxFullWeekendsOff: 8,
			MinWeekendDaysOff: 0,
			MaxWeekendDaysOff: 16,
			Filters: [
				{
					Id: '79c466f0-cbe8-4209-b949-9b5e015b23f7',
					FilterType: 'contract',
					Name: 'Full time Fixed staff'
				}
			],
			Priority: 5,
			PreferencePercent: 23
		};
	}));

	afterEach(function() {
		$httpBackend.verifyNoOutstandingExpectation();
		$httpBackend.verifyNoOutstandingRequest();
	});

	it('should call function with debounce 250', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparams }, {settingInfo: defaultSettingInfo});
		vm.searchString = 'skill';
		vm.inputFilterData();
		$httpBackend.flush();

		expect(PlanGroupSettingService.getFilterData).toHaveBeenCalled();
	});

	it('should get filter results', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparams }, {settingInfo: defaultSettingInfo});
		vm.searchString = 'skill';
		vm.inputFilterData();
		$httpBackend.flush();

		expect(vm.filterResults.length).toEqual(2);
		expect(vm.filterResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
		expect(vm.filterResults[1].Id).toEqual('a98d2c45-a8f4-4c70-97f9-907ab364af75');
	});

	it('should display preference value', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparamsForDefaultDo}, {settingInfo: defaultSettingInfo});

		expect(vm.settingInfo.PreferencePercent).toEqual(23);
	});

	it('should add one filter from filter results', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparams }, {settingInfo: defaultSettingInfo});
		vm.searchString = 'skill';
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);

		expect(vm.settingInfo.Filters.length).toEqual(1);
		expect(vm.settingInfo.Filters[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
	});

	it('should remove one filter from filter results', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparams }, {settingInfo: defaultSettingInfo});
		vm.searchString = 'skill';
		vm.inputFilterData();
		$httpBackend.flush();

		vm.selectResultItem(vm.filterResults[0]);
		vm.removeSelectedFilter(vm.settingInfo.Filters[0]);

		expect(vm.settingInfo.Filters.length).toEqual(0);
	});

	it('should load selected undefault scheduling setting', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparamsForUndefaultDo }, {settingInfo: nonDefaultSettingInfo});

		expect(vm.settingInfo.Id).toEqual('8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb');
		expect(vm.settingInfo.Name).toEqual('Scheduling setting 101');
		expect(vm.settingInfo.Default).toEqual(false);
	});

	it('should load default scheduling setting', function() {
		var vm = $controller('planningGroupSettingEditController', { $stateParams: stateparamsForDefaultDo }, {settingInfo: defaultSettingInfo});

		expect(vm.settingInfo.Id).toEqual('33f52ff4-0314-4a9e-80fa-5c958c57c92f');
		expect(vm.settingInfo.Name).toEqual('Default');
		expect(vm.settingInfo.Default).toEqual(true);
	});
});
