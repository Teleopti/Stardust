(function () {
    'use strict';

    angular
        .module('wfm.gamification')
        .component('gamificationSettingInfo', {
            templateUrl: 'app/gamification/html/g.component.gamificationSettingInfo.tpl.html',
            controller: GamificationSettingInfoController,
            bindings: {
                settingInfo: '<',
                selectItemCallback: '<',
                currentRuleId: '<'
            },
        });

    GamificationSettingInfoController.$inject = ['$element', '$scope', '$document'];
    function GamificationSettingInfoController($element, $scope, $document) {
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
        ctrl.nameEditModel = false;

        ctrl.onEditNameClicked = function (event, id) {
            ctrl.nameEditModel = true;
            var element = document.getElementById("setting-name-id-" + id);
            element.focus();
        }

        ctrl.changeName = function (event) {
            ctrl.nameEditModel = false;
        }

        ctrl.$onInit = function () {

        };

        ctrl.nameClicked = function (event) {
            event.stopPropagation();
        }

        ctrl.onSelected = function () {

            if (ctrl.selectItemCallback) {
                var result = ctrl.selectItemCallback();
                if (result) {
                    ctrl.settingInfo.enabled = false;
                }
            }
        }

        ctrl.$postLink = function () {
            $document.bind('click', function () {
                var nameInputs = document.getElementsByClassName("setting-name-input");
                for (var i = 0; i < nameInputs.length; i++) {
                    var input = nameInputs[i];
                    input.blur();
                }
            })
        }

        ctrl.getSettingItems = function () {
            for (var index = 0; index < ctrl.settingInfo.rule_settings.length; index++) {
                var setting = ctrl.settingInfo.rule_settings[index];
                if (setting.rule_id == ctrl.currentRuleId) {
                    return setting.items;
                }
            }
        }

        ctrl.$onChanges = function (changesObj) { };
        ctrl.$onDestroy = function () { };
    }
})();