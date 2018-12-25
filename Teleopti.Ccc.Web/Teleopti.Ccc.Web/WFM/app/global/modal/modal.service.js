(function (document, angular) {

	'use strict';

	angular.module('wfm.confirmModal', [])
		.factory('$wfmConfirmModal', wfmModalService);

	wfmModalService.$inject = ['$rootScope', '$compile', '$translate', '$templateRequest', '$q'];

	function wfmModalService($rootScope, $compile, $translate, $templateRequest, $q) {

		var confirmModalTemplate = 'app/global/modal/miniConfirmModal.tpl.html';

		function confirm(message, title, buttonTexts) {
			return $q(function (resolve) {
				message = message || '';
				title = title || 'Confirm';
				buttonTexts = buttonTexts || [$translate.instant('Apply'), $translate.instant('Cancel')];

				$templateRequest(confirmModalTemplate).then(makeConfirmModal);

				function makeConfirmModal(template) {
					var scope = $rootScope.$new();
					var parent = angular.element(document.body);

					scope.message = message;
					scope.title = title;
					scope.buttonTexts = buttonTexts;
					scope.close = function closeModal(result) {
						scope.$evalAsync(function (scope) {
							scope.$destroy();
							compiledConfirmModal.remove();
							resolve(result);
						});
					};

					var compiledConfirmModal = $compile(template)(scope);
					parent.append(compiledConfirmModal);
					var focusTarget = parent[0].querySelector('.mini-confirm .dialog-mini .focus-default');
					angular.element(focusTarget).focus();
				}
			});
		}

		return {
			confirm: confirm
		};

	}

})(document, angular);
