(function() {
    'use strict';
    var start = angular.module('wfm.start', []);

    start.controller("FeedCtrl", [
        '$scope',
        function($scope) {
            $scope.data = {
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
            }

        }
    ]);


})();
