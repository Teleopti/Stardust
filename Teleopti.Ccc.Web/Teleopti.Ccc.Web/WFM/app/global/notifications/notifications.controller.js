(function() {
    'use strict';

    angular
        .module('wfm.notifications', ['restNotificationService'])
        .controller('NotificationsController', NotificationsController);

    NotificationsController.$inject = ['$scope','NotificationsSvrc'];

    function NotificationsController($scope, NotificationsSvrc) {
        var vm = this;

				vm.notificationResult = [];
				NotificationsSvrc.getNotifications().then(function (result) {
					vm.notificationResult = result;
				});
    }
})();
