'use strict';

fdescribe('treePickerComponent', function () {
    var
        $componentController,
        ctrl,
        mockedData,
        badMockedData;

    beforeEach(function () {
        module('wfm.treePicker');
    });

    beforeEach(inject(function (_$componentController_) {
        $componentController = _$componentController_;

        mockedData = {
            parents: [
                {
                    name: 'parent1',
                    children: [
                        {
                            name: 'child1',
                            children: [
                                {
                                    name: 'grandchild1',
                                    children: []
                                }
                            ]
                        }
                    ]
                },
                {
                    name: 'parent2',
                    children: [
                        {
                            name: 'child1',
                            children: [
                                {
                                    name: 'grandchild1',
                                    children: []
                                }
                            ]
                        },
                        {
                            name: 'child2',
                            children: [
                                {
                                    name: 'grandchild2',
                                    children: []
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        badMockedData = {
            topNode: [
                {
                    name: 'parent1',
                    childNodes: {}
                }
            ]
        };

    }));

    it('should only get correct data structure', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit()

        expect(ctrl.validData).not.toBe(undefined);
    });

    it('should throw error on icorrect data structure', function () {
        ctrl = $componentController('treePicker', null, {
            data: badMockedData
        });

        expect(function() { ctrl.$onInit()}).toThrow(new Error('fix the data jao'));
        expect(ctrl.validData).toBe(undefined);

    });
});
