(function () {
    'use strict';

    angular
        .module('wfm.gamification')
        .component('gamificationSettingInfo', {
            templateUrl: 'app/gamification/html/g.component.gamificationSettingInfo.tpl.html',
            controller: GamificationSettingInfoController,
            bindings: {
                settingInfo: '<'
            },
        });

    GamificationSettingInfoController.$inject = ['$element', '$scope'];
    function GamificationSettingInfoController($element, $scope) {
        var ctrl = this;
        ctrl.collapsed = true;
        ctrl.iconName = "chevron-up";
        ctrl.onCollapsed = function () {
            ctrl.collapsed = !ctrl.collapsed;
            if (ctrl.collapsed) {
                ctrl.iconName = "chevron-up";
            } else {
                ctrl.iconName = "chevron-down";
            }
        }
        ctrl.$onInit = function () {

        };
        ctrl.$onChanges = function (changesObj) { };
        ctrl.$onDestroy = function () { };
    }
})();