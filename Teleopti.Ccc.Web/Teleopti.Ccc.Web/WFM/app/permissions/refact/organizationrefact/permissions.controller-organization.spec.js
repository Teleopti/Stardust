'use strict';
//DONT REMOVE X
xdescribe('PermissionsCtrlRefact', function () {
    var $httpBackend,
        fakeBackend,
        $controller,
        vm;

    beforeEach(function () {
        module('wfm.permissions');
    });

    beforeEach(inject(function (_$httpBackend_, _fakePermissionsBackend_, _$controller_) {
        $httpBackend = _$httpBackend_;
        fakeBackend = _fakePermissionsBackend_;
        $controller = _$controller_;

        fakeBackend.clear();
        vm = $controller('PermissionsCtrlRefact');
    }));


    it('should get a organization selection', function () {
        var BusinessUnit =  {
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
        fakeBackend.withOrganizationSelection(BusinessUnit,DynamicOptions);

        $httpBackend.flush();

        expect(vm.organizationSelection.BusinessUnit.ChildNodes).toEqual([]);
        expect(vm.organizationSelection.BusinessUnit.Id).toEqual("928dd0bc-bf40-412e-b970-9b5e015aadea");
        expect(vm.organizationSelection.BusinessUnit.Name).toEqual("TeleoptiCCCDemo");
        expect(vm.organizationSelection.BusinessUnit.Type).toEqual("BusinessUnit");
        expect(vm.organizationSelection.DynamicOptions[0].RangeOption).toEqual(0);
        expect(vm.organizationSelection.DynamicOptions[0].Name).toEqual("None");
    });

});