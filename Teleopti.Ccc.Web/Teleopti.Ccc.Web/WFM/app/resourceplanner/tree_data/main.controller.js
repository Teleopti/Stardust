(function () {
    'use strict';

    angular
        .module('wfm.resourceplanner')
        .controller('TreeMainController', Controller);

    Controller.$inject = ['$state'];

    function Controller($state) {
        var vm = this;

        vm.option = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected"
        }

        vm.optionTwo = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected",
            RootSelectUnique: true
        }

        vm.optionThree = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected",
            nodeSemiSelected: "semiSelected",
            RootSelectUnique: true
        }


        var data = {
            nodes: [
                {
                    label: 'parent1',
                    id: '1',
                    selected: false,
                    nodes: [
                        {
                            label: 'child1',
                            id: '2',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '3',
                                    selected: false,
                                    nodes: [
                                        {
                                            label: 'child1',
                                            id: '12',
                                            selected: true,
                                            nodes: [
                                                {
                                                    label: 'grandchild1',
                                                    id: '13',
                                                    selected: true,
                                                    nodes: []
                                                }
                                            ]
                                        },
                                        {
                                            label: 'child1',
                                            id: '22',
                                            selected: false,
                                            nodes: [
                                                {
                                                    label: 'grandchild2',
                                                    id: '23',
                                                    selected: false,
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
                    nodes: [
                        {
                            label: 'child1',
                            id: '5',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '6',
                                    selected: false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child2',
                            id: '7',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child3',
                            id: '7',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild1',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild3',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild4',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                },
                                {
                                    label: 'grandchild5',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child4',
                            id: '7',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                }
                            ]
                        },
                        {
                            label: 'child5',
                            id: '7',
                            selected: false,
                            nodes: [
                                {
                                    label: 'grandchild2',
                                    id: '8',
                                    selected: false,
                                    nodes: []
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        vm.dataOne = angular.copy(data);
        vm.dataTwo = angular.copy(data);
        vm.dataThree = angular.copy(data);
    }
})();
