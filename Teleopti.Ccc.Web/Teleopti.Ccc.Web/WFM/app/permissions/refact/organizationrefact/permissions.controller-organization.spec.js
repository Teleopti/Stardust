'use strict';
//DONT REMOVE X
xdescribe('component: permissionsList', function () {
    var $httpBackend,
        fakeBackend,
        $controller,
        $componentController,
        ctrl,
        vm;

    beforeEach(function () {
        module('wfm.permissions');
    });

    beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$componentController_, _$controller_) {
        $httpBackend = _$httpBackend_;
        fakeBackend = _fakePermissionsBackend_;
        $componentController = _$componentController_;
        $controller = _$controller_;

        fakeBackend.clear();
        vm = $controller('PermissionsCtrlRefact');
    }));

    afterEach(function () {
        $httpBackend.verifyNoOutstandingExpectation();
        $httpBackend.verifyNoOutstandingRequest();
    });

    it('should be able to select BusinessUnit', function () {
        var BusinessUnit = {
            ChildNodes: [],
            Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
            Name: "TeleoptiCCCDemo",
            Type: "BusinessUnit"
        };
        var DynamicOptions = [
            {
                RangeOption: 0,
                Name: "None"
            }
        ];
        fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
        $httpBackend.flush();
        ctrl = $componentController('permissionsList', null, { bu: vm.organizationSelection.ChildNodes });

        ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

        expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
    });

    it('should be able to deselect BusinessUnit', function () {
        var BusinessUnit = {
            ChildNodes: [],
            Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
            Name: "TeleoptiCCCDemo",
            Type: "BusinessUnit",
            IsSelected: true
        };
        var DynamicOptions = [
            {
                RangeOption: 0,
                Name: "None"
            }
        ];
        fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
        $httpBackend.flush();
        ctrl = $componentController('permissionsList', null, { bu: vm.organizationSelection.ChildNodes });

        ctrl.toggleNode(vm.organizationSelection.BusinessUnit);

        expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(false);
    });

    it('should select businessunit when selecting site', function () {
        var BusinessUnit = {
            ChildNodes: [{
                ChildNodes: [],
                Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                Name: "London",
                Type: "Site"
            }],
            Id: "928dd0bc-bf40-412e-b970-9b5e015aadea",
            Name: "TeleoptiCCCDemo",
            Type: "BusinessUnit"
        };
        var DynamicOptions = [
            {
                RangeOption: 0,
                Name: "None"
            }
        ];
        fakeBackend.withOrganizationSelection(BusinessUnit, DynamicOptions);
        $httpBackend.flush();
        ctrl = $componentController('permissionsList', null, { bu: vm.organizationSelection.ChildNodes });

        ctrl.toggleNode(vm.organizationSelection.BusinessUnit.ChildNodes[0]);

        expect(vm.organizationSelection.BusinessUnit.IsSelected).toEqual(true);
    });

});