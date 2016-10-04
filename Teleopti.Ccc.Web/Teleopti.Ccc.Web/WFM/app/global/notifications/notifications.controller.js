(function() {
    'use strict';

    angular
        .module('wfm.notifications', ['restNotificationService'])
        .controller('NotificationsCtrl', NotificationsCtrl);

    NotificationsCtrl.$inject = ['$scope','NotificationsSvrc'];

    function NotificationsCtrl($scope, NotificationsSvrc) {
        var vm = this;

				vm.notificationResult = [];
				NotificationsSvrc.getNotifications().then(function (result) {
					vm.notificationResult = result;
				});
    }
})();
