(function () {
    'use strict';

    angular
    .module('wfm.popup', [])
    .component('wfmPopup', {
        transclude: true,
        templateUrl: 'app/styleguide/popup-menu/popup.tpl.html',
        controller: PopupComponentController,
        bindings: {
            show: '='
        },
    });
    PopupComponentController.inject = [];
    function PopupComponentController() {
        var ctrl = this;
    }
})();
