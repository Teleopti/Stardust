'use strict';

var search = angular.module('wfm.notifications', []);
search.controller('NotificationsCtrl', [
	'$scope','NotificationsSvrc',
	function ($scope, NotificationsSvrc) {
		//need to call the service here to get the notifications
		//$scope.notifications = NotificationSvrc.notifications;
		$scope.notifications =  NotificationsSvrc.search.query({ }).$promise.then(function (result) {
			$scope.searchResult = result;
			$scope.first = result[0];
			console.log($scope.searchResult);
		});
		
	}
]);