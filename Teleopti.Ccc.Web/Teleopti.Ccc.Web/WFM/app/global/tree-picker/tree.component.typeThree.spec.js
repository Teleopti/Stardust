xdescribe('TreeDataThreeController', function () {
    var vm,
        $controller,
        $compile,
        $rootScope,
        attachedElements = [],
        data,
        picker,
        componentData,
        componentOption,
        rootSelectUnique,
        option = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected",
            NodeSemiSelected: "semiSelected"        
        };

    beforeEach(function () {
        module('wfm.templates', 'wfm.treePicker');
        inject(function (_$controller_, _$compile_, _$rootScope_) {
            $controller = _$controller_;
            $compile = _$compile_;
            $rootScope = _$rootScope_;
        });
        data = {
            nodes: [
                {
                    label: 'parent1',
                    id: '1',
                    selected: false,
                    semiSelected:false,
                    nodes: [
                        {
                            label: 'child1',
                            id: '2',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '3',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: [
                                        {
                                            label: 'child1',
                                            id: '12',
                                            selected: false,
                                            semiSelected:false,
                                            nodes: [
                                                {
                                                    label: 'grandchild1',
                                                    id: '13',
                                                    selected: false,
                                                    semiSelected:false,
                                                    nodes: []
                                                }
                                            ]
                                        },
                                        {
                                            label: 'child2',
                                            id: '22',
                                            selected: false,
                                            semiSelected:false,
                                            nodes: [
                                                {
                                                    label: 'grandchild2',
                                                    id: '23',
                                                    selected: false,
                                                    semiSelected:false,
                                                    nodes: []
                                                }
                                            ]
                                        }
                                    ]
                                }
                            ]
                        }
                    ]
                },
                {
                    label: 'parent2',
                    id: '4',
                    selected: false,
                    semiSelected:false,
                    nodes: [
                        {
                            label: 'child1',
                            id: '5',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '6',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child2',
                            id: '7',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child3',
                            id: '7',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild3',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild4',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild5',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child4',
                            id: '7',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child5',
                            id: '7',
                            selected: false,
                            semiSelected:false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    semiSelected:false,
                                    nodes: []
                                }
                            ]
                        }
                    ]
                }
            ]
        }

        $rootScope.data = data;
        $rootScope.option = option;
        picker = setupPicker('data="data" option="option"');
        componentData = picker.find('div').scope().vm.data;
        componentOption = picker.find('div').scope().vm.option;
    });

    afterEach(function () {
        attachedElements.forEach(function (element) {
            var scope = element.scope();
            scope && scope.$destroy();
            element.remove();
        });
        attachedElements = [];
    });

    function setupPicker(attrs, scope, optCompileOpts) {
        var el;
        var template = '' +
            '<tree-data-three ' + (attrs || '') + '>' +
            '</tree-data-three>';

        el = $compile(template)(scope || $rootScope);
        $rootScope.$digest();
        attachedElements.push(el);

        return el;
    }

    function mapParentIndex(item, indexList) {
        if (angular.isUndefined(item))
            return;
        if (indexList == null) {
            var indexList = [];
        }
        if (angular.isDefined(item.$parent.$index)) {
            indexList.splice(0, 0, item.$parent.$index);
            mapParentIndex(item.$parent.$parent, indexList)
        }
        return indexList;
    }

    it('should prepare data form other controller to component', function () {
        expect(componentData).toEqual(data);
    });

    it('should prepare options form other controller to component', function () {
        expect(componentOption).toEqual(option);
    });

    it('should select a node', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[0].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;

        expect(componentSelectedNode.$parent.node[componentOption.NodeSelectedMark]).toEqual(true);
    });

    it('should unselect a node', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[0].click();
        nodes[0].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;

        expect(componentSelectedNode.$parent.node[componentOption.NodeSelectedMark]).toEqual(false);
    });

    xit('should select the clicked node and its parents when node is single child', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[4].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        expect(data.nodes[mapParents[0]].selected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].selected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].selected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].nodes[mapParents[4]].selected).toEqual(true);    
    });

    it('should unselect everything when unselecting a parent with only one child', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[4].click();
        nodes[3].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        expect(data.nodes[mapParents[0]].selected).toEqual(false);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(false);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].selected).toEqual(false);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].selected).toEqual(false);      
        expect(componentSelectedNode.$parent.node[componentOption.NodeSelectedMark]).toEqual(false);
    });

    xit('should select parents when all children are selected', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[13].click();
        nodes[14].click();
        nodes[15].click();
        nodes[16].click();
        nodes[17].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        expect(data.nodes[mapParents[0]].selected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(true);    
    });

    it('should unselect all parents when all children are unselected', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[13].click();
        nodes[14].click();
        nodes[15].click();
        nodes[16].click();
        nodes[17].click();
        nodes[13].click();
        nodes[14].click();
        nodes[15].click();
        nodes[16].click();
        nodes[17].click();

        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        expect(data.nodes[mapParents[0]].selected).toEqual(false);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(false);    
    });

    it('should select all children when parent is selected', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[12].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);
 
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[0].selected).toEqual(true);  
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[1].selected).toEqual(true);  
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[2].selected).toEqual(true);
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[3].selected).toEqual(true);
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[4].selected).toEqual(true);
    });

    
    it('should unselect all children when parent is unselected', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[12].click();
        nodes[12].click();
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);
 
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[0].selected).toEqual(false);  
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[1].selected).toEqual(false);  
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[2].selected).toEqual(false);
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[3].selected).toEqual(false);
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[4].selected).toEqual(false);
    });

    xit('should semi select parents when some children are selected', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[13].click();
        nodes[14].click();

        // console.log('clicked nodes', nodes[13], nodes[14], 'componentSelectedNode', componentSelectedNode);
        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        // console.log('componentSelectedNode', componentSelectedNode);
 
        // console.log('parents that should be semiselected', data.nodes[mapParents[0]], data.nodes[mapParents[0]].nodes[mapParents[1]]);
        expect(data.nodes[mapParents[0]].semiSelected).toEqual(true);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].semiSelected).toEqual(true); 
    });

 // this looks like a wrong test - clicking node 8 should select that node(child1) and it's children(node[9] = grandchild1). Clicking twice on grandchild1 will first dselect it and then select it again
    // it('should keep parent selected when unselecting a sibling child', function () {
    //     var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
    //     nodes[8].click();
    //     nodes[9].click();
    //     nodes[9].click();

    //     var componentSelectedNode = picker.find('div').scope().vm.node;
    //     var mapParents = mapParentIndex(componentSelectedNode);

    //     console.log('hej', data.nodes[mapParents[0]].nodes[1], data.nodes[mapParents[0]].nodes[1].nodes[0]);

    //     expect(data.nodes[mapParents[0]].selected).toEqual(true);    
    //     expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(true);    
    //     expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].selected).toEqual(true);     
    //     expect(data.nodes[mapParents[0]].nodes[1].selected).toEqual(false);    
    //     expect(data.nodes[mapParents[0]].nodes[1].nodes[0].selected).toEqual(false);       
    // });

    it('should not select siblings', function () {
        var nodes = picker[0].getElementsByClassName('tree-handle-wrapper');
        nodes[3].click();

        var componentSelectedNode = picker.find('div').scope().vm.node;
        var mapParents = mapParentIndex(componentSelectedNode);

        // console.log('nodes', nodes, 'clicking', nodes[3], data.nodes[mapParents[0]], data.nodes[mapParents[0]].nodes[mapParents[1]], data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]], data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]], data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].nodes[0], data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[1], data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[1].nodes[0])
        //Are the commented out asserts below necessary?
        // expect(data.nodes[mapParents[0]].selected).toEqual(true);    
        // expect(data.nodes[mapParents[0]].nodes[mapParents[1]].selected).toEqual(true);    
        // expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].selected).toEqual(true);     
        // expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].selected).toEqual(true);     
        // //missing one expect on child of the clicked one?
        // expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[mapParents[3]].nodes[0].selected).toEqual(true); 
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[1].selected).toEqual(false);    
        expect(data.nodes[mapParents[0]].nodes[mapParents[1]].nodes[mapParents[2]].nodes[1].nodes[0].selected).toEqual(false);       
    });
});
