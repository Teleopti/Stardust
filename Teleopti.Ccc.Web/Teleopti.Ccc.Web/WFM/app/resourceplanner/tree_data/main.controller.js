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
            NodeSelectedMark: "selected",
            DisplayTreeFilter: true
        }

        vm.optionTwo = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected",
            RootSelectUnique: true,
            DisplayTreeFilter: true
        }

        vm.optionThree = {
            NodeDisplayName: "label",
            NodeChildrenName: "nodes",
            NodeSelectedMark: "selected",
            nodeSemiSelected: "semiSelected",
            RootSelectUnique: true,
            DisplayTreeFilter: true
        }

        vm.optionFour = {
            NodeDisplayName: "Name",
            NodeChildrenName: "Children",
            NodeSelectedMark: "selected",
            nodeSemiSelected: "semiSelected",
            DisplayTreeFilter: true
        }


        var data = {
            nodes: [
                {
                    label: 'parent1 中国',
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
                                            label: 'child1 中国',
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
                                                    label: 'grandchild2 中国',
                                                    id: '23',
                                                    selected: false,
                                                    nodes: []
                                                }
                                            ]
                                        }
                                    ]
                                },
                                {
                                    label: 'grandchild30',
                                    id: '33',
                                    selected: false,
                                    nodes: [
                                        {
                                            label: 'child31',
                                            id: '12',
                                            selected: false,
                                            nodes: [
                                                {
                                                    label: 'grandchild312',
                                                    id: '13',
                                                    selected: false,
                                                    nodes: []
                                                }
                                            ]
                                        },
                                        {
                                            label: 'child32',
                                            id: '22',
                                            selected: false,
                                            nodes: [
                                                {
                                                    label: 'grandchild321',
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

        var org = {
            Children: [
                {
                    Children: [
                        {
                            Name: "BTS",
                            Id: "9d013613-7c79-4621-b166-a39a00b9d634"
                        }
                    ],
                    Id: "7a6c0754-4de8-48fb-8aee-a39a00b9d1c3",
                    Name: "BTS"
                },
                {
                    Children: [
                        {
                            Name: "Denver 1",
                            Id: "a5cee4f9-5ca0-464c-8728-a7ba00f808a6"
                        },
                        {
                            Name: "Denver 2",
                            Id: "6a7b26a1-c4ad-4a8c-aa62-a7bc00d6ea90"
                        }
                    ],
                    Id: "6e0fad0e-5548-458f-8c58-a7ba00f7f2c3",
                    Name: "Denver"
                },
                {
                    Children: [
                        {
                            Name: "Dubai 1",
                            Id: "3f6236f8-dfb0-4b44-93ed-a7ba00f816bf"
                        }
                    ],
                    Id: "6bf99381-0110-4866-a3f6-a7ba00f7fdd4",
                    Name: "Dubai"
                },
                {
                    Children: [
                        {
                            Name: "Students",
                            Id: "e5f968d7-6f6d-407c-81d5-9b5e015ab495"
                        },
                        {
                            Name: "Team Flexible",
                            Id: "d7a9c243-8cd8-406e-9889-9b5e015ab495"
                        },
                        {
                            Name: "Team Outbound",
                            Id: "a74e1f94-7662-4a7f-9746-a56e00a66f17"
                        },
                        {
                            Name: "Team Preferences",
                            Id: "34590a63-6331-4921-bc9f-9b5e015ab495"
                        },
                        {
                            Name: "Team Rotations",
                            Id: "e7ce8892-4db3-49c8-bdf6-9b5e015ab495"
                        }
                    ],
                    Id: "d970a45a-90ff-4111-bfe1-9b5e015ab45c",
                    Name: "London"
                },
                {
                    Children: [
                        {
                            Name: "Manila 1",
                            Id: "3b7851b2-020f-474b-a00b-a7ba00f81d87"
                        }
                    ],
                    Id: "992217bc-86f8-47fb-bef2-a7ba00f802c0",
                    Name: "Manila"
                },
                {
                    Children: [
                        {
                            Name: "Moscow 1",
                            Id: "11aaa765-70e4-49c3-a085-a7ba00f80f9b"
                        }
                    ],
                    Id: "afe5efa8-531a-491c-9141-a7ba00f7f780",
                    Name: "Moscow"
                },
                {
                    Children: [
                        {
                            Name: "Shenzhen 1",
                            Id: "c1899467-c8ef-4b02-adb3-a7ba00f841dd"
                        }
                    ],
                    Id: "cb6ad6f8-f9fb-484a-a791-a7ba00f83bad",
                    Name: "Shenzhen"
                },
                {
                    Children: [
                        {
                            Name: "Stockholm 1",
                            Id: "b923caf7-c199-4a46-8f72-a7bb00e7ec00"
                        },
                        {
                            Name: "Team Nights Rotation",
                            Id: "ccca512d-33d5-4a36-8296-a7bc00fea698"
                        }
                    ],
                    Id: "3c0b7719-557d-4b3a-b349-a7bb00e7e198",
                    Name: "Stockholm"
                },
                {
                    Children: [
                        {
                            Name: "London North",
                            Id: "c61d59b7-300a-4bb3-a194-a0a200da318a"
                        },
                        {
                            Name: "London South",
                            Id: "d1b49334-ca3d-4572-bff3-a0a200f9e3fd"
                        },
                        {
                            Name: "New York 5:th Ave",
                            Id: "37418d2a-9650-417a-8212-a0a200da28be"
                        },
                        {
                            Name: "New York Broadway",
                            Id: "5f809a09-cf69-4040-bcc1-a0a200da1fc4"
                        },
                        {
                            Name: "New York Soho",
                            Id: "518e7013-3540-4536-8a09-a0a200f9ef71"
                        },
                        {
                            Name: "Tokyo Ginza",
                            Id: "143e088c-b6f4-453c-9656-a0a200da099a"
                        }
                    ],
                    Id: "413157c4-74a9-482c-9760-a0a200d9f90f",
                    Name: "Stores"
                }
            ]
        };

        vm.dataOne = angular.copy(data);
        vm.dataTwo = angular.copy(data);
        vm.dataThree = angular.copy(data);
        vm.dataFour = angular.copy(org);

        vm.getChange = function () {
           
        };
    }
})();
