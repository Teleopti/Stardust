(function () {
    'use strict';
    angular.module('wfm.teamSchedule')
        .directive('tsActionNotice', function () {
            return {
                transclude: true,
                template: '<div class="notice-container"><div class="notice-item" ng-class="$setType(notice)"><i ng-class="$setIcon(notice)"></i> <span ng-bind-html="notice.content"></span> <span class="actions pull-right"><i class="pull-right mdi mdi-close notice-close" ng-click="$close()"></i><ng-transclude></ng-transclude></span></div></div>'
            };
        })
        .service('actionNoticeService', ['$rootScope', '$compile', '$controller', '$timeout', function ActionNoticeService($rootScope, $compile, $controller, $timeout) {
            this.newNotice = newActionNotice;

            function newActionNotice(options) {
                var types = {
                    success: 'notice-success',
                    error: 'notice-error',
                    info: 'notice-info',
                    warning: 'notice-warning',
                };
                var icons = {
                    success: 'mdi mdi-thumb-up',
                    error: 'mdi mdi-alert-octagon',
                    info: 'mdi mdi-alert-circle',
                    warning: 'mdi mdi-alert',
                };
                var CONTAINER_TAG_NAME = 'wfm-notice';
                var actionNoticeContainer = document.querySelector(CONTAINER_TAG_NAME);
                var containerForContainer = document.body.querySelector('#materialcontainer') || document.body;

                var notice = compile();
                append(notice);

                function compile() {
                    var compiled, controller;

                    var templateScope = $rootScope.$new();
                    var template = options.template ? ['<ts-action-notice>', options.template, '</ts-action-notice>'].join('') : '<ts-action-notice></ts-action-notice>';

                    templateScope.notice = options;
                    templateScope.$setType = function (notice) {
                        return types[notice.type];
                    };
                    templateScope.$setIcon = function (notice) {
                        return icons[notice.icon];
                    };

                    compiled = $compile(template)(templateScope);

                    templateScope.$close = function removeThisNotice() {
                        removeNotice(compiled);
                    };
                    var locals = {
                        $scope: templateScope,
                        wfmNoticeRemoveThis: templateScope.$close
                    };

                    function FallbackSingleNoticeCtrl($scope, wfmNoticeRemoveThis) { }
                    FallbackSingleNoticeCtrl.$inject = ['$scope', 'wfmNoticeRemoveThis'];

                    controller = $controller(options.controller ? options.controller : FallbackSingleNoticeCtrl, locals);
                    setNgController(compiled, controller);

                    if (options.destroyOnStateChange === true) {
                        $rootScope.$on('$stateChangeSuccess', function removeNoticeOnStateChangeSuccess() {
                            removeNotice(compiled);
                        });
                    }

                    if (options.timeToLive) {
                        $timeout(function noticeAutoClose() {
                            removeNotice(compiled);
                        }, options.timeToLive);
                    }

                    return compiled;
                }

                function removeNotice(element) {
                    angular.element(element).remove();
                }

                function setNgController(element, controller) {
                    angular.element(element).data('$ngControllerController', controller);
                }

                function append(newNotice) {
                    if (!actionNoticeContainer) {
                        actionNoticeContainer = document.createElement(CONTAINER_TAG_NAME);
                        angular.element(containerForContainer).append(actionNoticeContainer);
                    }
                    angular.element(actionNoticeContainer).append(newNotice);
                }
            }
        }]);
})();
