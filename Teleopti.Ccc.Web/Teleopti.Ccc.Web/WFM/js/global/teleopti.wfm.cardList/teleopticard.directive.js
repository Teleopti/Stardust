'use strict';

(function () {
	angular.module('teleopti.wfm.cardList', []);
})();

(function () {

	function teleoptiCardDirectiveController() {

		var vm = this;

		vm.isSelected = function () {
			return vm.parentVm.isSelectedCard(vm);
		};

		vm.select = function () {
			vm.parentVm.selectCard(vm);
		}
	};

	function teleoptiCardDirective() {

		return {
			controller: 'TeleoptiCardCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {},
			templateUrl: "js/global/teleopti.wfm.cardList/teleopticard.html",
			transclude: true,
			require: ['teleoptiCard', '^teleoptiCardList'],
			link: function (scope, elem, attr, ctrl, transcludeFn) {

				var vm = ctrl[0];
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
			}
		};
	};

	angular.module('teleopti.wfm.cardList').controller('TeleoptiCardCtrl', teleoptiCardDirectiveController);

	angular.module('teleopti.wfm.cardList').directive('teleoptiCard', teleoptiCardDirective);

}());