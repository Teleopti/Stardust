(function () {
    angular
        .module('wfm.treePicker', [])
        .component('treePicker', {
            templateUrl: 'app/global/tree-picker/tree-picker.html',
            controller: TreePickerController,
            bindings: {
                data: '<',
                outputData: '<'
            }
        })

    TreePickerController.inject = [];

    function TreePickerController() {

        var ctrl = this;
        ctrl.$onInit = function () {
            ctrl.isolatedData = JSON.parse(JSON.stringify(ctrl.data));
            traverseNodes(ctrl.isolatedData.nodes, buildNode);
            handledNodes = {};
        }

        function buildNode(node) {
            node.selectNode = selectNode;
            node.isSelectedInUI = false;
            if (doesNodeHaveChildren(node)) {
                node.isOpenInUI = false;
                node.openNode = openNode;
            }
        }

        function selectNode(node) {
            node.isSelectedInUI = !node.isSelectedInUI;
            if (isNodeSelected(node.id)) {
                removeNode(node);
                if (doesNodeHaveChildren(node)) traverseNodes(node.nodes, removeNode);
            } else {
                ctrl.outputData.push(node.id);
                if (doesNodeHaveChildren(node)) traverseNodes(node.nodes, selectNode);
            }
        }

        var handledNodes = {};

        function traverseNodes(nodes, callback) {
            nodes.forEach(function (node) {
                if (!isNodeHandled(node)) {
                    handledNodes[node.id] = true;
                    callback(node);
                } else {
                    handledNodes = {};
                }

                if (doesNodeHaveChildren(node)) {
                    traverseNodes(node.nodes, callback);
                }

            });
        }

        function isNodeHandled(node) {
            return !!handledNodes[node.id];
        }

        function openNode(node) {
            node.isOpenInUI = !node.isOpenInUI;
        }

        function isNodeSelected(id) {
            return ctrl.outputData.indexOf(id) > -1;
        }

        function removeNode(id) {
            var index = ctrl.outputData.indexOf(id);
            ctrl.outputData.splice(index, 1);
        }

        function doesNodeHaveChildren(node) {
            return node.nodes != null && node.nodes.length;
        }
    }

})();