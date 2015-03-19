'use strict';

var search = angular.module('wfm.notifications', []);
search.controller('NotificationsCtrl', [
	'$scope','NotificationsSvrc',
	function ($scope, NotificationsSvrc) {
		$scope.searchResult = [];
		NotificationsSvrc.getNotifications.query({ }).$promise.then(function (result) {
			$scope.searchResult = result;
			console.log(result);
		});
	}
]);