(function () {
    'use strict';

    angular
        .module('wfm.treePicker')
        .component('treeDataOne', {
            templateUrl: 'app/global/tree-picker/tree_data.tpl.html',
            controller: 'TreeDataOneController',
            controllerAs: 'vm',
            bindings: {
                data: "=",
                option: "="
            }
        })
        .component('treeDataTwo', {
            templateUrl: 'app/global/tree-picker/tree_data.tpl.html',
            controller: 'TreeDataTwoController',
            controllerAs: 'vm',
            bindings: {
                data: "=",
                option: "="
            }
        })
        .component('treeDataThree', {
            templateUrl: 'app/global/tree-picker/tree_data.tpl.html',
            controller: 'TreeDataThreeController',
            controllerAs: 'vm',
            bindings: {
                data: "=",
                option: "="
            }
        })
        .controller('TreeDataOneController', TreeDataOneController)
        .controller('TreeDataTwoController', TreeDataTwoController)
        .controller('TreeDataThreeController', TreeDataThreeController)
        .directive('treeAnimate', treeAnimate);

    TreeDataOneController.$inject = ['$element'];
    TreeDataTwoController.$inject = [];
    TreeDataThreeController.$inject = [];

    function TreeDataOneController($element) {
        var vm = this;

        vm.node;
        vm.nodeDisplayName = "name";
        vm.nodeChildrenName = "children";
        vm.nodeSelectedMark = "mark";
        vm.selectedState = "none";
        vm.selectNode = selectNode;

        vm.$onInit = fetchSetting;

        function fetchSetting() {
            if (angular.isDefined(vm.option)) {
                vm.nodeDisplayName = vm.option.NodeDisplayName ? vm.option.NodeDisplayName : "name";
                vm.nodeChildrenName = vm.option.NodeChildrenName ? vm.option.NodeChildrenName : "children";
                vm.nodeSelectedMark = vm.option.NodeSelectedMark ? vm.option.NodeSelectedMark : "mark";
            }
            return;
        }

        function selectNode(item, event) {
            vm.node = item;
            if (item.$parent.node[vm.nodeSelectedMark] == true) {
                item.$parent.node[vm.nodeSelectedMark] = false;
                if (item.$parent.node[vm.nodeChildrenName] && item.$parent.node[vm.nodeChildrenName].length !== 0) {
                    removeSemiStateToAllChildren(event.target.parentNode.nextElementSibling);
                    setChildrenNodesToUnselect(item.$parent.node[vm.nodeChildrenName]);
                }
            } else {
                setParentNodesSelectState(item.$parent, true);
            }
            removeSemiStateToNode(event.target);
            return checkSemiStateToSelectedNodeParent(item.$parent, event.target.parentNode.parentNode.parentNode.parentNode);
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
                return setParentNodesSelectState(data.$parent.$parent, state);
            }
        }

        function removeSemiStateToAllChildren(checkItem) {
            var items = checkItem.getElementsByClassName("semi-select");
            for (var index = 0; index < items.length; index++) {
                removeSemiStateToNode(items[index]);
            }
        }

        function addSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("tree-handle-wrapper")) {
                checkItem.classList.add("semi-select");
            }
        }

        function removeSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("semi-select")) {
                checkItem.classList.remove("semi-select");
            }
        }

        function checkAnyChildrenNodesSelectedState(parentSiblings, checkItem) {
            var selectedSiblings = parentSiblings.filter(function (sib) { return sib[vm.nodeSelectedMark] == false || !sib[vm.nodeSelectedMark]; })
            if (selectedSiblings.length == 0 || selectedSiblings.length == parentSiblings.length)
                return removeSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
            if (selectedSiblings.length < parentSiblings.length)
                return addSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
        }

        function checkSemiStateToSelectedNodeParent(data, checkItem) {
            if (data.$parent.$parent.node) {
                checkAnyChildrenNodesSelectedState(data.$parent.$parent.node[vm.nodeChildrenName], checkItem);
                return checkSemiStateToSelectedNodeParent(data.$parent.$parent, checkItem.parentNode.parentNode);
            }
            return removeSemiStateToNode(checkItem);
        }
    }

    function TreeDataTwoController() {
        var vm = this;

        var rootSelectUnique = "false;"
        vm.node;
        vm.nodeDisplayName = "name";
        vm.nodeChildrenName = "children";
        vm.nodeSelectedMark = "mark";
        vm.selectNode = selectNode;

        vm.$onInit = fetchSetting;

        function fetchSetting() {
            if (angular.isDefined(vm.option)) {
                vm.nodeDisplayName = vm.option.NodeDisplayName ? vm.option.NodeDisplayName : "name";
                vm.nodeChildrenName = vm.option.NodeChildrenName ? vm.option.NodeChildrenName : "children";
                vm.nodeSelectedMark = vm.option.NodeSelectedMark ? vm.option.NodeSelectedMark : "mark";
                rootSelectUnique = vm.option.RootSelectUnique ? vm.option.RootSelectUnique : false;
            }
            return;
        }

        function selectNode(item, event) {
            vm.node = item;
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
                removeSemiStateToAllChildren(event.target.parentNode.nextElementSibling);
                setChildrenNodesSelectState(item.$parent.node[vm.nodeChildrenName], state);
            }
            removeSemiStateToNode(event.target);
            return checkSemiStateToSelectedNodeParent(item.$parent, event.target.parentNode.parentNode.parentNode.parentNode);
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

        function removeSemiStateToAllChildren(checkItem) {
            var items = checkItem.getElementsByClassName("semi-select");
            for (var index = 0; index < items.length; index++) {
                removeSemiStateToNode(items[index]);
            }
        }

        function addSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("tree-handle-wrapper")) {
                checkItem.classList.add("semi-select");
            }
        }

        function removeSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("semi-select")) {
                checkItem.classList.remove("semi-select");
            }
        }

        function checkAnyChildrenNodesSelectedState(parentSiblings, checkItem) {
            var selectedSiblings = parentSiblings.filter(function (sib) { return sib[vm.nodeSelectedMark] == false || !sib[vm.nodeSelectedMark]; })
            if (selectedSiblings.length == 0 || selectedSiblings.length == parentSiblings.length)
                return removeSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
            if (selectedSiblings.length < parentSiblings.length)
                return addSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
        }

        function checkSemiStateToSelectedNodeParent(data, checkItem) {
            if (!!data.$parent.$parent.node) {
                checkAnyChildrenNodesSelectedState(data.$parent.$parent.node[vm.nodeChildrenName], checkItem);
                return checkSemiStateToSelectedNodeParent(data.$parent.$parent, checkItem.parentNode.parentNode);
            }
            return;
        }
    }

    function TreeDataThreeController() {
        var vm = this;

        var rootSelectUnique = "false";
        vm.node;
        vm.nodeDisplayName = "name";
        vm.nodeChildrenName = "children";
        vm.nodeSelectedMark = "selected";
        vm.nodeSemiSelected = "semiSelected";
        vm.selectNode = selectNode;

        vm.$onInit = fetchSetting;

        function fetchSetting() {
            if (angular.isUndefined(vm.option)) return;
            vm.nodeDisplayName = vm.option.NodeDisplayName ? vm.option.NodeDisplayName : "name";
            vm.nodeChildrenName = vm.option.NodeChildrenName ? vm.option.NodeChildrenName : "children";
            vm.nodeSelectedMark = vm.option.nodeSelected ? vm.option.nodeSelected : "selected";
            vm.nodeSemiSelected = vm.option.nodeSemiSelected ? vm.option.nodeSemiSelected : "semiSelected";
            rootSelectUnique = vm.option.RootSelectUnique ? vm.option.RootSelectUnique : false;
        }

        function selectNode(item, event) {
            vm.node = item;
            var state = !item.$parent.node[vm.nodeSelectedMark];
            item.$parent.node[vm.nodeSelectedMark] = state;

            if (item.$parent.$parent.$parent.node && item.$parent.$parent.$parent.node[vm.nodeChildrenName].length) {
                setParentNodesSelectState(item.$parent.$parent.$parent);
            }
            if (item.$parent.node[vm.nodeChildrenName] && item.$parent.node[vm.nodeChildrenName].length) {
                if(item.$parent.node[vm.nodeSemiSelected]) {
                    item.$parent.node[vm.nodeSemiSelected] = false;
                    removeSemiStateToNode(event.target);
                 }
                removeSemiStateToAllChildren(event.target.parentNode.nextElementSibling);
                setChildrenNodesSelectState(item.$parent.node[vm.nodeChildrenName], state);
            }
            return checkSemiStateToSelectedNodeParent(item.$parent, event.target.parentNode.parentNode.parentNode.parentNode);
        }

        function siblingsHasSomeSelected(siblings) {
            var selectedSiblings = siblings.filter(function(s){return s[vm.nodeSelectedMark]});
            return selectedSiblings.length > 0 && selectedSiblings.length < siblings.length;
        }

        function siblingsHasAllSelected(siblings) {
            var selectedSiblings = siblings.filter(function(s){return s[vm.nodeSelectedMark]});
            return selectedSiblings.length === siblings.length;
        }

        function siblingsHasSemiSelected(siblings) {
            return siblings.some(function (item) {
                return item[vm.nodeSemiSelected] == true;
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
            var children = data.node[vm.nodeChildrenName];
            var hasSomeSelected = siblingsHasSomeSelected(children);
            var hasSemiSelected = siblingsHasSemiSelected(children);
            var hasNoneSelectedOrSemiSelected = !hasSomeSelected && !hasSemiSelected && !hasAllSelected;
            var hasAllSelected = siblingsHasAllSelected(children);
            if(hasSomeSelected || hasSemiSelected) {
                data.node[vm.nodeSelectedMark] = false;
                data.node[vm.nodeSemiSelected] = true;
            }
            if(hasNoneSelectedOrSemiSelected) {
                data.node[vm.nodeSelectedMark] = false;
                data.node[vm.nodeSemiSelected] = false;
            }
            if(hasAllSelected) {
                data.node[vm.nodeSelectedMark] = true;
                data.node[vm.nodeSemiSelected] = false;
            }
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

        function removeSemiStateToAllChildren(checkItem) {
            var items = checkItem.getElementsByClassName("semi-select");
            for (var index = 0; index < items.length; index++) {
                removeSemiStateToNode(items[index]);
            }
        }

        function addSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("tree-handle-wrapper")) {
                checkItem.classList.add("semi-select");
            }
        }

        function removeSemiStateToNode(checkItem) {
            if (checkItem.classList.contains("semi-select")) {
                checkItem.classList.remove("semi-select");
            }
        }

        function checkAnyChildrenNodesSelectedState(parentSiblings, checkItem) {
            var hasSemiSelected = siblingsHasSemiSelected(parentSiblings);
            var hasSomeSelected = siblingsHasSomeSelected(parentSiblings);
            var hasAllSelected = siblingsHasAllSelected(parentSiblings);
            var hasNoneSelectedOrSemiSelected = !hasSomeSelected && !hasSemiSelected && !hasAllSelected;;
            if((hasSemiSelected || hasSomeSelected) || (hasSemiSelected && hasSomeSelected))  {
                return addSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
            }
            
            if(hasNoneSelectedOrSemiSelected || hasAllSelected) {
                return removeSemiStateToNode(checkItem.childNodes[1].childNodes[3]);
            }
        }

        function checkSemiStateToSelectedNodeParent(data, checkItem) {
            if (!!data.$parent.$parent.node) {
                checkAnyChildrenNodesSelectedState(data.$parent.$parent.node[vm.nodeChildrenName], checkItem);
                return checkSemiStateToSelectedNodeParent(data.$parent.$parent, checkItem.parentNode.parentNode);
            }
            return;
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
                    if (!subTree[i].classList.contains("hidden")) {
                        subTree[i].classList.add("hidden");
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
                    children[i].classList.remove("hidden");
                }
            }
        }
    }
})();
