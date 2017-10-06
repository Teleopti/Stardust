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

        vm.data = {
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
                                            selected: false,
                                            nodes: [
                                                {
                                                    label: 'grandchild1',
                                                    id: '13',
                                                    selected: false,
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
                                                    label: 'grandchild1',
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
                        }
                    ]
                }
            ]
        };

    }
})();
