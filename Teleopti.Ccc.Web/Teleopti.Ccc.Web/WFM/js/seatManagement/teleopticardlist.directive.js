

(function () {

	angular.module("wfm.seatPlan")
		.factory("TranscludeTeleoptiCard", function () {
			return {
				transclude: function (iElem, transcludeFn) {
					transcludeFn(function (clone) {

						angular.forEach(clone, function (cloneEl) {

							if (cloneEl.nodeType === 3) { return; }

							var tag;

							if (cloneEl.nodeName == "CARD-HEADER") {
								tag = '[transclude-id="header"]';
							}
							if (cloneEl.nodeName == "CARD-BODY") {
								tag = '[transclude-id="body"]';
							}

							var destination = angular.element(iElem[0].querySelector(tag));

							if (destination != null && destination.length) {
								destination.append(cloneEl);

							} else {
								cloneEl.remove();

							}
						});
					});
				}
			}
		});


	angular.module('wfm.seatPlan').controller('TeleoptiCardCtrl', seatPlanCardDirectiveController);

	function seatPlanCardDirectiveController() {

		var vm = this;

		vm.isSelected = function () {
			return vm.parentVm.isSelectedCard(vm);
		};

		vm.select = function () {
			vm.parentVm.selectCard(vm);
		}
	};

	var directive = function (TranscludeTeleoptiCard) {

		return {
			controller: 'TeleoptiCardCtrl',
			controllerAs: 'vm',
			bindToController: true,
			scope: {},
			templateUrl: "js/seatManagement/html/teleopticard.html",
			transclude: true,
			require: ['teleoptiCard', '^teleoptiCardList'],
			link: function (scope, elem, attr, ctrl, transcludeFn) {

				var vm = ctrl[0];
				var parentVm = ctrl[1];
				vm.parentVm = parentVm;

				TranscludeTeleoptiCard.transclude(elem, transcludeFn);
			}
		};
	};

	angular.module('wfm.seatPlan').directive('teleoptiCard', directive);

}());

(function () {
	angular.module('wfm.seatPlan')
		.controller('CardListCtrl', function () {

			var vm = this;
			vm.selectedCard = null;
			vm.selectCard = function (card) {
				vm.selectedCard = vm.isSelectedCard(card) ? null : card;
			};
			vm.isSelectedCard = function (card) {
				return vm.selectedCard == card;
			};
		}
		);

	angular.module("wfm.seatPlan")
		.directive("teleoptiCardList", function () {
			return {
				controller: 'CardListCtrl',
				controllerAs: 'vm',
				bindToController: true,
				transclude: true,
				templateUrl: "js/seatManagement/html/teleopticardlist.html"

			};
		});

}());