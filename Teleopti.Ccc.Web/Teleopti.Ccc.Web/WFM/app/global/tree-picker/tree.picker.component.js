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
    }


})();