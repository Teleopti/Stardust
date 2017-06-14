'use strict';
describe('dayoffRuleCreateController', function () {
    var $httpBackend,
        $controller,
        $state,
        $injector,
        dayOffRuleService,
        debounceService,
        stateparams = { groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e' },
        stateparamsForDefaultDo = {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            filterId: '33f52ff4-0314-4a9e-80fa-5c958c57c92f',
            isDefault: true
        },
        stateparamsForUndefaultDo = {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            filterId: '8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb',
            isDefault: false
        };

    beforeEach(function () {
        module('wfm.resourceplanner');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _dayOffRuleService_, _debounceService_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        dayOffRuleService = _dayOffRuleService_;
        debounceService = _debounceService_;
        $state = _$state_;

        spyOn($state, 'go');
        spyOn(debounceService, 'debounce').and.callFake(function (cb) { return function () { cb(); } });
        spyOn(dayOffRuleService, 'getFilterData').and.callThrough();

        $httpBackend.whenGET(/.*?api\/filters\?.*/).respond(function (method, url, data, headers) {
            return [200, [{
                Id: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
                Name: "Skill 1",
                FilterType: "Skill"
            }, {
                Id: 'a98d2c45-a8f4-4c70-97f9-907ab364af75',
                Name: "Skill 2",
                FilterType: "Skill"
            }], {}];
        });

        $httpBackend.whenPOST('../api/resourceplanner/dayoffrules').respond(function (method, url, data, headers) {
            return [200, true];
        });

        $httpBackend.whenGET('../api/resourceplanner/dayoffrules/33f52ff4-0314-4a9e-80fa-5c958c57c92f').respond(function (method, url, data, headers) {
            return [200, {
                Id: '33f52ff4-0314-4a9e-80fa-5c958c57c92f',
                PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                Name: "Default",
                MinDayOffsPerWeek: 1,
                MaxDayOffsPerWeek: 3,
                MinConsecutiveWorkdays: 2,
                MaxConsecutiveWorkdays: 6,
                MinConsecutiveDayOffs: 1,
                MaxConsecutiveDayOffs: 3,
                Filters: []
            }, {}];
        });

        $httpBackend.whenGET('../api/resourceplanner/dayoffrules/8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb').respond(function (method, url, data, headers) {
            return [200, {
                Id: '8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb',
                PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
                Name: "Day off rule 101",
                Default: false,
                MinDayOffsPerWeek: 1,
                MaxDayOffsPerWeek: 3,
                MinConsecutiveWorkdays: 2,
                MaxConsecutiveWorkdays: 6,
                MinConsecutiveDayOffs: 1,
                MaxConsecutiveDayOffs: 3,
                Filters: [{
                    Id: "79c466f0-cbe8-4209-b949-9b5e015b23f7",
                    FilterType: "contract",
                    Name: "Full time Fixed staff"
                }]
            }, {}];
        });
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should call function with debounce 250', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        expect(dayOffRuleService.getFilterData).toHaveBeenCalled();
    });

    it('should get filter results', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        expect(vm.filterResults.length).toEqual(2);
        expect(vm.filterResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
        expect(vm.filterResults[1].Id).toEqual('a98d2c45-a8f4-4c70-97f9-907ab364af75');
    });

    it('should add one filter from filter results', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        vm.selectResultItem(vm.filterResults[0]);

        expect(vm.selectedResults.length).toEqual(1);
        expect(vm.selectedResults[0].Id).toEqual('0ffeb898-11bf-43fc-8104-9b5e015ab3c2');
    });

    it('should remove one filter from filter results', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        vm.selectResultItem(vm.filterResults[0]);
        vm.removeSelectedFilter(vm.selectedResults[0]);

        expect(vm.selectedResults.length).toEqual(0);
    });

    it('should not create day off rule when submit data is invalid', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.persist();

        expect($state.go).not.toHaveBeenCalledWith('resourceplanner.dayoffrulesoverview');
    });

    it('should create new day off rule when submit data is valid', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparams });
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        vm.selectResultItem(vm.filterResults[0]);
        vm.name = 'New day off rule';
        vm.persist();
        $httpBackend.flush();

        expect($state.go).toHaveBeenCalledWith('resourceplanner.dayoffrulesoverview', {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e'
        });
    });

    it('should load selected undefault day off rule', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparamsForUndefaultDo });
        $httpBackend.flush();

        expect(vm.filterId).toEqual('8c6dd6f6-37d0-4135-9fdd-491b1f8b12fb');
        expect(vm.name).toEqual('Day off rule 101');
        expect(vm.default).toEqual(false);
    });

    it('should save new name for selected undefault day off rule', function () {
        spyOn(dayOffRuleService, 'saveDayOffRule').and.callThrough();
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparamsForUndefaultDo });
        $httpBackend.flush();

        vm.name = 'Day off rule 911';
        vm.persist();
        $httpBackend.flush();

        expect(dayOffRuleService.saveDayOffRule).toHaveBeenCalledWith({
            Id: vm.filterId,
            Name: 'Day off rule 911',
            Default: vm.default,
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Filters: vm.selectedResults,
            MinDayOffsPerWeek: 1,
            MaxDayOffsPerWeek: 3,
            MinConsecutiveWorkdays: 2,
            MaxConsecutiveWorkdays: 6,
            MinConsecutiveDayOffs: 1,
            MaxConsecutiveDayOffs: 3
        });

        expect($state.go).toHaveBeenCalledWith('resourceplanner.dayoffrulesoverview', {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e'
        });
    });

    it('should save new setting for selected undefault day off rule', function () {
        spyOn(dayOffRuleService, 'saveDayOffRule').and.callThrough();
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparamsForUndefaultDo });
        $httpBackend.flush();

        vm.dayOffsPerWeek = {
            MinDayOffsPerWeek: 2,
            MaxDayOffsPerWeek: 4
        };
        vm.consecDaysOff = {
            MinConsecDaysOff: 3,
            MaxConsecDaysOff: 3
        };
        vm.consecWorkDays = {
            MinConsecWorkDays: 1,
            MaxConsecWorkDays: 5
        };

        vm.persist();
        $httpBackend.flush();

        expect(dayOffRuleService.saveDayOffRule).toHaveBeenCalledWith({
            Id: vm.filterId,
            Name: vm.name,
            Default: vm.default,
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Filters: vm.selectedResults,
            MinDayOffsPerWeek: 2,
            MaxDayOffsPerWeek: 4,
            MinConsecutiveWorkdays: 1,
            MaxConsecutiveWorkdays: 5,
            MinConsecutiveDayOffs: 3,
            MaxConsecutiveDayOffs: 3
        });

        expect($state.go).toHaveBeenCalledWith('resourceplanner.dayoffrulesoverview', {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e'
        });
    });


    it('should load default day off rule', function () {
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparamsForDefaultDo });
        $httpBackend.flush();

        expect(vm.filterId).toEqual('33f52ff4-0314-4a9e-80fa-5c958c57c92f');
        expect(vm.name).toEqual('Default');
        expect(vm.default).toEqual(true);
    });

    it('should save new setting for default day off rule', function () {
        spyOn(dayOffRuleService, 'saveDayOffRule').and.callThrough();
        var vm = $controller('dayoffRuleCreateController', { $stateParams: stateparamsForDefaultDo });
        $httpBackend.flush();

        vm.dayOffsPerWeek = {
            MinDayOffsPerWeek: 2,
            MaxDayOffsPerWeek: 4
        };
        vm.consecDaysOff = {
            MinConsecDaysOff: 3,
            MaxConsecDaysOff: 3
        };
        vm.consecWorkDays = {
            MinConsecWorkDays: 1,
            MaxConsecWorkDays: 5
        };

        vm.persist();
        $httpBackend.flush();

        expect(vm.default).toEqual(true);
        expect(dayOffRuleService.saveDayOffRule).toHaveBeenCalledWith({
            Id: vm.filterId,
            Name: vm.name,
            Default: vm.default,
            PlanningGroupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e',
            Filters: vm.selectedResults,
            MinDayOffsPerWeek: 2,
            MaxDayOffsPerWeek: 4,
            MinConsecutiveWorkdays: 1,
            MaxConsecutiveWorkdays: 5,
            MinConsecutiveDayOffs: 3,
            MaxConsecutiveDayOffs: 3
        });

        expect($state.go).toHaveBeenCalledWith('resourceplanner.dayoffrulesoverview', {
            groupId: 'aad945dd-be2c-4c6a-aa5b-30f3e74dfb5e'
        });
    });
});
