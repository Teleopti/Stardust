(function() {
    'use strict'

    function RtaCardController() {
        var ctrl = this
    }

    function ToggleableDirective() {
        return {
            restrict: 'A',
            scope: false,
            link: function link(scope) {
                var ctrl = scope.$ctrl
                var opened = ctrl.defaultOpen != null

                ctrl.toggle = function toggle() {
                    set(!ctrl.isOpen())
                }

                ctrl.open = function open() {
                    set(true)
                }

                ctrl.close = function close() {
                    set(false)
                }

                ctrl.isOpen = function isOpen() {
                    return opened
                }

                function set(value) {
                    opened = value
                }
            }
        }
    }

    angular
        .module('wfm.rta')
        .component('rtaCard', {
            templateUrl: 'app/rta/historical/rta-card-component.html',
            controller: RtaCardController,
            transclude: {
                header: 'rtaCardHeader',
                body: 'rtaCardBody'
            },
            bindings: {
                defaultOpen: '@'
            }
        })
        .directive('toggleable', ToggleableDirective)
}())