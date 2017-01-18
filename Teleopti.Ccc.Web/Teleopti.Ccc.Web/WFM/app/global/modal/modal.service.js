(function (document, angular) {

    'use strict';

    angular.module('wfm.modal', [])
        .factory('$wfmModal', wfmModalService);

    wfmModalService.$inject = ['$rootScope', '$compile', '$controller', '$templateRequest', '$q'];

    function wfmModalService($rootScope, $compile, $controller, $templateRequest, $q) {

        var confirmModalTemplate = 'app/global/modal/miniConfirmModal.tpl.html';

        function confirm(message, title) {
            return $q(function (resolve, reject) {
                message = message || '';
                title = title || 'Confirm';

                $templateRequest(confirmModalTemplate).then(makeConfirmModal);

                function makeConfirmModal(template) {
                    var scope = $rootScope.$new();
                    var parent = angular.element(document.body);

                    scope.message = message;
                    scope.title = title;
                    scope.close = function closeModal(result) {
                        scope.$evalAsync(function (scope) {
                            scope.$destroy();
                            compiledConfirmModal.remove();
                            resolve(result);
                        });
                    };

                    var compiledConfirmModal = $compile(template)(scope);
                    parent.append(compiledConfirmModal);
                }
            });
        }

        return {
            confirm: confirm
        };

    }

})(document, angular);
