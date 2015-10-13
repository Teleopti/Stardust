'use strict';

(function () {
	angular.module('wfm.cardList', []);
})();

(function () {

	function wfmCardDirectiveController() {

		var vm = this;

		vm.isSelected = function () {
			return vm.parentVm.isSelectedCard(vm);
		};

		vm.select = function () {
			vm.parentVm.selectCard(vm);
		}
	};

	function wfmCardDirective() {

		return {
			controller: 'WfmCardCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: { preselected: "=" },
			templateUrl: "js/global/wfmcardList/wfmcard.html",
			transclude: true,
			require: ['wfmCard', '^wfmCardList'],
			link: function (scope, elem, attr, ctrl, transcludeFn) {

				var vm = ctrl[0];
				vm.id = scope.$id;

				var parentVm = ctrl[1];
				vm.parentVm = parentVm;
				
				transcludeFn(function (clone) {

					angular.forEach(clone, function (cloneEl) {

						if (cloneEl.nodeType === 3) {
							return;
						}

						var tag;

						if (cloneEl.nodeName == "CARD-HEADER") {
							tag = '[transclude-id="header"]';
						}
						if (cloneEl.nodeName == "CARD-BODY") {
							tag = '[transclude-id="body"]';
						}

						var destination = angular.element(elem[0].querySelector(tag));

						if (destination != null && destination.length) {
							destination.append(cloneEl);

						} else {
							cloneEl.remove();

						}
					});
				});

				if (vm.preselected) {
					vm.select();
				}
			}
		};
	};

	angular.module('wfm.cardList').controller('WfmCardCtrl', wfmCardDirectiveController);
	angular.module('wfm.cardList').directive('wfmCard', wfmCardDirective);

}());