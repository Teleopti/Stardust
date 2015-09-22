'use strict';

var notice = angular.module('wfm.notice', ['angular-growl', 'ngAnimate']);
notice.controller('NoticeCtrl', [
	'$scope', 'growl',
	function ($scope, growl) {
		//this is just a test code here should be removed once the mock is finalized
		$scope.dynamicText = "";
		$scope.triggerCallbacks = function () {
			growl.warning("<i class='mdi mdi-alert'></i> Warning: Press refresh as the data was updated by another user.", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!";
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!";
				},
				ttl: 8000
			});

			growl.success("<i class='mdi mdi-thumb-up'></i> Success: User is updated successfully.", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!";
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

			growl.info("<i class='mdi mdi-information'></i> Info: A user logged out.", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!";
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!";
				},
				ttl: 5000
			});

			growl.error("<i class='mdi mdi-alert-octagon'></i> Error: Something exploded so fix it.", {
				onclose: function () {
					$scope.dynamicText = "Trigger Close!"
				},
				onopen: function () {
					$scope.dynamicText = "Trigger Open!"
				},
				ttl: 5000,
				disableCountDown: true
			});

		};

		$(function () {
			$('#materialcontainer').scroll(function () {
				var uiHeight = $('[ui-view]').height();
				var containerHeight = $('#materialcontainer').height();
				var scrollTop = $('#materialcontainer').scrollTop();

				if (uiHeight > containerHeight) {
					$('#notice').css({ bottom: -scrollTop });
				} else {
					$('#notice').css({ bottom: 0 });
				}
			});

			$(window).resize(function () {
				$('#materialcontainer').scrollTop(0);
			});
		});
	}
]);