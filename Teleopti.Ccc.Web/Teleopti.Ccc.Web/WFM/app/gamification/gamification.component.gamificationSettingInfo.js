(function () {
    'use strict';

    angular
        .module('wfm.gamification')
        .component('gamificationSettingInfo', {
            templateUrl: 'app/gamification/html/g.component.gamificationSettingInfo.tpl.html',
            controller: GamificationSettingInfoController,
            bindings: {
                _settingInfo: '<settingInfo',
                selectItemCallback: '<',
                saveDataCallback: '<',
                saveValueCallback: '<',
                currentRuleId: '<',
                currentSettingId: '<'
            }
        });

    GamificationSettingInfoController.$inject = ['$element', '$scope', '$timeout', '$document'];
    function GamificationSettingInfoController($element, $scope, $timeout, $document) {
        var ctrl = this;
        ctrl.settingInfo = {};
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
            $timeout(function () {
                var element = angular.element(document.getElementById("setting-name-id-" + id));
                element.focus();
            });
        }

        ctrl.changeName = function () {
            ctrl.nameEditModel = false;
            var data = {
                GamificationSettingId: ctrl.currentSettingId,
                QualityId: ctrl.settingInfo.id,
                Name: ctrl.settingInfo.name
            };

            if (ctrl.saveDataCallback) {

                ctrl.saveDataCallback('ExternalBadgeSettingDescription', data, ctrl.settingInfo);
            }
        }

        ctrl.$onInit = function () {

        };

        ctrl.onSelected = function (id) {
            if (ctrl.selectItemCallback) {
                var result = ctrl.selectItemCallback();
                if (result) {
                    ctrl.settingInfo.enable = false;
                    return;
                }
            }

            if (ctrl.settingInfo.is_buildin) {
                if (ctrl.saveValueCallback) {
                    ctrl.saveValueCallback(id, ctrl.settingInfo.enable);
                }
            }
            else {
                var data = {
                    GamificationSettingId: ctrl.currentSettingId,
                    QualityId: ctrl.settingInfo.id,
                    Value: ctrl.settingInfo.enable
                }

                if (ctrl.saveDataCallback) {
                    ctrl.saveDataCallback('ExternalBadgeSettingEnabled', data);
                }
            }
        }

        ctrl.itemChanged = function (item) {
            if (item.hasError) {
                item.value = findOriginalValue(item);
                item.hasError = false;
                return;
            }

            if (ctrl.settingInfo.is_buildin) {
                if (ctrl.saveValueCallback) {
                    var value = item.value;
                    ctrl.saveValueCallback(item.id, value);
                }
            } else {
                var data = {
                    GamificationSettingId: ctrl.currentSettingId,
                    QualityId: ctrl.settingInfo.id,
                    UnitType: item.unit_type,
                    ThresholdValue: item.value
                };

                if (ctrl.saveDataCallback) {
                    ctrl.saveDataCallback(item.id, data);
                }
            }
        }

        ctrl.largerChanged = function () {

        }

        var findOriginalValue = function (newItem) {
            for (var index = 0; index < ctrl._settingInfo.rule_settings.length; index++) {
                var setting = ctrl._settingInfo.rule_settings[index];
                if (setting.rule_id == ctrl.currentRuleId) {
                    for (var i = 0; i < setting.items.length; i++) {
                        var item = setting.items[i];
                        if (item.id == newItem.id) {
                            return item.value;
                        }
                    }
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
                            //console.log(input);
                            //input.blur();
                        }
                    }
                }
            });
        }

        ctrl.getSettingItems = function () {
            if (!ctrl.settingInfo) {
                return [];
            }
            for (var index = 0; index < ctrl.settingInfo.rule_settings.length; index++) {
                var setting = ctrl.settingInfo.rule_settings[index];
                if (setting.rule_id == ctrl.currentRuleId) {
                    return setting.items;
                }
            }
        }

        var getValidationRules = function () {
            if (!ctrl.settingInfo) {
                return nul;
            }
            for (var index = 0; index < ctrl.settingInfo.rule_settings.length; index++) {
                var setting = ctrl.settingInfo.rule_settings[index];
                if (setting.rule_id == ctrl.currentRuleId) {
                    return setting.validation;
                }
            }
        }

        ctrl.keyUp = function (item) {
            var pattern = ''
            if (item.type == 'time') {
                pattern = /^(0[0-1]):([0-5][0-9]):([0-5][0-9])$/;
            }

            if (item.type == 'number') {
                if (ctrl.settingInfo.is_buildin) {
                    pattern = /^\+?[1-9][0-9]*$/;
                }
                else {
                    pattern = /^[1-9]\d*.\d*|0.\d*[1-9]\d*$/;
                }
            }

            if (item.type == 'percent') {
                pattern = /^0.\d+$/;
            }

            if (!pattern.test(item.value)) {
                item.hasError = true;
                item.errorMsg = 'InvalidFormat';
                return;
            } else {
                item.hasError = false;
                item.errorMsg = '';
            }

            validateValues(item);
        }

        var validateValues = function (item) {
            var validation = getValidationRules();
            if (validation) {
                if (validation.max) {
                    checkMaxValue(item, validation.max);
                }

                if (item.hasError) {
                    return;
                }

                if (validation.valueOrder) {
                    checkValueOrder(item, validation.valueOrder);
                }
            }
        }

        var checkValueOrder = function (item, order) {
            var items = ctrl.getSettingItems();
            for (var index = 0; index < items.length; index++) {
                var currentItem = items[index];
                if (currentItem.id == item.id) {
                    var previous, next;

                    if (index > 0) {
                        previous = items[index - 1];
                    }

                    if (index < items.length - 1) {
                        next = items[index + 1];
                    }

                    if (order == 'asc') {
                        if (next) {
                            item.hasError = convertValueForItem(item) >= convertValueForItem(next);
                            if (item.hasError) {
                                item.errorMsg = 'Value cannot be smaller than next item';
                                return;
                            }
                        }

                        if (previous) {
                            item.hasError = convertValueForItem(item) < convertValueForItem(previous);
                            if (item.hasError) {
                                item.errorMsg = 'Value cannot be larger than previous item';
                                return;
                            }
                        }
                    }

                    if (order == 'desc') {
                        if (next) {
                            item.hasError = convertValueForItem(item) <= convertValueForItem(next);
                            if (item.hasError) {
                                item.errorMsg = 'Value cannot be samller than next item';
                                return;
                            }
                        }

                        if (previous) {
                            item.hasError = convertValueForItem(item) > convertValueForItem(previous);
                            if (item.hasError) {
                                item.errorMsg = 'Value cannot be larger than previous item';
                                return;
                            }
                        }
                    }
                }
            }
        }

        var convertValueForItem = function (item) {
            if (item.type == 'number' || item.value == 'percent') {
                return parseFloat(item.value);
            }
            else {
                var subValues = item.value.split(':');

                var hourValue = convertTimeToNumber(subValues[0], 100);
                var minuteValue = convertTimeToNumber(subValues[1], 10);
                var secondValue = convertTimeToNumber(subValues[2], 1);

                return hourValue + minuteValue + secondValue;
            }
        }

        function convertTimeToNumber(time, scale) {
            var result = 0;

            if (time == '00') {
                result = 0;
            } else {
                if (time.indexOf('0') == 0) {
                    result = parseInt(time.substr(1)) * scale
                } else {
                    result = parseInt(time) * scale;
                }
            }

            return result;
        }

        var checkMaxValue = function (item, max) {
            if (item.value > max) {
                item.hasError = true;
                item.errorMsg = 'Value cannot be large than ' + max;
            }
        }

        ctrl.$onChanges = function (changesObj) {
            if (changesObj._settingInfo && changesObj._settingInfo.currentValue) {
                angular.copy(changesObj._settingInfo.currentValue, ctrl.settingInfo);
            }
        };

        ctrl.$onDestroy = function () { };
    }
})();