'use strict';
describe('NotificationsCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend,
	    $translate;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$translate_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, { "meta": { "code": 200, "errors": null }, "response": { "allowed": false } });
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, { "meta": { "code": 200, "errors": null }, "response": { "allowed": false } });
		$translate = _$translate_;
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
