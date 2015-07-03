'use strict';

(function() {
	angular.module('teleopti.wfm.cardList')
		.controller('TeleoptiCardListCtrl', function() {

				var vm = this;
				vm.selectedCard = null;
				vm.selectCard = function(card) {
					vm.selectedCard = vm.isSelectedCard(card) ? null : card;
				};
				vm.isSelectedCard = function(card) {
					return vm.selectedCard == card;
				};
			}
		);

	angular.module("teleopti.wfm.cardList")
		.directive("teleoptiCardList", function() {
			return {
				controller: 'TeleoptiCardListCtrl',
				controllerAs: 'vm',
				bindToController: true,
				transclude: true,
				templateUrl: "js/global/teleopti.wfm.cardList/teleopticardlist.html"

			};
		});
}());