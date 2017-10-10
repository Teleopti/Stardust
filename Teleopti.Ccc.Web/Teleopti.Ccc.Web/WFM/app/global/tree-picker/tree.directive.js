(function () {
    'use strict';

    angular
        .module('wfm.treePicker')
        .controller('TreeDataOneController', TreeDataOneController)
        .controller('TreeDataTwoController', TreeDataTwoController)
        .directive('treeDataOne', treeDataOneDirective)
        .directive('treeDataTwo', treeDataTwoDirective)
        .directive('treeAnimate', treeAnimate);

    TreeDataOneController.$inject = ['$state', '$stateParams', '$translate'];

    function TreeDataOneController($state, $stateParams, $translate) {
        var vm = this;

        vm.nodeDisplayName = vm.option.NodeDisplayName ? vm.option.NodeDisplayName : "name";
        vm.nodeChildrenName = vm.option.NodeChildrenName ? vm.option.NodeChildrenName : "children";
        vm.nodeSelectedMark = vm.option.NodeSelectedMark ? vm.option.NodeSelectedMark : "mark";

        vm.selectNode = selectNode;

        function selectNode(item) {
            if (item.$parent.node[vm.nodeSelectedMark] == true) {
                if (item.$parent.node[vm.nodeChildrenName] && item.$parent.node[vm.nodeChildrenName].length !== 0) {
                    setChildrenNodesToUnselect(item.$parent.node[vm.nodeChildrenName]);
                }
                return item.$parent.node[vm.nodeSelectedMark] = false;
            }
            return setParentNodesSelectState(item.$parent, true);
        }

        function setChildrenNodesToUnselect(children) {
            children.forEach(function (child) {
                child[vm.nodeSelectedMark] = false;
                if (child[vm.nodeChildrenName] && child[vm.nodeChildrenName].length !== 0) {
                    return setChildrenNodesToUnselect(child[vm.nodeChildrenName]);
                }
            });
        }

        function setParentNodesSelectState(data, state) {
            data.node[vm.nodeSelectedMark] = state;
            if (data.$parent.$parent.node) {
                return setParentNodesSelectState(data.$parent.$parent);
            }
        }
    }

    function TreeDataTwoController($state, $stateParams, $translate) {
        var vm = this;

        vm.nodeDisplayName = vm.option.NodeDisplayName ? vm.option.NodeDisplayName : "name";
        vm.nodeChildrenName = vm.option.NodeChildrenName ? vm.option.NodeChildrenName : "children";
        vm.nodeSelectedMark = vm.option.NodeSelectedMark ? vm.option.NodeSelectedMark : "mark";
        var rootSelectUnique = vm.option.RootSelectUnique ? vm.option.RootSelectUnique : false;

        vm.selectNode = selectNode;

        function selectNode(item) {
            var state = !item.$parent.node[vm.nodeSelectedMark];
            item.$parent.node[vm.nodeSelectedMark] = state;
            if (rootSelectUnique) {
                var rootIndex = mapParentIndex(item)[0];
                setSiblingsToUnselect(vm.data[vm.nodeChildrenName], rootIndex);
            }
            if (item.$parent.$parent.$parent.node && item.$parent.$parent.$parent.node[vm.nodeChildrenName].length !== 0) {
                setParentNodesSelectState(item.$parent.$parent.$parent);
            }
            if (item.$parent.node[vm.nodeChildrenName] && item.$parent.node[vm.nodeChildrenName].length !== 0) {
                setChildrenNodesSelectState(item.$parent.node[vm.nodeChildrenName], state);
            }
            return;
        }

        function siblingsHasSelected(siblings) {
            return siblings.some(function (item) {
                return item[vm.nodeSelectedMark] == true;
            })
        }

        function setChildrenNodesSelectState(children, state) {
            children.forEach(function (child) {
                child[vm.nodeSelectedMark] = state;
                if (child[vm.nodeChildrenName] && child[vm.nodeChildrenName].length !== 0) {
                    return setChildrenNodesSelectState(child[vm.nodeChildrenName], state);
                }
            });
        }

        function setParentNodesSelectState(data) {
            var check = siblingsHasSelected(data.node[vm.nodeChildrenName]);
            data.node[vm.nodeSelectedMark] = check;
            if (data.$parent.$parent.node) {
                return setParentNodesSelectState(data.$parent.$parent);
            }
        }

        function mapParentIndex(item, indexList) {
            if (indexList == null) {
                var indexList = [];
            }
            if (angular.isDefined(item.$parent.$index)) {
                indexList.splice(0, 0, item.$parent.$index);
                mapParentIndex(item.$parent.$parent, indexList)
            }
            return indexList;
        }

        function setSiblingsToUnselect(item, index) {
            item.forEach(function (child, i) {
                if (i !== index) {
                    item[i][vm.nodeSelectedMark] = false;
                    setChildrenNodesSelectState(child[vm.nodeChildrenName], false);
                }
            });
        }
    }

    function treeAnimate() {
        var directive = {
            restrict: 'EA',
            link: linkFunc,
        };

        return directive;

        function linkFunc(scope, element, attrs, ctrl) {
            var el = element[0];
            el.children[0].addEventListener(
                'click',
                function () {
                    var icon = el.getElementsByTagName("i")[0].classList;
                    if (icon.contains("mdi-chevron-right")) {
                        icon.remove("mdi-chevron-right");
                        icon.add("mdi-chevron-down");
                        return OpenByRoot(el);
                    }
                    if (icon.contains("mdi-chevron-down")) {
                        icon.remove("mdi-chevron-down");
                        icon.add("mdi-chevron-right");
                        return CloseByRoot(el);
                    }
                }
            );

            function CloseByRoot(el) {
                var subTree = el.nextElementSibling.getElementsByTagName("li");
                for (var i = 0; i < subTree.length; i++) {
                    if (!subTree[i].classList.contains("close")) {
                        subTree[i].classList.add("close");
                    }
                    var icon = subTree[i].getElementsByTagName("i")[0].classList;
                    if (icon.contains("mdi-chevron-down")) {
                        icon.remove("mdi-chevron-down");
                        icon.add("mdi-chevron-right");
                    }
                }
            }

            function OpenByRoot(el) {
                var children = el.nextElementSibling.children;
                for (var i = 0; i < children.length; i++) {
                    children[i].classList.remove("close");
                }
            }
        }
    }

    function treeDataOneDirective() {
        var directive = {
            restrict: 'EA',
            scope: {
                data: "=",
                option: "="
            },
            templateUrl: 'app/global/tree-picker/tree_data.html',
            controller: 'TreeDataOneController as vm',
            bindToController: true,
        };
        return directive;
    }

    function treeDataTwoDirective() {
        var directive = {
            restrict: 'EA',
            scope: {
                data: "=",
                option: "="
            },
            templateUrl: 'app/global/tree-picker/tree_data.html',
            controller: 'TreeDataTwoController as vm',
            bindToController: true,
        };
        return directive;
    }
})();
