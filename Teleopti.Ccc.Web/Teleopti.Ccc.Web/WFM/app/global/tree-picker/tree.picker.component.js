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
        }

        function buildNode(node) {
            node.selectNode = selectNode;
            node.isSelectedInUI = false;
            if (node.nodes != null && node.nodes.length) {
                node.isOpenInUI = false;
                node.openNode = openNode;
            }
        }

        function selectNode(node) {
            node.isSelectedInUI = !node.isSelectedInUI;
            if (isNodeSelected(node.id)) {
                console.log('im removing stuff', node.id);
                removeNode(node.id)
            }
            else {
                console.log(node);
                ctrl.outputData.push(node.id);
                if (node.nodes != null && node.nodes.length) traverseNodes(node.nodes, selectNode);
            }   
        }

        function traverseNodes(nodes, callback) {
            console.log('nodes in traverse', nodes);
            nodes.forEach(function (node) {
                callback(node);
                if (node.nodes != null && node.nodes.length) {
                    traverseNodes(node.nodes, callback);
                }
            });
        }


        function openNode(node) {
            node.isOpenInUI = !node.isOpenInUI;
        }

        function isNodeSelected(id) {
            return ctrl.outputData.indexOf(id) > -1;
        }

        function removeNode(id) {
            console.log(ctrl.outputData);
            var index = ctrl.outputData.indexOf(id);
            ctrl.outputData.splice(index, 1);
        }
    }

})();