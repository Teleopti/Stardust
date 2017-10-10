(function () {
    'use strict';

    angular
        .module('wfm.treePicker')
        .controller('TreeDataOneController', TreeDataOneController)
        .directive('treeDataOne', treeDataOneDirective)
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
                if (item.$parent.node[vm.nodeChildrenName].length !== 0) {
                    setChildrenNodesToUnselect(item.$parent.node[vm.nodeChildrenName]);
                    return item.$parent.node[vm.nodeSelectedMark] = false;
                } else {
                    return item.$parent.node[vm.nodeSelectedMark] = false;
                }
            }
            var indexList = MapParentIndex(item);
            return setParentNodesToSelect(vm.data[vm.nodeChildrenName], 0, indexList);
        }

        function setChildrenNodesToUnselect(children) {
            children.forEach(function (child) {
                child[vm.nodeSelectedMark] = false;
                if (child[vm.nodeChildrenName] && child[vm.nodeChildrenName].length !== 0) {
                    return setChildrenNodesToUnselect(child[vm.nodeChildrenName]);
                }
            });
        }

        function setParentNodesToSelect(data, i, indexList) {
            var index = indexList[i];
            data[index][vm.nodeSelectedMark] = true;
            if (data[index][vm.nodeChildrenName] && data[index][vm.nodeChildrenName].length > 0) {
                if (i + 1 == indexList.length)
                    return;
                setParentNodesToSelect(data[index][vm.nodeChildrenName], i + 1, indexList);
            }
        }

        function MapParentIndex(item, indexList) {
            if (indexList == null) {
                var indexList = [];
            }
            if (angular.isDefined(item.$parent.$index)) {
                indexList.splice(0, 0, item.$parent.$index);
                MapParentIndex(item.$parent.$parent, indexList)
            }
            return indexList;
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
            templateUrl: 'app/global/tree-picker/tree_data_one.html',
            controller: 'TreeDataOneController as vm',
            bindToController: true,
        };
        return directive;
    }
})();
