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

    it('should add selection method to each node', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();

        expect(typeof ctrl.isolatedData.nodes[0].selectNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[1].selectNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[0].nodes[0].selectNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[1].nodes[0].selectNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[0].nodes[0].nodes[0].selectNode).toBe('function');
    });

      it('should add open method to each node that has children', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();

        expect(typeof ctrl.isolatedData.nodes[0].openNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[1].openNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[0].nodes[0].openNode).toBe('function');
        expect(typeof ctrl.isolatedData.nodes[1].nodes[0].openNode).toBe('function');
        expect(ctrl.isolatedData.nodes[0].nodes[0].nodes[0].openNode).toBe(undefined);
    });

    it('should add isSelectedInUI property to each node', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();

        expect(ctrl.isolatedData.nodes[0].isSelectedInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[1].isSelectedInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[0].nodes[0].isSelectedInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[1].nodes[0].isSelectedInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[0].nodes[0].nodes[0].isSelectedInUI).toBe(false);
    });

    it('should add  isOpenInUI property to each node', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();

        expect(ctrl.isolatedData.nodes[0].isOpenInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[1].isOpenInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[0].nodes[0].isOpenInUI).toBe(false);
        expect(ctrl.isolatedData.nodes[1].nodes[0].isOpenInUI).toBe(false);
    });

    it('should see parent selected', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].selectNode(ctrl.isolatedData.nodes[0]);

        expect(ctrl.isolatedData.nodes[0].isSelectedInUI).toBe(true);
    });

    it('should see parent unselected', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].selectNode(ctrl.isolatedData.nodes[0]);
        ctrl.isolatedData.nodes[0].selectNode(ctrl.isolatedData.nodes[0]);

        expect(ctrl.isolatedData.nodes[0].isSelectedInUI).toBe(false);
    });

    it('should see grandchild selected', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].nodes[0].nodes[0].selectNode(ctrl.isolatedData.nodes[0].nodes[0].nodes[0]);

        expect(ctrl.isolatedData.nodes[0].nodes[0].nodes[0].isSelectedInUI).toBe(true);
    });

    it('should see grandchild unselected', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].nodes[0].nodes[0].selectNode(ctrl.isolatedData.nodes[0].nodes[0].nodes[0]);
        ctrl.isolatedData.nodes[0].nodes[0].nodes[0].selectNode(ctrl.isolatedData.nodes[0].nodes[0].nodes[0]);

        expect(ctrl.isolatedData.nodes[0].nodes[0].nodes[0].isSelectedInUI).toBe(false);
    });

    
    it('should see parent open', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].openNode(ctrl.isolatedData.nodes[0]);

        expect(ctrl.isolatedData.nodes[0].isOpenInUI).toBe(true);
    });

    it('should see parent closed', function () {
        ctrl = $componentController('treePicker', null, {
            data: mockedData
        });

        ctrl.$onInit();
        ctrl.isolatedData.nodes[0].openNode(ctrl.isolatedData.nodes[0]);
        ctrl.isolatedData.nodes[0].openNode(ctrl.isolatedData.nodes[0]);

        expect(ctrl.isolatedData.nodes[0].isOpenInUI).toBe(false);
    });
});
