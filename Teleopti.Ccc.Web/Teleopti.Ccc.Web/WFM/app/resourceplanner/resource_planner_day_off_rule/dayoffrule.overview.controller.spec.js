'use strict';
xdescribe('dayoffRuleOverviewController', function () {
    var $httpBackend,
        $controller,
        $state,
        $injector,
        $q,
        dayOffRuleService,
        agentGroupService,
        vm;

    beforeEach(function () {
        module('wfm.resourceplanner');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_, _$state_, _$q_, _dayOffRuleService_, _agentGroupService_) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
        dayOffRuleService = _dayOffRuleService_;
        agentGroupService = _agentGroupService_;
        $q = _$q_;
        $state = _$state_;

        spyOn($state, 'go');
        // spyOn(dayOffRuleService, 'getFilterData').and.callThrough();

        vm = $controller('dayoffRuleOverviewController');

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
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should get agent group information by groupId', function () {
        vm.searchString = "skill";
        vm.inputFilterData();
        $httpBackend.flush();

        expect(dayOffRuleService.getFilterData).toHaveBeenCalled();
    });

});
