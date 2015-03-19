'use strict';

var searchService = angular.module('restNotificationService', ['ngResource']);
searchService.service('NotificationsSvrc', [
	'$resource', function ($resource) {
		this.getNotifications = $resource('../api/notifications', {}, {
			query: { method: 'GET', params: { }, isArray: true }
		});
		//this.notifications = ['Quick forcast triggered by Ashley', 'Scheduled published for team yellow', 'Scheduling complete for 2015-04-15 to 2015-05-16'];
	}
]);