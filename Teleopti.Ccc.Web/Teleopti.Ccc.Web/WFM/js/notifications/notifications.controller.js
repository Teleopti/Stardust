'use strict';

var notification = angular.module('wfm.notifications', []);
notification.controller('NotificationsCtrl', [
	'$scope','NotificationsSvrc',
	function ($scope, NotificationsSvrc) {
		$scope.notificationResult = [];
		NotificationsSvrc.getNotifications.query({ }).$promise.then(function (result) {
			$scope.notificationResult = result;
		});
	}
]);