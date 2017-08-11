(function () {
    'use strict';
    var start = angular.module('wfm.start', []);

    start.controller("FeedCtrl", [
        '$scope',
        function ($scope) {
            $scope.data = {
                nodes: [
                    {
                        name: 'parent1',
                        id: '1',
                        nodes: [
                            {
                                name: 'child1',
                                id: '2',
                                nodes: [
                                    {
                                        name: 'grandchild1',
                                        id: '3',
                                        nodes: []
                                    }
                                ]
                            }
                        ]
                    },
                    {
                        name: 'parent2',
                        id: '4',
                        nodes: [
                            {
                                name: 'child1',
                                id: '5',
                                nodes: [
                                    {
                                        name: 'grandchild1',
                                        id: '6',
                                        nodes: []
                                    }
                                ]
                            },
                            {
                                name: 'child2',
                                id: '7',
                                nodes: [
                                    {
                                        name: 'grandchild2',
                                        id: '8',
                                        nodes: []
                                    }
                                ]
                            }
                        ]
                    }
                ]
            }

            $scope.output = [];

            $scope.$watchCollection('data', function (newValue, oldValue) {
                console.log('new', newValue)
                console.log('old', oldValue)
            })
        }
    ]);

})();

