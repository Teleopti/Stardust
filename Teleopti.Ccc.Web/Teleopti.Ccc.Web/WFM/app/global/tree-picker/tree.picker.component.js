(function () {
    angular
        .module('wfm.treePicker', [])
        .component('treePicker', {
            templateUrl: 'app/global/tree-picker/tree-picker.html',
            controller: TreePickerController,
            bindings: {
                data: '<',
                outputData: '<',
                node: '<'
            }
        })

    TreePickerController.inject = [];

    function TreePickerController() {

        var ctrl = this;
      
        function traverseTree(tree) {
            if (tree.nodes != null) {
                tree.nodes.forEach(function (node) {
                    node.parent = tree
                })
                if (nodeHasChildren(tree))
                    traverseNodes(tree.nodes, traverseTree)
            }
        }

        ctrl.$onInit = function () {
            if (ctrl.node == null) {
                ctrl.treeCollection = Object.assign(ctrl.data);
                ctrl.treeCollection.nodes.forEach(function (tree) {
                    traverseTree(tree)
                });
                traverseNodes(ctrl.treeCollection.nodes, buildNode);
            } else {
                ctrl.treeCollection = ctrl.node;
            }
        }

        function buildNode(node) {
            node.clickedNode = clickedNode;
            node.isSelectedInUI = false;
            if (nodeHasChildren(node)) {
                node.isOpenInUI = false;
                node.openNode = openNode;
            }
        }

        function toggleParent(node) {
            if (node.parent != null) {
                var found = false;
                for (var i = 0; i < node.parent.nodes.length && !found; i++) {
                    if (node.parent.nodes[i].isSelectedInUI) {
                        addNode(node.parent)
                        found = true;
                    } 
                }
                 if (!found) {
                    removeNode(node.parent)
                }
                toggleParent(node.parent)
            }
        }

        function toggleNode(node) {
            if (isNodeSelected(node.id)) {
                removeNode(node);
            } else {
                addNode(node)
            }
        }

        function clickedNode(node) {
            toggleNode(node);
            toggleParent(node);

            if (node.nodes != null)
                traverseNodes(node.nodes, toggleNode)
        }

        function addNode(node) {
            if (!isNodeSelected(node.id)) {
                node.isSelectedInUI = true;
                ctrl.outputData.push(node.id);
            }
        }

        function traverseNodes(nodes, callback) {
            nodes.forEach(function (node) {
                callback(node);
                if (nodeHasChildren(node))
                    traverseNodes(node.nodes, callback);
            });
        }

        function removeNode(node) {
            node.isSelectedInUI = false;
            var index = ctrl.outputData.indexOf(node.id);
            ctrl.outputData.splice(index, 1);
        }

        function openNode(node) {
            node.isOpenInUI = !node.isOpenInUI;
        }

        function isNodeSelected(id) {
            return ctrl.outputData.indexOf(id) > -1;
        }

        function nodeHasChildren(node) {
            return node.nodes != null && node.nodes.length;
        }

    }

})();