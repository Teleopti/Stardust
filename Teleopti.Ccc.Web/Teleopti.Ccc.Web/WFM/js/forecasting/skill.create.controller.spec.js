'use strict';
describe('ForecastingSkillCreateCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend;

	var mockToggleService = {
		isFeatureEnabled: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({
					IsEnabled: true
				});
				return { $promise: queryDeferred.promise };
			}
		},
	}

	var mockSkillService = {
		activities: {
			get: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{ Id: "6fbf235d-59f6-4f00-855d-9b5e015ab3c6", Name: "Chat" }, { Id: "472e02c8-1a84-4064-9a3b-9b5e015ab3c6", Name: "E-mail" }]);
				return { $promise: queryDeferred.promise };
			}
		},
		timezones: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ DefaultTimezone: "W. Europe Standard Time", Timezones: [{ Id: "W. Europe Standard Time", Name: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna" }] });
				return { $promise: queryDeferred.promise };
			}
		},
		queues: {
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{ Id: "073aea7a-d071-4585-99ce-9f0800da823d", Name: "Queue 1", LogObjectName: "ACD", Description: "Queue 1" }]);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	var mockGrowl = {};

	beforeEach(function () {
		module('wfm.forecasting');
		module('externalModules');
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));

	it("should have correct default values", inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ForecastingSkillCreateCtrl', { $scope: scope, growl: mockGrowl, SkillService: mockSkillService });

		scope.$digest();
		expect(scope.model.serviceLevelPercent).toBe(80);
		expect(scope.model.serviceLevelSeconds).toBe(20);
		expect(scope.model.shrinkage).toBe(0);
		expect(scope.model.selectedTimezone.Id).toBe("W. Europe Standard Time");
		expect(scope.model.workingHours[0].WeekDaySelections.length).toBe(7);
		expect(scope.model.workingHours[0].StartTime.getTime()).toBe(new Date(2000, 1, 1, 0, 0, 0, 0).getTime());
		expect(scope.model.workingHours[0].EndTime.getTime()).toBe(new Date(2000, 1, 2, 0, 0, 0, 0).getTime());
	}));

	it("should display queues", inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('ForecastingSkillCreateCtrl', { $scope: scope, growl: mockGrowl, SkillService: mockSkillService });

		scope.$digest();
		expect(scope.gridOptions.data.length).toBe(1);
		expect(scope.gridOptions.data[0].Id).toBe("073aea7a-d071-4585-99ce-9f0800da823d");
		expect(scope.gridOptions.data[0].Name).toBe("Queue 1");
		expect(scope.gridOptions.data[0].LogObjectName).toBe("ACD");
		expect(scope.gridOptions.data[0].Description).toBe("Queue 1");
	}));
});
