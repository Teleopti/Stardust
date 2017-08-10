(function () {
    angular
        .module('wfm.treePicker', [])
        .component('treePicker', {
            templateUrl: 'app/global/tree-picker/tree-picker.html',
            controller: TreePickerController,
            bindings: {
                data: '<'
            }
        })

    TreePickerController.inject = [];

    function TreePickerController() {
        var ctrl = this;

        ctrl.$onInit = function () {
            ctrl.isolatedData = JSON.parse(JSON.stringify(ctrl.data));
            traverseNodes(ctrl.isolatedData.nodes);
        }

        function traverseNodes(nodes) {
            nodes.forEach(function (node) {
                node.selectNode = selectNode;
                node.isSelectedInUI = false;
                if (node.nodes != null && node.nodes.length) {
                    node.isOpenInUI = false;
                    node.openNode = openNode;
                    traverseNodes(node.nodes);
                }
            });
        }

        function selectNode(node) {
            node.isSelectedInUI = !node.isSelectedInUI;
        }

        function openNode(node) { 
            node.isOpenInUI = !node.isOpenInUI;
        }

    }


})();