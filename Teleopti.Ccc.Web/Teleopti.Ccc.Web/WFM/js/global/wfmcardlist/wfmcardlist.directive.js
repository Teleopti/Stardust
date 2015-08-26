'use strict';

(function () {
	angular.module('wfm.cardList')
		.controller('WfmCardListCtrl', function () {

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

	angular.module("wfm.cardList")
		.directive("wfmCardList", function () {
			return {
				controller: 'WfmCardListCtrl',
				controllerAs: 'vm',
				bindToController: true,
				transclude: true,
				templateUrl: "js/global/wfmcardList/wfmcardlist.html",
				link: linkFunction

			};
		});

	function linkFunction(scope, element, attributes, vm) {
		vm.allowMultiSelect = 'multiSelect' in attributes;
	};
	
}());