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


	it('not null', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('NotificationsCtrl', { $scope: scope, NotificationsSvrc: mockNotificationService });
		expect($controller).not.toBe(null);
	}));

	it('contains atleast 5 notifications', inject( function ($controller) {
		var scope = $rootScope.$new();

		$controller('NotificationsCtrl', { $scope: scope, NotificationsSvrc: mockNotificationService });
		scope.$digest();
		expect(scope.notificationResult.length).toEqual(5);
	}));
});
