'use strict';


describe('SeatPlanCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(function() {
		module('wfm');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));


	var mockResourcePlannerService = {
		getPlanningPeriodsForRange: {
			query: function (param) {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(true);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	var mockSeatPlanService = {
		seatPlans: {
			query: function (param) {
				var queryDeferred = $q.defer();
				var result = [{ Date: "2015-03-02", Status: "2" }];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			},
		}
	};

	var mockAllTrueToggleService = {
		isFeatureEnabled: {
			query: function(param) {
				var queryDeferred = $q.defer();
				var result = { IsEnable: true };
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise }
			}
		}
	};
	
	it('returns the correct class for a seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerService, seatPlanService: mockSeatPlanService, Toggle: mockAllTrueToggleService });
		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();
		var dayClass = controller.getDayClass("2015-03-02", 'day');

		expect(dayClass).toEqual('seatplan-status-error');
	}));
	
	it('returns the correct info for a seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerService, seatPlanService: mockSeatPlanService, Toggle: mockAllTrueToggleService });
		
		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();

		var expectedInfo = controller.seatPlanStatus[2];

		var info = controller.getToDayInfo();
		expect(info).toEqual(expectedInfo);

	}));


	it('returns the correct month name for a seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();
		var controller = $controller('SeatPlanCtrl', { $scope: scope, ResourcePlannerSvrc: mockResourcePlannerService, seatPlanService: mockSeatPlanService, Toggle: mockAllTrueToggleService });

		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();
		var info = controller.getSelectedMonthName();

		expect(info).toEqual('March');

	}));

});
