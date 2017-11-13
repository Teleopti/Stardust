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
                saveDatacallback: '<',
                saveValueCallback: '<',
                currentRuleId: '<',
                currentSettingId: '<'
            },
        });

    GamificationSettingInfoController.$inject = ['$element', '$scope', '$timeout', '$document'];
    function GamificationSettingInfoController($element, $scope, $timeout, $document) {
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
            // var element = angular.element(document.getElementById("setting-name-id-" + id));
            // event.preventDefault();
            // event.stopPropagation();
            // console.log(element.focus);
            // element.focus();

            $timeout(function () {
                var element = angular.element(document.getElementById("setting-name-id-" + id));
                console.log(element);
                element.focus();
            });


        }

        ctrl.changeName = function (event) {
            ctrl.nameEditModel = false;
        }

        ctrl.$onInit = function () {

        };

        ctrl.nameClicked = function (event) {
            //event.stopPropagation();
        }

        ctrl.onSelected = function (id) {

            // if (ctrl.selectItemCallback) {
            //     var result = ctrl.selectItemCallback();
            //     if (result) {
            //         ctrl.settingInfo.enabled = false;
            //     }
            // }

            //todo check if maximun selected items is bigger than 3

            if (ctrl.settingInfo.is_buildin) {
                if (ctrl.saveValueCallback) {
                    ctrl.saveValueCallback(id, ctrl.settingInfo.enable);
                }
            }
        }

        ctrl.itemChanged = function (event, id) {
            if (ctrl.settingInfo.is_buildin) {
                if (ctrl.saveValueCallback) {
                    var value = event.target.value;
                    ctrl.saveValueCallback(id, value);
                }
            }
        }

        ctrl.$postLink = function () {
            $document.bind('click', function (event) {
                if (event) {
                    var activeElement = document.activeElement;
                    var inputs = document.getElementsByClassName("setting-name-input");
                    for (var i = 0; i < inputs.length; i++) {
                        var input = inputs[i];
                        if (input == activeElement) {
                            console.log(input);
                            input.blur();
                        }
                    }
                }
            });
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