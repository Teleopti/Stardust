'use strict';

var notice = angular.module('wfm.notice', ['angular-growl', 'ngAnimate']);
notice.controller('NoticeCtrl', [
	'$scope', 'growl',
	function ($scope, growl) {
		//this is just a test code here should be removed once the mock is finalized
		$scope.dynamicText = "";
		$scope.triggerCallbacks = function () {
			growl.warning("Warning: Press refresh as the data is updated by another user", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000
			});

			growl.success("User is updated successfully", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

			growl.info("Info: Update the schedule of the agent", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

			growl.error("Error: Something exploded so fix it", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

		}
	}
]);