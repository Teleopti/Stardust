(function() {
    'use strict';

    angular
        .module('restNotificationService', ['ngResource'])
        .factory('NotificationsSvrc', NotificationsSvrc);

    NotificationsSvrc.$inject = ['$resource'];

    function NotificationsSvrc($resource) {
        var service = {
            getNotifications: getNotifications
        };

        return service;

				function getNotifications() {
					return $resource('../api/notifications', {}, {
						query: { method: 'GET', params: { }, isArray: true }
					});
				}
    }
})();
