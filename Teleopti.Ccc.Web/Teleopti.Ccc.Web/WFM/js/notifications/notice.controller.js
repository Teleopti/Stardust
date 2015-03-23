'use strict';

var notice = angular.module('wfm.notice', ['angular-growl', 'ngAnimate']);
notice.controller('NoticeCtrl', [
	'$scope', 'growl',
	function ($scope, growl) {
		//this is just a test code here should be removed once the mock is finalized
		$scope.dynamicText = "";
		$scope.triggerCallbacks = function () {
			growl.warning("<i class='mdi mdi-alert-circle'></i> Warning: Press refresh as the data is updated by another user", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 8000
			});

			growl.success("<i class='mdi mdi-message-alert'></i> User is updated successfully", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

			growl.info("<i class='mdi mdi-information'></i> Info: Update the schedule of the agent", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000
			});

			growl.error("<i class='mdi mdi-alert-octagon'></i> Error: Something exploded so fix it", {
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