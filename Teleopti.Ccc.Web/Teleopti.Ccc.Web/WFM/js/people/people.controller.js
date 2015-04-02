(function() {
	'use strict';


	angular.module('wfm.people', ['peopleService', 'peopleSearchService'])
		.controller('PeopleCtrl', [
			'$scope', '$filter', '$state', 'PeopleSearch',PeopleController
		])
		.directive('modalDialog', function() {
			return {
				restrict: 'E',
				scope: {
					show: '='
				},
				replace: true, // Replace with the template below
				transclude: true, // we want to insert custom content inside the directive
				link: function(scope, element, attrs) {
					scope.dialogStyle = {};
					if (attrs.width)
						scope.dialogStyle.width = attrs.width;
					if (attrs.height)
						scope.dialogStyle.height = attrs.height;
					scope.hideModal = function() {
						scope.show = false;
					};
				},
				template: "<div class='ng-modal' ng-show='show'>" +
					"<div class='ng-modal-overlay' ng-click='hideModal()'>" +
					"</div>" +
					"<div class='ng-modal-dialog' ng-style='dialogStyle'>" +
					"<div class='ng-modal-close' ng-click='hideModal()'>x</div>" +
					"<div class='ng-modal-dialog-content' ng-transclude></div>" +
					"</div>" +
					"</div>"
			};
		});

	function PeopleController($scope, $filter, $state, SearchSvrc) {
		$scope.searchResult = [];
		$scope.keyword = '';
		$scope.searchKeyword = function() {
			SearchSvrc.search.query({ keyword: $scope.keyword }).$promise.then(function(result) {
				$scope.searchResult = result;
				console.log($scope.searchResult);
			});
		};

		$scope.modalShown = false;
		$scope.toggleModal = function() {
			$scope.modalShown = !$scope.modalShown;
		};
	}

})();