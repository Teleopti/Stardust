'use strict';

var people = angular.module('wfm.people', []);

people.controller('PeopleCtrl', [
	'$scope', '$filter',
	function($scope, $filter) {
		$scope.peopleList = [
			{
				"FirstName": "Ada",
				"LastName": "Clinton",
				"Phone": "123123123",
				"Email": "ada@teleopti.com",
				"EmploymentNumber": "100",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Ada",
				"LastName": "Russo",
				"Phone": "3546364758",
				"Email": "ada@teleopti.com",
				"EmploymentNumber": "7999",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Adam",
				"LastName": "Bundy",
				"Phone": "567849369",
				"Email": "adam@teleopti.com",
				"EmploymentNumber": "8000",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Aileen",
				"LastName": "Broccardo",
				"Phone": "7934759347593",
				"Email": "Aileen.Broccardo@insurance.com",
				"EmploymentNumber": "137956",
				"DefaultTimeZone": "W. Europe Standard Time"
			},
			{
				"FirstName": "Alfred",
				"LastName": "Bork",
				"Phone": "04958204385",
				"Email": "Alfred.Bork@insurance.com",
				"EmploymentNumber": "137784",
				"DefaultTimeZone": "W. Europe Standard Time"
			}
		];

		$scope.modalShown = false;
		$scope.toggleModal = function() {
			$scope.modalShown = !$scope.modalShown;
		};
	}
]);
people.directive('modalDialog', function () {
	return {
		restrict: 'E',
		scope: {
			show: '='
		},
		replace: true, // Replace with the template below
		transclude: true, // we want to insert custom content inside the directive
		link: function (scope, element, attrs) {
			scope.dialogStyle = {};
			if (attrs.width)
				scope.dialogStyle.width = attrs.width;
			if (attrs.height)
				scope.dialogStyle.height = attrs.height;
			scope.hideModal = function () {
				scope.show = false;
			};
		},
		template: "<div class='ng-modal' ng-show='show'>" +
			"<div class='ng-modal-overlay' ng-click='hideModal()'>" +
			"</div>" +
			"<div class='ng-modal-dialog' ng-style='dialogStyle'>" +
			"<div class='ng-modal-close' ng-click='hideModal()'>X</div>" +
			"<div class='ng-modal-dialog-content' ng-transclude></div>" +
			"</div>" +
			"</div>"
	};
});
