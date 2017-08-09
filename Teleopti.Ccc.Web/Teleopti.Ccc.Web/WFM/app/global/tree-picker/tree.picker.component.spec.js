'use strict';

describe('treePickerComponent', function () {
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
                nodes: [
                    {
                        name: 'parent1',
                        nodes: [
                            {
                                name: 'child1',
                                nodes: [
                                    {
                                        name: 'grandchild1',
                                        nodes: []
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        name: 'parent2',
                        nodes: [
                            {
                                name: 'child1',
                                nodes: [
                                    {
                                        name: 'grandchild1',
                                        nodes: []
                                    }
                                ]
                            },
                            {
                                name: 'child2',
                                nodes: [
                                    {
                                        name: 'grandchild2',
                                        nodes: []
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

    it('', function () {

    });

   
});
