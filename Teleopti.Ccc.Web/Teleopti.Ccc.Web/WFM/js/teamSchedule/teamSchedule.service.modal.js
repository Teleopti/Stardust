(function (document, angular) {

    'use strict';

    angular.module('wfm.teamSchedule')
        .factory('$wfmModal', wfmModalService);

    wfmModalService.$inject = ['$rootScope', '$compile', '$controller', '$templateRequest'];

    function wfmModalService($rootScope, $compile, $controller, $templateRequest) {

        function show(options) {

            if (!options.template && !options.templateUrl) {
                return;
            }

            if (options.template) {
                makeModal(options.template);
                return;
            }

            $templateRequest(options.templateUrl).then(makeModal);

            function makeModal(template) {

                var modalString = '<modal-dialog show="showModal" on-close="onClose()" ng-cloak>' + template + '</modal-dialog>';
                var scope = $rootScope.$new();
                var parent = options.parent || angular.element(document.body);

                if (typeof options.locals === 'object') {
                    for (var key in options.locals) {
                        scope[key] = options.locals[key];
                    }
                }

                if (options.controller) {
                    $controller(options.controller, { $scope: scope });
                }

                scope.showModal = true;
                scope.onClose = removeModal;

                var compiledModal = $compile(modalString)(scope);
                parent.append(compiledModal);

                function removeModal() {
                    scope.$evalAsync(function (scope) {
                        if (typeof options.onRemoving === 'function') {
                            options.onRemoving();
                        }

                        scope.$destroy();
                        compiledModal.remove();
                    });
                }

            }

        }

        return {
            show: show
        };

    }

})(document, angular);
