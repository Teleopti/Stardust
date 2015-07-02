
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