'use strict';

var search = angular.module('wfm.notifications', []);
search.controller('NotificationCtrl', [
	'$scope','NotificationSvrc',
	function ($scope, NotificationSvrc) {
		//need to call the service here to get the notifications
		//$scope.notifications = ['Quick forcast triggered by Ashley', 'Scheduled published for team yellow', 'Scheduling complete for 2015-04-15 to 2015-05-16'];
		$scope.notifications = NotificationSvrc.notifications;
	}
]);