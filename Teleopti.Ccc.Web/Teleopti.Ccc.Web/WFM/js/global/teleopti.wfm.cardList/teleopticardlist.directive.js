'use strict';

(function () {
	angular.module('teleopti.wfm.cardList')
		.controller('TeleoptiCardListCtrl', function () {

			var vm = this;
			vm.selectedCards = {};
			
			vm.selectCard = function (card) {

				var cardSelected = (vm.selectedCards[card.id] == undefined);

				if (!vm.allowMultiSelect) {
					vm.selectedCards = {};
				}

				if  (cardSelected){
					vm.selectedCards[card.id] = card;				
				} else {
					delete vm.selectedCards[card.id];
				}
			};

			vm.isSelectedCard = function (card) {
				return vm.selectedCards[card.id] != undefined;
			};
		}
	);

	angular.module("teleopti.wfm.cardList")
		.directive("teleoptiCardList", function () {
			return {
				controller: 'TeleoptiCardListCtrl',
				controllerAs: 'vm',
				bindToController: true,
				transclude: true,
				templateUrl: "js/global/teleopti.wfm.cardList/teleopticardlist.tpl.html",
				link: linkFunction

			};
		});

	function linkFunction(scope, element, attributes, vm) {
		vm.allowMultiSelect = 'multiSelect' in attributes;
	};
	

}());