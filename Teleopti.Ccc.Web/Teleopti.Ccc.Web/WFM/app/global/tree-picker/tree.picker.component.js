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
            ctrl.validData = validateData();
        };

        function validateData() {
            if (ctrl.data.length == undefined && !ctrl.data.hasOwnProperty('parents')){
                throw new Error('fix the data jao');
                return;
            } 
            else {
                return JSON.parse(JSON.stringify(ctrl.data));
            } 
        }


    }


})();