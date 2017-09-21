'use strict';
describe('SeatPlanCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	beforeEach(function() {
		module('wfm.seatPlan');
		module('externalModules');

		module(function ($provide) {
			$provide.service('Toggle', function() {
				return {
					togglesLoaded: {
						then: function(cb) { cb(); }
					}
				}
			});
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	var mockSeatPlanService = {
		seatPlans: {
			query: function (param) {
				var queryDeferred = $q.defer();
				var result = [{ Date: "2015-03-02", Status: "2" }];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		},
		seatBookingSummaryForDay: {
			get: function() {
				var queryDeferred = $q.defer();
				var result = [{ NumberOfBookings: "10", NumberOfUnscheduledAgentDays: "20", NumberOfScheduleDaysWithoutBookings: "40" }];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it('returns the correct class for a successful seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, seatPlanService: mockSeatPlanService });
		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();
		var dayClass = controller.getDayClass("2015-03-02", 'day');

		expect(dayClass).toEqual('seatplan-status-planned');
	}));

	it('returns the correct class for a failed seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, seatPlanService: mockSeatPlanService });
		controller.loadMonthDetails(moment("2016-07-20"));
		scope.$digest();
		var dayClass = controller.getDayClass("2016-07-20", 'day');

		expect(dayClass).toEqual('');
	}));

	it('returns the correct info for a successful seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, seatPlanService: mockSeatPlanService });

		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();

		var info = controller.getToDayInfo();
		expect(info).toEqual("SeatBookingSummary");
	}));

	it('returns the correct info for a failed seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();

		var controller = $controller('SeatPlanCtrl', { $scope: scope, seatPlanService: mockSeatPlanService });

		controller.loadMonthDetails(moment("2016-07-20"));
		scope.$digest();

		var expectedInfo = controller.seatPlanStatus[3];

		var info = controller.getToDayInfo();
		expect(info).toEqual(expectedInfo);
	}));

	it('returns the correct month name for a seatplan status', inject(function ($controller) {
		var scope = $rootScope.$new();
		var controller = $controller('SeatPlanCtrl', { $scope: scope, seatPlanService: mockSeatPlanService });

		controller.loadMonthDetails(moment("2015-03-02"));
		scope.$digest();
		var info = controller.getSelectedMonthName();

		expect(info).toEqual('March');
	}));
});
