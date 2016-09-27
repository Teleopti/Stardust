'use strict';

var restNotificationService = angular.module('restNotificationService', ['ngResource']);
restNotificationService.service('NotificationsSvrc', [
	'$resource', function ($resource) {
		this.getNotifications = $resource('../api/notifications', {}, {
			query: { method: 'GET', params: { }, isArray: true }
		});
	}
]);