(function() {
    'use strict';
    var start = angular.module('wfm.start', []);

    start.controller("FeedCtrl", [
        '$scope',
        function($scope) {
            $scope.data = {
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
            }

        }
    ]);
})();
