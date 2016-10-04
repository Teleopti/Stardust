'use strict';
describe('NotificationsCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(function () {
		module('wfm.notifications');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	var mockNotificationService = {
		getNotifications: {
			query: function () {
				var queryDeferred = $q.defer();
				var result = ["one", "two", "three", "four", "five"];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};


	xit('not null', inject(function ($controller) {
		var scope = $rootScope.$new();
		var vm = $controller('NotificationsCtrl', { $scope: scope, NotificationsSvrc: mockNotificationService });
		expect($controller).not.toBe(null);
	}));

	xit('contains atleast 5 notifications', inject( function ($controller) {
		var scope = $rootScope.$new();

		var vm = $controller('NotificationsCtrl', { $scope: scope, NotificationsSvrc: mockNotificationService });
		scope.$digest();
		expect(vm.notificationResult.length).toEqual(5);
	}));
});
