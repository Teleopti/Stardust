'use strict';

var searchService = angular.module('restNotificationService', ['ngResource']);
searchService.service('NotificationsSvrc', [
	'$resource', function ($resource) {
		this.getNotifications = $resource('../api/notifications', {}, {
			query: { method: 'GET', params: { }, isArray: true }
		});
	}
]);